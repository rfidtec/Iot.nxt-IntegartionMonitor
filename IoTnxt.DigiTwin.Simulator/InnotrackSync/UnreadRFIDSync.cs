using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALX.Core;
using DALX.Core.Sql.Filters;
using Innotrack.DeviceManager.Entities;
using Innotrack.Logger;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class UnreadRFIDSync : IoTBase
    {
        private readonly ILogger<UnreadRFIDSync> _logger;
        #region Properties
        List<Device> Devices { get; set; }
        List<TagLastSeen> _tagList { get; set; }
        private List<TagLastSeen> _tagLastSeenList { get; set; }
        #endregion
        public UnreadRFIDSync(IRedGreenQueueAdapter redq, ILogger<UnreadRFIDSync> logger,LoggerX loggerX) : base(redq,loggerX)
        {
            _logger = logger;
            Devices = new List<Device>();
            _tagLastSeenList = new List<TagLastSeen>();
        }

        public async Task StartUnSeenMonitor()
        {
            while (true)
            {
                try
                {
                    await UpdateLastCheckedLog(InterfaceType.unreadtaglist);
                    Devices = await new Device().ReadAsync();
                    var lst = new List<(string, string, object)>();
                    foreach (var device in Devices)
                    {
                        var _iotObject = new IotObject();
                        _iotObject.Device = device;
                        _iotObject.ObjectType = "TAGS";
                        DateTime removedatetime = DateTime.Now;
                        removedatetime = removedatetime.AddSeconds(IotGateway.RemoveTimeout);
                        var removeList = await GetRemovedTags(device.ID.ToString(), removedatetime);
                        if (removeList.Count == 0)
                            continue;
                        LoggerX.WriteEventLog($"{removeList.Count} unread tags at device: {device.DeviceName}");
                        var value = new JObject
                        {
                            ["remove"] = JToken.FromObject(removeList)
                        };
                        _iotObject.Object = value;
                        lst.Add(_iotObject.ToString());
                    }
                    if (lst.Count > 0)
                    {
                        LoggerX.WriteEventLog("Remove List try...");
                        await SendNotification(lst);
                        UpdateAllTagSeen();
                        await UpdateLastUpdatedLog(InterfaceType.unreadtaglist);
                        LoggerX.WriteEventLog($"Remove list notifcation sent");
                        await Task.Delay(50);
                    }

                    if (IotGateway.UnSeenListInterval > 0)
                        await Task.Delay((int)(IotGateway.UnSeenListInterval * 1000));
                }
                catch (Exception ex)
                {
                    LoggerX.WriteErrorLog(ex);
                    if (ex.Message == "Unable to write data to the transport connection: An existing connection was forcibly closed by the remote host.")
                        continue;
                    _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                }

            }
        }

        private async Task<Dictionary<string, JObject >> GetRemovedTags(string deviceID, DateTime removeTimeLmit)
        {
            Innotrack.DeviceManager.Entities.Device device = new Innotrack.DeviceManager.Entities.Device(deviceID);
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                new QueryFilter("DeviceID",deviceID,FilterOperator.Equals,LogicalOperator.AND),
                new QueryFilter("DateTime",removeTimeLmit,FilterOperator.LessThan,LogicalOperator.AND),
                new QueryFilter("HostSeen",false,FilterOperator.Equals)
            };
             _tagList = await new Innotrack.DeviceManager.Entities.TagLastSeen().ReadAsync(filters);
            
            Dictionary<string, JObject> tags = new Dictionary<string, JObject>();
            foreach (var read in _tagList)
            {
                tags.Add(read.RFID.ToString(), JObject.Parse("{}"));
                _tagLastSeenList.Add(read);
            }
            return tags;
        }

        private void UpdateAllTagSeen()
        {
            foreach(var tag in _tagLastSeenList)
            {
                tag.HostSeen = true;
                tag.DeviceID = 0;
                tag.Update();
            }
            _tagLastSeenList.Clear();
        }
    }
}
