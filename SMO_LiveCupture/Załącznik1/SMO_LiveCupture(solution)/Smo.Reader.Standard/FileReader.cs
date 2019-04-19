using Newtonsoft.Json;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using SmoReader.Entities;
using static SmoReader.Utils.TimeFormatter;
using Smo.Common;
using Smo.Common.Entities;
using Smo.Common.Enums;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Repositories;
using SmoReader;

namespace SmoReader
{
    public class FileReader
    {
        public FileReader(
            IAircraftDataProvider aircraftDataProvider,
            IConfigParser configParser,
            ILoggingService loggingService = null)
        {
            _loggingService = loggingService;
            _aircraftDataProvider = aircraftDataProvider;
            _configParser = configParser;
        }

        private IAircraftDataProvider _aircraftDataProvider;
        private ILoggingService _loggingService;
        private IConfigParser _configParser;


        public FileReadResult ReadFile(string filepath, string aircraftName, DateTime? configDate)
        {
            var finalResult = ReadFileBase(filepath, aircraftName, configDate, false);
            var powerUpCount = GetPowerUpCount(finalResult);

            try
            {
                if (!powerUpCount.HasValue)
                {
                    var msg = $"ReaderEngine: Extracted reader result {filepath} has no power-up count";
                    Debug.WriteLine(msg);
                    _loggingService?.Log(msg, LogLevel.Error);
                    finalResult.Metadata.IsReadSuccessful = false;
                }
                else
                {
                    finalResult.Metadata.PowerUpCount = powerUpCount.Value;
                }
            }
            catch
            {
                if (finalResult != null)
                {
                    finalResult.Metadata.IsReadSuccessful = false;
                }
            }

            return finalResult;
        }
        /// <summary>
        /// used to get preliminary info like date, mac for aircraft number etc
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="aircraftName"></param>
        /// <returns></returns>
        public PreliminaryReadResult PreliminaryReadFile(string filepath)
        {
            var result = new PreliminaryReadResult()
            {
                ReadMetadata = new FileReadMetadata(filepath),
                IsReadSuccessful = false
            };


            var dateAircraftResult = ReadFileBase(filepath, null, null, true);
            result.ReadMetadata = dateAircraftResult.Metadata;

            var configDate = dateAircraftResult.AcraStartTime;

            if (configDate == null)
                result.IsReadSuccessful = false;

            var aircraftName = dateAircraftResult.Metadata.AircraftName;

            int? powerUpCount = null;

            //read a single sample to get powerupcount - mstefaniuk
            if (!string.IsNullOrWhiteSpace(aircraftName) && configDate != null)
            {
                var preliminaryReadResult = ReadFileBase(filepath, aircraftName, configDate, true);
                powerUpCount = GetPowerUpCount(preliminaryReadResult);
                result.ReadMetadata = preliminaryReadResult.Metadata;
            }

            if (powerUpCount.HasValue && configDate.HasValue)
                result.IsReadSuccessful = true;

            result.ReadMetadata.AircraftName = aircraftName;
            result.ReadMetadata.PowerUpCount = powerUpCount;

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        private FileReadResult ReadFileBase(string filepath, string aircraftName, DateTime? configDate, bool isSinglePacketRead = false)
        {
            FileReadResult readResult = new FileReadResult(filepath);
            readResult.Metadata = new FileReadMetadata(filepath);

            CaptureFileReaderDevice fileReader = null;
            var flags = new ReadErrorFlags();

            var parameterDefinitions = new List<ParameterDefinition>();
            string sourceMac = null;

            if (configDate.HasValue)
                parameterDefinitions = _configParser.ReadConfigurationXml(aircraftName, configDate.Value);
            try
            {
                _loggingService?.Log($"reading {filepath} {DateTime.Now.ToString()}");

                fileReader = new CaptureFileReaderDevice(
                    filepath);

                RawCapture packet = null;
                RawCapture lastProperPacket = null;

                var sortedParameters = parameterDefinitions.OrderBy(p => p.OffsetBytes).ToList();

                //creating sample holder - here the samples from packets are added in the extraction loop
                var extractedSamples = new Dictionary<string, List<ValueType>>();

                var ethernetTimeSignatures = new List<DateTime>();
                var acraTimeSignatures = new List<DateTime>();

                int frameIndex = 0;

                //todo hardcode
                var errorCutoffDateTime = new DateTime(2010, 1, 1);

                _loggingService?.Log("Creating Packet Decoder instance");
                var packetReader = new PacketDecoder(errorCutoffDateTime, parameterDefinitions, true, _aircraftDataProvider);

                var _fileSize = fileReader.FileSize;

                var sizeTooLargeFlag = false;
                //this is because we encountered 150 Mb error files
                if (_fileSize > 100776768)
                {
                    flags.GeneralReadError = true;
                    sizeTooLargeFlag = true;
                    _loggingService?.Log("encountered a huge file");
                }


                #region Packet Extraction Loop
                while (GetNextPacketSafe(fileReader, out packet) > 0 && !sizeTooLargeFlag)
                {
                    try
                    {
                        frameIndex++;
                        Debug.WriteLine(frameIndex);
                        if (frameIndex % 2 == 0) continue;

                        var packetReadResult = packetReader.DecodePacket(packet, aircraftName);
                        if (packetReadResult.IsSuccess)
                        {
                            lastProperPacket = packet;
                            ethernetTimeSignatures.Add(packetReadResult.EthernetTime.Value);
                            acraTimeSignatures.Add(packetReadResult.AcraTime.Value);

                            if (frameIndex < 6)
                                packetReadResult.Samples.ForEach(s =>
                                {
                                    if (extractedSamples.ContainsKey(s.Key))
                                        extractedSamples?[s.Key].Add(s.Value);
                                    else
                                        extractedSamples[s.Key] = new List<ValueType>() { s.Value };
                                });
                        }

                        //we only need a single sample to get the acra date
                        if (isSinglePacketRead && acraTimeSignatures.Any())
                        {
                            // var destinationAddress = packetReadResult.DestinationIPAddress;
                            fileReader.Close();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        flags.GeneralReadError = true;
                        _loggingService?.Log($"Error while reading packet: {filepath}", LogLevel.Error);
                        continue;
                    }

                }
                #endregion

                _loggingService?.Log($"Packet extraction loop done: {filepath}");

                if (lastProperPacket != null)
                {
                    EthernetPacket ethernetPacket = (EthernetPacket)EthernetPacket.ParsePacket(LinkLayers.Ethernet, lastProperPacket.Data);
                    sourceMac = ethernetPacket.SourceHwAddress.ToString();
                }

                aircraftName = _aircraftDataProvider.GetAircraftNameByMacAddress(sourceMac);

                readResult.Signals =
                        extractedSamples.Select(s => new SignalData()
                        {
                            Name = s.Key,
                            Samples = s.Value.ToArray()
                        }).ToList();

                readResult.Metadata.SourceMacAddress = sourceMac;
                readResult.Metadata.AircraftName = aircraftName;

                fileReader.StopCapture();
                fileReader.Close();
                _loggingService?.Log($"Pcap reader closed: {filepath}");

                if (!ethernetTimeSignatures.Any())
                {
                    flags.NoSamplesError = true;
                    readResult.Metadata.IsReadSuccessful = false;
                }

                readResult.AcraTime = acraTimeSignatures?.ToArray();
                readResult.EthernetTime = ethernetTimeSignatures?.ToArray();
                readResult.Metadata.IsReadSuccessful = readResult?.AcraTime?.Any() ?? false;
                readResult.Metadata.EthernetStartTs = ethernetTimeSignatures?.First();
                readResult.Metadata.EthernetEndTs = ethernetTimeSignatures?.Last();
                readResult.Metadata.AcraStartTs = (acraTimeSignatures.Any()) ? (DateTime?)acraTimeSignatures.First() : null;
                readResult.Metadata.AcraEndTs = (acraTimeSignatures.Any()) ? (DateTime?)acraTimeSignatures.Last() : null;
                readResult.Metadata.AircraftName = aircraftName;
            }
            catch (Exception e)
            {
                if (e is AccessViolationException)
                {
                    flags.AccessViolationError = true;
                    _loggingService?.Log($"Access violation: {filepath}", LogLevel.Warning);
                }
                else
                {
                    flags.GeneralReadError = true;
                    _loggingService?.Log($"General read error: {filepath}", LogLevel.Error);
                }

                readResult.Metadata.IsReadSuccessful = false;
            }

            if (fileReader != null)
            {
                fileReader.StopCapture();
                fileReader.Close();

                _loggingService?.Log($"Pcap reader finally closed: {filepath}");
            }

            readResult.Metadata.Flags = flags;
            readResult.Metadata.isPreliminaryRead = isSinglePacketRead;
            return readResult;
        }
        private int? GetPowerUpCount(FileReadResult readerResult)
        {

            var powerupSignalName = readerResult?.Signals?.FirstOrDefault(s => s.Name.ToLower().Contains("powerup"))?.Name;

            if (String.IsNullOrWhiteSpace(powerupSignalName))
                return null;

            var powerUpCountSignal = readerResult?.GetSamples(powerupSignalName);

            if (powerUpCountSignal == null || !powerUpCountSignal.Any())
                return null;

            var powerUpCount = (int?)powerUpCountSignal?
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .First().Key;

            _loggingService?.Log($"Extracted powerupcount: {powerUpCount}");
            return powerUpCount;
        }
        private DataConverter _dataConverter = new DataConverter();
        private int GetNextPacketSafe(CaptureFileReaderDevice reader, out RawCapture packet)
        {
            int flag = 0;
            try
            {
                flag = reader.GetNextPacket(out packet);
            }
            catch (Exception)
            {
                packet = null;
            }
            return flag;
        }




    }
}

