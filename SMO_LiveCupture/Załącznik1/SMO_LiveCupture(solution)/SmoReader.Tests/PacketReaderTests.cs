using Moq;
using SharpPcap;
using Smo.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SmoReader.Tests
{
    
    public class PacketDecoderTests
    {
        //509 aircraft 2017 SEP 20 13:08 to 14:15, frame 85 
        RawCapture valid509packet1 { get; set; }

        public PacketDecoderTests()
        {
            //taken from 509_valid_file1 - frame 85
            var packetDataString = "01005e000001000c4d0002be08004500007c2b124000ff1188b3c0a81c01eb000001000003ff0068000011000000000000ab00002b110000006059c2684103b9ae9400000000705e7fc1843748527bfc9764832a4551ccf23280c02d00110101725672280000041e00001525026300000006000001120065028953472406000000155045025680cc0096";
            valid509packet1 = new RawCapture(PacketDotNet.LinkLayers.Ethernet, new PosixTimeval(), SmoReaderTestUtils.StringToByteArray(packetDataString));
        }

        [Fact]
        public void ReadPacket_NullInput_IsUnsuccessful()
        {
            //arrange
            var decoderUnderTest = new PacketDecoder(new List<ParameterDefinition>());

            //act
            var result = decoderUnderTest.DecodePacket(null,null);

            //assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ReadPacket_2ByteAllZeroInput_IsUnsuccessful()
        {
            //arrange
            var inputPacket = new RawCapture(PacketDotNet.LinkLayers.Ethernet, new PosixTimeval(), new byte[2]);
            var decoderUnderTest = new PacketDecoder(new List<ParameterDefinition>());

            //act
            var result = decoderUnderTest.DecodePacket(inputPacket,null);

            //assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ReadPacket_Valid509Packet1AsInputANDNoParameterDefinitions_ResultDatetimeCorrectANDNoSamples()
        {
            //arrange
            var inputPacket = valid509packet1;
            var decoderUnderTest = new PacketDecoder(new List<ParameterDefinition>());

            //act
            var result = decoderUnderTest.DecodePacket(inputPacket,null);
            var resultTs = result.AcraTime.Value;


            //assert
            Assert.True(result.IsSuccess); 
            Assert.Equal(SmoReaderTestUtils.TrimMilliseconds(resultTs), new DateTime(2017, 9, 20, 13, 8, 17));
            Assert.Empty(result.Samples);
        }

        [Fact]
        public void ReadPacket_Valid509Packet1AsInputWithSingleOffsetBinaryParameter_SignalValueCorrect()
        {
            //arrange
            var inputPacket = valid509packet1;
            var offsetBinaryParameterDefinition = new ParameterDefinition()
            {
                BaseUnit = "BaseUnit",
                DataFormat = "OffsetBinary",
                Name = "Signal1",
                OffsetBytes = 42,
                SizeInBits = 16
            };
            var decoderUnderTest = new PacketDecoder(
                
                new List<ParameterDefinition>() { offsetBinaryParameterDefinition });

            //act
            var result = decoderUnderTest.DecodePacket(inputPacket,null);
            var resultSample = result.Samples.SingleOrDefault();

            //assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Signal1", resultSample.Key);
            Assert.Equal(17745U, resultSample.Value);
        }


        [Fact]
        public void ReadPacket_Valid509Packet1AsInputWithSingleRegisterParameter_SignalValueCorrect()
        {
            //arrange
            var inputPacket = valid509packet1;
            var offsetBinaryParameterDefinition = new ParameterDefinition()
            {
                BaseUnit = "BitVector",
                DataFormat = "BitVector",
                Name = "SignalWithRegister",
                OffsetBytes = 92,
                SizeInBits = 16,
                Registers = new List<RegisterDefinition>()
                {
                    new RegisterDefinition()
                    {
                        BitOffset = 4,
                        BitSize = 4,
                        DataFormat = "",
                        Name = "SattelitesInView",
                        ParameterName = "StatusGPS"
                    }
                }
            };

            var decoderUnderTest = new PacketDecoder(
               
                new List<ParameterDefinition>() { offsetBinaryParameterDefinition });

            //act
            var result = decoderUnderTest.DecodePacket(inputPacket,null);
            var resultSample = result.Samples.SingleOrDefault();

            //assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Samples.Where(s=>s.Key == "SignalWithRegister - SattelitesInView").Any(),"Register signal not extracted!");
            Assert.Equal(12U, resultSample.Value);
        }

     

    }
}
