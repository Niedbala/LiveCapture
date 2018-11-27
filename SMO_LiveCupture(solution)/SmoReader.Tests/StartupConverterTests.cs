using SharpPcap;
using Smo.Startup;
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
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;

namespace SmoReader.Tests
{
    public class StartupConverterTests
    {

        //509 aircraft 2017 SEP 20 13:08 to 14:15, frame 85 
        RawCapture valid509packet1 { get; set; }

        public StartupConverterTests()
        {
            //taken from 509_valid_file1 - frame 85
            var packetDataString = "01005e000001000c4d0002be08004500007c2b124000ff1188b3c0a81c01eb000001000003ff0068000011000000000000ab00002b110000006059c2684103b9ae9400000000705e7fc1843748527bfc9764832a4551ccf23280c02d00110101725672280000041e00001525026300000006000001120065028953472406000000155045025680cc0096";
            valid509packet1 = new RawCapture(PacketDotNet.LinkLayers.Ethernet, new PosixTimeval(), SmoReaderTestUtils.StringToByteArray(packetDataString));
        }

        //integration
        [Fact]
        public void LiveConverter_509validPacket_ResultIsSuccess()
        {
            //arrange
            var scalingTsvPath = SmoReaderTestUtils.testDataPath + "\\scalingTables\\509_Maj_2018.tsv";
            var configXmlPath = Path.Combine(SmoReaderTestUtils.testDataPath, "smoConfig2.xml");
            var instrumentSettingsPath = SmoReaderTestUtils.instrumentSettingsXml2017Path;
            var aircraftName = "509";

            //TODO: w zasadzie insturment settings xml sie nigdy nie bedzie zmienial (??) - mstefaniuk
            //TODO: wydaje mi sie ze aircaftname jest nieistotne
            var converter = Converter.BuildLiveConverter(aircraftName, configXmlPath, scalingTsvPath, instrumentSettingsPath);


            //act
            var result = converter.DecodePacket(valid509packet1);

            //assert
            Assert.True(result.IsSuccess);
        }
        private static int capturedPackets;
        [Fact]
        public void CaptureFinite()
        {
            var device = new CaptureFileReaderDevice("C:\\Users\\pniedbala\\Desktop\\test_data\\305_509_fulltest\\10_305 17032018 27032018\\0000_0000.cap");
            device.OnPacketArrival += HandleDeviceOnPacketArrival;
            device.Open();

            var expectedPackets = 51677;
            capturedPackets = 0;
            device.Capture();

            Assert.Equal(expectedPackets, capturedPackets);
        }
        void HandleDeviceOnPacketArrival(object sender, CaptureEventArgs e)
        {
            Console.WriteLine("got packet " + e.Packet.ToString());
            capturedPackets++;
        }

        [Fact]
        public void Capture_File_Decode_Pacet()
        {
            var scalingTsvPath = SmoReaderTestUtils.testDataPath + "\\scalingTables\\509_Maj_2018.tsv";
            var configXmlPath = Path.Combine(SmoReaderTestUtils.testDataPath, "smoConfig2.xml");
            var instrumentSettingsPath = SmoReaderTestUtils.instrumentSettingsXml2017Path;
            var aircraftName = "509";
            var extractedSamples = new Dictionary<string, List<ValueType>>();
            //TODO: w zasadzie insturment settings xml sie nigdy nie bedzie zmienial (??) - mstefaniuk
            //TODO: wydaje mi sie ze aircaftname jest nieistotne
            var converter = Converter.BuildLiveConverter(aircraftName, configXmlPath, scalingTsvPath, instrumentSettingsPath);
            var device = new CaptureFileReaderDevice("C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\0011_0000.cap");
            string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\0011_0000_read.tsv";
            //var device = new CaptureFileReaderDevice("C:\\Users\\pniedbala\\Desktop\\test_data\\305_509_fulltest\\10_305 17032018 27032018\\0000_0000.cap");
            device.OnPacketArrival += new PacketArrivalEventHandler((sender,e) => HandleDeviceOnPacketArrivalDecode(this, e, converter, extractedSamples));
            device.Open();

            var expectedPackets = 8;
            capturedPackets = 0;
            device.Capture();
            string headerLine = "";
            foreach (string key in extractedSamples.Keys)
            {
                headerLine = headerLine + key + "\t";
            }
            var keys_test = new List<string>(extractedSamples.Keys);
            var test = keys_test[0];
            int len = extractedSamples[test].Count;
            using (StreamWriter sw = File.CreateText(dumpTextPath))
            {
                sw.WriteLine(headerLine);
                for (int i = 0; i < len; i++)
                {
                    string valuesLine = "";
                    foreach (string key in extractedSamples.Keys)
                    {
                        valuesLine = valuesLine + extractedSamples[key][i] + "\t";
                    }
                    sw.WriteLine(valuesLine);
                }
            }

                Assert.Equal("-175.792892905117", extractedSamples["P_KAD_ADC_109_C_S1_0_AnalogIn(0)"][0].ToString());
        }
        
        void HandleDeviceOnPacketArrivalDecode(object sender, CaptureEventArgs e, LiveConverter converter,  Dictionary<string, List<ValueType>> extractedSamples)
        {
            Console.WriteLine("got packet " + e.Packet.ToString());
            capturedPackets++;
            var result = converter.DecodePacket(e.Packet);
            
            result.Samples.ForEach(s =>
            {
                if (extractedSamples.ContainsKey(s.Key))
                    extractedSamples?[s.Key].Add(s.Value);
                else
                    extractedSamples[s.Key] = new List<ValueType>() { s.Value };
            });
            //readResult= extractedSamples.Select(s => new SignalData()
              //            {
                //              Name = s.Key,
                  //            Samples = s.Value.ToArray()
                    //      }).ToList();

        }
        [Fact]
        public void Capture_File_Decode_Pacet_New_Config()
        {
            var scalingTsvPath = SmoReaderTestUtils.testDataPath + "\\scalingTables\\509_Maj_2018.tsv";
            var configXmlPath = Path.Combine(SmoReaderTestUtils.testDataPath, "SSR_test.xml");
            var instrumentSettingsPath = SmoReaderTestUtils.instrumentSettingsXml2017Path;
            var aircraftName = "509";
            var extractedSamples = new Dictionary<string, List<ValueType>>();
            //TODO: w zasadzie insturment settings xml sie nigdy nie bedzie zmienial (??) - mstefaniuk
            //TODO: wydaje mi sie ze aircaftname jest nieistotne
            var converter = Converter.BuildLiveConverter(aircraftName, configXmlPath, scalingTsvPath, instrumentSettingsPath);
            var device = new CaptureFileReaderDevice("C:\\Users\\pniedbala\\Desktop\\CAP_file\\cap_ssr_test3.cap");
            string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\CAP_file\\cap_ssr_test3_read.tsv";
            //var device = new CaptureFileReaderDevice("C:\\Users\\pniedbala\\Desktop\\test_data\\305_509_fulltest\\10_305 17032018 27032018\\0000_0000.cap");
            device.OnPacketArrival += new PacketArrivalEventHandler((sender, e) => HandleDeviceOnPacketArrivalDecode2(this, e, converter, extractedSamples));
            device.Open();

            var expectedPackets = 60;
            capturedPackets = 0;
            device.Capture();
           

            
            string headerLine = "";
            foreach (string key in extractedSamples.Keys)
            {
                headerLine = headerLine + key + "\t";
            }
            var keys_test = new List<string>(extractedSamples.Keys);
            if (keys_test.Count != 0)
            {
                var test = keys_test[0];
                int len = extractedSamples[test].Count;
                using (StreamWriter sw = File.CreateText(dumpTextPath))
                {
                    sw.WriteLine(headerLine);
                    for (int i = 0; i < len; i++)
                    {
                        string valuesLine = "";
                        foreach (string key in extractedSamples.Keys)
                        {
                            valuesLine = valuesLine + extractedSamples[key][i] + "\t";
                        }
                        sw.WriteLine(valuesLine);
                    }
                }
            }

            Assert.Equal(expectedPackets,capturedPackets);
        }

        void HandleDeviceOnPacketArrivalDecode2(object sender, CaptureEventArgs e, LiveConverter converter, Dictionary<string, List<ValueType>> extractedSamples)
        {
            Console.WriteLine("got packet " + e.Packet.ToString());
            capturedPackets++;
            var result = converter.DecodePacket(e.Packet);

            result.Samples.ForEach(s =>
            {
                if (extractedSamples.ContainsKey(s.Key))
                    extractedSamples?[s.Key].Add(s.Value);
                else
                    extractedSamples[s.Key] = new List<ValueType>() { s.Value };
            });
            //readResult= extractedSamples.Select(s => new SignalData()
            //            {
            //              Name = s.Key,
            //            Samples = s.Value.ToArray()
            //      }).ToList();

        }



    }


}
