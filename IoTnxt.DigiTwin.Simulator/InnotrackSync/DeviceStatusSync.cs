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

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class DeviceStatusSync : IoTBase
    {
        #region Properties

        List<Device> Devices { get; set; }

        #endregion

        public DeviceStatusSync(IRedGreenQueueAdapter redq) : base(redq)
        {
            Devices = new List<Device>();
           
        }

        public async Task StartDeviceStatus()
        {
            while (true)
            {
                
                try
                {
                    RefreshDeviceStatusList();
                    
                    foreach (var device in Devices)
                    {
                        //Add Heartbeat of device and send
                        //_iotObject.DeviceName = device.DeviceName;
                        //_iotObject.DeviceType = "DEVICE";
                        _iotObject.Device = device;
                        _iotObject.ObjectType = "TAGS";
                        _iotObject.Object = new DeviceObject()
                        {
                            Device = device.DeviceName,
                            Heartbeat = device.Status == "Active" ? true : false,
                            Zone = DeviceZone.GetDeviceZoneName((int)device.ID)
                        };
                        lst.Add(_iotObject.ToString());
                        //Send list to Iot Cloud Platform
                        await SendNotification(lst);
                        //Clear list
                        lst.Clear();
                        device.HostSeen = true;
                        device.Update();
                    }

                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                }
            }

        }


        private void RefreshDeviceStatusList()
        {
            //Get a list of all devices that status has changed
            QueryFilter filter = new QueryFilter("HostSeen", false, DALX.Core.FilterOperator.Equals);
            Devices = new Device().Read(filter);
        }


    }
}
