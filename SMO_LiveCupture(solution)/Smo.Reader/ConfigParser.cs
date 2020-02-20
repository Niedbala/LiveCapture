﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smo.Common.Entities;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Smo.Common.Public.Repositories;
using Smo.Common.Infrastructure;

namespace SmoReader
{
   

    public class Mapping
    {
        public string Name;
        public int OffsetBytes;
        public int Occurrences;
        public int StreamID;
    }



    public class ConfigParser : IConfigParser
    {
        private readonly IGlobalPaths _globalPaths;

        IAircraftDataProvider _smoAircraftDataProvider { get; set; }
        string _instrumentSettingsXmlPath { get; set; }

        string _defaultConfigXmlContent { get; set; }

        public ConfigParser(IAircraftDataProvider smoAircraftDataProvider, string instrumentSettingsXmlPath)
        {
            _smoAircraftDataProvider = smoAircraftDataProvider;

            _instrumentSettingsXmlPath = instrumentSettingsXmlPath;
            _defaultConfigXmlContent = smoAircraftDataProvider.GetDefaultConfigXmlContent();
        }

        public List<RegisterDefinition> ReadInstrumentSettingsXml()
        {
            XElement root = XElement.Load(_instrumentSettingsXmlPath);
            var resultantRegisters = new List<RegisterDefinition>();
            var resultMessages = new List<string>();

            root.Descendants("Parameter").ToList().ForEach(p =>
            {
                var parameterName = p.Attribute("Name").Value;
                var dataFormat = "";

                var orderedRegisters = p.Descendants("Register").ToList()
                 .OrderBy(d => Convert.ToInt16(d.Descendants("BitOffset").FirstOrDefault().Value));

                var parameterSize = Convert.ToInt16(p.Attribute("BitSize").Value);

                var highestBit = 0;

                orderedRegisters.ToList().ForEach(r =>
                 {
                     var dataFormatAttribute = r.Attribute("DataFormat");

                     if (dataFormatAttribute != null)
                         dataFormat = dataFormatAttribute.Value;

                     var name = r.Attribute("Name").Value;

                     var currentOffset = Convert.ToInt16(r.Descendants("BitOffset").FirstOrDefault().Value);
                     var size = Convert.ToInt16(r.Descendants("BitSize").FirstOrDefault().Value);

                     var registerNameMessage = string.Format("Parameter {0} Register {1} starting at {2}", parameterName, name, currentOffset);

                     var currentHighest = currentOffset - 1 + size;

                     if (currentOffset < highestBit)
                         resultMessages.Add(registerNameMessage + "collides with other registers");

                     if (currentHighest >= parameterSize)
                         resultMessages.Add(registerNameMessage + "exceeds register size");

                     highestBit = Math.Max(highestBit, currentHighest);

                     resultantRegisters.Add(new RegisterDefinition()
                     {
                         BitOffset = currentOffset,
                         BitSize = size,
                         Name = name,
                         ParameterName = parameterName,
                         DataFormat = dataFormat
                     });
                 });
            });

            return resultantRegisters;
        }

        public List<ParameterDefinition> ReadConfigurationXml(string aircraftName, DateTime flightDate)
        {
            var configXmlContent = _smoAircraftDataProvider.GetConfigXmlContent(aircraftName, flightDate);

            return ReadConfigurationXml(configXmlContent);
        }

        public List<ParameterDefinition> ReadConfigurationXml(string configXmlContent)
        {

            //TODO default to first xml, hardcode. This make take place during initial file read (aircraft unknown);
            if (String.IsNullOrEmpty(configXmlContent))
            {
                configXmlContent = _defaultConfigXmlContent;
            }


            var registers = ReadInstrumentSettingsXml();

            XElement root = null;
            try
            {
                root = XElement.Parse(configXmlContent);
            }
            catch (Exception e)
            {
                var errorParsingContentException = new Exception("Error while parsing configXmlContent", e);
                throw errorParsingContentException;
            }

            var parameterSet = root.Descendants("ParameterSet").FirstOrDefault();

            var parameters = parameterSet.Elements().Select(e =>
            {
                var paramteterName = e.Attribute("Name").Value;
                var parameter = new ParameterDefinition()
                {
                    Name = paramteterName,
                    BaseUnit = e.Element("BaseUnit").Value,
                    SizeInBits = Convert.ToInt16(e.Element("SizeInBits").Value),
                    DataFormat = e.Element("DataFormat").Value,
                    Registers = registers.Where(r =>
                    paramteterName.ToLower().Trim()

                    .Contains(r.ParameterName.ToLower().Trim())
                    ).ToList()
                };
                return parameter;
            }).ToList();

            var Packages = root.Descendants("Packages").FirstOrDefault();
            int ID_count;
            var List_StreamID = root.Descendants("StreamID").ToList();
            var List_Content = root.Descendants("Content").ToList();
            var mappings = new List<Mapping>();
            for(int i =0; i < List_StreamID.Count(); i++)
            {
                var stream_id_num = Convert.ToInt32(List_StreamID[i].LastNode.ToString(),16);

                var mappings_new = List_Content[i].Descendants("Mapping").Select(
                m =>
                {
                    var mapping = new Mapping();
                    mapping.Name = m.Descendants("ParameterReference").FirstOrDefault().Value;
                    mapping.OffsetBytes = Convert.ToInt16(m.Descendants("Offset_Bytes").FirstOrDefault().Value);
                    mapping.Occurrences = Convert.ToInt16(m.Descendants("Occurrences").FirstOrDefault().Value);
                    mapping.StreamID = stream_id_num;

                    return mapping;
                }
                )
                .ToList();

                mappings = mappings.Concat(mappings_new).ToList();
            }

            //var StreamID = root.Descendants("StreamID").FirstOrDefault();
            //var stream_id_num = StreamID.LastNode.ToString();
            //var StreamID2 = root.Descendants("StreamID").Last();
            //var stream_id2_num = StreamID2.LastNode.ToString();
            //if (StreamID == StreamID2)
            //{
            //    ID_count = 1;
            //}
            //else { ID_count = 2; }
            //var Content = root.Descendants("Content").FirstOrDefault();
            //var Content2 = root.Descendants("Content").Last();
            //var mappings = Content.Descendants("Mapping").Select(
            //    m =>
            //        {
            //            var mapping = new Mapping();
            //            mapping.Name = m.Descendants("ParameterReference").FirstOrDefault().Value;
            //            mapping.OffsetBytes = Convert.ToInt16(m.Descendants("Offset_Bytes").FirstOrDefault().Value);
            //            mapping.Occurrences = Convert.ToInt16(m.Descendants("Occurrences").FirstOrDefault().Value);
            //            mapping.StreamID = Convert.ToInt16(stream_id_num);

            //            return mapping;
            //        }
            //    )
            //    .ToList();
            //if (ID_count == 2)
            //{
            //    var mappings2 = Content2.Descendants("Mapping").Select(
            //        m =>
            //        {
            //            var mapping2 = new Mapping();
            //            mapping2.Name = m.Descendants("ParameterReference").FirstOrDefault().Value;
            //            mapping2.OffsetBytes = Convert.ToInt16(m.Descendants("Offset_Bytes").FirstOrDefault().Value);
            //            mapping2.Occurrences = Convert.ToInt16(m.Descendants("Occurrences").FirstOrDefault().Value);
            //            mapping2.StreamID = Convert.ToInt16(stream_id2_num);
            //            return mapping2;
            //        }
            //        )
            //        .ToList();
            //    mappings = mappings.Concat(mappings2).ToList();
            //}
            //sprawdzic czy m.name sie nie powtarza
            var map_names = mappings.Select(m =>m.Name).ToList();
            var map_names2 = map_names.Distinct().ToList();
            var duplicates = map_names.GroupBy(x=>x).SelectMany(g=>g.Skip(1)).ToList();

            if (duplicates.Count != 0)
            {
                foreach (var duplicate in duplicates)
                {
                    var maps = mappings.Where(m => m.Name == duplicate).ToList();
                    maps.RemoveAt(0);
                    int i = 2;
                    foreach (var map in maps)
                    {
                        mappings.Remove(map);
                        var param = parameters.Where(p => p.Name == map.Name).ToList();
                        map.Name = map.Name + i.ToString();
                        mappings.Add(map);
                        var param_obj = new ParameterDefinition
                        {
                            BaseUnit = param[0].BaseUnit,
                            DataFormat = param[0].DataFormat,
                            Name = map.Name,
                            Occurrences = param[0].Occurrences,
                            OffsetBytes = param[0].OffsetBytes,
                            Registers = param[0].Registers,
                            SizeInBits = param[0].SizeInBits,
                            StreamID = param[0].StreamID
                            
                        };
                        //param_obj.Name = map.Name;
                        parameters.Add(param_obj);
                    }
                }
                    
            }
            parameters = mappings.Join(parameters, m => m.Name, p => p.Name, (m, p) =>
            {
                p.OffsetBytes = m.OffsetBytes;
                p.Occurrences = m.Occurrences;
                p.StreamID = m.StreamID;
                return p;
            }).ToList();



            return parameters;
        }


    }

    public interface IConfigParser
    {
        List<ParameterDefinition> ReadConfigurationXml(string configXmlContent);
        List<ParameterDefinition> ReadConfigurationXml(string aircraftName, DateTime flightDate);
    }
}
