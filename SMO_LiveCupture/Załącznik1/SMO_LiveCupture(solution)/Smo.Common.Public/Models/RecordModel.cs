using System;
using System.Collections.Generic;

namespace Smo.Common.Public.Models
{
    public class RecordModel
    {
        public string TsvPath { get; set; }

        public List<FileReadMetadata> InputFiles { get; set; }

        public string AircraftName { get; set; }

        public RecordMetaData MetaData { get; set; }

        public List<Tuple<string, double>> MaxValues { get; set; }
        public List<Tuple<string, double>> MinValues { get; set; }



    }
}
