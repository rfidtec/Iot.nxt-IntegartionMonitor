using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALX.Core;
using DALX.Core.Sql.Filters;
using Innotrack.DeviceManager.Entities;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class UnreadRFIDSync : IoTBase
    {
        #region Properties
        List<Device> Devices { get; set; }
        #endregion
        public UnreadRFIDSync(IRedGreenQueueAdapter redq) : base(redq)
        {
            Devices = new List<Device>();
        }

        public async Task StartUnSeenMonitor()
        {
            try
            {

                while (true)
                {
                    Devices = new Device().Read();
                    foreach (var device in Devices)
                    {
                        _iotObject.Device = device;
                        _iotObject.ObjectType = "TAGS";
                        DateTime removedatetime = DateTime.Now;
                        removedatetime = removedatetime.AddSeconds(IotGateway.RemoveTimeout);
                        var removeList = GetRemovedTags(device.ID.ToString(), removedatetime);
                        if (removeList.Count == 0)
                            continue;

                        var value = new JObject
                        {
                            ["remove"] = JToken.FromObject(removeList)
                        };
                        _iotObject.Object = value;
                        lst.Add(_iotObject.ToString());
                        await SendNotification();

                        lst.Clear();
                    }

                    if (IotGateway.UnSeenListInterval > 0)
                        await Task.Delay((int)(IotGateway.UnSeenListInterval * 1000));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
            }
        }

        private Dictionary<string, RfidTag> GetRemovedTags(string deviceID, DateTime removeTimeLmit)
        {
            Innotrack.DeviceManager.Entities.Device device = new Innotrack.DeviceManager.Entities.Device(deviceID);
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                new QueryFilter("DeviceID",deviceID,FilterOperator.Equals,LogicalOperator.AND),
                new QueryFilter("DateTime",removeTimeLmit,FilterOperator.LessThan)
            };
            var tagList = new Innotrack.DeviceManager.Entities.TagLastSeen().Read(filters);

            Dictionary<string, RfidTag> tags = new Dictionary<string, RfidTag>();
            foreach (var read in tagList)
            {
                var tag = new RfidTag
                {
                    Rfid = read.RFID,
                    DateSeen = read.DateTime.ToString()
                };
                tags.Add(device.DeviceName, tag);
            }
            return tags;
        }
    }
}
