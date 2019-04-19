using Smo.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Linq;
using Smo.Common.Infrastructure;
using Smo.Common.Entities;
using Smo.Common.Contracts;
using Smo.Common;

namespace SmoReader.Entities
{
    public class AnalysisResult : DataContainerBase, ITimeData, IHasMetadata<RecordMetaData>, IFileGroupBased
    {

        public bool isValidForAnalysis { get; set; }

        //scalars - general:
        public DateTime StartTs
        {
            get { return AcraStartTime.Value; }
            set { return; }
        }
        public DateTime EndTs
        {
            get { return AcraEndTime.Value; }
            set { return; }
        }
        public TimeSpan TimeSpan { get; set; }

        public int FileCount { get { return FileSummaries.Count; } set { return; } }

        //scalars - data correctness benchmarks
        public double SatelliteErrorPercentage { get; set; }
        public double GpsLockErrorPercentage { get; set; }
        public double VelocityErrorPercentage { get; set; }
        public double FixErrorPercentage { get; set; }
        public double AltitudeErrorPercentage { get; set; }
        public double TimeErrorPercentage { get; set; }


        //named signals
        public double[] Altitude => this.GetSamples(GlobalSettings.Names.Altitude);
        public double[] Velocity => this.GetSamples(GlobalSettings.Names.Velocity);
        public double[] ToFewSatellites => GetSamples(GlobalSettings.Names.ToFewSatellites);
        public double[] GpsLock => GetSamples(GlobalSettings.Names.GpsLock);
        public double[] FixError => GetSamples(GlobalSettings.Names.FixError);







        public List<Tuple<string, double>> MaxValues { get; set; }
        public List<Tuple<string, double>> MinValues { get; set; }

        public RecordMetaData Metadata { get; set; }
        public List<FileReadMetadata> FileSummaries
        { get; set; }

        //helpers
        private T[] CopyArray<T>(T[] original)
        {
            T[] copy = new T[original.Length];
            original.CopyTo(copy, 0);

            return copy;
        }
        private List<T> CopyList<T>(List<T> original)
        {
            List<T> copy = new List<T>();

            if (original != null)
                copy.AddRange(original);

            return copy;
        }

    }
}
