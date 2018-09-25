using IoTnxt.Common.Bootstrap;
using System;
using System.Windows.Forms;

namespace IoTnxt.DigiTwin.Simulator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
           // Application.EnableVisualStyles();
            BuildMicroServiceHost(args).Run();

            
        }

        public static IMicroServiceHost BuildMicroServiceHost(string[] args) =>
            MicroServiceHost.CreateDefaultBuilder(args)
                .UseStartup<MicroServiceStartup>()
                .Build();
    }
}
