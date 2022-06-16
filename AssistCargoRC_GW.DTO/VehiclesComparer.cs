using System;
using System.Collections.Generic;

namespace Assist_GW.DTO
{
    public class VehiclesComparer : IEqualityComparer<DTO.FleetVehicle>
    {
        public bool Equals(FleetVehicle x, FleetVehicle y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.VehicleId == y.VehicleId &&
                    x.LicencePlate == y.LicencePlate &&
                        x.AccountId == y.AccountId;
        }
        public int GetHashCode(FleetVehicle obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashVehicleLicense = obj.LicencePlate == null ? 0 : obj.LicencePlate.GetHashCode();

            int hashPVehicleId = obj.VehicleId.GetHashCode();

            int hashPAccountId = obj.AccountId.GetHashCode();

            return hashVehicleLicense ^ hashPVehicleId ^ hashPAccountId;
        }
    }
}
