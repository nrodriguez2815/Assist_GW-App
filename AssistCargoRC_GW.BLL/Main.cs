using System;
using System.Linq;
using System.Threading;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Estructura principal de la aplicación.
    /// </summary> 
    public static class Main
    {
        private static Timer _timerForXmlsSender;
        private static Timer _timerForVehiclesListCheck;
        private static bool _accountListModified;
        
        public static void StartUp()
        {
            _accountListModified = false;            

            Feedback.Log("Getting information from database.", true, 1);

            Helpers.GenericVerification("Unable to load local configuration. Remaining attempts: ", Configurations.GetConfigs);

            int senderProcessPeriod = Cache.Config.XmlsSenderTimer * 1000;
            int vehiclesProcessPeriod = Cache.Config.VehiclesListCheckTimer * 60000;

            CheckInstanceId();

            Helpers.GenericVerification("Unable to get access token, verify if server is running. Remaining attempts: ", DataOut.CheckTokenFromCache, null, null, "Access token has been successfully obtained!");

            if (DbVerifications(0))
            {
                Feedback.Log("Configured Api client: " + Cache.ApiClient.UrlBase, true, 1);

                foreach (var acc in Cache.Instance.Account)
                {
                    Feedback.Log("Clients configured correctly: " + acc.Name, true, 1);
                    Feedback.Log("Number of vehicles: " + acc.Vehicles.Count(), true, 1);
                }

                Feedback.Log("Vehicles list verification interval: " + (vehiclesProcessPeriod / 60000).ToString() + " minutes.", false, 1);
                Feedback.Log("Xmls sending interval: " + (senderProcessPeriod / 1000).ToString() + " seconds.", false, 1);
            }

            if (!_accountListModified)
            {
                SetTimers();
                Listener();
            }
        }
        public static void Listener()
        {
            try
            {
                DataIn.Start(Cache.Instance);
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has occurred with the Listener: " + ex.Message, true, 2);

                Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }
        public static void SendXmls()
        {
            Feedback.Log("Looking for Xmls to send.", false, 4);

            if (Cache.XmlsReadyToSend())
            {
                if (Cache.IsSending == true)
                    Feedback.Log("Another thread is currently sending. Starting a new one.", true, 4);
                else
                    DataOut.Send();
            }
            else
            {
                if (Cache.IsSending == true)
                    Feedback.Log("Another thread is currently sending, and there's no more Xmls to send.", true, 4);
                else
                    Feedback.Log("There's no Xmls to send.", true, 4);
            }
        }
        public static void CheckVehiclesList()
        {
            try
            {
                if (Configurations.CheckOrSetVehiclesList())
                {
                    var cant = Cache.GetVehiclesList().Count().ToString();
                    Feedback.Log("Looking for changes in vehicles list.", false, 4);
                    Feedback.Log("Changes were found in the vehicles list, now the application have: " + cant + " vehicles.", true, 1);
                }
                else
                {
                    Feedback.Log("Looking for changes in vehicles list.", false, 4);
                    Feedback.Log("No changes were found in the vehicles list.", true, 4);
                }
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred with vehicles list: " + ex.Message, true, 2);
            }
        }
        public static bool DbVerifications(int origin)
        {
            if (origin != 0)
            {
                while (Cache.IsSending)
                {
                    Thread.Sleep(500);
                    continue;
                }

                DisposeTimers();

                while (Cache.IsReading)
                {
                    Thread.Sleep(500);
                    continue;
                }

                Cache.IsReading = true;
                Cache.NewAccount = true;

                Feedback.Log("Looking for new information in database.", true, 1);
                Helpers.GenericVerification("Unable to load local configuration. Remaining attempts: ", Configurations.GetConfigs);
            }

            Helpers.GenericVerification("Incorrect ClientDBF Address configuration, remaining attempts: ",
                CheckClientDBFAddressConfig, null, "Assist Address configuration: \n Ip - " + Cache.Instance.IPAddressIn +
                                      "\n Port - " + Cache.Instance.PortIn + "\n Account ID - " + AccountsList()
                                      + "\n Api client url set: " + Cache.ApiClient.UrlBase);

            Helpers.GenericVerification("Incorrect ClientDBF Items configuration, ", null, VerifyClientDBFItems, null, "ClientDBF configured correctly.");

            if (origin != 0)
            {
                _accountListModified = true;
                Cache.IsReading = false;
                SetTimers();
            }

            return true;
        }

        #region Events
        private static void SenderTimerCallback(object obj)
        {
            SendXmls();
        }
        private static void CheckVehiclesList(object obj)
        {
            CheckVehiclesList();
        }
        #endregion

        #region Private Method
        private static bool CheckInstanceId()
        {
            try
            {
                if (Cache.Config.InstallationDate != null)
                {
                    Feedback.Log("Instance number: " + Cache.Config.InstanceId, false, 1);
                    Feedback.Log("Instance name: " + Cache.Config.InstanceName, true, 1);

                    return false;
                }
                else
                {
                    DAL.Configuration.SetInstance();

                    Feedback.Log("Starting the service for the first time...", false, 1);
                    Feedback.Log("Instance number: " + Cache.Config.InstanceId, false, 1);
                    Feedback.Log("Instance name: " + Cache.Config.InstanceName, true, 1);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Feedback.Log("An error has ocurred: " + ex.Message, true, 2);

                return false;
            }
        }
        private static bool CheckClientDBFAddressConfig()
        {
            return DAL.Configuration.CheckClientDBFAddress(Cache.Instance.IPAddressIn, Cache.Instance.PortIn, Cache.Instance.Account);
        }
        private static string VerifyClientDBFItems()
        {
            return DAL.Configuration.CheckClientDBFItems(DTO.Properties.Settings.Default.ProtocolId, Cache.Instance.IPAddressIn, Cache.Instance.PortIn, Cache.Instance.Account);
        }
        private static string AccountsList()
        {
            string result = null;

            foreach (var acc in Cache.Instance.Account)
            {
                result += acc.AccountId + " ";
            }

            return result.TrimEnd();
        }
        private static void SetTimers()
        {
            int senderProcessPeriod = Cache.Config.XmlsSenderTimer * 1000;
            int vehiclesProcessPeriod = Cache.Config.VehiclesListCheckTimer * 60000;

            _timerForXmlsSender = new Timer(SenderTimerCallback, null, senderProcessPeriod, senderProcessPeriod);
            _timerForVehiclesListCheck = new Timer(CheckVehiclesList, null, vehiclesProcessPeriod, vehiclesProcessPeriod);            
        }
        private static void DisposeTimers()
        {
            _timerForXmlsSender.Dispose();
            _timerForVehiclesListCheck.Dispose();
        }
        #endregion
    }
}
