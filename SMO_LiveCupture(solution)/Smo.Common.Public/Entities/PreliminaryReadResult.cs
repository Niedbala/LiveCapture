using Smo.Common;
using System;

namespace SmoReader.Entities
{
    public class PreliminaryReadResult
    {
        public  FileReadMetadata ReadMetadata { get; set; }

        public DateTime? StartDate
        {
            get
            {
                if (ReadMetadata.AcraStartTs.HasValue)
                    return ReadMetadata.AcraStartTs.Value;
                else if (ReadMetadata.EthernetStartTs.HasValue)
                    return ReadMetadata.EthernetStartTs.Value;
                else if (ReadMetadata.FolderBasedStartTs.HasValue)
                    return ReadMetadata.FolderBasedStartTs.Value;

                return null;
            }
        }

        public bool IsReadSuccessful { get; set; }
    }
}