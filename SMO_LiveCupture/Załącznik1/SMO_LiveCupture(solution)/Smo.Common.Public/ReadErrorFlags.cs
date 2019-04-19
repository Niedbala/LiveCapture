using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common
{
    [Serializable]
    public class ReadErrorFlags
    {
        public bool EmptyPacketData;
        public bool NoIpV4Packet;
        public bool GeneralReadError;
        public bool AccessViolationError;
        public bool NoSamplesError;
        public bool NoUdpPacket;

    }
}
