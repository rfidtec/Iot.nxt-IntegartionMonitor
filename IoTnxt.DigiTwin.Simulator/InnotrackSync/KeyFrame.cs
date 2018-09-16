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

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class KeyFrame : IoTBase
    {
        List<Device> DeviceList { get; set; }

        public KeyFrame(IRedGreenQueueAdapter redq) : base(redq)
        {
            DeviceList = new List<Device>();
        }

        /// <summary>
        /// Loop through all devices and send current tag list to IoT Cloud Platform
        /// </summary>
        /// <returns></returns>
        public async Task SendKeyFrames()
        {
            DeviceList = new Device().Read();
            while(true)
            foreach (var device in DeviceList)
            {
                //Add all added tag of device
                var Alltags = GetCurrentDeviceTagList(device);
                var value = new JObject
                {
                    ["CurrentTags"] = JToken.FromObject(Alltags)
                };
                lst.Add((device.DeviceName, device.DeviceName, value));

                await SendNotification();

                lst.Clear();
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
            }
            return tags;
        }
    }
}
