
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
    public class IntegrationTests : IDisposable
    {

        private readonly string _testDataPath = SmoReaderTestUtils.testDataPath;
        private string _outputDir;

        public IntegrationTests()
        {
            _outputDir = System.IO.Directory.CreateDirectory("FileMergerTestOutput").FullName;
        }


        private Tuple<AnalysisResult, MergeResult> MergeFilesAndAnalyze_TwoSmallFilesBase(bool useScalingTables)
        {
            //arrange
            var filePaths = new List<string>() { _testDataPath + "File_Merger\\part1_305_.cap",
                _testDataPath + "File_Merger\\part2_305_.cap"
            };
            var scannedFiles = filePaths.Select(p => new FileScanResult(p)
            {
                ReadMetadata = new Smo.Common.FileReadMetadata(p)
                {
                    AircraftName = "305",
                    PowerUpCount = 1054,
                    AcraStartTs = new DateTime(2017, 10, 12, 0, 0, 0)
                }


            }).ToList();
            var mockAircraftDataProvider = GetMock305AircraftDataProvider();

            if (!useScalingTables)
            {
                //no scaling tables
                mockAircraftDataProvider.Setup(m => m.GetScalingTableTsvContent(It.IsAny<string>()))
                    .Returns((string)null);
            }
            else
            {
                mockAircraftDataProvider.Setup(m => m.GetScalingTableTsvContent(It.IsAny<string>()))
                    .Returns(File.ReadAllText(_testDataPath + "\\scalingTables\\305_Maj_2018.tsv"));
            }


            var mockGlobalPaths = GetMock305GlobalPaths().Object;


            //act
            var merger = new FileMerger(
                mockAircraftDataProvider.Object,
                mockGlobalPaths);
            //TODO: perhaps uncouple (mock)
            var fileGroupReader = new FileGroupReader(mockAircraftDataProvider.Object, mockGlobalPaths);
            var groupReadResult = fileGroupReader.ReadFiles(scannedFiles);

            var conversionResult = merger.MergeFiles(groupReadResult);

            var analyzer = new AnalysisEngine(mockAircraftDataProvider.Object);
            var analysisResult = analyzer.Analyze(conversionResult);

            return new Tuple<AnalysisResult, MergeResult>(analysisResult, conversionResult);
        }


        [Fact]
        //integration
        public void MergeFilesAndAnalyze_TwoSmall305FilesNoScalingTables_52SignalsHave97Samples()
        {
            //arrange
            //act
            var result = MergeFilesAndAnalyze_TwoSmallFilesBase(false);
            var analysisResult = result.Item1;
            var conversionResult = result.Item2;

            //assert
            //this must be empty!!!
            var signalsWithWrongSampleCount = analysisResult.Signals.Where(s => s.Samples.Count() != 97);

            Xunit.Assert.Empty(signalsWithWrongSampleCount);
            Assert.Equal(52, conversionResult.Signals.Count());
            Assert.Equal(52, analysisResult.Signals.Count());

            //TODO please remove this cast in program code; unit because unscaled
            var straingauge8firstSampleValue = (uint)analysisResult.Signals.SingleOrDefault(s => s.Name == "P_KAD_ADC_109_C_S1_0_AnalogIn(7)").Samples[0];
            //unscaled value must be different from the scaled one
            Assert.NotEqual(-1438.0, straingauge8firstSampleValue, 0);
        }

        [Fact]
        //integration
        public void MergeFilesAndAnalyze_TwoSmall305FilesWithScalingTables_52SignalsHave97Samples()
        {
            //arrange
            var testOutputPath = "/test_output/";

            //act
            var result = MergeFilesAndAnalyze_TwoSmallFilesBase(true);
            //interim result
            var conversionResult = result.Item2;
            var analysisResult = result.Item1;
           

            //assert
            //this must be empty!!!
            var signalsWithWrongSampleCount = analysisResult.Signals.Where(s => s.Samples.Count() != 97);

            Xunit.Assert.Empty(signalsWithWrongSampleCount);
            Assert.Equal(52, conversionResult.Signals.Count());
            Assert.Equal(52, analysisResult.Signals.Count());

            var straingauge8firstSampleValue = (double)analysisResult.Signals.SingleOrDefault(s => s.Name == "P_KAD_ADC_109_C_S1_0_AnalogIn(7)").Samples[0];
            Assert.Equal(-1438.0, straingauge8firstSampleValue, 0);



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

            mockAircraftDataProvider.Setup(m => m.GetAllAircraftNames())
                .Returns(new List<string>() { "305" });

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
