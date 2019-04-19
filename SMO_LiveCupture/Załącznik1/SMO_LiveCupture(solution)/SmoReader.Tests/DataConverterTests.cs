using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmoReader.Tests
{
    public class DataConverterTests
    {
        [Fact]
        public void ConvertFromOffsetBinary_input1000_0000_0000_0000_output0()
        {

            var input = new byte[] { Convert.ToByte("10000000", 2), Convert.ToByte("00000000", 2) };
            var converter = new DataConverter();

            var output = converter.ConvertFromOffsetBinary(input);
           
            Assert.Equal(0,output);
        }

        [Fact]
        public void ConvertFromOffsetBinary_input1000_0000_0000_0001_output1()
        {

            var input = new byte[] { Convert.ToByte("10000000", 2), Convert.ToByte("00000001", 2) };
            var converter = new DataConverter();

            var output = converter.ConvertFromOffsetBinary(input);

            Assert.Equal(1, output);
        }

        [Fact]
        public void ConvertFromOffsetBinary_input0111_1111_1111_1111_outputMinus1()
        {

            var input = new byte[] { Convert.ToByte("01111111", 2), Convert.ToByte("11111111", 2) };
            var converter = new DataConverter();

            var output = converter.ConvertFromOffsetBinary(input);

            Assert.Equal(-1, output);
        }

        [Fact]
        public void ConvertFromOffsetBinary_input0111_1111_1111_1111_outputMinus2()
        {

            var input = new byte[] { Convert.ToByte("01111111", 2), Convert.ToByte("11111110", 2) };
            var converter = new DataConverter();

            var output = converter.ConvertFromOffsetBinary(input);

            Assert.Equal(-2, output);
        }

        [Fact]
        public void ConvertFromOffsetBinary_input0111_1111_outputMinus1()
        {

            var input = new byte[] { Convert.ToByte("01111111", 2) };
            var converter = new DataConverter();

            var output = converter.ConvertFromOffsetBinary(input);

            Assert.Equal(-1, output);
        }

    }
}
