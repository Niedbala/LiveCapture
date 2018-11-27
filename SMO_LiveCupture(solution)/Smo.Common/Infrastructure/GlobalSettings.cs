using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Infrastructure
{
    public static class GlobalSettings
    {
        public struct Names
        {
            //altitude Lo and Hi summed
            public const string Altitude = "AltitudeTotal";

            public const string AltitudeLo = "AltitudeLo";
            public const string Altitude10KsMeters = "Altitude - Tens of thousands of meters";
            public const string AltitudeIsNegative = "AltitudeIsNegative";

            public const string Velocity = "VelocityInKph";
            public const string ToFewSatellites = "ToFewSatellites";
            public const string GpsLock = "GPSLock";
            public const string FixError = "FixError";

            public static string StrainGauge(int number)
            {
                return $"AnalogIn({number.ToString()})";
            }
        }


    }
}
