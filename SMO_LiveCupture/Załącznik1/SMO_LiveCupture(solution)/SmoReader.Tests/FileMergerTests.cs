using Moq;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Repositories;
using SmoReader.Entities;
using SmoReader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmoReader.Tests
{
    public class FileMergerTests : IDisposable
    {

        private readonly string _testDataPath = SmoReaderTestUtils.testDataPath;
        private string _outputDir;

        public FileMergerTests()
        {
            _outputDir = System.IO.Directory.CreateDirectory("FileMergerTestOutput").FullName;
        }


        [Fact]
        //integration
        public void MergeFiles_TwoSmallFiles_52SignalsHave97Samples()
        {

            //arrange
            var filePaths = new List<string>() { _testDataPath + "File_Merger\\part1_.cap",
                _testDataPath + "File_Merger\\part2_.cap"
            };
            var scannedFiles = filePaths.Select(p => new FileScanResult(p)
            {
                AircraftName = "305",
                PowerUpCount = 1054,
                StartDate = new DateTime(2017, 10, 12, 0, 0, 0)

            }).ToList();
            var mockAircraftDataProvider = GetMock305AircraftDataProvider().Object;
            var mockGlobalPaths = GetMock305GlobalPaths().Object;


            //act
            var merger = new FileMerger(
                mockAircraftDataProvider,
                mockGlobalPaths);
            //TODO: perhaps uncouple (mock)
            var fileGroupReader = new FileGroupReader(mockAircraftDataProvider, mockGlobalPaths);
            var groupReadResult = fileGroupReader.ReadFiles(scannedFiles);

            var result = merger.MergeFiles(groupReadResult);


            //assert
            var signalsWithWrongSampleCount = result.Signals.Where(s => s.Samples.Count() != 97);

            Xunit.Assert.Empty(signalsWithWrongSampleCount);
            Assert.Equal(52, result.Signals.Count());
        }
        public void Dispose()
        {
            // Do "global" teardown here; Called after every test method.
            Directory.Delete(_outputDir);
        }

        private Mock<IAircraftDataProvider> GetMock305AircraftDataProvider()
        {
            var mockAircraftDataProvider = new Mock<IAircraftDataProvider>();
            var configXmlContent = File.ReadAllText(_testDataPath + "smoConfig2.xml");

            mockAircraftDataProvider.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetAircraftNameByMacAddress(It.IsAny<string>()))
                .Returns("305");

            return mockAircraftDataProvider;
        }

        private Mock<IGlobalPaths> GetMock305GlobalPaths()
        {
            var mockGlobalPaths = new Mock<IGlobalPaths>();
            mockGlobalPaths.Setup(m => m.InstrumentSettingsXml)
                .Returns(SmoReaderTestUtils.instrumentSettingsXml2017Path);

            return mockGlobalPaths;
        }
    }
}
