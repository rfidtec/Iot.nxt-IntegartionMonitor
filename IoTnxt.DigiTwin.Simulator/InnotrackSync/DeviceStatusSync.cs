using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using Innotrack.DeviceManager.Entities;
using DALX.Core.Sql.Filters;
using Microsoft.Extensions.Logging;
using Innotrack.DeviceManager.Data;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Innotrack.Logger;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class DeviceStatusSync : IoTBase
    {

        #region Properties

        List<Device> Devices { get; set; }

        private readonly ILogger<DeviceStatusSync> _logger;

        #endregion

        public DeviceStatusSync(IRedGreenQueueAdapter redq, ILogger<DeviceStatusSync> logger) : base(redq)
        {
            Devices = new List<Device>();
            _logger = logger;
        }

        public async Task StartDeviceStatus()
        {
            while (true)
            {
                try
                {
                    RefreshDeviceStatusList();

                    var lst = new List<(string, string, object)>();
                    foreach (var device in Devices)
                    {
                        //if (device.Status != "Active")
                        //    continue;
                        //Add Heartbeat of device and send
                        var _iotObject = new IotObject();
                        //_iotObject.Device = device;
                        _iotObject.DeviceName = device.DeviceName;
                        _iotObject.DeviceType = "DEVICE";
                        _iotObject.ObjectType = "HEARTBEAT";
                        _iotObject.Object = device.Status == "Active" ? 1 : 0;
                        lst.Add(_iotObject.ToString());
                        //Send list to Iot Cloud Platform
                    }
                    await SendNotification(lst);
                }
                catch (Exception ex)
                {
                    LoggerX.WriteErrorLog(ex);
                    _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                }
                await Task.Delay(IotGateway.DeviceHeatbeatInterval * 1000);
            }
        }


        private void RefreshDeviceStatusList()
        {
            //Get a list of all devices that status has changed
            //QueryFilter filter = new QueryFilter("HostSeen", false, DALX.Core.FilterOperator.Equals);
            Devices = new Device().Read();
        }


    }
}
