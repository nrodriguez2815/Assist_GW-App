using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Administración de conexión TCP.
    /// </summary>
    public static class DataIn
    {
        private static string _ipAddress;
        private static int _port;
        private static TcpListener _server;
        private static TcpClient _client;
        private static bool _running;
        private static DTO.Instance _instanceInfo;
        private static int _buffer;

        public static void Start(DTO.Instance instanceInfo)
        {
            try
            {
                _instanceInfo = instanceInfo;
                _ipAddress = _instanceInfo.IPAddressIn;
                _port = _instanceInfo.PortIn;
                _server = new TcpListener(IPAddress.Parse(_ipAddress), _port);
                _running = true;

                _server.Start();
                Feedback.Log("Listening on the TCP port: " + Cache.Instance.PortIn, true, 1);

                while (_running)
                {
                    _client = _server.AcceptTcpClient();
                    Stream stream = _client.GetStream();

                    _buffer = _client.ReceiveBufferSize;

                    var receiveThread = new Thread(new ParameterizedThreadStart(ManageReceivedData));
                    receiveThread.IsBackground = true;
                    receiveThread.Start(stream);
                }
            }
            catch (SocketException e)
            {
                Feedback.Log("An error has ocurred with the incoming connection: " + e.Message, true, 2);
                Thread.Sleep(5000);
                CheckConnection();
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred: " + ex.Message, true, 2);
            }
        }        

        #region Private Methods        
        private static void ManageReceivedData(object obj)
        {
            try
            {
                DateTime receivedDate = DateTime.Now;
                byte[] receivedDataByte = new byte[_buffer];
                var stream = (Stream)obj;

                stream.Read(receivedDataByte, 0, receivedDataByte.Length);
                string receivedData = Encoding.ASCII.GetString(receivedDataByte, 0, receivedDataByte.Length);
                stream.Close();

                if (DAL.Instance.CheckAccountsInCache(Cache.GetAccountsInCache()))
                {
                    List<DTO.Uplink> uplinksToSend = GetUplinkMessagesFromReceivedData(receivedData, receivedDate);

                    ProccessAndSetXml(uplinksToSend, receivedDate);
                }
                else
                {
                    Feedback.Log("A unconfigured account was found, proceeding to verificate it: ",false, 1);

                    CheckAccountsInDb(receivedData, receivedDate);
                }
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred: " + ex.Message, true, 2);
            }
        }
        private static List<DTO.Uplink> GetUplinkMessagesFromReceivedData(string receivedData, DateTime receivedDate)
        {
            var uplinkMessages = new List<DTO.Uplink>();
            string[] packages = receivedData.Split('|');
            var items = DTO.Constants.ItemType.GetNames(typeof(DTO.Constants.ItemType)).Length;

            foreach (var package in packages)
            {
                if (package.Length == 0)
                    continue;

                string[] receivedItems = package.Split(',');

                if (receivedItems.Length != items)
                    continue;

                DTO.Uplink uplinkMessage = GetUplinkMessageByItems(receivedItems);
                uplinkMessage.ReceivedDate = receivedDate;
                uplinkMessages.Add(uplinkMessage);
            }

            return uplinkMessages;
        }
        private static DTO.Uplink GetUplinkMessageByItems(string[] receivedItems)
        {
            var uplinkMessage = new DTO.Uplink();

            for (int i = 0; i < receivedItems.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        uplinkMessage.UnitId = receivedItems[i];
                        break;

                    case 1:
                        if (receivedItems[i] == null)
                        {
                            uplinkMessage.LicensePlate = uplinkMessage.UnitId;
                        }
                        else
                        {
                            uplinkMessage.LicensePlate = receivedItems[i];
                        }
                        break;

                    case 2:
                        uplinkMessage.MessageDate = GetDateTimeFromString(receivedItems[i]);
                        break;

                    case 3:
                        if (receivedItems[i] == null)
                            uplinkMessage.Latitude = "0";
                        else
                            uplinkMessage.Latitude = receivedItems[i];
                        break;

                    case 4:
                        if (receivedItems[i] == null)
                            uplinkMessage.Longitude = "0";
                        else
                            uplinkMessage.Longitude = receivedItems[i];
                        break;

                    case 5:
                        if (receivedItems[i] == null)
                            uplinkMessage.Speed = "0";
                        else
                            uplinkMessage.Speed = receivedItems[i].PadLeft(3, '0');
                        break;

                    case 6:
                        if (receivedItems[i] == null)
                            uplinkMessage.Event = "0";
                        else
                            uplinkMessage.Event = receivedItems[i];
                        break;

                    case 7:
                        if (receivedItems[i] == null)
                            uplinkMessage.MileCounter = "0";
                        else
                            uplinkMessage.MileCounter = receivedItems[i];
                        break;

                    case 8:
                        uplinkMessage.InputA = receivedItems[i];
                        break;

                    case 9:
                        uplinkMessage.InputB = receivedItems[i];
                        break;

                    case 10:
                        uplinkMessage.InputC = receivedItems[i];
                        break;

                    case 11:
                        uplinkMessage.InputD = receivedItems[i];
                        break;

                    case 12:
                        uplinkMessage.InputE = receivedItems[i];
                        break;

                    case 13:
                        uplinkMessage.InputF = receivedItems[i];
                        break;

                    case 14:
                        if (receivedItems[i] == null)
                            uplinkMessage.BackBatVoltage = "0";
                        else
                            uplinkMessage.BackBatVoltage = receivedItems[i];
                        break;
                    case 15:
                        if (receivedItems[i] == null)
                        {
                            uplinkMessage.EngineOn = "false";
                        }
                        else
                        {
                            if (receivedItems[i] == "1")
                                uplinkMessage.EngineOn = "true";
                            else
                                uplinkMessage.EngineOn = "false";
                        }
                        break;
                }
            }

            return uplinkMessage;
        }
        private static string GetDateTimeFromString(string dateTimeString)
        {
            string dateTime = null;

            if(dateTimeString != null)
            {
                string day = dateTimeString.Substring(0, 2);
                string month = dateTimeString.Substring(2, 2);
                string year = "20" + dateTimeString.Substring(4, 2);
                string hour = dateTimeString.Substring(6, 2);
                string minute = dateTimeString.Substring(8, 2);
                string second = dateTimeString.Substring(10, 2);

                dateTime = year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second;
            }

            return dateTime;
        }
        private static void CheckAccountsInDb(string receivedData, DateTime receivedDate)
        {
            try
            {
                if (Main.DbVerifications(1))
                {
                    List<DTO.Uplink> uplinksToSend = GetUplinkMessagesFromReceivedData(receivedData, receivedDate);

                    ProccessAndSetXml(uplinksToSend, receivedDate);
                }
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred: " + ex.Message, true, 2);
            }
        }
        private static void ProccessAndSetXml(List<DTO.Uplink> uplinksToSend, DateTime receivedDate)
        {
            var someLicensePlateFromTrama = uplinksToSend.First(x => x.LicensePlate != null).LicensePlate;

            var uplinkMessagesReceived = new DTO.UplinksReceived
            {
                Quantity = uplinksToSend.Count,
                Date = receivedDate,
                AccountName = CheckConfiguredAccount(someLicensePlateFromTrama),
            };

            Feedback.Log("Receiving trama with: " + uplinkMessagesReceived.Quantity.ToString() + " uplinks from " + uplinkMessagesReceived.AccountName, true, 1);

            if (uplinkMessagesReceived != null)
            {
                var processedUplinks = new List<DTO.Xml>();

                while (Cache.IsReading)
                {
                    Thread.Sleep(300);
                    continue;
                }

                Cache.IsReading = true;

                processedUplinks.AddRange(CheckUplinksFromClientDBF(uplinksToSend));

                Cache.SetXmlsInCache(processedUplinks);

                Cache.IsReading = false;
            }
        }
        private static string CheckConfiguredAccount(string licensePlateOrUnitId)
        {
            var vehicleList = Cache.GetVehiclesList();
            string result = "Account Name Not Found";

            if (vehicleList != null && licensePlateOrUnitId != null)
            {
                var vehicle = vehicleList.FirstOrDefault(x => x.LicencePlate.ToUpper().Equals(licensePlateOrUnitId.ToUpper()));
                
                if(vehicle == null)
                    vehicle = vehicleList.FirstOrDefault(x => x.UnitSysId.Equals(int.Parse(licensePlateOrUnitId)));

                if (vehicle != null)
                    result = vehicle.AccountName;
            }

            return result;
        }
        private static List<DTO.Xml> CheckUplinksFromClientDBF(List<DTO.Uplink> uplinkMessages)
        {
            var processedXmls = new List<DTO.Xml>();            

            try
            {
                foreach (var x in uplinkMessages)
                {
                    if (x.LicensePlate != null)
                    {                        
                        var trama = new DTO.Xml()
                        {
                            asset = x.LicensePlate,
                            battery = x.BackBatVoltage,
                            code = x.Event,
                            date = x.MessageDate,
                            ingnition = x.EngineOn,
                            latitude = x.Latitude,
                            longitude = x.Longitude,
                            odometer = x.MileCounter,
                            serialNumber = x.UnitId,
                            speed = x.Speed.PadLeft(3, '0'),
                            temperature = GetTemperature(x.InputA, x.InputB, x.InputC, x.InputD, x.InputE, x.InputF)
                        };

                        processedXmls.Add(trama);
                    }
                }

                return processedXmls;
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred: " + ex.Message, true, 2);

                return processedXmls;
            }
        }
        private static string GetTemperature(string a, string b, string c, string d, string e, string f)
        {
            if (a != null) return a;
            if (b != null) return b;
            if (c != null) return c;
            if (d != null) return d;
            if (e != null) return e;
            
            return f;
        }
        private static void CheckConnection()
        {
            var instanceId = DAL.Instance.GetInstance();

            if (instanceId != null)
                Start(instanceId);
        }        
        #endregion
    }
}
