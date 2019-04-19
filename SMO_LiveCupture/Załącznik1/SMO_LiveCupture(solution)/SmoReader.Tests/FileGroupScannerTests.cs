using Moq;
using Smo.Common.Public.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmoReader.Tests
{

    public class FileGroupScannerTests
    {
        private string _testDataPath = SmoReaderTestUtils.testDataPath;
        string _509_valid_file1_4packets_with_dupesPath;
        string _305_valid_file1_200packets_with_dupesPath;


        public FileGroupScannerTests()
        {
            _509_valid_file1_4packets_with_dupesPath = _testDataPath + "509_valid_file1\\4good_packets_with_dupes.cap";
            _305_valid_file1_200packets_with_dupesPath = _testDataPath + "lot_weryfikacyjny_305_12_OCT_2017\\200_packets_with_dupes_305.cap";
        }


        //the date is taken from the folder signature
        [Fact]
        public void Scan_WrongCaps_2FilesInResultANDOneHasDate()
        {
            //arrange
            var filePaths = Directory.GetFiles(_testDataPath + "\\WrongCapsForScanner", "*.cap", SearchOption.AllDirectories).ToList();
            var aircraftDataProviderMock = GetMock305AND509AircraftDataProvider();


            //act
            var scanner = new FileGroupScanner(aircraftDataProviderMock.Object, SmoReaderTestUtils.GetMockGlobalPaths().Object);
            var result = scanner.Scan(filePaths);

            //assert
            var orphanedFileWithDate = result.OrphanedFiles.SingleOrDefault(f => f.StartDate.HasValue);
            var orphanedFileWithoutDate = result.OrphanedFiles.SingleOrDefault(f => !f.StartDate.HasValue);
            Assert.Empty(result.FileGroupsValidForFurtherProcessing);
            Assert.Equal(2, result.OrphanedFiles.Count);
            Assert.Equal(new DateTime(2016, 10, 16).Date, orphanedFileWithDate.StartDate.Value.Date);
        } 

        [Fact]
        public void Scan_TwoSmallFilesFrom305and509_TwoGroupsDetectedWITHCorrectAircraftANDPowerupCounts()
        {
            //arrange
            var filePaths = new List<string>() { _509_valid_file1_4packets_with_dupesPath, _305_valid_file1_200packets_with_dupesPath };
            var aircraftDataProviderMock = GetMock305AND509AircraftDataProvider();

            //act 
            var scanner = new FileGroupScanner(aircraftDataProviderMock.Object, SmoReaderTestUtils.GetMockGlobalPaths().Object);
            var result = scanner.Scan(filePaths);

            var result305 = result.FileGroupsValidForFurtherProcessing.SingleOrDefault(fg => fg.AircraftName == "305");
            var result509 = result.FileGroupsValidForFurtherProcessing.SingleOrDefault(fg => fg.AircraftName == "509");

            //assert
            Assert.Equal(317,result305.PowerUpCount);
            Assert.Equal(1054, result509.PowerUpCount);
        }

        private Mock<IAircraftDataProvider> GetMock305AND509AircraftDataProvider()
        {
            var mockAircraftDataProvider = new Mock<IAircraftDataProvider>();
            var defaultConfigXmlContent = File.ReadAllText(_testDataPath + "referenceSmoConfig2017.xml");
            var configXmlContent = File.ReadAllText(_testDataPath + "smoConfig2.xml");

            mockAircraftDataProvider.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(defaultConfigXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetAircraftNameByMacAddress(It.IsAny<string>()))
                .Returns<string>(GetAircraftNameByMacAddress);

            return mockAircraftDataProvider;
        }

        private string GetAircraftNameByMacAddress(string mac)
        {
            if (mac == "000C4D000322")
                return "305";

            if (mac == "000C4D0002BE")
                return "509";

            return null;
        }


    }
}
