using Dapper;
using System;
using System.Data;

namespace Assist_GW.DAL
{
    public static class LogInDB
    {
        public static void InsertFeedback(int instanceId, string msg, int typeId, string instanceName)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@InstanceId", instanceId);
                param.Add("@Message", msg);
                param.Add("@InstanceName", instanceName);
                param.Add("@TypeId", typeId);

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    connection.Execute("FeedbackLogInsert", param, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
            }
        }
        public static void InsertUplinkLog(int instanceId, string license, string raw, string instance, string account, string remarks)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@InstanceId", instanceId);
                    param.Add("@LicensePlate", license);
                    param.Add("@Instance", instance);
                    param.Add("@Account", account);
                    param.Add("@Trama", raw);
                    param.Add("@Remarks", remarks);

                    connection.Execute("InsertOrUpdateUplinkLog", param, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred: " + ex.Message);
            }
        }
        public static bool CheckLicenseLog(string license)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@License", license);

                var result = connection.Query<string>("GetLicenseNumber", param, commandType: CommandType.StoredProcedure);

                if (result != null)
                    return true;

                return false;
            }
        }
    }
}
