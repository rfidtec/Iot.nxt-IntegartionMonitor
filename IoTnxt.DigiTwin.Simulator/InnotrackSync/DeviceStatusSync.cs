using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTnxt.DAPI.RedGreenQueue.Abstractions;
using Innotrack.DeviceManager.Entities;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
   public class DeviceStatusSync :IoTBase
    {
        #region Properties

        List<Device> LastDeviceStatusList { get; set; }
        #endregion

        public DeviceStatusSync(IRedGreenQueueAdapter redq):base(redq)
        {
            LastDeviceStatusList = new List<Device>();
        }

        public async Task SyncDeviceStatus()
        {
            while(true)
            {

                //Send list to Iot Cloud Platform
                await SendNotification();
            }
                
        }

        private void CheckDeviceStatuses()
        {
            foreach(var device in new Device().Read())
            {
               
            }
        }


    }
}
