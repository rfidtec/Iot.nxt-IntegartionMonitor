using IoTnxt.Common.Bootstrap;

namespace IoTnxt.DigiTwin.Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildMicroServiceHost(args).Run();
        }

        public static IMicroServiceHost BuildMicroServiceHost(string[] args) =>
            MicroServiceHost.CreateDefaultBuilder(args)
                .UseStartup<MicroServiceStartup>()
                .Build();
    }
}
