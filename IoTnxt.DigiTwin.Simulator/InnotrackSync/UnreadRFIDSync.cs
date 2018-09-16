using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DALX.Core;
using DALX.Core.Sql.Filters;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using IoTnxt.DigiTwin.Simulator.Collection_Property;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
   public class UnreadRFIDSync : IoTBase
    {

        public UnreadRFIDSync(IRedGreenQueueAdapter redq): base(redq)
        {

        }

        private Dictionary<string, RfidTag> GetRemovedTags(string deviceID, DateTime removeTimeLmit)
        {
            Innotrack.DeviceManager.Entities.Device device = new Innotrack.DeviceManager.Entities.Device(deviceID);
            List<QueryFilter> filters = new List<QueryFilter>()
            {
                new QueryFilter("DeviceID",deviceID,FilterOperator.Equals),
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
