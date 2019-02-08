using Smo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader
{
    public class PacketReadResult
    {
        public bool IsSuccess = false;
        public ReadErrorFlags Flags = new ReadErrorFlags();

        public string Message { get; set; }
        public int streamID { get; set; }
        //taken directly from ethernet packet header
        public DateTime? EthernetTime = null;
        public long? TimeDeltaTicks = null;
        public DateTime? AcraTime = null;

        //these are the actual samples
        public List<KeyValuePair<string, ValueType>> Samples = new List<KeyValuePair<string, ValueType>>();

        public IPAddress DestinationIPAddress { get; set; }
    }
}
