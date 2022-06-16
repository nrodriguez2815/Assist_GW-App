using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Administración del token y del envío de datos.
    /// </summary>
    public static class DataOut
    {        
        public static void Send()
        {
            Cache.IsSending = true;
            var retries = Cache.Config.Retries;
            string urlSoapAction = null;
            string soapAction = null;

            while (!CheckToken())
            {
                Thread.Sleep(300);
                continue;
            }

            while (Cache.IsReading)
            {
                Thread.Sleep(300);
                continue;
            }

            if (Cache.ApiClient.Actions != null)
            {
                urlSoapAction = Cache.ApiClient.Actions.FirstOrDefault(x => x.Type.Equals(DTO.Properties.Settings.Default.GPSAssetTracking)).Url;
                soapAction = Cache.ApiClient.Actions.FirstOrDefault(x => x.Type.Equals(DTO.Properties.Settings.Default.GPSAssetTracking)).Type;

                if (urlSoapAction == null || soapAction == null)
                {
                    Feedback.Log("Something went wrong, no API url or action found.", true, 2);
                    return;
                }
            }

            Cache.IsReading = true;

            string token = Cache.ApiToken.Token;
            var sortedXml = new List<DTO.Xml>();
            var sendedXml = new List<DTO.Xml>();
            int discarded = 0;

            try
            {
                foreach (var x in SortedXmlsToSend(Cache.GetXmlsToSend()))
                {
                    var xml = new DTO.Xml();
                    xml = x;
                    sortedXml.Add(xml);
                }

                Cache.RemoveXmlsInCache();
                Cache.IsReading = false;

                var total = sortedXml.Count;

                Feedback.Log("Trying to send: " + total + " Xmls.", true, 1);

                for (int x = 0; x < sortedXml.Count; x++)
                {
                    var parms = Helpers.SetAssetParameters(sortedXml[x]);
                    var request = DataOut.SendRequest(parms, urlSoapAction, soapAction, false);
                    XmlDocument xml = new XmlDocument();

                    while (request.Contains("Error001: ") && retries >= 0)
                    {
                        Thread.Sleep(300);
                        
                        if (retries == 0)
                        {
                            Feedback.Log("All send attempts failed, current xml discarded.", true, 2);
                            retries = -1;
                            break;
                        }
                        else
                        {
                            Feedback.Log("An error has ocurred trying to send to the Api.", false, 2);
                            Feedback.Log("Retrying send, remaining attempts: " + retries, true, 2);                                                       

                            request = DataOut.SendRequest(parms, urlSoapAction, soapAction, false);

                            retries--;
                        }
                    }

                    if (retries == -1)
                    {
                        retries = Cache.Config.Retries;
                        discarded++;
                        continue;
                    }                        

                    xml.LoadXml(request);
                    XmlNodeList node = xml.GetElementsByTagName(DTO.Properties.Settings.Default.GPSAssetNode);
                    
                    if (node != null)
                    {
                        if(node.Count != 0)
                        {
                            if (node[0].InnerText != "0")
                            {
                                sortedXml[x].IdJob = node[0].InnerText;
                                sendedXml.Add(sortedXml[x]);
                                retries = Cache.Config.Retries;
                            }
                            else
                            {
                                var response = xml.LastChild.FirstChild.InnerText;
                                Feedback.Log("Something went wrong: " + response, true, 2);

                                if(response.Contains(DTO.Properties.Settings.Default.ApiResponseTokenError))
                                {
                                    Cache.ApiClient = DAL.Api.GetConfig(Cache.Instance.ApiAuthId);
                                    Feedback.Log("Starting attempt to get correct authentication.", false, 4);

                                    while (!GetTokenFromClient())
                                    {
                                        Feedback.Log("Starting attempt to get correct authentication again", true, 2);
                                        Thread.Sleep(1000);
                                    }

                                    Feedback.Log("The authentication was fully succeeded.", true, 1);

                                    x--;
                                }
                            }
                        }
                    }                    
                }

                if(discarded > 0)
                {
                    Feedback.Log("Something went wrong: " + discarded + "xmls were discarded", true, 2);
                }

                if(sendedXml.Count > 0)
                {
                    InsertLicenseLog(sendedXml, soapAction, token);
                    Feedback.Log(sendedXml.Count + " xmls were sended correctly", true, 1);
                }                
            }
            catch (Exception ex)
            {
                Feedback.Log("Something went wrong: " + ex, true, 2);
            }

            Cache.IsSending = false;
        }
        public static bool CheckTokenFromCache()
        {
            if (Cache.ApiToken != null)
            {
                if (CheckToken())
                {
                    return true;
                }
                else
                {
                    if (GetTokenFromClient())
                    {
                        return true;
                    }

                    return false;
                }
            }
            else
            {
                if (GetTokenFromClient())
                {
                    return true;
                }

                return false;
            }
        }        

        #region Private Methods
        public static bool GetTokenFromClient()
        {
            var parms = new Dictionary<string, string>();
            string urlSoapAction = null;
            string soapAction = null;


            if (Cache.ApiClient.Username != "" && Cache.ApiClient.Password != "" && Cache.ApiClient.Actions.Count() > 0)
            {
                parms.Add(DTO.Properties.Settings.Default.ApiUserId, Cache.ApiClient.Username);
                parms.Add(DTO.Properties.Settings.Default.ApiUserPass, Cache.ApiClient.Password);
                
                urlSoapAction = Cache.ApiClient.Actions.FirstOrDefault(x => x.Type.Equals(DTO.Properties.Settings.Default.GetUserToken)).Url;
                soapAction = Cache.ApiClient.Actions.FirstOrDefault(x => x.Type.Equals(DTO.Properties.Settings.Default.GetUserToken)).Type;

                if (urlSoapAction == null || soapAction == null)
                {
                    Configurations.GetConfigs();
                    return false;
                }
            }                
            else
            {
                Configurations.GetConfigs();
                return false;
            }

            try
            {
                var request = DataOut.SendRequest(parms, urlSoapAction, soapAction, true);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(request);
                XmlNodeList node = xml.GetElementsByTagName(DTO.Properties.Settings.Default.TokenNode);

                if(node != null)
                {
                    if(node[0].InnerText != "")
                    {
                        var token = new DTO.ApiToken
                        {
                            ApiAuthId = Cache.Instance.ApiAuthId,
                            Token = node[0].InnerText,
                            GetDate = DateTime.Now,
                        };

                        Cache.ApiToken = token;
                        DAL.Api.SetApiToken(token);

                        return true;
                    }
                    else
                    {
                        Feedback.Log("Something went wrong: " + xml.LastChild.FirstChild.InnerText, true, 2);
                    }
                }
            }
            catch(XmlException ex)
            {
                Feedback.Log("Something went wrong: " + ex.Message, true, 2);
            }
            catch(Exception ex)
            {
                Feedback.Log("Something went wrong: " + ex.Message, true, 2);
            }

            Configurations.GetConfigs();
            return false;
        }
        private static bool CheckToken()
        {
            if (Cache.ApiToken.GetDate.AddHours(23) > DateTime.Now)
                return true;

            return false;
        }
        private static List<DTO.Xml> SortedXmlsToSend(List<DTO.Xml> xmls)
        {
            var processedXmls = new List<DTO.Xml>();
            var vehicles = new List<DTO.FleetVehicle>();

            vehicles = Cache.GetVehiclesList();

            IEnumerable<DTO.Xml> licenses = xmls.GroupBy(x => x.asset.ToString())
                                                               .Select(g => g.First());

            foreach (var license in licenses)
            {
                var licensesOk = vehicles.FindAll(x => x.LicencePlate.Equals(license.asset.TrimEnd())).ToList();

                if (licensesOk != null && licensesOk.Count > 0)
                {
                    foreach (var licenseOk in licensesOk)
                    {
                        var result = xmls.FindAll(x => x.asset.TrimEnd().Equals(licenseOk.LicencePlate)).OrderBy(f => f.date).ToList();

                        if (result != null && result.Count > 0)
                        {
                            var trama = new List<DTO.Xml>();
                            trama = result;
                            processedXmls.AddRange(trama);
                        }
                    }
                }
            }

            return processedXmls;
        }
        private static void InsertLicenseLog(List<DTO.Xml> xmls, string soapAction, string token)
        {
             IEnumerable<DTO.Xml> licenses = xmls.GroupBy(x => x.asset).Select(g => g.First());

             foreach (var license in licenses)
             {
                var result = new List<DTO.Xml>();
                var xml = new DTO.Xml();

                if (DAL.LogInDB.CheckLicenseLog(license.asset.TrimEnd()))
                {
                    result = xmls.FindAll(x => x.asset.Equals(license.asset)).OrderBy(f => (f.date)).ToList();
                    xml = result.Last();
                }
                else
                {
                    result = xmls.FindAll(x => x.asset.Equals(license.asset)).OrderBy(f => f.date).ToList();
                    xml = result.First();          
                }

                var raw = XmlModelToString(xml);
                var vehicles = Cache.GetVehiclesList();
                var accountName = vehicles.Find(x => x.LicencePlate.Equals(xml.asset)).AccountName;

                DAL.LogInDB.InsertUplinkLog(Cache.Instance.InstanceId, xml.asset, raw, Cache.Config.InstanceName, accountName, xml.IdJob);
             }
        }
        private static string SendRequest(Dictionary<string, string> parameters, string urlSoapAction, string soapAction, bool isForToken)
        {
            /// <summary>
            /// Sends a custom sync SOAP request to given URL and receive a request
            /// </summary>           
            /// <param name="parameters">A dictionary containing the parameters in a key-value fashion</param>
            /// <param name="urlSoapAction">The SOAPAction value, as specified in the Web Service's WSDL (or NULL to use the url parameter)</param>            
            /// <param name="soapAction">The WebService action name</param>
            /// <param name="isForToken">Define el contenido del request</param>
            /// <returns>A string containing the raw Web Service response</returns>
            string url = null;
            string token = null;
            string result = null;

            if (Cache.ApiClient != null)
                if (Cache.ApiClient.UrlBase != null)
                    url = Cache.ApiClient.UrlBase;

            if (Cache.ApiToken != null)
                if (Cache.ApiToken.Token != null)
                    token = Cache.ApiToken.Token;

            XmlDocument soapEnvelopeXml = new XmlDocument();

            var xmlStr = Helpers.XmlToString(parameters, soapAction, token, isForToken);

            if(xmlStr != null)
            {
                try
                {
                    soapEnvelopeXml.LoadXml(xmlStr);

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Headers.Add("SOAPAction", urlSoapAction);
                    webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    webRequest.Accept = "text/xml";
                    webRequest.Method = "POST";

                    using (Stream stream = webRequest.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }

                    using (WebResponse response = webRequest.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            result = rd.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    return "Error001: " + ex.Message;
                }
                catch (Exception ex)
                {
                    return "Error001: " + ex.Message;
                }
            }

            return result;
        }
        private static string XmlModelToString(DTO.Xml xml)
        {
            string result = "";

            Type _type = xml.GetType();
            System.Reflection.PropertyInfo[] listaPropiedades = _type.GetProperties();

            foreach (System.Reflection.PropertyInfo propiedad in listaPropiedades)
            {
                if(!propiedad.Name.Equals("IdJob"))
                {
                    result += propiedad.Name + ":" + propiedad.GetValue(xml, null) + " ";
                }
            }

            return result;
        }
        #endregion
    }
}
