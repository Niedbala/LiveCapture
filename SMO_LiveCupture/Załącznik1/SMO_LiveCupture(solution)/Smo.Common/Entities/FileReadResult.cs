using Smo.Common;
using Smo.Common.Contracts;
using Smo.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static SmoReader.Utils.TimeFormatter;

namespace SmoReader.Entities
{
    //used for initial file scanning

    [Serializable]
    public class FileReadResult : DataContainerBase, IHasMetadata<FileReadMetadata>
    {
        public FileReadMetadata Metadata { get; set; }

        public FileReadResult(string filePath)
        {
            Metadata = new FileReadMetadata(filePath);
        }

        public FileReadResult()
        {
            Metadata = null;
        }


        //public FileReadMetadata GetFileReadSummary()
        //{
        //    return _fileReadSummary;
        //}
        //public void SetFileReadSummary(FileReadMetadata summary)
        //{
        //    _fileReadSummary = summary;
        //}



            //public List<ParameterDefinition> Parameters;
            //public string SourceMacAddress { get; set; }
            //public string AircraftName { get; set; }
            //public int PowerUpCount { get; set; }

        private FileReadMetadata _fileReadSummary = null;
    }
}