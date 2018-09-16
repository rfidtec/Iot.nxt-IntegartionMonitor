using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Microsoft.Extensions.Logging;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class RNCHeartbeatSync : IoTBase
    {

        public RNCHeartbeatSync(IRedGreenQueueAdapter redq) : base(redq)
        {

        }

        public async Task StartAsync()
        {

            try
            {
                _iotObject.Group = "RFID";
                _iotObject.DeviceType = "ZONE";
                _iotObject.DeviceName = IotGateway.GatewayId;
                _iotObject.ObjectType = "TAGS";
              
                while (true)
                {
                    //Check if the RNC Server is online -RNC IP Address is configurable in app.config
                    if (IsRNCAlive(IotGateway.RNCIpAddress))
                    {
                        _iotObject.Object = new DeviceObject() { Device = "RNC", Heartbeat =true }; 
                        IotGateway.Heartbeat = true;
                    }
                    else
                    {
                        _iotObject.Object = new DeviceObject() { Device = "RNC", Heartbeat = false };
                        IotGateway.Heartbeat = false;
                    }

                    lst.Add(_iotObject.ToString());
                    await SendNotification();

                    lst.Clear();

                    if(IotGateway.RNCCheckInterval > 0)
                    await Task.Delay((int)(IotGateway.RNCCheckInterval * 1000));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");

            }
        }
    }
}
