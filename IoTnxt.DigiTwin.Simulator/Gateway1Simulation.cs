using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTnxt.DigiTwin.Simulator
{
    public class Gateway1Simulation
    {
        private string _gatewayId = Guid.NewGuid().ToString();
        public bool Active { get; set; } = true;

        public string GatewayId
        {
            get => _gatewayId;
            set {
                _gatewayId = value;
                if (_gatewayId == "auto" || _gatewayId == null)
                    _gatewayId = Guid.NewGuid().ToString();
            }
        }

        public string ClientId { get; set; }

        public double IntervalSeconds { get; set; } = 5;
        public double? ResetIntervalSeconds { get; set; } = 20;

        public bool Heartbeat { get; set; } = true;
        public bool Associate { get; set; } = true;
        public string Secret { get; set; }

        public int? StartIdAt { get; set; }
        public int EndIdAt { get; set; }

        public List<Gateway1SimulationProperty> Properties { get; set; } = new List<Gateway1SimulationProperty>();

        public Gateway1Simulation Clone() => JObject.FromObject(this).ToObject<Gateway1Simulation>();
    }
}