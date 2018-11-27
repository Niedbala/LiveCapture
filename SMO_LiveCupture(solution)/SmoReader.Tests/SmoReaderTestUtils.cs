using Moq;
using Smo.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Tests
{
    public class SmoReaderTestUtils
    {
        private static readonly string _solutionDir = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        public static string testDataPath = Path.GetFullPath(Path.Combine(_solutionDir, "..\\..\\test_data\\"));
        public static string instrumentSettingsXml2017Path = testDataPath + "instrumentSettings2017.xml";
        
        public static DateTime TrimMilliseconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
        }

        public static Mock<IGlobalPaths> GetMockGlobalPaths()
        {
            var mock = new Mock<IGlobalPaths>();
            mock.Setup(m => m.InstrumentSettingsXml)
                .Returns(instrumentSettingsXml2017Path);

            return mock;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
