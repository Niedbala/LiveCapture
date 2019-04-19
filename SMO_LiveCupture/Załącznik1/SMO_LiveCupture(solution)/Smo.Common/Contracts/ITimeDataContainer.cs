using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Contracts
{
    public interface ITimeData
    {
        DateTime[] AcraTime { get; set; }
        DateTime[] EthernetTime { get; set; }
        DateTime? AcraStartTime { get; set; }
        DateTime? AcraEndTime { get; set; }

        DateTime? EthernetStartTime { get; set; }
        DateTime? EthernetEndTime { get; set; }
    }
}
