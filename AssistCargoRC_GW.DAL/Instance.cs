using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Assist_GW.DAL
{    
    public static class Instance
    {
        public static DTO.Instance GetInstance()
        {
            try
            {
                int instanceIdDB = DTO.Properties.Settings.Default.InstanceId;

                var clientAccountId = GetVerifiedAccountsInDB();

                if (clientAccountId != null)
                {
                    if(clientAccountId.Count > 0)
                    {
                        DateTime dateLog = DateTime.Now;

                        using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                        {
                            var instance = connection.QueryFirstOrDefault<DTO.Instance>("GetInstance @InstanceId", new { InstanceId = instanceIdDB });

                            if (instance != null)
                            {
                                var instanceInfo = new DTO.Instance
                                {
                                    InstanceId = instance.InstanceId,
                                    IPAddressIn = instance.IPAddressIn,
                                    PortIn = instance.PortIn,
                                    ApiAuthId = instance.ApiAuthId,
                                    Account = new List<DTO.Account>(),
                                };

                                FillAccountInformationForInstance(instanceInfo, clientAccountId);

                                return instanceInfo;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("An error has occurred: No properly configured cacheAccounts were found.");
                    }
                }

                throw new Exception("An error has occurred: Incorrect Client ID or AddressId");
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred: " + ex.Message);
            }
        }
        public static List<int> GetVerifiedAccountsInDB()
        {
            try
            {
                int instanceIdDB = DTO.Properties.Settings.Default.InstanceId;

                string query = "SELECT AccountId FROM InstanceAccount WHERE InstanceId = @instanceIdDB;";
                
                List<int> accountIds = new List<int>();

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    accountIds = connection.Query<int>(query, new { instanceIdDB }).ToList();
                }

                if (accountIds != null)
                {
                    if (accountIds.Count() > 0)
                    {
                        query = "SELECT AccountId FROM AccountsDetails WHERE AccountId = @acc;";

                        var fleetResult = new List<int>();

                        using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.ClientF_DB))
                        {
                            foreach (var acc in accountIds)
                            {
                                var result = connection.QueryFirstOrDefault<int>(query, new { acc });

                                if (result != 0)
                                    fleetResult.Add(result);
                            }
                        }

                        if (fleetResult.Except(accountIds).Count() == 0)
                            return accountIds;
                    }
                }

                accountIds.Clear();

                return accountIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static bool CheckAccountsInCache(List<int> cacheAccounts)
        {
            var instanceIdDB = DTO.Properties.Settings.Default.InstanceId;

            string query = "SELECT AccountId FROM InstanceAccount WHERE InstanceId = @instanceIdDB;";
            var dbAccounts = new List<int>();

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
            {
                dbAccounts = connection.Query<int>(query, new { instanceIdDB }).ToList();
            }

            if (cacheAccounts != null && dbAccounts != null)
            {
                if(cacheAccounts.Count > 0 && dbAccounts.Count > 0)
                {
                    var firstNotSecond = cacheAccounts.OrderBy(x => x).Except(dbAccounts).OrderBy(y => y).ToList();
                    var secondNotFirst = dbAccounts.OrderBy(x => x).Except(cacheAccounts).OrderBy(y => y).ToList();

                    if(firstNotSecond.Count == 0 && secondNotFirst.Count == 0)
                        return true;
                }
            }            

            return false;
        }

        #region Private Methods     
        private static void FillAccountInformationForInstance(DTO.Instance instance, List<int> accountIds)
        {
            try
            {
                string query = "SELECT ad.AccountId, ad.Name, av.VehicleId, v.UnitSysId, v.LicencePlate, v.ConfigurationId " +
                        "FROM AccountsDetails AS ad INNER JOIN AccountVehicles AS av " +
                        "ON ad.AccountId = av.AccountId INNER JOIN Vehicles AS v " +
                        "ON av.VehicleId = v.VehicleId WHERE ad.AccountId = @AccountId";

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.ClientF_DB))
                {
                    foreach (var accountId in accountIds)
                    {
                        var accountDB = connection.Query(query, new { AccountId = accountId }).ToList();
                        var account = new DTO.Account();

                        if (accountDB.Count > 0)
                        {
                            account.AccountId = accountId;
                            account.Name = accountDB.Find(x => x.AccountId.Equals(account.AccountId)).Name;

                            var vehicles = new List<DTO.FleetVehicle>();

                            foreach (var a in accountDB)
                            {
                                var vehicle = new DTO.FleetVehicle
                                {
                                    VehicleId = (int)a.VehicleId,
                                    UnitSysId = a.UnitSysId ?? 0,
                                    LicencePlate = a.LicencePlate,
                                    ConfigurationId = a.ConfigurationId ?? 0,
                                    AccountId = accountId,
                                    AccountName = account.Name
                                };

                                vehicles.Add(vehicle);
                            }

                            account.Vehicles = vehicles;
                            instance.Account.Add(account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
