using PacketDotNet;
using SharpPcap;
using Smo.Common.Entities;
using Smo.Common.Public.Repositories;
using SmoReader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader
{
    public class PacketDecoder
    {
        private string _lastDetectedAircraftName = null;
        private DataConverter _dataConverter = new DataConverter();
        decimal microMultiplier = (decimal)Math.Pow(10, -6);
        private readonly IAircraftDataProvider _aircraftDataProvider;

        /// <summary>
        /// Packets Below this date will be IGNORED
        /// </summary>
        public DateTime CutoffDate { get; set; }
       List<ParameterDefinition> _parameterDefinitions { get; set; }
        
        private string _cachedAircraftName = null;

        Dictionary<string, AnalogSignalScaler> signalScalers = new Dictionary<string, AnalogSignalScaler>();

        string _sourceMac = null;

        public PacketDecoder(List<ParameterDefinition> parameterDefinitions, IAircraftDataProvider aircraftDataProvider = null)
        {
            CutoffDate = new DateTime(2010, 1, 1);

            _parameterDefinitions = parameterDefinitions;
            this._aircraftDataProvider = aircraftDataProvider;

           

            CreateScalers();
        }

        //Todo: move messages outside
        public PacketReadResult DecodePacket(RawCapture packet, string aircraftName)
        {

            var result = new PacketReadResult();

            Packet udpPacket = null;
            IPv4Packet payloadPacket;

            var dataLength = packet?.Data?.Length ?? 0;

            //initial check before any packet extracted
            if (dataLength <= 0)
            {
                result.Flags.EmptyPacketData = true;
                result.Message = ($"Empty packet data; ");
                return result;
            }

            try
            {
                udpPacket = UdpPacket.ParsePacket(LinkLayers.Ethernet, packet.Data);

                //UdpPacket.ParsePacket(packet.LinkLayerType, packet.Data);
            }
            catch (Exception e)
            {
                result.Flags.NoUdpPacket = true;
                result.Message = ($"Couldn't parse UDP packet; ");
                return result;
            }


            try
            {
                payloadPacket = (IPv4Packet)udpPacket.PayloadPacket;
            }
            catch (Exception e)
            {
                result.Flags.NoIpV4Packet = true;
                result.Message = ($"no IPv4Packet " + e.Message);
                return result;
            }
            if (payloadPacket == null) return result;

            //other destination adresses mean invalid packet
            if (payloadPacket.DestinationAddress.ToString() != "127.0.0.1")
            {
                var payloadLength = payloadPacket.PayloadLength;
                if (payloadLength < 76) return result;

                var encapsulatedPacket = udpPacket.Extract(typeof(UdpPacket));
                if (encapsulatedPacket == null) return result;

                var payloadDataSize = encapsulatedPacket.PayloadData.Length;

                var AcraKamSeconds =
                    BitConverter.ToInt32(
                        encapsulatedPacket.PayloadData.Skip(16).Take(4).Reverse().ToArray(), 0);
                
                var stream_ID = 
                    BitConverter.ToInt32(
                        encapsulatedPacket.PayloadData.Skip(4).Take(4).Reverse().ToArray(), 0);
                


                if (stream_ID > 100) { stream_ID = 1; }
                result.streamID = stream_ID;
                var AcraKamNanoSeconds =
                    BitConverter.ToInt32(
                        encapsulatedPacket.PayloadData.Skip(20).Take(4).Reverse().ToArray(), 0);

                var acraDateTimeSample = TimeFormatter.UnixTimeStampToDateTime(AcraKamSeconds, AcraKamNanoSeconds);

                //TODO: crude check of invalid timestamp. use median or some other solution
                if (acraDateTimeSample < CutoffDate)
                    return result;

                result.AcraTime = acraDateTimeSample;

                DateTime packetDateTime = TimeFormatter.UnixTimeStampToDateTime((long)packet.Timeval.Seconds,
                    (long)packet.Timeval.MicroSeconds * 1000);

                result.EthernetTime = packetDateTime;
                result.TimeDeltaTicks = (acraDateTimeSample - packetDateTime).Ticks;

                //warning - this may be a costly operation
                //todo - not use in file mode
                if (String.IsNullOrWhiteSpace(_sourceMac)) 
                {
                    EthernetPacket ethernetPacket = (EthernetPacket)EthernetPacket.ParsePacket(LinkLayers.Ethernet, packet.Data);
                    _sourceMac = ethernetPacket.SourceHwAddress.ToString();
                }
                var sourceMac = _sourceMac;

                if (string.IsNullOrEmpty(aircraftName))
                    aircraftName = _aircraftDataProvider?.GetAircraftNameByMacAddress(sourceMac);

                //if (!String.IsNullOrWhiteSpace(aircraftName)
                //    && !String.IsNullOrWhiteSpace(_lastDetectedAircraftName)
                //    && _lastDetectedAircraftName != aircraftName
                //    && _isLiveMode
                //    )
                //    throw new Exception("aircraftName changed during a single file read!!!!");

                AnalogSignalScaler currentScaler = null;

                if (!String.IsNullOrWhiteSpace(aircraftName) && signalScalers.Any())
                {
                   currentScaler = signalScalers?[aircraftName];
                }

                //get samples per parameter
                _parameterDefinitions.ForEach(parameterDefinition =>
                {
                    if (parameterDefinition.StreamID == stream_ID)
                    {
                        var offset = parameterDefinition.OffsetBytes;
                        var size = (parameterDefinition.SizeInBits / 8);
                        if (parameterDefinition.Occurrences == 0)
                        {
                            var parameterBytes = encapsulatedPacket.PayloadData.Skip(offset).Take(size).ToArray();
                            var parameterData = new ParameterData(parameterBytes);

                            var samples = GetSamplesFromParameterData(parameterData, parameterDefinition);

                            if (currentScaler != null)
                            {
                                samples = samples.Select(s => ScaleSample(currentScaler, s)).ToList();
                            }
                            result.Samples.AddRange(samples);
                        }

                        else
                        {
                            for (int i = 0; i < parameterDefinition.Occurrences; i++)
                            {
                                var parameterBytes = encapsulatedPacket.PayloadData.Skip(offset).Take(size).ToArray();
                                var parameterData = new ParameterData(parameterBytes);

                                var samples = GetSamplesFromParameterData(parameterData, parameterDefinition);

                                if (currentScaler != null)
                                {
                                    samples = samples.Select(s => ScaleSample(currentScaler, s)).ToList();
                                }
                                result.Samples.AddRange(samples);

                                offset = offset + size;
                            }

                        }
                    }
                });

                result.IsSuccess = true;
                var destinationMacAddress = payloadPacket.DestinationAddress;
                result.DestinationIPAddress = destinationMacAddress;
                return result;
            }

            return result;
        }

        //remember: ParameterData may be split up into RegisterDatas, depending on the xml config schema
        private List<KeyValuePair<string, ValueType>> GetSamplesFromParameterData(ParameterData parameterData, ParameterDefinition parameterDefinition)
        {
            var outputSamples = new List<KeyValuePair<string, ValueType>>();

            if (parameterData.Bytes.Length == 0)
                return outputSamples;

            uint convertedValue = 0;

            //extracting samples:
            if (parameterDefinition.Registers.Any())
            {
                var extractedRegisterSamples = GetSamplesFromRegisters(parameterData, parameterDefinition);
                extractedRegisterSamples.ForEach(registerSample =>
                {
                    outputSamples.Add(registerSample);
                });

            }

            else if (parameterDefinition.DataFormat == "BinaryCodedDecimal")
            {
                var hexString = parameterData.Hex;
                convertedValue = _dataConverter.ConvertBCD(hexString);
                outputSamples.Add(new KeyValuePair<string, ValueType>(parameterDefinition.Name, convertedValue));
            }

            else if (parameterDefinition.DataFormat == "OffsetBinary")
            {
                //WARNING: warning in fact we are not using offset binary - we use 2s complement AND EXTRACT UINT.
                //This is done to match Artur Kurnyta's ACRA data export reference/test values    
 
                    convertedValue = _dataConverter.ConvertFromTwosComplement(parameterData.Bytes);
                    outputSamples.Add(new KeyValuePair<string, ValueType>(parameterDefinition.Name, convertedValue));
                
            }
            else
            {
                convertedValue = _dataConverter.ConvertFromTwosComplement(parameterData.Bytes);
                outputSamples.Add(new KeyValuePair<string, ValueType>(parameterDefinition.Name, convertedValue));
            }

            return outputSamples;
        }
        private List<KeyValuePair<string, ValueType>> GetSamplesFromRegisters(ParameterData parameterData, ParameterDefinition parameter)
        {
            var registerSamples = new List<KeyValuePair<string, ValueType>>();

            var extractedSignals = new Dictionary<string, List<ValueType>>();

            parameter.Registers.ForEach(reg =>
            {
                uint converted = 0;
                var registerBits = new List<bool>();

                if (reg.DataFormat == "BCD")
                {
                    var hexString = parameterData.Hex;
                    var charsToSkip = (int)Math.Ceiling((double)(reg.BitOffset / 4.0));
                    var charsToTake = (int)Math.Ceiling((double)(reg.BitSize / 4.0));
                    hexString = String.Concat(hexString.Reverse().Skip(charsToSkip).Take(charsToTake).Reverse());
                    converted = _dataConverter.ConvertBCD(hexString);
                }
                else
                {
                    //take register bits offseted by register offset
                    registerBits = parameterData.Bits
                        .Skip(reg.BitOffset).Take(reg.BitSize)
                        .ToList();

                    converted = _dataConverter.ConvertRegisterBitsToInt(registerBits);
                    //AddToLogstring(parameter.Name, converted, parameterHex);
                }

                var registerName = string.Format("{0} - {1}", parameter.Name, reg.Name);

                var sample = new KeyValuePair<string, ValueType>(registerName, converted);
                registerSamples.Add(sample);
            });

            return registerSamples.ToList();
        }

        private KeyValuePair<string, ValueType> ScaleSample(AnalogSignalScaler scaler, KeyValuePair<string, ValueType> inputSample)
        {
            var scaledSample = scaler.ScaleSample(inputSample);

            return scaledSample;
        }

        //seed the scaler dictionary depending on aircraft data
        private void CreateScalers()
        {           
            _aircraftDataProvider?.GetAllAircraftNames()?.ForEach(name =>
            {

                var scaler = AnalogSignalScaler.BuildScaler(name, _aircraftDataProvider);

                if (scaler != null)
                    signalScalers.Add(name, scaler);
            });
        }

    }
}
