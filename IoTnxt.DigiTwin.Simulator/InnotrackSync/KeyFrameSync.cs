using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using Innotrack.DeviceManager.Entities;
using Newtonsoft.Json.Linq;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using DALX.Core.Sql.Filters;
using DALX.Core;
using Microsoft.Extensions.Logging;
using Innotrack.Logger;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class KeyFrameSync : IoTBase
    {
        List<Device> DeviceList { get; set; }

        public KeyFrameSync(IRedGreenQueueAdapter redq) : base(redq)
        {
            DeviceList = new List<Device>();
        }

        /// <summary>
        /// Loop through all devices and send current tag list to IoT Cloud Platform
        /// </summary>
        /// <returns></returns>
        public async Task StartKeyFrameMonitor()
        {
            DeviceList = new Device().Read();
            while (true)
                try
                {
                    foreach (var device in DeviceList)
                    {
                        //Add all added tag of device
                        var Alltags = GetCurrentDeviceTagList(device);
                        if (Alltags.Count == 0)
                            break;

                        LoggerX.WriteEventLog($"{Alltags.Count} tags found at device {device.DeviceName}");
                        var value = new JObject
                        {
                            ["reset"] = JToken.FromObject(Alltags)
                        };
                        lst.Add((device.DeviceName, device.DeviceName, value));

                        await SendNotification(lst);
                        LoggerX.WriteEventLog("Reset Notification Sent");
                        lst.Clear();
                    }

                        //Delay For Total Key Frame Interval Seconds
                        if (IotGateway.KeyframeInterval > 0)
                            await Task.Delay((int)(IotGateway.KeyframeInterval * 1000));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                    LoggerX.WriteErrorLog(ex);
                }
        }
        /// <summary>
        /// Get all current taglist of device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private Dictionary<string, RfidTag> GetCurrentDeviceTagList(Device device)
        {
            QueryFilter filter = new QueryFilter("DeviceID", device.ID, FilterOperator.Equals);
            var list = new TagLastSeen().Read(filter);

            Dictionary<string, RfidTag> tags = new Dictionary<string, RfidTag>();
            foreach (var read in list)
            {
                var tag = new RfidTag
                {
                    Rfid = read.RFID,
                    DateSeen = read.DateTime.ToString()
                };
                tags.Add(read.RFID,tag);
            }
            return tags;
        }
    }
}
