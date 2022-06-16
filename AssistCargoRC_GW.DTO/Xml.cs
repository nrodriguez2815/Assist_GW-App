using System.Runtime.Serialization;

namespace Assist_GW.DTO
{
    public class Xml
    {
        public string asset { get; set; }
        public string battery { get; set; }
        public string code { get; set; }
        public string date { get; set; }
        public string ingnition { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string odometer { get; set; }
        public string serialNumber { get; set; }
        public string speed { get; set; }
        public string temperature { get; set; }

        [IgnoreDataMember]
        public string IdJob { get; set; }
    }
}
