using IoTnxt.Common.Bootstrap;
using IoTnxt.Common.Bootstrap.WindowsService;
using IoTnxt.Data.DbHelper;
using IoTnxt.Data.DbHelper.Abstractions;
using IoTnxt.DAPI.Abstractions;
using IoTnxt.DAPI.Base;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DAPI.RedGreenQueue.Adapter;
using IoTnxt.DAPI.RedGreenQueue.Extensions;
using IoTnxt.DAPI.RedGreenQueue.Proxy;
using IoTnxt.Entity.API.Abstractions;
using IoTnxt.Gateway.API.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoTnxt.DigiTwin.Simulator
{
    public class MicroServiceStartup : DefaultWindowsMicroserviceStartup
    {
        //private IConfiguration configuration = null;

        public override void ConfigurePicoServices(IServiceCollection services)
        {
            base.ConfigurePicoServices(services);

            services.AddSingleton<IMicroServiceRunAs, RunAsService>();
            services.AddSingleton<IAutoStartup, AutoStartup>();
            services.AddSingleton<IDbHelperProvider, DbHelperProvider>();
            services.AddSingleton<Gateway1Simulator>();
            services.AddDapiRedGreenQueue(configuration);

            services.AddDapiRedGreenQueueProxy()
                .AddSingletonProxy<IGatewayApi>()
                .AddSingletonProxy<IEntityApi>();
            
            services.AddDapiRedGreenQueueServer().AddSingletonController<IDapiStatusService, DapiStatusService>();
            services.AddDapiStatusService();

            services.Configure<RedGreenQueueAdapterOptions>(configuration.GetSection(nameof(RedGreenQueueAdapterOptions)));
            services.Configure<SimulatorOptions>(configuration.GetSection(nameof(SimulatorOptions)));
            services.Configure<DbHelperProviderOptions>(configuration.GetSection(nameof(DbHelperProviderOptions)));
            services.Configure<Gateway1SimulatorOptions>(configuration.GetSection(nameof(Gateway1SimulatorOptions)));
            services.Configure<DapiRedGreenQueueProxyOptions>(configuration.GetSection(nameof(DapiRedGreenQueueProxyOptions)));
        }

        public MicroServiceStartup(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
