using Innotrack.DeviceManager.Data;
using Innotrack.DeviceManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class IotObject
    {
        #region Properties

        public string Group { get; set; } = "RFID";
        public string DeviceType { get; set; }
        public string DeviceName { get; set; }
        private Device device;
        public async Task<Device> SetDevice(Device device)
        {
           DeviceName = await DeviceHelper.GetDeviceZoneNameAsync(device);
            if (DeviceName == null)
                return null;
            DeviceType = DeviceName.Substring(DeviceName.IndexOf("::") + 2);
            DeviceName = DeviceName.Substring(0, DeviceName.IndexOf("::"));
            return device;
        }
        public string ObjectType { get; set; }
        public object Object { get; set; }

        #endregion
        /// <summary>
        /// New tostring method of a object to send the to Iot.Nxt
        /// </summary>
        private (string, string, object) tostring;
        public new(string, string, object) ToString()
        {
            tostring = ($"{Group}|1:{DeviceType}|{DeviceName}", ObjectType, Object);
            return tostring;
        }

        #region Constructrors

        public IotObject()
        {

        }
        public IotObject(string group)
        {

        }
        public IotObject(string group, string deviceType)
        {
            this.Group = group;
            this.DeviceType = deviceType;
        }

        #endregion
    }
}
