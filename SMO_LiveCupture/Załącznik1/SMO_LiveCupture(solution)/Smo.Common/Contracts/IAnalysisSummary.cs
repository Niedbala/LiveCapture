using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Entities
{
    public interface IAnalysisMetadata
    {
        double SatelliteErrorPercentage { get; set; }
        double GpsLockErrorPercentage { get; set; }
        double VelocityErrorPercentage { get; set; }
        double FixErrorPercentage { get; set; }
        double AltitudeErrorPercentage { get; set; }

        DateTime StartTs { get; set; }
        DateTime EndTs { get; set; }

        TimeSpan TimeSpan { get; set; }
        double TimeErrorPercentage { get; set; }

        string DirectoryName { get; set; }
        Smo.Contracts.Enums.DataClassification Classification { get; set; }
        string AircraftName { get; set; }
        
        int FileCount { get; set; }

        List<Tuple<string, double>> MaxValues { get; set; }
        List<Tuple<string, double>> MinValues { get; set; }
        DateTime TakeOffTs { get; set; }
        DateTime LandingTs { get; set; }
        int PowerUpCount { get; set; }



    }

}
