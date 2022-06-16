using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Assist_GW.DAL
{
    public static class Configuration
    {
        public static DTO.Configuration GetConfig(int instanceId)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    var config = connection.QueryFirstOrDefault<DTO.Configuration>("GetConfiguration @InstanceId", new { InstanceId = instanceId });

                    return config;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static bool CheckClientDBFAddress(string ip, int port, List<DTO.Account> accounts)
        {
            try
            {
                int aux = 0;

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.ClientDBF_DB))
                {
                    var query = "SELECT COUNT(AccountId) FROM Address WITH (NOLOCK)" +
                        " WHERE IPAddress = @IPAddress AND Port = @Port AND AccountId = @AccountId AND ProtocolId = " + DTO.Properties.Settings.Default.ProtocolId + " AND Enabled = 1";

                    foreach (var acc in accounts)
                    {
                        var dbfConfig = connection.ExecuteScalar<int>(query, new { IPAddress = ip, Port = port, AccountId = acc.AccountId });

                        if (dbfConfig == 0)
                            return false;

                        aux++;
                    }

                    if (aux != accounts.Count())
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string CheckClientDBFItems(int protocol, string ip, int port, List<DTO.Account> accountIds)
        {
            try
            {
                var dbfItems = new List<int>();
                var reqItems = new List<int>();
                string result = "Account: ";

                foreach (var x in Enum.GetValues(typeof(DTO.Constants.ItemType)))
                {
                    reqItems.Add((int)x);
                }

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.ClientDBF_DB))
                {
                    foreach (var acc in accountIds)
                    {
                        DynamicParameters param = new DynamicParameters();

                        param.Add("@Protocol", protocol);
                        param.Add("@IPAddress", ip);
                        param.Add("@Port", port);
                        param.Add("@AccountId", acc.AccountId);
                        param.Add("@Enable", (int)1);

                        var query = "SELECT ai.ItemTypeId FROM Address AS a " +
                            "INNER JOIN AddressItem AS ai ON a.AddressId = ai.AddressId " +
                            "WHERE a.ProtocolId = @Protocol and a.IPAddress = @IPAddress and " +
                            "a.Port = @Port and a.AccountId = @AccountId and a.Enabled = @Enable";

                        dbfItems = connection.Query<int>(query, param).ToList();

                        var aux = reqItems.Except(dbfItems);

                        if (aux.Count() != 0)
                        {
                            result += acc.Name + "\n Items Ids not found: ";

                            foreach (var x in aux)
                                result += x + " ";
                        }

                        var aux2 = dbfItems.Except(reqItems);

                        if (aux2.Count() != 0)
                        {
                            if (result.Equals("Account: "))
                                result += acc.Name;

                            result += "\n Unnecessary Items Ids found: ";

                            foreach (var y in aux2)
                                result += y + " ";
                        }
                    }

                    return result.TrimEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static void SetInstance()
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@InstanceId", DTO.Properties.Settings.Default.InstanceId);

                    connection.Execute("SetInstance", param, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static List<DTO.FleetVehicle> GetVehiclesList(int accountId)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.ClientF_DB))
                {
                    var query = "SELECT av.VehicleId, v.LicencePlate, v.UnitSysId, v.ConfigurationId, ad.Name " +
                                "FROM AccountsDetails AS ad INNER JOIN AccountVehicles AS av " +
                                "ON ad.AccountId = av.AccountId INNER JOIN Vehicles AS v " +
                                "ON av.VehicleId = v.VehicleId WHERE ad.AccountId = @AccountId";

                    var config = new List<DTO.FleetVehicle>();

                    var result = connection.Query<DTO.FleetVehicle>(query, new { AccountId = accountId }).ToList();

                    foreach (var v in result)
                    {
                        var vehicle = new DTO.FleetVehicle
                        {
                            VehicleId = v.VehicleId,
                            LicencePlate = v.LicencePlate,
                            UnitSysId = v.UnitSysId,
                            ConfigurationId = v.ConfigurationId,
                            AccountId = accountId,
                            AccountName = v.AccountName
                        };

                        config.Add(vehicle);
                    }

                    return config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error with vehicles list has occurred: " + ex.Message);
                throw ex;
            }
        }
    }
}
