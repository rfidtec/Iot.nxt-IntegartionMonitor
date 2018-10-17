using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTnxt.DAPI.Base;
using IoTnxt.DAPI.RedGreenQueue.Adapter;
using IoTnxt.Entity.API.Abstractions;
using IoTnxt.Gateway.API.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using DALX.Core.Sql.Filters;
using DALX.Core;
using System.Configuration;
using IoTnxt.DigiTwin.Simulator.InnotrackSync;
using Innotrack.Logger;
using System.Windows.Forms;
using System.Threading;
using IoTnxt.Common.Bootstrap;

namespace IoTnxt.DigiTwin.Simulator
{
    public class Gateway1Simulator: IAutoStartup
    {
        private readonly IGatewayApi _gatewayApi;
        private readonly ILogger<Gateway1Simulator> _logger;
        private readonly IRedGreenQueueAdapter _redq;
        private readonly IOptions<Gateway1SimulatorOptions> _options;
        private readonly IEntityApi _entityApi;
        private LoggerX LoggerX { get; set; }
        private bool connected = false;

        TagReadsSync _tagReadsSync { get; set; }
        DeviceStatusSync _devicestatusSync { get; set; }
        RNCHeartbeatSync _RNCHeartbeatSync { get; set; }
        UnreadRFIDSync _unreadRFIDSync { get; set; }
        KeyFrameSync _keyFrameSync { get; set; }

        public Gateway1Simulator(
    IRedGreenQueueAdapter redq,
    IOptions<Gateway1SimulatorOptions> options,
    ILogger<Gateway1Simulator> logger,
    IGatewayApi gatewayApi,
    IEntityApi entityApi,
    TagReadsSync tagReadsSync,
    DeviceStatusSync devicestatusSync,
    RNCHeartbeatSync RNCHeartbeatSync,
    UnreadRFIDSync unreadRFIDSync,
    KeyFrameSync keyFrameSync,
    LoggerX loggerX
    )
        {
            _entityApi = entityApi ?? throw new ArgumentNullException(nameof(entityApi));
            _gatewayApi = gatewayApi ?? throw new ArgumentNullException(nameof(gatewayApi));
            _redq = redq ?? throw new ArgumentNullException(nameof(redq));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentException(nameof(options));
            if (options.Value.Active)
                logger.LogInformation("GATEWAY.1 simulator active");

            //Init all Innotrack Sync Classes
            _tagReadsSync = tagReadsSync;
            _devicestatusSync = devicestatusSync;
            _RNCHeartbeatSync = RNCHeartbeatSync;
            _unreadRFIDSync = unreadRFIDSync;
            _keyFrameSync = keyFrameSync;
            LoggerX = loggerX;

            

        }

        private void RunIntegration()
        {

           
             Task.Run(InitAsync);
            
           

        }

        public async Task StartTagReads()
        {
            await InitAsync();

            await Task.Run(() => _tagReadsSync.StartTagReads());
        }

        public async Task StartRNCHeartBeat()
        {
            await InitAsync();

           await Task.Run(() => _RNCHeartbeatSync.StartAsync());
        }

        public async Task InitAsync()
        {
            try
            {
                _logger.LogInformation("Waiting for connecting...");
                LoggerX.WriteEventLog("Waiting for connecting...");

                if (!string.IsNullOrWhiteSpace(IotGateway.Secret))
                {
                    _logger.LogInformation($"Registering gateway {IotGateway.GatewayId}");
                    LoggerX.WriteEventLog($"Registering gateway {IotGateway.GatewayId}");


                    var gw = new Gateway.API.Abstractions.Gateway
                    {
                        GatewayId = IotGateway.GatewayId,
                        Secret = IotGateway.Secret,
                        Make = "Innotrack",
                        Model = "Vodaworld-RNC",
                        FirmwareVersion = typeof(Gateway1Simulation).Assembly.GetName().Version.ToString(),
                        Devices = new Dictionary<string, Device>()
                    };
                    gw.Devices = IotGateway.GetIotDevices();
                    // gw.Devices.Add("RFID|1:ZONE|ZONE1", new Device() { DeviceName = "RFID|1:ZONE|ZONE1", DeviceType = "ZONE", Properties = new Dictionary<string, DeviceProperty>() { { "TAGS", new DeviceProperty() { PropertyName = "TAGS", DataType = "RFIDDictionary" } } } });

                    await DapiContext.ExecuteAsync(username: IotGateway.UserName, action: () => _gatewayApi.RegisterGatewayFromGatewayAsync(gw));

                    if (!string.IsNullOrWhiteSpace(IotGateway.ClientId))
                    {
                        _logger.LogInformation($"Associating gateway {IotGateway.GatewayId} with client {IotGateway.ClientId}");
                        LoggerX.WriteEventLog($"Associating gateway {IotGateway.GatewayId} with client {IotGateway.ClientId}");
                        await DapiContext.ExecuteAsync(username: "root", action: () => _entityApi.AssociateGatewayAndClientAsync(IotGateway.GatewayId, IotGateway.ClientId));
                    }
                }
                connected = true;
                _logger.LogInformation($"Simulating gateway {IotGateway.GatewayId} started...");
                LoggerX.WriteEventLog($"Simulating gateway {IotGateway.GatewayId} started...");

                StartIntegration();
            }
            catch (Exception ex)
            {
                connected = false;
                _logger.LogError(ex, "Error initialization simulator");
            }
        }

        /// <summary>
        /// Added tags collection
        /// </summary>
        /// <param name="uniqueDicCode"></param>
        /// <returns></returns>
        /// 
        private void StartIntegration()
        {
            new Thread(() =>
            {
                Task.Run(() => _tagReadsSync.StartTagReads());
            }).Start();
            LoggerX.WriteEventLog("Tag Read Monitor has started...");
            new Thread(() =>
            {
                Task.Run(() => _RNCHeartbeatSync.StartAsync());
            }).Start();
            LoggerX.WriteEventLog("RNC Heartbeat Monitor started...");
            new Thread(() =>
            {
                Task.Run(() => _devicestatusSync.StartDeviceStatus());
            }).Start();
            Task.Run(() => _devicestatusSync.StartDeviceStatus());
            LoggerX.WriteEventLog("Device Status Monitor started...");
            new Thread(() =>
            {
                Task.Run(() => _unreadRFIDSync.StartUnSeenMonitor());
            }).Start();
            LoggerX.WriteEventLog("Unread list Monitor started...");
            new Thread(() =>
            {
                Task.Run(() => _keyFrameSync.StartKeyFrameMonitor());
            }).Start();
            LoggerX.WriteEventLog("Keyframe sending started...");
        }

        public Task StartAsync()
        {
            return Task.Run(() => RunIntegration());
        }

        public void Dispose()
        {
             
        }
    }
}
