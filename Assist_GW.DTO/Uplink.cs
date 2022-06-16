using System;
using System.Runtime.Serialization;

namespace Assist_GW.DTO
{
    ///<summary>
    ///Información recibida de ClientDBF.
    ///</summary>  
    public class Uplink
    {
        public string LicensePlate { get; set; }
        public string BackBatVoltage { get; set; }
        public string Event { get; set; }
        public string MessageDate { get; set; }
        public string EngineOn { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string MileCounter { get; set; }
        public string UnitId { get; set; }
        public string Speed { get; set; }
        public string InputA { get; set; }
        public string InputB { get; set; }
        public string InputC { get; set; }
        public string InputD { get; set; }
        public string InputE { get; set; }
        public string InputF { get; set; }

        [IgnoreDataMember]
        public string AccountName { get; set; }
        [IgnoreDataMember]
        public DateTime ReceivedDate { get; set; }
    }
}
