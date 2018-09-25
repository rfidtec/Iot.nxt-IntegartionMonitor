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

        public RNCHeartbeatSync(IRedGreenQueueAdapter redq) : base(redq)
        {

        }

        public async Task StartAsync()
        {

            try
            {
                _iotObject.Group = "RFID";
                _iotObject.DeviceType = "DEVICE";
                _iotObject.DeviceName = IotGateway.GatewayId;
                _iotObject.ObjectType = "HEARTBEAT";

                while (true)
                {
                    Console.WriteLine("Check RNC Heartbeat");
                    //Check if the RNC Server is online -RNC IP Address is configurable in app.config
                    if (IsRNCAlive(IotGateway.RNCIpAddress))
                    {
                        _iotObject.Object = IotGateway.Heartbeat = true;
                      
                        LoggerX.WriteEventLog($"RNC Heartbeat is alive");
                    }
                    else
                    {
                        _iotObject.Object = IotGateway.Heartbeat = false;
                        
                        LoggerX.WriteEventLog($"RNC Heartbeat is dead");

                    }
                    lst.Add(_iotObject.ToString());
                    Console.WriteLine("Attempting to send");
                    await SendNotification(lst);
                    LoggerX.WriteEventLog($"RNC Heartbeat Notification Sent");
                    lst.Clear();

                    if (IotGateway.RNCCheckInterval > 0)
                        await Task.Delay((int)(IotGateway.RNCCheckInterval * 1000));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                LoggerX.WriteErrorLog(ex);
            }
        }
    }
}
