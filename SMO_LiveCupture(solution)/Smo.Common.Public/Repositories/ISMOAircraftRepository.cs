using System;
using System.Collections.Generic;
using Smo.Common.Public.Models;

namespace Smo.Common.Public.Repositories
{
    public interface IAircraftDataProvider
    {
     
        string GetAircraftNameByMacAddress(string sourceMacAddress);

        string GetConfigXmlContent(string aircraftName, DateTime flightDate);

        string GetDefaultConfigXmlContent();

        //get xml string
        string GetScalingTableTsvContent(string aircraftName);

        List<string> GetAllAircraftNames();


    }

   
}
