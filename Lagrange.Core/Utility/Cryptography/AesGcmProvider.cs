using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

namespace Lagrange.Core.Utility.Cryptography;

internal static class AesGcmProvider
{
    public static byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var aes = new AesGcmImpl(key);

        Span<byte> iv = stackalloc byte[12];
        RandomNumberGenerator.Fill(iv);

        Span<byte> tag = stackalloc byte[16];
        byte[] result = new byte[12 + data.Length + 16];
        aes.Encrypt(iv, data, ReadOnlySpan<byte>.Empty, result.AsSpan()[12..], tag);

        iv.CopyTo(result);
        tag.CopyTo(result.AsSpan()[^16..]);

        return result;
    }

    public static byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var aes = new AesGcmImpl(key);

        var iv = data[..12];
        var cipher = data[12..^16];
        var tag = data[^16..];
        byte[] result = new byte[data.Length - 28];
        aes.Decrypt(iv, cipher, ReadOnlySpan<byte>.Empty, tag, result);

        return result;
    }
}

internal readonly ref struct AesGcmImpl
{
    #region Constants
    
    private static readonly byte[] SBox =
    [
        0x63,0x7c,0x77,0x7b,0xf2,0x6b,0x6f,0xc5,0x30,0x01,0x67,0x2b,0xfe,0xd7,0xab,0x76,
        0xca,0x82,0xc9,0x7d,0xfa,0x59,0x47,0xf0,0xad,0xd4,0xa2,0xaf,0x9c,0xa4,0x72,0xc0,
        0xb7,0xfd,0x93,0x26,0x36,0x3f,0xf7,0xcc,0x34,0xa5,0xe5,0xf1,0x71,0xd8,0x31,0x15,
        0x04,0xc7,0x23,0xc3,0x18,0x96,0x05,0x9a,0x07,0x12,0x80,0xe2,0xeb,0x27,0xb2,0x75,
        0x09,0x83,0x2c,0x1a,0x1b,0x6e,0x5a,0xa0,0x52,0x3b,0xd6,0xb3,0x29,0xe3,0x2f,0x84,
        0x53,0xd1,0x00,0xed,0x20,0xfc,0xb1,0x5b,0x6a,0xcb,0xbe,0x39,0x4a,0x4c,0x58,0xcf,
        0xd0,0xef,0xaa,0xfb,0x43,0x4d,0x33,0x85,0x45,0xf9,0x02,0x7f,0x50,0x3c,0x9f,0xa8,
        0x51,0xa3,0x40,0x8f,0x92,0x9d,0x38,0xf5,0xbc,0xb6,0xda,0x21,0x10,0xff,0xf3,0xd2,
        0xcd,0x0c,0x13,0xec,0x5f,0x97,0x44,0x17,0xc4,0xa7,0x7e,0x3d,0x64,0x5d,0x19,0x73,
        0x60,0x81,0x4f,0xdc,0x22,0x2a,0x90,0x88,0x46,0xee,0xb8,0x14,0xde,0x5e,0x0b,0xdb,
        0xe0,0x32,0x3a,0x0a,0x49,0x06,0x24,0x5c,0xc2,0xd3,0xac,0x62,0x91,0x95,0xe4,0x79,
        0xe7,0xc8,0x37,0x6d,0x8d,0xd5,0x4e,0xa9,0x6c,0x56,0xf4,0xea,0x65,0x7a,0xae,0x08,
        0xba,0x78,0x25,0x2e,0x1c,0xa6,0xb4,0xc6,0xe8,0xdd,0x74,0x1f,0x4b,0xbd,0x8b,0x8a,
        0x70,0x3e,0xb5,0x66,0x48,0x03,0xf6,0x0e,0x61,0x35,0x57,0xb9,0x86,0xc1,0x1d,0x9e,
        0xe1,0xf8,0x98,0x11,0x69,0xd9,0x8e,0x94,0x9b,0x1e,0x87,0xe9,0xce,0x55,0x28,0xdf,
        0x8c,0xa1,0x89,0x0d,0xbf,0xe6,0x42,0x68,0x41,0x99,0x2d,0x0f,0xb0,0x54,0xbb,0x16
    ];

    private static readonly byte[] Rcon =
    [
        0x00, // Rcon[0] is not used
        0x01, 0x02, 0x04, 0x08,
        0x10, 0x20, 0x40, 0x80,
        0x1B, 0x36, 0x6C, 0xD8,
        0xAB, 0x4D, 0x9A, 0x2F,
        0x5E, 0xBC, 0x63, 0xC6,
        0x97, 0x35, 0x6A, 0xD4,
        0xB3, 0x7D, 0xFA, 0xEF,
        0xC5, 0x91, 0x39, 0x72,
        0xE4, 0xD3, 0xBD, 0x61,
        0xC2, 0x9F, 0x25, 0x4A,
        0x94, 0x33, 0x66, 0xCC,
        0x83, 0x1D, 0x3A, 0x74,
        0xE8, 0xCB, 0x8D, 0x01,
        0x02, 0x04, 0x08, 0x10,
        0x20, 0x40, 0x80, 0x1B,
        0x36, 0x6C, 0xD8, 0xAB
    ];

    #endregion
    
    private readonly int Nk;
    private readonly int Nr;
    private readonly int NbWords;
    private readonly Span<byte> _roundKey;
    private readonly bool _useHwAes;

    public const int BlockSize = 16;
    public const int TagSize = 16;

    public AesGcmImpl(ReadOnlySpan<byte> key, bool useHwAes = true)
    {
        if (key.Length is not 16 and not 24 and not 32) throw new ArgumentException("Key length must be 128, 192, or 256 bits.");

        const int Nb = 4;
        Nk = key.Length / 4;
        Nr = Nk + 6;
        NbWords = Nb * (Nr + 1);

        Span<byte> w = new byte[NbWords * 4];
        key.CopyTo(w);
        KeyExpansion(w);
        _roundKey = w;

        _useHwAes = (System.Runtime.Intrinsics.X86.Aes.IsSupported || System.Runtime.Intrinsics.Arm.Aes.IsSupported) && useHwAes;
    }
    
    private void TransformBlock(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (_useHwAes)
        {
            TransformBlockHardware(input, output);
        }
        else
        {
            TransformBlockSoftware(input, output);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void TransformBlockHardware(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length != BlockSize || output.Length != BlockSize) throw new ArgumentException("Block size must be 16 bytes.");

        fixed (byte* pInput = input)
        fixed (byte* pOutput = output)
        fixed (byte* pRoundKey = _roundKey)
        {
            if (System.Runtime.Intrinsics.X86.Aes.IsSupported)
            {
                var block = Sse2.LoadVector128(pInput);
                var state = Sse2.LoadVector128(pRoundKey);

                state = Sse2.Xor(block, state);

                for (int round = 1; round < Nr; round++)
                {
                    var roundKey = Sse2.LoadVector128(pRoundKey + round * BlockSize);
                    state = System.Runtime.Intrinsics.X86.Aes.Encrypt(state, roundKey);
                }

                var lastRoundKey = Sse2.LoadVector128(pRoundKey + Nr * BlockSize);
                state = System.Runtime.Intrinsics.X86.Aes.EncryptLast(state, lastRoundKey);

                Sse2.Store(pOutput, state);
            }
            else if (System.Runtime.Intrinsics.Arm.Aes.IsSupported)
            {
                var state = AdvSimd.LoadVector128(pInput);

                for (int round = 0; round < Nr - 1; round++)
                {
                    var roundKey = AdvSimd.LoadVector128(pRoundKey + round * BlockSize);
                    state = System.Runtime.Intrinsics.Arm.Aes.Encrypt(state, roundKey);
                    state = System.Runtime.Intrinsics.Arm.Aes.MixColumns(state);
                }

                var penultimateKey = AdvSimd.LoadVector128(pRoundKey + (Nr - 1) * BlockSize);
                state = System.Runtime.Intrinsics.Arm.Aes.Encrypt(state, penultimateKey);

                var lastKey = AdvSimd.LoadVector128(pRoundKey + Nr * BlockSize);
                state = AdvSimd.Xor(state, lastKey);

                AdvSimd.Store(pOutput, state);
            }
            else
            {
                TransformBlockSoftware(input, output);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TransformBlockSoftware(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length != 16) throw new ArgumentException("Input block size must be 16 bytes.");
        if (output.Length != 16) throw new ArgumentException("Output block size must be 16 bytes.");

        Span<byte> state = stackalloc byte[16];
        input.CopyTo(state);

        AddRoundKey(state, _roundKey[..16]);

        for (int round = 1; round < Nr; round++)
        {
            SubBytes(state);
            ShiftRows(state);
            MixColumns(state);
            AddRoundKey(state, _roundKey.Slice(round * 16, 16));
        }

        SubBytes(state);
        ShiftRows(state);
        AddRoundKey(state, _roundKey.Slice(Nr * 16, 16));

        state.CopyTo(output);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SubBytes(Span<byte> state)
    {
        for (int i = 0; i < state.Length; i++) state[i] = SBox[state[i]];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddRoundKey(Span<byte> state, ReadOnlySpan<byte> roundKey)
    {
        for (int i = 0; i < state.Length; i++) state[i] ^= roundKey[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RotWord(Span<byte> word)
    {
        byte t = word[0];
        word[0] = word[1];
        word[1] = word[2];
        word[2] = word[3];
        word[3] = t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SubWord(Span<byte> word)
    {
        for (int i = 0; i < 4; i++) word[i] = SBox[word[i]];
    }
    
    private void KeyExpansion(scoped Span<byte> destRoundKey)
    {
        Span<byte> temp = stackalloc byte[4];

        for (int i = Nk; i < NbWords; i++)
        {
            for (int j = 0; j < 4; j++) temp[j] = destRoundKey[(i - 1) * 4 + j];

            if (i % Nk == 0)
            {
                RotWord(temp);
                SubWord(temp);
                temp[0] ^= Rcon[i / Nk];
            }
            else if (Nk > 6 && i % Nk == 4)
            {
                SubWord(temp);
            }

            for (int j = 0; j < 4; j++) destRoundKey[i * 4 + j] = (byte)(destRoundKey[(i - Nk) * 4 + j] ^ temp[j]);
        }
    }
    
    private static byte GFMul(byte a, byte b)
    {
        byte p = 0;
        for (int i = 0; i < 8; i++)
        {
            if ((b & 1) != 0) p ^= a;
            byte hiBitSet = (byte)(a & 0x80);
            a <<= 1;
            if (hiBitSet != 0) a ^= 0x1b; // irreducible polynomial
            b >>= 1;
        }
        return p;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ShiftRows(Span<byte> state)
    {
        byte tmp = state[1];
        state[1] = state[5];
        state[5] = state[9];
        state[9] = state[13];
        state[13] = tmp;

        tmp = state[2];
        byte tmp2 = state[6];
        state[2] = state[10];
        state[6] = state[14];
        state[10] = tmp;
        state[14] = tmp2;

        tmp = state[3];
        state[3] = state[15];
        state[15] = state[11];
        state[11] = state[7];
        state[7] = tmp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MixColumns(Span<byte> state)
    {
        for (int c = 0; c < 4; c++)
        {
            int i = c * 4;
            byte a0 = state[i + 0];
            byte a1 = state[i + 1];
            byte a2 = state[i + 2];
            byte a3 = state[i + 3];

            byte r0 = (byte)(GFMul(0x02, a0) ^ GFMul(0x03, a1) ^ a2 ^ a3);
            byte r1 = (byte)(a0 ^ GFMul(0x02, a1) ^ GFMul(0x03, a2) ^ a3);
            byte r2 = (byte)(a0 ^ a1 ^ GFMul(0x02, a2) ^ GFMul(0x03, a3));
            byte r3 = (byte)(GFMul(0x03, a0) ^ a1 ^ a2 ^ GFMul(0x02, a3));

            state[i + 0] = r0;
            state[i + 1] = r1;
            state[i + 2] = r2;
            state[i + 3] = r3;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void IncrementCounter(Span<byte> counter)
    {
        for (int i = counter.Length - 1; i >= counter.Length - 4; i--)
        {
            if (++counter[i] != 0) break;
        }
    }

    private static void GHASH(ReadOnlySpan<byte> H, ReadOnlySpan<byte> data, Span<byte> tag)
    {
        tag.Clear();
        Span<byte> tmp = stackalloc byte[BlockSize];

        for (int offset = 0; offset < data.Length; offset += BlockSize)
        {
            tmp.Clear();
            var block = data.Slice(offset, Math.Min(BlockSize, data.Length - offset));
            block.CopyTo(tmp);
            Xor(tag, tmp, tag);
            Gfmul(tag, H, tag);
        }
    }

    private static void Gfmul(ReadOnlySpan<byte> X, ReadOnlySpan<byte> Y, Span<byte> result)
    {
        Span<byte> Z = stackalloc byte[BlockSize];
        Span<byte> V = stackalloc byte[BlockSize];
        X.CopyTo(V);
        Z.Clear();

        for (int i = 0; i < BlockSize; ++i)
        {
            byte y = Y[i];
            for (int bit = 7; bit >= 0; --bit)
            {
                if ((y & (1 << bit)) != 0) Xor(Z, V, Z);
                
                bool lsb = (V[15] & 1) != 0;
                ShiftRight(V);
                if (lsb) V[0] ^= 0xE1; // reduction polynomial
            }
        }
        Z.CopyTo(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ShiftRight(Span<byte> bytes)
    {
        for (int i = bytes.Length - 1; i > 0; i--) bytes[i] = (byte)((bytes[i] >> 1) | ((bytes[i - 1] & 1) << 7));
        bytes[0] >>= 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Xor(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, Span<byte> output)
    {
        for (int i = 0; i < BlockSize; i++) output[i] = (byte)(a[i] ^ b[i]);
    }
    
    private void InitializeHAndCounter(ReadOnlySpan<byte> nonce, Span<byte> H, Span<byte> counter)
    {
        H.Clear();
        TransformBlock(H, H); // H = AES_k(0)
    
        counter.Clear();
        if (nonce.Length == 12)
        {
            nonce.CopyTo(counter);
            BinaryPrimitives.WriteUInt32BigEndian(counter[12..], 1);
        }
        else
        {
            GHASH(H, nonce, counter);
            Span<byte> nonceLenBlock = stackalloc byte[BlockSize];
            nonceLenBlock.Clear();
            BinaryPrimitives.WriteUInt64BigEndian(nonceLenBlock[8..], (ulong)(nonce.Length * 8));
            Xor(counter, nonceLenBlock, counter);
            Gfmul(counter, H, counter);
        }
    }
    
    private void ComputeAuthTag(ReadOnlySpan<byte> associatedData, ReadOnlySpan<byte> ciphertext, Span<byte> H, Span<byte> encCounter, Span<byte> tag)
    {
        int aLen = ((associatedData.Length + 15) / 16) * 16;
        int cLen = ((ciphertext.Length + 15) / 16) * 16;
        int totalLen = aLen + cLen + 16;
        var authData = new byte[totalLen];

        associatedData.CopyTo(authData.AsSpan(0, associatedData.Length));
        ciphertext.CopyTo(authData.AsSpan(aLen, ciphertext.Length));
        BinaryPrimitives.WriteUInt64BigEndian(authData.AsSpan(totalLen - 16), (ulong)(associatedData.Length * 8));
        BinaryPrimitives.WriteUInt64BigEndian(authData.AsSpan(totalLen - 8), (ulong)(ciphertext.Length * 8));

        Span<byte> computedTag = stackalloc byte[BlockSize];
        GHASH(H, authData, computedTag);
        Xor(computedTag, encCounter, tag);
    }
    
    private void CryptCTR(ReadOnlySpan<byte> input, Span<byte> output, Span<byte> counter)
    {
        Span<byte> buffer = stackalloc byte[BlockSize];
        int offset = 0;

        while (offset < input.Length)
        {
            IncrementCounter(counter);
            TransformBlock(counter, buffer);
            int blockSize = Math.Min(BlockSize, input.Length - offset);
            for (int i = 0; i < blockSize; i++) output[offset + i] = (byte)(input[offset + i] ^ buffer[i]);
            offset += blockSize;
        }
    }
    
    public void Encrypt(
        ReadOnlySpan<byte> nonce, 
        ReadOnlySpan<byte> plaintext, 
        ReadOnlySpan<byte> associatedData, 
        Span<byte> ciphertext, 
        Span<byte> tag)
    {
        Span<byte> H = stackalloc byte[BlockSize];
        Span<byte> counter = stackalloc byte[BlockSize];
        InitializeHAndCounter(nonce, H, counter);

        Span<byte> encCounter = stackalloc byte[BlockSize];
        TransformBlock(counter, encCounter);

        CryptCTR(plaintext, ciphertext, counter);

        ComputeAuthTag(associatedData, ciphertext[..plaintext.Length], H, encCounter, tag);
    }

    public void Decrypt(
        ReadOnlySpan<byte> nonce, 
        ReadOnlySpan<byte> ciphertext, 
        ReadOnlySpan<byte> associatedData, 
        ReadOnlySpan<byte> tag, 
        Span<byte> plaintext)
    {
        Span<byte> H = stackalloc byte[BlockSize];
        Span<byte> counter = stackalloc byte[BlockSize];
        InitializeHAndCounter(nonce, H, counter);

        Span<byte> encCounter = stackalloc byte[BlockSize];
        TransformBlock(counter, encCounter);

        Span<byte> computedTag = stackalloc byte[BlockSize];
        ComputeAuthTag(associatedData, ciphertext, H, encCounter, computedTag);
        if (!CryptographicOperations.FixedTimeEquals(computedTag, tag)) throw new CryptographicException("Authentication tag mismatch - ciphertext invalid or corrupted.");

        CryptCTR(ciphertext, plaintext, counter);
    }
}