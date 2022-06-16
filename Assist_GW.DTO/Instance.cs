using System.Collections.Generic;

namespace Assist_GW.DTO
{
    public class Instance
    {
        public int InstanceId { get; set; }
        public string IPAddressIn { get; set; }
        public int PortIn { get; set; }
        public int ApiAuthId { get; set; }
        public List<Account> Account { get; set; }
    }
}
