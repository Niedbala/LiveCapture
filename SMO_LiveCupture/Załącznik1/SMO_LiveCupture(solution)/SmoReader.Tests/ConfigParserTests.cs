using Moq;
using Smo.Common.Public.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SmoReader.Tests
{
    public class ConfigParserTests
    {

        private readonly string _testDataPath = SmoReaderTestUtils.testDataPath;

        //TODO this is a brittle "integration" test
        [Fact]
        public void ReadConfigurationXml_ReferenceSmoConfig2017xmlAsInput_ResultNotNull()
        {
            
            //arrange
            var configXmlContent = File.ReadAllText(_testDataPath + "referenceSmoConfig2017.xml");

            var mockSmoAircraftRepository = new Mock<IAircraftDataProvider>();

            mockSmoAircraftRepository.Setup(m => m.GetConfigXmlContent(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(configXmlContent);

            mockSmoAircraftRepository.Setup(m => m.GetDefaultConfigXmlContent())
                .Returns(configXmlContent);

            var parserUnderTest = new ConfigParser(mockSmoAircraftRepository.Object, SmoReaderTestUtils.GetMockGlobalPaths().Object.InstrumentSettingsXml);

            //act
            //todo - not an unit           
            var result = parserUnderTest.ReadConfigurationXml("Aircraft1", new DateTime(2016, 1, 1));

            //assert
            Assert.NotNull(result);
        }
    }
}
