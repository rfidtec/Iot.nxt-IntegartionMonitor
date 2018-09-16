using Newtonsoft.Json;

namespace IoTnxt.DigiTwin.Simulator.Collection_Property
{
    public class RfidTag
    {
        [JsonProperty("rssi")]
        public long Rssi { get; set; }
        [JsonProperty("rfid")]
        public string Rfid { get; set; }
        [JsonProperty("dateseen")]
        public string DateSeen { get; set; }
        [JsonProperty("device")]
        public string device { get; set; }


        public override bool Equals(object obj)
        {
            if (obj is RfidTag tag)
                return Rssi == tag.Rssi;
            

            return false;
        }

        public override int GetHashCode() => -1;
    }
}
