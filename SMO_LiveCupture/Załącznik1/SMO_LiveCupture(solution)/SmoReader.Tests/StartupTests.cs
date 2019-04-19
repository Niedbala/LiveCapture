
using Moq;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Repositories;
using Smo.Startup;
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
    public class StartupTests : IDisposable
    {

        private readonly string _testDataPath = SmoReaderTestUtils.testDataPath;
        private string _outputDir;

        private string _509_valid_file1_4packets_with_dupesPath;

        public StartupTests()
        {

            _outputDir = System.IO.Directory.CreateDirectory("SmoLauncherTestOutput").FullName;
            Directory.Delete(_outputDir, true);
            _outputDir = System.IO.Directory.CreateDirectory("SmoLauncherTestOutput").FullName;
            _509_valid_file1_4packets_with_dupesPath = _testDataPath + "509_valid_file1\\4good_packets_with_dupes.cap";
        }




        [Fact]
        //integration
        public async void Launch_3Files305and509_OutputTsvsExist()
        {
            //arrange
            var mockAircraftDataProvider = GetMock305AND509AircraftDataProvider();

            var cap305Paths = new List<string>()
            {
                _testDataPath + "\\File_Merger\\part1_305_.cap",
                 _testDataPath + "\\File_Merger\\part2_305_.cap"
            };
            var powerupCount305 = 317;
            var scannedFileGroup305 = new ScannedFileGroup()
            {
                AircraftName = "305",
                ContainingDirectory = "",
                Files = cap305Paths.Select(s => new FileScanResult(s)
                {

                    ReadMetadata = new Smo.Common.FileReadMetadata(s)
                    {
                        AircraftName = "305",
                        CheckSum = "checksum1",
                        PowerUpCount = powerupCount305,
                        AcraStartTs = new DateTime(2017, 9, 20, 0, 0, 0)
                    }
                }).ToList(),
                PowerUpCount = powerupCount305
            };

            var powerupCount509 = 1054;
            var scannedFiles509 = new List<FileScanResult>() {
                new FileScanResult(_509_valid_file1_4packets_with_dupesPath)
                {
                    ReadMetadata = new Smo.Common.FileReadMetadata(_509_valid_file1_4packets_with_dupesPath)
                    {
                        AircraftName = "509",
                        CheckSum = "checksum2",
                        PowerUpCount = powerupCount509,
                        AcraStartTs = new DateTime(2017, 9, 20, 0, 0, 0)
                    }
                }
            };
            var scannedFileGroup509 = new ScannedFileGroup()
            {
                AircraftName = "509",
                ContainingDirectory = "",
                Files = scannedFiles509,
                PowerUpCount = powerupCount509

            };

            var fakeScannedFileGroups = new List<ScannedFileGroup>() {
                scannedFileGroup509, scannedFileGroup305
            };

            //act 
            var launcher = Converter.BuildBatchConverter(mockAircraftDataProvider.Object, SmoReaderTestUtils.instrumentSettingsXml2017Path);
            var result = await launcher.StartConversion(new Smo.Common.Public.Models.ConversionLaunchCommand(fakeScannedFileGroups)
            {
                EarliestDate = DateTime.Now,
                TsvOutputFolder = _outputDir,
                letSmallFilesThrough = true
            });

            //assert           
            var tsvs = result.Records.Select(r => r.TsvPath);
            var tsvsExist = tsvs.All(t => File.Exists(t));

            Assert.Equal(2, tsvs.Count());
            Assert.True(tsvsExist);

        }

        [Fact]
        //integration
        public async void ScanAndLaunch_305and509fullFolderScanned_OutputTsvsExistANDClassificationCorrect()
        {
            //arrange
            var filePaths = Directory
                .GetFiles(_testDataPath + "\\305_509_fulltest", "*.*", SearchOption.AllDirectories)
                .ToList();
            var mockAircraftDataProvider = GetMock305AND509AircraftDataProvider();


            //act

            var launcher = Converter.BuildBatchConverter(mockAircraftDataProvider.Object, SmoReaderTestUtils.instrumentSettingsXml2017Path);

            var folderScanResult = launcher.ScanFiles(filePaths);
            var launchCommand = new Smo.Common.Public.Models.ConversionLaunchCommand(folderScanResult.FileGroupsValidForFurtherProcessing)
            {
                TsvOutputFolder = _outputDir
            };
            var result = await launcher.StartConversion(launchCommand);

            //assert           
            var tsvs = result.Records.Select(r => r.TsvPath);
            var tsvsExist = tsvs.All(t => File.Exists(t));
            //classification:
            var validRecordCount = result.Records.Where(r => r.MetaData.Classification == Smo.Contracts.Enums.DataClassification.Valid).Count();

            Assert.Equal(10, validRecordCount);
            Assert.Equal(11, tsvs.Count());
            Assert.True(tsvsExist);

        }

        //[Fact]
        ////integration
        //public async void Launch_Problematic310file_()
        //{
        //    //arrange
        //    var filePaths = Directory
        //        .GetFiles(_testDataPath + "\\310_powerup_error_file", "*.*", SearchOption.AllDirectories)
        //        .ToList();
        //    var mockAircraftDataProvider =// GetMock305AND509AircraftDataProvider();
        //        GetMockAircraftDataProvider("310");



        //    var scannedFiles310 = filePaths.Select(pth =>
        //    {
        //        return new FileScanResult(pth)
        //        {
        //            ReadMetadata = new Smo.Common.FileReadMetadata(pth)
        //            {
        //                AircraftName = "310",
        //                CheckSum = "checksum2",
        //                PowerUpCount = 666,
        //                AcraStartTs = new DateTime(2017, 9, 20, 0, 0, 0)
        //            }
        //        };
        //    })
        //    .ToList();

        //    var fileGroup = new ScannedFileGroup()
        //    {
        //        AircraftName = "310",
        //        ContainingDirectory = "",
        //        Files = scannedFiles310,
        //        PowerUpCount = 666
        //    };


        //    //act
        //    var launcher = Launcher.Build(mockAircraftDataProvider.Object, SmoReaderTestUtils.instrumentSettingsXml2017Path);

        //    var folderScanResult = launcher.ScanFiles(filePaths);

        //    var launchCommand = new Smo.Common.Public.Models.ConversionLaunchCommand(folderScanResult.FileGroupsValidForFurtherProcessing)
        //    {
        //        TsvOutputFolder = _outputDir,
        //        letSmallFilesThrough = false
        //    };
        //    var result = await launcher.StartConversion(launchCommand);

        //    //assert           
        //    //   var tsvs = result.Records.Select(r => r.TsvPath);
        //    //  var tsvsExist = tsvs.All(t => File.Exists(t));

        //    //  Assert.Equal(11, tsvs.Count());
        //    Assert.True(false);

        //}

        //[Fact]
        ////integration
        //public async void ScanAndLaunch_WrongCaps_OrphanedFilesExist()
        //{
        //    //arrange
        //    var filePaths = 
        //        Directory.GetFiles(_testDataPath + "\\WrongCapsForScanner", "*.cap", SearchOption.AllDirectories)
        //        .ToList();              
        //    var mockAircraftDataProvider = GetMock305AND509AircraftDataProvider();


        //    //act
        //    var launcher = Launcher.Build(mockAircraftDataProvider.Object, SmoReaderTestUtils.instrumentSettingsXml2017Path);

        //    var folderScanResult = launcher.ScanFiles(filePaths);
        //    var launchCommand = new Smo.Common.Public.Models.ConversionLaunchCommand(folderScanResult.FileGroupsValidForFurtherProcessing)
        //    {
        //        TsvOutputFolder = _outputDir
        //    };
        //    var result = await launcher.StartConversion(launchCommand);

        //    //assert         
        //    Assert.Empty(result.Records);
        //    Assert.Equal(2, result.OrphanedFiles.Count);

        //}

        


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
                .Returns<string>(FakeGetAircraftNameByMacAddress);

            return mockAircraftDataProvider;
        }

        //generates any aircraftname from input
        private Mock<IAircraftDataProvider> GetMockAircraftDataProvider(string AircraftName)
        {
            var mockAircraftDataProvider = new Mock<IAircraftDataProvider>();
            var defaultConfigXmlContent = File.ReadAllText(_testDataPath + "referenceSmoConfig2017.xml");
            var configXmlContent = File.ReadAllText(_testDataPath + "smoConfig2.xml");

            mockAircraftDataProvider.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(defaultConfigXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetAircraftNameByMacAddress(It.IsAny<string>()))
                .Returns<string>(s => AircraftName);

            return mockAircraftDataProvider;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Called after every test method.
            Directory.Delete(_outputDir, true);
        }

        private string FakeGetAircraftNameByMacAddress(string mac)
        {
            if (mac == "000C4D000322")
                return "305";

            if (mac == "000C4D0002BE")
                return "509";

            return null;
        }

        private string FakeGetScalingTable(string aircraftName)
        {
            if (aircraftName == "305")
                return File.ReadAllText(_testDataPath + "\\scalingTables\\305_Maj_2018.tsv");

            if (aircraftName == "509")
                return File.ReadAllText(_testDataPath + "\\scalingTables\\509_Maj_2018.tsv");

            return null;
        }

    }
}
