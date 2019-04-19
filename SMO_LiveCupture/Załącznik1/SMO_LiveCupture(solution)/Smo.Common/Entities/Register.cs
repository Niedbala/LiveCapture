using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Entities
{
    [Serializable]
    public struct RegisterDefinition
    {
        public string ParameterName { get; set; }
        public string Name { get; set; }
        public int BitOffset { get; set; }
        public int BitSize { get; set; }
        public string DataFormat { get; set; }
        public override string ToString() { return Name; }
    }
}
