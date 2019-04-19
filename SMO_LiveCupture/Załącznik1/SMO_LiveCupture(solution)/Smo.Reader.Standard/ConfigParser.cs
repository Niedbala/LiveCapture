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
    }



    public class ConfigParser : IConfigParser
    {
        private readonly IGlobalPaths _globalPaths;

        IAircraftDataProvider _smoAircraftDataProvider { get; set; }
        string _instrumentSettingsXmlPath { get; set; }

        string _defaultConfigXmlContent { get; set; }

        public ConfigParser(IAircraftDataProvider smoAircraftDataProvider, IGlobalPaths globalPaths)
        {
            _smoAircraftDataProvider = smoAircraftDataProvider;
            this._globalPaths = globalPaths;
            _instrumentSettingsXmlPath = _globalPaths.InstrumentSettingsXml;
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
            var Content = root.Descendants("Content").FirstOrDefault();
            var mappings = Content.Descendants("Mapping").Select(
                m =>
                    {
                        var mapping = new Mapping();
                        mapping.Name = m.Descendants("ParameterReference").FirstOrDefault().Value;
                        mapping.OffsetBytes = Convert.ToInt16(m.Descendants("Offset_Bytes").FirstOrDefault().Value);
                        return mapping;
                    }
                )
                .ToList();

            parameters = mappings.Join(parameters, m => m.Name, p => p.Name, (m, p) =>
                {
                    p.OffsetBytes = m.OffsetBytes;
                    return p;
                }).ToList();

            return parameters;
        }


    }

    public interface IConfigParser
    {
        List<RegisterDefinition> ReadInstrumentSettingsXml();
        List<ParameterDefinition> ReadConfigurationXml(string aircraftName, DateTime flightDate);
    }
}
