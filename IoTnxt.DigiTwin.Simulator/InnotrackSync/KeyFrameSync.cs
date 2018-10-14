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

        private readonly ILogger<KeyFrameSync> _logger;

        public KeyFrameSync(IRedGreenQueueAdapter redq, ILogger<KeyFrameSync> logger) : base(redq)
        {
            DeviceList = new List<Device>();
            _logger = logger;
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
                    var lst = new List<(string, string, object)>();
                    foreach (var device in DeviceList)
                    {
                        var _iotObject = new IotObject();
                        _iotObject.Group = "RFID";
                        _iotObject.Device = device;
                        _iotObject.ObjectType = "TAGS";
                        //Add all added tag of device
                        await Task.Delay(100);
                        var Alltags = GetCurrentDeviceTagList(device);
                        //if (Alltags.Count == 0)
                        //    continue;

                        LoggerX.WriteEventLog($"{Alltags.Count} tags found at device {device.DeviceName}");
                        var value = new JObject
                        {
                            ["reset"] = JObject.FromObject(Alltags)
                        };
                        _iotObject.Object = value;
                        lst.Add(_iotObject.ToString());
                        //Console.WriteLine(JArray.FromObject(lst).ToString());
                    }
                    _logger.LogTrace("Logging: Key frame attempt");
                    Console.WriteLine("Key frame try..");
                    LoggerX.WriteEventLog("Reset Notification Try");
                    await SendNotification(lst);
                    LoggerX.WriteEventLog("Reset Notification Sent");

                    //Delay For Total Key Frame Interval Seconds
                    if (IotGateway.KeyframeInterval > 0)
                        await Task.Delay((int)(IotGateway.KeyframeInterval * 1000));
                }
                catch (Exception ex)
                {
                    LoggerX.WriteErrorLog(ex);
                    _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                }
        }
        /// <summary>
        /// Get all current taglist of device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private Dictionary<string, JObject> GetCurrentDeviceTagList(Device device)
        {
            DateTime removedatetime = DateTime.Now;
            removedatetime = removedatetime.AddSeconds(-IotGateway.KeyFrameTimeout);
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                new QueryFilter("DeviceID", device.ID, FilterOperator.Equals,LogicalOperator.AND),
                new QueryFilter("DateTime",removedatetime,FilterOperator.GreaterThan)
            };

            //QueryFilter filter = new QueryFilter("DeviceID", device.ID, FilterOperator.Equals);
            var list = new TagLastSeen().Read(filters);

            Dictionary<string, JObject> tags = new Dictionary<string, JObject>();
            foreach (var read in list)
            {
                var tag = new RfidTag
                {
                    Rfid = read.RFID,
                    DateSeen = read.DateTime.ToString()
                };
                tags.Add(read.RFID, JObject.FromObject(tag));
            }
            return tags;
        }
    }
}
