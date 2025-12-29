using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Lagrange.Core.Utility.Cryptography;

public sealed class EcdhProvider
{
    private EllipticCurve Curve { get; }

    private BigInteger Secret { get; }

    private EllipticPoint Public { get; }

    private readonly struct JacobianPoint(BigInteger x, BigInteger y, BigInteger z)
    {
        public BigInteger X { get; } = x;
        public BigInteger Y { get; } = y;
        public BigInteger Z { get; } = z;
        public bool IsInfinity => Z.IsZero;

        public static JacobianPoint FromAffine(EllipticPoint p) =>
            p.IsDefault ? new JacobianPoint(0, 1, 0) : new JacobianPoint(p.X, p.Y, 1);
    }

    public EcdhProvider(EllipticCurve curve)
    {
        Curve = curve;
        Secret = CreateSecret();
        Public = CreatePublic();
    }

    public EcdhProvider(EllipticCurve curve, byte[] secret)
    {
        Curve = curve;
        Secret = UnpackSecret(secret);
        Public = CreatePublic();
    }

    /// <summary>
    /// Key exchange with bob
    /// </summary>
    /// <param name="ecPub">The unpacked public key from bob</param>
    /// <param name="isHash">Whether to pack the shared key with MD5</param>
    public byte[] KeyExchange(byte[] ecPub, bool isHash)
    {
        var shared = CreateShared(Secret, UnpackPublic(ecPub));
        return PackShared(shared, isHash);
    }

    public byte[] PackPublic(bool compress)
    {
        if (compress)
        {
            var result = new byte[Curve.Size + 1];

            result[0] = (byte)(Public.Y.IsEven ^ Public.Y.Sign < 0 ? 0x02 : 0x03);
            var xBytes = ToFixedBytes(Public.X, Curve.Size);
            xBytes.CopyTo(result, 1);

            return result;
        }
        else
        {
            var result = new byte[Curve.Size * 2 + 1];

            result[0] = 0x04;
            var xBytes = ToFixedBytes(Public.X, Curve.Size);
            var yBytes = ToFixedBytes(Public.Y, Curve.Size);
            xBytes.CopyTo(result, 1);
            yBytes.CopyTo(result, Curve.Size + 1);

            return result;
        }
    }

    public byte[] PackSecret()
    {
        int rawLength = Secret.GetByteCount();
        var result = new byte[rawLength + 4];
        Secret.TryWriteBytes(result.AsSpan()[4..], out _, true, true);
        result[3] = (byte)rawLength;
        return result[..(rawLength + 4)];
    }

    private byte[] PackShared(EllipticPoint ecShared, bool isHash)
    {
        var x = ToFixedBytes(ecShared.X, Curve.Size);
        return !isHash ? x : MD5.HashData(x[..Curve.PackSize]);
    }

    private EllipticPoint UnpackPublic(byte[] publicKey)
    {
        int length = publicKey.Length;
        if (length != Curve.Size * 2 + 1 && length != Curve.Size + 1) throw new Exception("Length does not match.");

        if (publicKey[0] == 0x04) // Not compressed
        {
            return new EllipticPoint(
                new BigInteger(publicKey.AsSpan()[1..(Curve.Size + 1)], true, true),
                new BigInteger(publicKey.AsSpan()[(Curve.Size + 1)..], true, true)
            );
        }
        else // find the y-coordinate from x-coordinate by y^2 = x^3 + ax + b
        {
            var px = new BigInteger(publicKey.AsSpan()[1..], true, true);
            var x3 = px * px * px;
            var ax = px * Curve.A;
            var right = (x3 + ax + Curve.B) % Curve.P;

            var tmp = (Curve.P + 1) >> 2;
            var py = BigInteger.ModPow(right, tmp, Curve.P);

            if (!(py.IsEven && publicKey[0] == 0x02 || !py.IsEven && publicKey[0] == 0x03))
            {
                py = Curve.P - py;
            }

            return new EllipticPoint(px, py);
        }
    }

    private static BigInteger UnpackSecret(byte[] ecSec)
    {
        int length = ecSec.Length - 4;
        if (length != ecSec[3]) throw new Exception("Length does not match.");

        return new BigInteger(ecSec.AsSpan(4, length), true, true);
    }

    private EllipticPoint CreatePublic() => CreateShared(Secret, Curve.G);

    private BigInteger CreateSecret()
    {
        BigInteger result;
        var array = new byte[Curve.Size];

        do
        {
            RandomNumberGenerator.Fill(array);
            result = new BigInteger(array, false, true);
        } while (result < 1 || result >= Curve.N);

        return result;
    }

    private EllipticPoint CreateShared(BigInteger ecSec, EllipticPoint ecPub)
    {
        if (ecSec % Curve.N == 0 || ecPub.IsDefault) return default;
        if (ecSec < 0) return CreateShared(-ecSec, ecPub);

        if (!Curve.CheckOn(ecPub)) throw new Exception("Public key does not correct, it is not on the curve.");

        var pr = new JacobianPoint(0, 1, 0); // Point at infinity
        var pa = JacobianPoint.FromAffine(ecPub);
        var ps = ecSec;

        while (ps > 0)
        {
            if ((ps & 1) > 0) pr = JacobianAdd(pr, pa);
            pa = JacobianDouble(pa);
            ps >>= 1;
        }

        return JacobianToAffine(pr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JacobianPoint JacobianDouble(JacobianPoint p)
    {
        if (p.IsInfinity) return p;

        var p2 = Curve.P;
        var x = p.X;
        var y = p.Y;
        var z = p.Z;

        var yy = Mod(y * y, p2);
        var s = Mod(4 * x * yy, p2);
        var m = Mod(3 * x * x + Curve.A * z * z * z * z, p2);
        var x3 = Mod(m * m - 2 * s, p2);
        var y3 = Mod(m * (s - x3) - 8 * yy * yy, p2);
        var z3 = Mod(2 * y * z, p2);

        return new JacobianPoint(x3, y3, z3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JacobianPoint JacobianAdd(JacobianPoint p1, JacobianPoint p2)
    {
        if (p1.IsInfinity) return p2;
        if (p2.IsInfinity) return p1;

        var p = Curve.P;
        var z1z1 = Mod(p1.Z * p1.Z, p);
        var z2z2 = Mod(p2.Z * p2.Z, p);
        var u1 = Mod(p1.X * z2z2, p);
        var u2 = Mod(p2.X * z1z1, p);
        var s1 = Mod(p1.Y * p2.Z * z2z2, p);
        var s2 = Mod(p2.Y * p1.Z * z1z1, p);

        if (u1 == u2)
        {
            if (s1 == s2) return JacobianDouble(p1);
            return new JacobianPoint(0, 1, 0); // Point at infinity
        }

        var h = Mod(u2 - u1, p);
        var hh = Mod(h * h, p);
        var hhh = Mod(h * hh, p);
        var r = Mod(s2 - s1, p);
        var v = Mod(u1 * hh, p);

        var x3 = Mod(r * r - hhh - 2 * v, p);
        var y3 = Mod(r * (v - x3) - s1 * hhh, p);
        var z3 = Mod(p1.Z * p2.Z * h, p);

        return new JacobianPoint(x3, y3, z3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private EllipticPoint JacobianToAffine(JacobianPoint p)
    {
        if (p.IsInfinity) return default;

        var zInv = ModInverse(p.Z, Curve.P);
        var zInv2 = Mod(zInv * zInv, Curve.P);
        var zInv3 = Mod(zInv2 * zInv, Curve.P);

        return new EllipticPoint(Mod(p.X * zInv2, Curve.P), Mod(p.Y * zInv3, Curve.P));
    }

    // Extended Euclidean Algorithm - faster than Fermat's little theorem
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BigInteger ModInverse(BigInteger a, BigInteger p)
    {
        if (a < 0) a = ((a % p) + p) % p;

        BigInteger t = 0, newT = 1;
        BigInteger r = p, newR = a;

        while (!newR.IsZero)
        {
            var quotient = r / newR;
            (t, newT) = (newT, t - quotient * newT);
            (r, newR) = (newR, r - quotient * newR);
        }

        if (r > 1) throw new Exception("Inverse does not exist.");
        if (t < 0) t += p;

        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BigInteger Mod(BigInteger a, BigInteger b)
    {
        var result = a % b;
        if (result < 0) result += b;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] ToFixedBytes(BigInteger value, int size)
    {
        var bytes = value.ToByteArray(true, true);
        if (bytes.Length == size) return bytes;

        var result = new byte[size];
        if (bytes.Length < size)
        {
            bytes.CopyTo(result, size - bytes.Length);
        }
        else
        {
            Array.Copy(bytes, bytes.Length - size, result, 0, size);
        }
        return result;
    }
}

public readonly struct EllipticCurve
{
    public static readonly EllipticCurve Secp192K1 = new()
    {
        P = new BigInteger(new byte[]
        {
            0x37, 0xEE, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0
        }),
        A = 0,
        B = 3,
        G = new EllipticPoint
        {
            X = new BigInteger(new byte[]
            {
                0x7D, 0x6C, 0xE0, 0xEA, 0xB1, 0xD1, 0xA5, 0x1D, 0x34, 0xF4, 0xB7, 0x80,
                0x02, 0x7D, 0xB0, 0x26, 0xAE, 0xE9, 0x57, 0xC0, 0x0E, 0xF1, 0x4F, 0xDB, 0
            }),
            Y =  new BigInteger(new byte[]
            {
                0x9D, 0x2F, 0x5E, 0xD9, 0x88, 0xAA, 0x82, 0x40, 0x34, 0x86, 0xBE, 0x15,
                0xD0, 0x63, 0x41, 0x84, 0xA7, 0x28, 0x56, 0x9C, 0x6D, 0x2F, 0x2F, 0x9B, 0
            })
        },
        N = new BigInteger(new byte[]
        {
            0x8D, 0xFD, 0xDE, 0x74, 0x6A, 0x46, 0x69, 0x0F, 0x17, 0xFC, 0xF2, 0x26,
            0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0
        }),
        H = 1,
        PackSize = 24,
        Size = 24
    };

    public static readonly EllipticCurve Prime256V1 = new()
    {
        P = new BigInteger(new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0
        }),
        A = new BigInteger(new byte[]
        {
            0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0
        }),
        B = new BigInteger(new byte[]
        {
            0x4B, 0x60, 0xD2, 0x27, 0x3E, 0x3C, 0xCE, 0x3B, 0xF6, 0xB0, 0x53, 0xCC, 0xB0, 0x06, 0x1D, 0x65,
            0xBC, 0x86, 0x98, 0x76, 0x55, 0xBD, 0xEB, 0xB3, 0xE7, 0x93, 0x3A, 0xAA, 0xD8, 0x35, 0xC6, 0x5A, 0
        }),
        G = new EllipticPoint
        {
            X = new BigInteger(new byte[]
            {
                0x96, 0xC2, 0x98, 0xD8, 0x45, 0x39, 0xA1, 0xF4, 0xA0, 0x33, 0xEB, 0X2D, 0x81, 0x7D, 0x03, 0x77,
                0xF2, 0x40, 0xA4, 0x63, 0xE5, 0xE6, 0xBC, 0xF8, 0x47, 0x42, 0x2C, 0xE1, 0xF2, 0xD1, 0x17, 0x6B, 0
            }),
            Y = new BigInteger(new byte[]
            {
                0xF5, 0x51, 0xBF, 0x37, 0x68, 0x40, 0xB6, 0xCB, 0xCE, 0x5E, 0x31, 0x6B, 0x57, 0x33, 0xCE, 0x2B,
                0x16, 0x9E, 0x0F, 0x7C, 0x4A, 0xEB, 0xE7, 0x8E, 0x9B, 0x7F, 0x1A, 0xFE, 0xE2, 0x42, 0xE3, 0x4F, 0
            })
        },
        N = new BigInteger(new byte[]
        {
            0x51, 0x25, 0x63, 0xFC, 0xC2, 0xCA, 0xB9, 0xF3, 0x84, 0x9E, 0x17, 0xA7, 0xAD, 0xFA, 0xE6, 0xBC,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0
        }),
        H = 1,
        Size = 32,
        PackSize = 16
    };

    public static readonly EllipticCurve Secp224R1 = new()
    {
        P = new BigInteger(new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        }),
        A = new BigInteger(new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE
        }),
        B = new BigInteger(new byte[]
        {
            0xB4, 0x05, 0x0A, 0x85, 0x0C, 0x04, 0xB3, 0xAB, 0xF5, 0x41, 0x32, 0x56,
            0x50, 0x44, 0xB0, 0xB7, 0xD7, 0xBF, 0xD8, 0xBA, 0x27, 0x0B, 0x39, 0x43, 0x23, 0x55, 0xFF, 0xB4
        }),
        G = new EllipticPoint
        {
            X = new BigInteger(new byte[]
            {
                0xB7, 0x0E, 0x0C, 0xBD, 0x6B, 0xB4, 0xBF, 0x7F, 0x32, 0x13, 0x90, 0xB9,
                0x4A, 0x03, 0xC1, 0xD3, 0x56, 0xC2, 0x11, 0x22, 0x34, 0x32, 0x80, 0xD6, 0x11, 0x5C, 0x1D, 0x21
            }),
            Y = new BigInteger(new byte[]
            {
                0xBD, 0x37, 0x63, 0x88, 0xB5, 0xF7, 0x23, 0xFB, 0x4C, 0x22, 0xDF, 0xE6,
                0xCD, 0x43, 0x75, 0xA0, 0x5A, 0x07, 0x47, 0x64, 0x44, 0xD5, 0x81, 0x99, 0x85, 0x00, 0x7E, 0x34
            })
        },
        N = new BigInteger(new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x16, 0xA2, 0xE0,
            0xB8, 0xF0, 0x3E, 0x13, 0xDD, 0x29, 0x45, 0x5C, 0x5C, 0x2A, 0x3D, 0x16, 0x3C, 0xBF, 0x05, 0x6D
        }),
        H = 1,
        Size = 28,
        PackSize = 16
    };

    public BigInteger P { get; private init; }

    public BigInteger A { get; private init; }

    public BigInteger B { get; private init; }

    public EllipticPoint G { get; private init; }

    public BigInteger N { get; private init; }

    public BigInteger H { get; private init; }

    public int Size { get; private init; }

    public int PackSize { get; private init; }

    public bool CheckOn(EllipticPoint point) => (point.Y * point.Y - point.X * point.X * point.X - A * point.X - B) % P == 0;
}

[DebuggerDisplay("ToString(),nq")]
public readonly struct EllipticPoint(BigInteger x, BigInteger y)
{
    public BigInteger X { get; init; } = x;

    public BigInteger Y { get; init; } = y;

    public bool IsDefault => X.IsZero && Y.IsZero;

    public static EllipticPoint operator -(EllipticPoint p) => new(-p.X, -p.Y);
}
