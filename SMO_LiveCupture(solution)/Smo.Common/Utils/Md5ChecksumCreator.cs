using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Utils
{
    public class Md5ChecksumCreator
    {
        public string checkMD5(string filename)
        {

            //this was too slow! TODO: implement!
            return "hash not computed";

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
    }
}
