using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader
{
    public class ParameterData
    {
        private DataConverter _dataConverter = new DataConverter();

        public ParameterData(byte[] byteData)
        {
            var parameterData_BitArray = new BitArray(byteData.Reverse().ToArray());
            Bits = new bool[parameterData_BitArray.Length];
            parameterData_BitArray.CopyTo(Bits, 0);
            Bytes = byteData;
            Hex = _dataConverter.ToHexString(byteData);

        }

        public byte[] Bytes { get; private set; }
        public string Hex { get; private set; }
        public bool[] Bits { get; private set; }
    }
}
