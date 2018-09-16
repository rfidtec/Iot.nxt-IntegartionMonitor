﻿using System;
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

namespace IoTnxt.DigiTwin.Simulator
{
    public class Gateway1Simulator
    {
        private readonly IGatewayApi _gatewayApi;
        private readonly ILogger<Gateway1Simulator> _logger;
        private readonly IRedGreenQueueAdapter _redq;
        private readonly IOptions<Gateway1SimulatorOptions> _options;
        private readonly IEntityApi _entityApi;

        TagReadsSync tagReadsSync { get; set; }

        public Gateway1Simulator(
    IRedGreenQueueAdapter redq,
    IOptions<Gateway1SimulatorOptions> options,
    ILogger<Gateway1Simulator> logger,
    IGatewayApi gatewayApi,
    IEntityApi entityApi)
        {
            _entityApi = entityApi ?? throw new ArgumentNullException(nameof(entityApi));
            _gatewayApi = gatewayApi ?? throw new ArgumentNullException(nameof(gatewayApi));
            _redq = redq ?? throw new ArgumentNullException(nameof(redq));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentException(nameof(options));
            if (options.Value.Active)
                logger.LogInformation("GATEWAY.1 simulator active");

            tagReadsSync = new TagReadsSync(redq);


            Task.Run(InitAsync);

        }

        private void Init()
        {

        }

        private async Task InitAsync()
        {
            try
            {
                _logger.LogInformation("Waiting for connecting...");

                    if (!string.IsNullOrWhiteSpace(IotGateway.Secret))
                    {
                        _logger.LogInformation($"Registering gateway {IotGateway.GatewayId}");

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
                            await DapiContext.ExecuteAsync(username: "root", action: () => _entityApi.AssociateGatewayAndClientAsync(IotGateway.GatewayId, IotGateway.ClientId));
                        }
                    }

                    _logger.LogInformation($"Simulating gateway {IotGateway.GatewayId} started...");

                    var unused = Task.Run(async () =>
                    {
                      await tagReadsSync.SyncTagReads();
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initialization simulator");
            }
        }


        /// <summary>
        /// Added tags collection
        /// </summary>
        /// <param name="uniqueDicCode"></param>
        /// <returns></returns>
        /// 
     

       



     
    }
}