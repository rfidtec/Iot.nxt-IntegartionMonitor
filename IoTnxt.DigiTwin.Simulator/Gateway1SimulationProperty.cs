using System;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Newtonsoft.Json.Linq;

namespace IoTnxt.DigiTwin.Simulator
{
    public class Gateway1SimulationProperty
    {
        public string DeviceId { get; set; }
        public string Property { get; set; }
        public int? StartIdAt { get; set; }
        public int EndIdAt { get; set; }

        public double PercentageNulls { get; set; }

        public string FixedValue { get; set; }
        public string StringValue { get; set; }
        public bool? BoolValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public GpsCoordinate GpsCoordinateValue { get; set; }

        public int? FromIntValue { get; set; }
        public int? ToIntValue { get; set; }

        public double? FromDoubleValue { get; set; }
        public double? ToDoubleValue { get; set; }

        public int?[] IntSequence { get; set; }
        public double?[] DoubleSequence { get; set; }
        public string[] StringSequence { get; set; }
        public bool?[] BoolSequence { get; set; }
        public DateTime?[] DateTimeSequence { get; set; }
        public GpsCoordinate[] GpsCoordinateSequence { get; set; }
        public RfidCollection RfidSequence { get; set; }

        public Gateway1SimulationProperty Clone() => JObject.FromObject(this).ToObject<Gateway1SimulationProperty>();

        public string GetDevicePropertyDataType()
        {
            return RfidSequence?.Count > 0 ? "RFIDDictionary" : null;
        }
    }
}