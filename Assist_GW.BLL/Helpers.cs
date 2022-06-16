using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Complementos requeridos por más de uan clase.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Verificación genérica de una funciones.
        /// </summary>
        /// <param name="msg">Leyenda para devolución en caso de error y muestra de intentos restantes.</param>
        /// <param name="funcForCheck">Función simple a verificar.</param>
        /// <param name="funcForCheck2">Verifica los items configurados en ClientDBF.</param>
        /// <param name="msg2">Mensaje opcional</param>
        /// <param name="okMsg">Mensaje de verificación exitosa.</param>
        public static void GenericVerification(string msg, Func<bool> funcForCheck = null, Func<string> funcForCheck2 = null, string msg2 = null, string okMsg = null)
        {
            int retriesDefault = DTO.Properties.Settings.Default.Retries;

            if (Cache.Config.Retries > 0)
                retriesDefault = Cache.Config.Retries;

            var retries = retriesDefault;

            if (funcForCheck2 != null)
            {
                var itemsDBF = funcForCheck2();

                while (!itemsDBF.Equals("Account:") && retries >= 0)
                {
                    if (retries == 0)
                    {
                        Console.Write("\n");
                        Feedback.Log("All verification attempts failed", true, 2);

                        Thread.Sleep(30000);
                        Environment.Exit(0);
                    }

                    Feedback.Log(msg + itemsDBF + ", \n Remaining attempts: " + retries.ToString(), true, 2);

                    if (msg2 != null)
                        Feedback.Log(msg2, true, 1);

                    retries--;

                    Thread.Sleep(30000);

                    itemsDBF = funcForCheck2();
                    continue;
                }
            }
            else
            {
                while (!funcForCheck() && retries >= 0)
                {
                    if (retries == 0)
                    {
                        Console.Write("\n");
                        Feedback.Log("All verification attempts failed", true, 2);

                        Thread.Sleep(6000);
                        Environment.Exit(0);
                    }

                    Console.Write("\n");
                    Feedback.Log(msg + retries.ToString(),false, 2);

                    if (msg2 != null)
                        Feedback.Log(msg2, true, 4);

                    retries--;

                    Thread.Sleep(6000);
                    continue;
                }
            }

            if (Cache.Config.Retries == 0)
                Cache.Config.Retries = retriesDefault;

            if (okMsg != null)
                Feedback.Log(okMsg, true, 1);
        }        
        public static string XmlToString(Dictionary<string, string> parameters, string soapAction, string token, bool isForToken)
        {
            string result = (isForToken)
                ? @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/""> 
                                <soapenv:Header/>                        
                                <soapenv:Body>
                                    <{0} xmlns=""{1}"">{2}</{0}>
                                </soapenv:Body>
                            </soapenv:Envelope>"
                : @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:iron=""http://schemas.datacontract.org/2004/07/IronTracking""> 
                                <soapenv:Header/>                        
                                <soapenv:Body>
                                    <tem:GPSAssetTracking>
                                        <!--Optional:-->
                                            <tem:token>{3}</tem:token>
                                        <!--Optional:-->
                                        <tem:events>
                                            <!--Zero or more repetitions:-->
                                            <{0} xmlns=""{1}"">{2}</{0}>
                                        </tem:events>
                                    </tem:GPSAssetTracking>
                                </soapenv:Body>
                            </soapenv:Envelope>";

                string parms = string.Join(string.Empty, parameters.Select(kv => String.Format("<{0}>{1}</{0}>", kv.Key, kv.Value)).ToArray());
                result = (isForToken)
                    ? String.Format(result, "tem:" + soapAction, "/", parms)
                    : String.Format(result, "iron:" + soapAction, "/", parms, token);           
            
            return result;
        }
        public static Dictionary<string, string> SetAssetParameters(DTO.Xml xml)
        {
            var result = new Dictionary<string, string>();

            result.Add("iron:asset", xml.asset);
            result.Add("iron:battery", xml.battery);
            result.Add("iron:code", xml.code);
            result.Add("iron:date", xml.date);
            result.Add("iron:ignition", xml.ingnition);
            result.Add("iron:latitude", xml.latitude);
            result.Add("iron:longitude", xml.longitude);
            result.Add("iron:odometer", xml.odometer);
            result.Add("iron:serialNumber", xml.serialNumber);
            result.Add("iron:speed", xml.speed);
            result.Add("iron:temperature", xml.temperature);

            return result;
        }
    }
}
