using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader
{
    public class DataConverter
    {
        public uint ConvertBCD(string hexString)
        {
            uint converted = 0;

            if (UInt32.TryParse(hexString, out converted))
            {
                //Means Bad BCD       
            }

            return converted;
        }

        public uint ConvertRegisterBitsToInt(List<bool> registerBits)
        {
            int converted = 0;

            var registerBitArray = new BitArray(registerBits.ToArray());

            if (registerBitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            return ConvertBitArrayToInt(registerBitArray);

        }

        public uint ConvertBitArrayToInt(BitArray registerBitArray)
        {
            int converted = 0;

            int[] array = new int[(registerBitArray.Length + 31) / 32];

            registerBitArray.CopyTo(array, 0);

            converted = array[0];
            return (uint) converted;
        }

        public uint ConvertFromTwosComplement(byte[] parameterBytes)
        {
            var inputByteCount = parameterBytes.Length;

            //means max 32 bit
            byte[] parameterBytesCopy = new byte[inputByteCount];
            parameterBytes.CopyTo(parameterBytesCopy, 0);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(parameterBytesCopy);

            if (inputByteCount == 1)
            {
                return (uint) parameterBytesCopy[0];
            }
            else if (inputByteCount == 2)
            {
                return BitConverter.ToUInt16(parameterBytesCopy, 0);
            }
            else if (inputByteCount == 4)
            {
                return BitConverter.ToUInt32(parameterBytesCopy, 0);
            }

            throw new Exception("Data Converter: Wrong input byte number, twos complement conversion accepts only 8, 16 or 32 bits");
        }

        //iff bitoffset        
        public int ConvertFromOffsetBinary(byte[] parameterBytes, bool flipEndianness = false)
        {
            var inputByteCount = parameterBytes.Length;

#if DEBUG
            var bitString = string.Concat(parameterBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
#endif

            if (inputByteCount > 4)
            {
                throw new Exception("Conversion from offset binary: input data length exceeds 32 bits ");
            }

            //means max 32 bit
            byte[] parameterBytesCopy = new byte[inputByteCount];
            parameterBytes.CopyTo(parameterBytesCopy, 0);

            if (flipEndianness)
                Array.Reverse(parameterBytesCopy);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(parameterBytesCopy);

            parameterBytesCopy[inputByteCount - 1] ^= (1 << 7);

            if (inputByteCount == 1)
            {
                return parameterBytesCopy[0] - 256;
            }
            else if (inputByteCount == 2)
            {
                return BitConverter.ToInt16(parameterBytesCopy, 0);
            }
            else if (inputByteCount == 4)
            {
                return BitConverter.ToInt32(parameterBytesCopy, 0);
            }

            //Debug.WriteLine("Bitwise result: {0}", Convert.ToString(converted, 2));

            throw new Exception("Data Converter: Wrong input byte number, offset binary conversion accepts only 8, 16 or 32 bits");

        }


        public string ToHexString(byte[] parameterBytes)
        {
            return BitConverter.ToString(parameterBytes).Replace("-", string.Empty);
        }
    }
}
