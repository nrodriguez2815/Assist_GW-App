using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Assist_GW.DAL
{
    public static class Api
    {
        public static DTO.ApiClient GetConfig(int apiAuthId)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    var api = connection.QueryFirstOrDefault<DTO.ApiClient>("GetApiAuth @ApiAuthId", new { ApiAuthId = apiAuthId });

                    if (api != null)
                    {
                        var apiClient = new DTO.ApiClient
                        {
                            Username = api.Username,
                            Password = api.Password,
                            UrlBase = api.UrlBase,
                            Actions = new List<DTO.ApiClientActions>(),
                        };

                        GetApiActions(apiClient, apiAuthId);

                        return apiClient;
                    }
                }

                throw new Exception("An error has occurred: Incorrect Api Client Id");
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred: " + ex.Message);
            }
        }
        public static DTO.ApiToken GetApiToken(int apiAuthId)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    var api = connection.QueryFirstOrDefault<DTO.ApiToken>("GetApiToken @ApiAuthId", new { ApiAuthId = apiAuthId });

                    return api;
                }

                throw new Exception("An error has occurred: Incorrect Api Client Id");
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred: " + ex.Message);
            }
        }
        public static void SetApiToken(DTO.ApiToken apiToken)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@ApiAuthId", apiToken.ApiAuthId);
                param.Add("@Token", apiToken.Token);

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    connection.Execute("SetApiToken", param, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
            }
        }

        #region Private Methods
        private static void GetApiActions(DTO.ApiClient api, int apiId)
        {
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(DTO.Properties.Settings.Default.Assist_DB))
                {
                    var actions = connection.Query<DTO.ApiClientActions>("GetApiActions @ApiAuthId", new { ApiAuthId = apiId }).ToList();

                    if (actions.Count > 0)
                    {
                        foreach (var item in actions)
                        {
                            api.Actions.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred: " + ex.Message);
            }
        }
        #endregion
    }
}
