using DALX.Core;
using DALX.Core.Sql.Filters;
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innotrack.DeviceManager.Entities;
using Innotrack.Logger;
using System.Threading;
using Innotrack.DeviceManager.Devices.RFID;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class TagReadsSync : IoTBase
    {
        private readonly ILogger<TagReads> _logger;

        private List<TagReads> TagList { get; set; }
        private Gateway1Simulator Gateway { get; set; }



        public TagReadsSync(IRedGreenQueueAdapter redq, ILogger<TagReads> logger) : base(redq)
        {
            _logger = logger;
            TagList = new List<TagReads>();


        }

        public async Task StartTagReads()
        {
            //  await Gateway.InitAsync();

            while (true)
            {
                //Get all un seen tags
                TagList = CheckForUnprocessedTags();
                //Loop through all unprocessed tags

                var lst = new List<(string, string, object)>();
                foreach (var read in TagList)
                {
                    try
                    {
                        var _iotObject = new IotObject("RFID");//Group name
                        UpdateLastCheckedLog(InterfaceType.tagreads);

                        //Add new tag read of device
                        var addedUpdatedTags = GetAddedTags(read.Device.DeviceName); 
                            //AddTagRead(read.Device.DeviceName, read.EPC, read.DateTime.ToString());
                        //Remove all tags from last device and add to 
                        AddRemovedTagsToList(lst);
                        // await Task.Delay(10);
                        LoggerX.WriteEventLog($"{read.EPC} tags read at device: {read.Device.DeviceName}");
                        var value = new JObject
                        {
                            ["addOrUpdate"] = JObject.FromObject(addedUpdatedTags)
                        };
                        _iotObject.Object = value;
                        _iotObject.Group = "RFID";
                        // _iotObject.DeviceType = "ZONE";
                        _iotObject.Device = (Innotrack.DeviceManager.Entities.Device)read.Device;
                        //_iotObject.DeviceName = read.Device.DeviceName;
                        _iotObject.ObjectType = "TAGS";
                        lst.Add(_iotObject.ToString());
                        // lst.Add(("RFID|1:ZONE|" + read.Device.DeviceName, "TAGS", value));
                        if (lst.Count > 0)
                        {
                            LoggerX.WriteEventLog(JArray.FromObject(lst).ToString());
                            LoggerX.WriteEventLog("Tag Reads Notification Try");
                            await SendNotification(lst);
                            LoggerX.WriteEventLog("Tag Reads Notification Sent");
                            UpdateTagReadsHostSeen();
                            //read.HostSeen = true;
                            //read.Update();
                            //UpdateLastUpdatedLog(InterfaceType.tagreads);
                            lst.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerX.WriteErrorLog(ex);
                        _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                        // await Task.Delay((int)(Math.Max(1, sim.IntervalSeconds) * 1000));
                    }
                    break;
                }
                await Task.Delay(50);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void UpdateTagReadsHostSeen()
        {
            foreach (var tag in TagList)
            {

                tag.HostSeen = true;
                tag.Update();
            }
        }


        private List<TagReads> CheckForUnprocessedTags()
        {
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                 new QueryFilter("HostSeen", false, FilterOperator.Equals)
                 };
            var tagList = new TagReads().Read(filters);

            return tagList;
        }

        private Dictionary<string, RfidTag> AddTagRead(string deviceName, string rfid, string dateseen)
        {
            Dictionary<string, RfidTag> tags = new Dictionary<string, RfidTag>();
            var tag = new RfidTag
            {
                Rfid = rfid,
                DateSeen = dateseen
            };
            tags.Add(rfid, tag);
            return tags;
        }

        private Dictionary<string, RfidTag> GetAddedTags(string deviceName)
        {
            Innotrack.DeviceManager.Entities.Device device = Innotrack.DeviceManager.Entities.Device.GetDeviceByName(deviceName);
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                 new QueryFilter("HostSeen", false, FilterOperator.Equals,LogicalOperator.AND),
                 new QueryFilter("DeviceID",device.ID,FilterOperator.Equals)
                 };
            TagList = new TagReads().Read(filters);

            int count = 0;
            Dictionary<string, RfidTag> tags = new Dictionary<string, RfidTag>();
            foreach (var read in TagList)
            {
                var tag = new RfidTag
                {
                    Rfid = read.EPC,
                    DateSeen = read.DateTime.ToString()
                };
                count += 1;
                if (!tags.ContainsKey(read.EPC))
                    tags.Add(read.EPC, tag);
                //read.HostSeen = true;
                //read.Update();
            }

            return tags;
        }

        private void AddRemovedTagsToList(List<(string, string, object)> lst)
        {
            foreach (var tag in TagList)
            {
                List<QueryFilter> filters = new List<QueryFilter>()
            {
                new QueryFilter("EPC",tag.EPC,FilterOperator.Equals)
            };
                TagReads readTag = tag.GetSecondLastTagRead(tag.EPC);
                if (readTag == null)
                    continue;
                Dictionary<string, JObject> tags = new Dictionary<string, JObject>();
                tags.Add(readTag.EPC, JObject.Parse("{}"));

                var value = new JObject
                {
                    ["remove"] = JToken.FromObject(tags)
                };
                var _iotObject = new IotObject();
                _iotObject.Object = value;
                _iotObject.Group = "RFID";
                // _iotObject.DeviceType = "ZONE";
                _iotObject.Device = (Innotrack.DeviceManager.Entities.Device)readTag.Device;
                //_iotObject.DeviceName = read.Device.DeviceName;
                _iotObject.ObjectType = "TAGS";
                lst.Add(_iotObject.ToString());
            }

        }

    }
}
