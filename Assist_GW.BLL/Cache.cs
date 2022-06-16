using System;
using System.Collections.Generic;
using System.Threading;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Administración de información en memoria.
    /// </summary>    
    public static class Cache
    {
        public static bool IsReading;
        public static bool IsSending;
        public static bool IsGettingToken;
        public static bool NewAccount = false;
        public static DTO.Instance Instance;
        public static DTO.Configuration Config;
        public static DTO.ApiClient ApiClient;
        public static DTO.ApiToken ApiToken;

        private static List<DTO.Xml> _xmlsToSends;
        private static List<DTO.FleetVehicle> _vehicles;

        public static bool XmlsReadyToSend()
        {
            if (_xmlsToSends != null)
                if (_xmlsToSends.Count > 0)
                    return true;

            return false;
        }
        public static List<DTO.Xml> GetXmlsToSend()
        {
            return _xmlsToSends;
        }
        public static void SetXmlsInCache(List<DTO.Xml> xmls)
        {
            try
            {
                if (_xmlsToSends == null)
                {
                    _xmlsToSends = new List<DTO.Xml>();

                    foreach (var x in xmls)
                    {
                        var xml = new DTO.Xml();
                        xml = x;

                        _xmlsToSends.Add(xml);
                    }
                }
                else
                {
                    foreach (var x in xmls)
                    {
                        var xml = new DTO.Xml();
                        xml = x;

                        _xmlsToSends.Add(xml);
                    }
                }
            }
            catch (Exception ex)
            {
                Feedback.Log("Error en SetXmlsInCache: " + ex.Message, true, 2);
            }
        }
        public static void RemoveXmlsInCache()
        {
            _xmlsToSends.Clear();
        }
        public static List<int> GetAccountsInCache()
        {
            List<int> result = new List<int>();

            if(Instance != null)
            {
                if(Instance.Account != null)
                {
                    foreach(var x in Instance.Account)
                        result.Add(x.AccountId);
                }
            }

            return result;
        }
        public static List<DTO.FleetVehicle> GetVehiclesList()
        {
            return _vehicles;
        }
        public static void SetVehiclesList(List<DTO.FleetVehicle> checkList)
        {
            if (NewAccount)
                IsReading = false;

            while (IsReading)
            {
                Thread.Sleep(500);
                continue;
            }

            if (_xmlsToSends != null)
            {
                if (_xmlsToSends.Count > 0)
                {
                    DataOut.Send();
                }
            }

            IsReading = true;

            _vehicles = checkList;

            IsReading = false;
        }
    }
}
