using Smo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Entities
{
    public class FileScanResult : PreliminaryReadResult
    {
        public FileScanResult(string filePath)
        {
            FilePath = filePath;
            CheckSum = "";
              //  new Md5ChecksumCreator().checkMD5(filePath);
        }

        public string FilePath { get; private set; }

        public string CheckSum { get; }

        //TODO this shouldnt be here. this should be filtered later
        public bool isDateBelowCutoff { get; set; }

    }
}
