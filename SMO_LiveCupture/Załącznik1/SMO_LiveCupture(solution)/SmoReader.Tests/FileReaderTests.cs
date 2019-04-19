using Moq;
using Smo.Common.Entities;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Repositories;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmoReader.Tests
{
    public class FileReaderTests
    {
        private readonly string _testDataPath = SmoReaderTestUtils.testDataPath;
        private readonly string _509_valid_file1_4packets_with_dupesPath;

        //plik weryfikacyjny od artura
        private readonly string _305_valid_file1_200packets_with_dupesPath;

        private readonly string _instrumentSettingsXmlPath;

        ParameterDefinition powerUpCountParameterDefinition = new ParameterDefinition()
        {
            BaseUnit = "Count",
            DataFormat = "OffsetBinary",
            Name = "P_KAD_BIT_101_0_PowerUpCount",
            OffsetBytes = 58,
            SizeInBits = 32
        };

        ParameterDefinition analog7ParameterDefinition = new ParameterDefinition()
        {
            BaseUnit = "Volt",
            DataFormat = "OffsetBinary",
            Name = "P_KAD_ADC_109_C_S1_0_AnalogIn(7)",
            OffsetBytes = 42,
            SizeInBits = 16
        };

        public FileReaderTests()
        {
            _509_valid_file1_4packets_with_dupesPath = _testDataPath + "509_valid_file1\\4good_packets_with_dupes.cap";
            _305_valid_file1_200packets_with_dupesPath = _testDataPath + "lot_weryfikacyjny_305_12_OCT_2017\\" +
                "200_packets_with_dupes_305.cap";
            //"4good_packets_with_dupes_305.cap";

            _instrumentSettingsXmlPath = SmoReaderTestUtils.instrumentSettingsXml2017Path;

        }

        [Fact]
        public void ReadFile_4PacketFileWithDupesANDnoParametersInConfig_Has2AcraTimeSamplesANDIsUnsuccessful()
        {
            //arrange
            var inputFilePath = _509_valid_file1_4packets_with_dupesPath;
            var mockAircraftDataProvider = GetMock509AircraftDataProvider();

            var mockConfigParser = new Mock<IConfigParser>();
            mockConfigParser.Setup(m => m.ReadConfigurationXml(It.IsAny<string>(), It.IsAny<DateTime>()))
                //no parameters
                .Returns(new List<ParameterDefinition>());

            //mockConfigParser.Setup(m => m.ReadInstrumentSettingsXml())
            //    .Returns(new List<RegisterDefinition>());

            var reader = new FileReader(
                mockAircraftDataProvider.Object,
                mockConfigParser.Object
                );

            //act
            var result = reader.ReadFile(inputFilePath, "509", null);

            //assert
            //read has to be unsuccessful because no Powerupcount extracted, since there are no parameterdefinitions from the mocked configuration
            Assert.False(result.Metadata.IsReadSuccessful);
            Assert.Equal(2, result.AcraTime.Length);
        }

        [Fact]
        public void ReadFile_4PacketFileWithDupesANDanalog7InConfig_SignalValuesCorrect()
        {
            //arrange
            var flightDate = new DateTime(2017, 10, 12, 0, 0, 0);
            var inputFilePath = _305_valid_file1_200packets_with_dupesPath;
            var mockAircraftDataProvider = GetMock305AircraftDataProvider();

            var mockConfigParser = new Mock<IConfigParser>();
            mockConfigParser.Setup(m => m.ReadConfigurationXml(It.IsAny<string>(), It.IsAny<DateTime>()))
                //no parameters
                .Returns(new List<ParameterDefinition>() {
                    //powerup count is needed for succesful 
                    powerUpCountParameterDefinition,
                    analog7ParameterDefinition
                });


            //mockConfigParser.Setup(m => m.ReadInstrumentSettingsXml())
            //    .Returns(new List<RegisterDefinition>());

            var reader = new FileReader(
                mockAircraftDataProvider.Object,
                mockConfigParser.Object
                );

            //act
            var result = reader.ReadFile(inputFilePath, "305", flightDate);

            var timeStampOfTestedSample = result.AcraTime.FirstOrDefault(ts => ts.Minute == 5 && ts.Second == 22 && ts.Millisecond == 437);
            var indexOfTestedSample = result.AcraTime.ToList().IndexOf(timeStampOfTestedSample);

            var analog7Value = result.GetSignal(analog7ParameterDefinition.Name).Samples[indexOfTestedSample];

            //assert
            Assert.True(result.Metadata.IsReadSuccessful);

            //result.GetSignal(analog7ParameterDefinition.Name).Samples[0]
            Assert.Equal(19825U, analog7Value);
        }


        //integration
        [Fact]
        public void ReadFile_UseXmlConfig2_SignalValuesSameInTextFileANDNoEmptySignals()
        {
            //arrange
            var flightDate = new DateTime(2017, 10, 12, 0, 0, 0);
            var inputFilePath = _305_valid_file1_200packets_with_dupesPath;
            var aircraftDataProvider = GetMock305AircraftDataProvider();
            var mockGlobalPaths = SmoReaderTestUtils.GetMockGlobalPaths();

            var configParser = new ConfigParser(aircraftDataProvider.Object, mockGlobalPaths.Object.InstrumentSettingsXml);

            //act
            var reader = new FileReader(
               aircraftDataProvider.Object,
               configParser
               );

            var result = reader.ReadFile(inputFilePath, "305", flightDate);

            var timeStampOfTestedSample = result.AcraTime.FirstOrDefault(ts => ts.Minute == 5 && ts.Second == 22 && ts.Millisecond == 437);
            var indexOfTestedSample = result.AcraTime.ToList().IndexOf(timeStampOfTestedSample);

            var referenceData = GetReferenceSampleFromFile();


            var errorSignals = new List<string>();
            //assert
            //here we compare the reference with test result signals - mstefaniuk
            referenceData.ForEach(signal =>
            {
                SignalData testedSignal = result?.Signals?.FirstOrDefault(s => s.Name == signal.Key);
                uint? testedVal = null;

                if (testedSignal != null && indexOfTestedSample < testedSignal?.Samples.Length)
                    testedVal = (uint)testedSignal?.Samples[indexOfTestedSample];

                if (testedVal != signal.Value)
                    errorSignals.Add($"{signal.Key} expected: {signal.Value} , actual: { testedVal}");
            });

            bool isNoEmptySignals = result.Signals.All(s => s.Samples.Any());

            Assert.True(isNoEmptySignals, "Empty signals in result");
            Assert.Empty(errorSignals);

        }


        [Fact]
        public void ReadFile_4PacketFileWithDupesANDwithPowerUpCountInConfig_IsSuccessful()
        {
            //arrange
            var flightDate = new DateTime(2017, 9, 20, 13, 8, 17);
            var inputFilePath = _509_valid_file1_4packets_with_dupesPath;
            var mockAircraftDataProvider = GetMock509AircraftDataProvider();



            var mockConfigParser = new Mock<IConfigParser>();
            mockConfigParser.Setup(m => m.ReadConfigurationXml(It.IsAny<string>(), It.IsAny<DateTime>()))
                //no parameters
                .Returns(new List<ParameterDefinition>() {
                    powerUpCountParameterDefinition
                });



            //mockConfigParser.Setup(m => m.ReadInstrumentSettingsXml())
            //    .Returns(new List<RegisterDefinition>());

            var reader = new FileReader(
                mockAircraftDataProvider.Object,
                mockConfigParser.Object
                );

            //act
            var result = reader.ReadFile(inputFilePath, "509", flightDate);

            //assert
            Assert.True(result.Metadata.IsReadSuccessful);
            //TODO: what about powerupcount conversion?
            Assert.Equal(1054, result.Metadata.PowerUpCount);
        }

        [Fact]
        //PreliminaryReadFile_UseXmlConfig2_SignalValuesSameInTextFileANDNoEmptySignals
        public void PreliminaryReadFile_UseXmlConfig2_AircraftNameANDDateANDPowerUpCountAreCorrect()
        {
            //arrange
            var inputFilePath = _305_valid_file1_200packets_with_dupesPath;
            var aircraftDataProvider = GetMock305AircraftDataProvider();
            var mockGlobalPaths = SmoReaderTestUtils.GetMockGlobalPaths();

            var configParser = new ConfigParser(aircraftDataProvider.Object, mockGlobalPaths.Object.InstrumentSettingsXml);

            //act
            var reader = new FileReader(
               aircraftDataProvider.Object,
               configParser
               );

            var result = reader.PreliminaryReadFile(inputFilePath);

            //assert
            Assert.Equal("305", result.ReadMetadata.AircraftName);
            Assert.Equal(317, result.ReadMetadata.PowerUpCount);
            Assert.Equal(new DateTime(2017, 10, 12, 11, 5, 21),
                SmoReaderTestUtils.TrimMilliseconds(result.StartDate.Value));

        }

        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private Mock<IAircraftDataProvider> GetMock509AircraftDataProvider()
        {
            var mockAircraftDataProvider = new Mock<IAircraftDataProvider>();
            var defaultConfigXmlContent = File.ReadAllText(_testDataPath + "referenceSmoConfig2017.xml");
            var configXmlContent = File.ReadAllText(_testDataPath + "smoConfig2.xml");

            mockAircraftDataProvider.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(defaultConfigXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetAircraftNameByMacAddress(It.IsAny<string>()))
                .Returns("509");

            return mockAircraftDataProvider;
        }

        private Mock<IAircraftDataProvider> GetMock305AircraftDataProvider()
        {
            var mockAircraftDataProvider = new Mock<IAircraftDataProvider>();
            var defaultConfigXmlContent = File.ReadAllText(_testDataPath + "referenceSmoConfig2017.xml");
            var configXmlContent = File.ReadAllText(_testDataPath + "smoConfig2.xml");

            mockAircraftDataProvider.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(defaultConfigXmlContent);

            mockAircraftDataProvider.Setup(m => m.GetAircraftNameByMacAddress(It.IsAny<string>()))
                .Returns("305");

            return mockAircraftDataProvider;
        }

        //set of signal values for a single timestamp - a single excel row
        private List<KeyValuePair<string, long>> GetReferenceSampleFromFile()
        {
            var singleSampleTextFile = _testDataPath + "lot_weryfikacyjny_305_12_OCT_2017\\singleReferenceSample.txt";

            var sampleLines = File.ReadAllLines(singleSampleTextFile);
            var headers = sampleLines[0].Split('\t');
            var values = sampleLines[1].Split('\t').Select(v => Convert.ToInt64(v)).ToList();

            var result = new List<KeyValuePair<string, long>>();

            for (int i = 0; i < headers.Length; i++)
            {
                var kv = new KeyValuePair<string, long>(headers[i], values[i]);
                result.Add(kv);
            }

            return result;
        }

    }
}
