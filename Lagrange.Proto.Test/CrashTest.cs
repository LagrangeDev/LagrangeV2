using NUnit.Framework;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test
{
    [TestFixture]
    public class CrashTest
    {
        [Test]
        public void TestNegativeInt32VarIntLength()
        {
            // This test is used to reproduce the IndexOutOfRangeException Bug
            int value = -1;
            
            int length = ProtoHelper.GetVarIntLength(value);
            
            // Verify: For a 32-bit all-ones number (0xFFFF), VarInt encoding should be 5 bytes
            Assert.That(length, Is.EqualTo(5));
        }

        [Test]
        public void TestNegativeInt64VarIntLength()
        {
            long value = -1;
            
            // For 64-bit numbers with all 1s, VarInt encoding should be 10 bytes
            int length = ProtoHelper.GetVarIntLength(value);
            
            Assert.That(length, Is.EqualTo(10));
        }
        
        [Test]
        public void TestOtherNegativeValues()
        {
             // Test other negative values to ensure stability
             int val1 = -100;
             int len1 = ProtoHelper.GetVarIntLength(val1);
             Assert.That(len1, Is.GreaterThan(0));

             long val2 = long.MinValue; // 0x8000000000000000
             int len2 = ProtoHelper.GetVarIntLength(val2);
             Assert.That(len2, Is.EqualTo(10));
        }
    }
}