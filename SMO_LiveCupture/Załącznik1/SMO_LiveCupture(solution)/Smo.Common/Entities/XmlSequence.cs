using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Entities
{
    public class TimeStampedXml
    {
        public int XmlId;
        public string path;
        public DateTime StartDate;

    }

    public class AircraftXmls
    {
        public string Name = "";
        public List<TimeStampedXml> Xmls;
    }

    public class XmlSequence
    {
        public List<AircraftXmls> Aircrafts = new List<AircraftXmls>();

    }
}
