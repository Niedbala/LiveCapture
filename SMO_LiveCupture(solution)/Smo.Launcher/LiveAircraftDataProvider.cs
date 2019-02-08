using Smo.Common.Public.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Launcher
{
    class LiveAircraftDataProvider : IAircraftDataProvider
    {
        public string _aircraftName { get; set; }
        public string _configXmlContent { get; set; }
        
        public string _scalingTsvContent { get; set; }

        public LiveAircraftDataProvider(string aircraftName, string configXmlPath, string scalingTsvPath)
        {
            _aircraftName = aircraftName;
            _configXmlContent = File.ReadAllText(configXmlPath);
            
            _scalingTsvContent = File.ReadAllText(scalingTsvPath);
        }

        public string GetAircraftNameByMacAddress(string sourceMacAddress)
        {
            return _aircraftName;
        }

        public List<string> GetAllAircraftNames()
        {
            return new List<string>() { _aircraftName };
        }

        public string GetConfigXmlContent(string aircraftName, DateTime flightDate)
        {
            return _configXmlContent;
        }

        public string GetDefaultConfigXmlContent()
        {
            return _configXmlContent;
        }

        public string GetScalingTableTsvContent(string aircraftName)
        {
            return _scalingTsvContent;
        }
    }
}
