using System.Collections.Generic;

namespace Assist_GW.DTO
{
    public class Account
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public List<FleetVehicle> Vehicles { get; set; }
    }
}
