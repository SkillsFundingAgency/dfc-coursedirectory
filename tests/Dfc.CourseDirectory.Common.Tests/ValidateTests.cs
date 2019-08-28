using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Dfc.CourseDirectory.Common.Tests
{
    public class ValidateTests
    {
        [Fact]
        public void IsBase64Encoded_Base64Text_True()
        {
            byte[] byteArray = Encoding.ASCII.GetBytes("TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFuY2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxlIGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhlbWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=");
            MemoryStream stream = new MemoryStream(byteArray);
            Assert.True(Validate.IsBase64EncodedStream(stream));
        }

        [Fact]
        public void IsBase64Encoded_NotBase64Text_False()
        {
            byte[] byteArray = Encoding.ASCII.GetBytes("I am not base 64");
            MemoryStream stream = new MemoryStream(byteArray);
            Assert.False(Validate.IsBase64EncodedStream(stream));
        }

        [Fact]
        public void IsBinary_SimpeText_False()
        {
            byte[] byteArray = Encoding.ASCII.GetBytes("I am not binary");
            MemoryStream stream = new MemoryStream(byteArray);
            Assert.False(Validate.isBinaryStream(stream));
        }

        [Fact]
        public void IsBinary_BinaryText_True()
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(@"MZ    ");
           // byte[] byteArray = new byte[] {0x21, 0x60, 0x1F, 0xA1, 0x00 };
            MemoryStream stream = new MemoryStream(byteArray);
            Assert.True(Validate.isBinaryStream(stream));
        }

         static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }
    }
}
