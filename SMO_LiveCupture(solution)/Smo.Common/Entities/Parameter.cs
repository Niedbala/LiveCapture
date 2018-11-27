using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Entities
{
    [Serializable]
    public class ParameterDefinition
    {
        public string BaseUnit;

        //TODO: to enum
        public string DataFormat;
        public int SizeInBits;
        public string Name;
        //position in packet
        public int OffsetBytes;
        public int Occurrences;
        public int StreamID;

        public override string ToString()
        {
            return Name;
        }

        public List<RegisterDefinition> Registers = new List<RegisterDefinition>();
    }
}
