using Assist_GW.DTO;
using System.Collections.Generic;
using System.Linq;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Administración de la configuración de la instancia.
    /// </summary>
    public static class Configurations
    {
        public static bool GetConfigs()
        {
            var vehicles = new List<DTO.FleetVehicle>();

            try
            {
                Cache.Instance = DAL.Instance.GetInstance();

                Cache.Config = DAL.Configuration.GetConfig(Cache.Instance.InstanceId);

                Cache.ApiClient = DAL.Api.GetConfig(Cache.Instance.ApiAuthId);

                Cache.ApiToken = DAL.Api.GetApiToken(Cache.Instance.ApiAuthId);

                foreach (var acc in Cache.Instance.Account)
                {
                    vehicles.AddRange(acc.Vehicles);
                }

                if (vehicles.Count == 0)
                    return false;                

                Cache.SetVehiclesList(vehicles);

                if (Cache.Instance != null && Cache.Config != null && Cache.ApiClient != null && Cache.ApiClient.Actions != null)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
        public static bool CheckOrSetVehiclesList()
        {
            var cacheVehicles = new List<DTO.FleetVehicle>();
            var checkList = new List<DTO.FleetVehicle>();
            cacheVehicles = Cache.GetVehiclesList();

            foreach (var acc in Cache.Instance.Account)
            {
                checkList.AddRange(DAL.Configuration.GetVehiclesList(acc.AccountId));
            }

            var flag = checkList.Except(cacheVehicles, new VehiclesComparer()).Count();
            flag += cacheVehicles.Except(checkList, new VehiclesComparer()).Count();

            if (flag > 0)
            {
                Cache.SetVehiclesList(checkList);
                return true;
            }

            return false;
        }
    }
}
