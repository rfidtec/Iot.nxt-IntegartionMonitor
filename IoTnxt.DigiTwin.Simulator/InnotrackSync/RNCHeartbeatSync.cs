using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innotrack.Logger;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Microsoft.Extensions.Logging;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class RNCHeartbeatSync : IoTBase
    {
        private readonly ILogger<RNCHeartbeatSync> _logger;

        public RNCHeartbeatSync(IRedGreenQueueAdapter redq, ILogger<RNCHeartbeatSync> logger,LoggerX loggerX) : base(redq,loggerX)
        {
            _logger = logger;
        }

        public async Task StartAsync()
        {
            try
            {
                var _iotObject = new IotObject();
                _iotObject.Group = "RFID";
                _iotObject.DeviceType = "RNC";
                _iotObject.DeviceName = IotGateway.GatewayId;
                _iotObject.ObjectType = "HEARTBEAT";

                while (true)
                {
                    await UpdateLastCheckedLog(InterfaceType.rncheartbeat);
                    var lst = new List<(string, string, object)>();
                    Console.WriteLine("Check RNC Heartbeat");
                    //Check if the RNC Server is online -RNC IP Address is configurable in app.config

                    if (await IsRNCAlive(IotGateway.RNCIpAddress))
                    {
                        _iotObject.Object = 1;
                        IotGateway.Heartbeat = true;
                        LoggerX.WriteEventLog($"RNC Heartbeat is alive");
                    }
                    else
                    {
                        _iotObject.Object = 0;
                        IotGateway.Heartbeat = false;
                        LoggerX.WriteEventLog($"RNC Heartbeat is dead");

                    }
                    lst.Add(_iotObject.ToString());
                    Console.WriteLine("Attempting to send " + DateTime.Now.ToString());
                    await SendNotification(lst);
                    await UpdateLastUpdatedLog(InterfaceType.rncheartbeat);
                    LoggerX.WriteEventLog($"RNC Heartbeat Notification Sent ");

                    if (IotGateway.RNCCheckInterval > 0)
                        await Task.Delay((int)(IotGateway.RNCCheckInterval * 1000));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LoggerX.WriteErrorLog(ex);
                _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
            }
        }
    }
}
