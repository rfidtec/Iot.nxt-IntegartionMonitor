using System.Collections.Generic;

namespace IoTnxt.DigiTwin.Simulator
{
    public class Gateway1SimulatorOptions
    {
        public bool Active { get; set; } = false;

        public List<Gateway1Simulation> Simulations { get; set; } = new List<Gateway1Simulation>();
    }
}