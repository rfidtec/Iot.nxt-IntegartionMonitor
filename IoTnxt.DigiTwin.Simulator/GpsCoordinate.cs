using Newtonsoft.Json;

namespace IoTnxt.DigiTwin.Simulator
{
    public class GpsCoordinate
    {
        [JsonProperty("lat")]
        public double? Lat { get; set; }

        [JsonProperty("lon")]
        public double? Lon { get; set; }
    }
}
