using System;

namespace Assist_GW.DTO
{
    public class Configuration
    {
        public int InstanceId { get; set; }
        public int VehiclesListCheckTimer { get; set; }
        public int XmlsSenderTimer { get; set; }
        public int LogTypeId { get; set; }
        public string Description { get; set; }
        public bool ActivityLog { get; set; }
        public string LogPath { get; set; }
        public int Retries { get; set; }
        public string InstanceName { get; set; }
        public DateTime? InstallationDate { get; set; }
    }
}
