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

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class TagReadsSync : IoTBase
    {
        private List<TagReads> TagList { get; set; }


        public TagReadsSync(IRedGreenQueueAdapter redq) : base(redq)
        {
            TagList = new List<TagReads>();
            _iotObject = new IotObject("RFID");//Group name

        }

        public async Task StartTagReads()
        {
            while (true)
            {
                //Get all un seen tags
                TagList = CheckForUnprocessedTags();
                //Loop through all unprocessed tags
                foreach (var read in TagList)
                {
                    try
                    {
                        //Add new tag read of device
                        var addedUpdatedTags = AddTagRead(read.Device.DeviceName, read.EPC, read.DateTime.ToString());
                        var value = new JObject
                        {
                            ["addOrUpdate"] = JToken.FromObject(addedUpdatedTags)
                        };

                        _iotObject.Object = value;
                        _iotObject.Group = "RFID";
                       // _iotObject.DeviceType = "ZONE";
                         _iotObject.Device = (Innotrack.DeviceManager.Entities.Device)read.Device;
                        //_iotObject.DeviceName = read.Device.DeviceName;
                        _iotObject.ObjectType = "TAGS";
                        lst.Add(_iotObject.ToString());
                        // lst.Add(("RFID|1:ZONE|" + read.Device.DeviceName, "TAGS", value));

                        await SendNotification();

                        lst.Clear();
                        read.HostSeen = true;
                        read.Update();
                        //if (IotGateway.IntervalSeconds > 0)
                        //    await Task.Delay((int)(sim.IntervalSeconds * 1000));
                        // TagList = CheckForUnprocessedTags();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Sending notification for gateway {IotGateway.GatewayId}");
                        // await Task.Delay((int)(Math.Max(1, sim.IntervalSeconds) * 1000));
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
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
            tags.Add(deviceName, tag);
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
            var tagList = new TagReads().Read(filters);

            Dictionary<string, RfidTag> tags = new Dictionary<string, RfidTag>();
            foreach (var read in tagList)
            {
                var tag = new RfidTag
                {
                    Rfid = read.EPC,
                    DateSeen = read.DateTime.ToString()
                };
                tags.Add(device.DeviceName, tag);
                read.HostSeen = true;
                read.Update();
            }

            return tags;
        }

    }
}
