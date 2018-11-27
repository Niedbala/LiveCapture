using Smo.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Smo.Common
{
    [Serializable]
    public class FileReadMetadata : RecordMetaData
    {
        private DateTime? GetTsFromFolderName()
        {
            DateTime? retVal = null;

            var pth = Path.GetDirectoryName(FilePath);
            var pathElements = pth.Split(Path.DirectorySeparatorChar).ToList();

            var matchedDateString = "";

            pathElements.ForEach(pe =>
            {
                var matches = Regex.Match(pe, @"\d{8}");
                var match = matches?.Groups[matches.Groups.Count - 1];



                var matchedString = match.ToString();
                if (!string.IsNullOrWhiteSpace(matchedString))
                    matchedDateString = matchedString;
            });

            if (!string.IsNullOrWhiteSpace(matchedDateString))
            {
                var day = Convert.ToInt32(String.Join("", matchedDateString.Take(2)));
                var month = Convert.ToInt32(String.Join("", matchedDateString.Skip(2).Take(2)));
                var year = Convert.ToInt32(String.Join("", matchedDateString.Skip(4).Take(4)));
                
                try
                {
                    retVal = new DateTime(year, month, day);
                }
                catch
                {
                    retVal = null;
                }

            }

            

            return retVal;
        }

        //try to extract date from folder Name
        public DateTime? FolderBasedStartTs { get; set; }
        //  public string FilePath { get; set; }
        public string FilePath { get; protected set; }
        public DateTime FileTs { get; set; }
        public double FileSizeMb { get; set; }
        public ReadErrorFlags Flags { get; set; }
        public bool isPreliminaryRead { get; set; }
        public string CheckSum { get; set; }
        public bool IsReadSuccessful { get; set; }
        public FileReadMetadata(string path)
        {
            FilePath = path;

            FolderBasedStartTs = GetTsFromFolderName();

            var fileInfo = new FileInfo(path);

            CheckSum = "";
              //  new Md5ChecksumCreator().checkMD5(path);

            if (fileInfo.LastAccessTimeUtc > fileInfo.LastWriteTimeUtc)
                FileTs = fileInfo.LastWriteTime;
            else
                FileTs = fileInfo.LastAccessTime;

            FileSizeMb = fileInfo.Length / (1024.0 * 1024.0);

            Classification = DataClassification.InvalidOrUnknown;
        }

    }

    [Serializable]
    //common metadata
    public class RecordMetaData
    {
        public DateTime? EthernetStartTs { get; set; }
        public DateTime? EthernetEndTs { get; set; }

        public DateTime? AcraStartTs { get; set; }
        public DateTime? AcraEndTs { get; set; }
        public DataClassification Classification { get; set; }
        public string AircraftName { get; set; }
        public int? PowerUpCount { get; set; }
        public string SourceMacAddress { get; set; }
    }

}
