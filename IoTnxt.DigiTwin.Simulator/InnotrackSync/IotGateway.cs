using Innotrack.DeviceManager.Data;
using IoTnxt.Gateway.API.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public static class IotGateway
    {
        public static string GatewayId { get; } = "RNCVodaworld";
        public static int IntervalSendUnReadTags { get; }
        public static int IntervalSendKeyFrame { get; }
        public static string UserName { get; } = "root";
        public static string Secret { get; } = "Inno65";
        public static string Make { get; } = "Innotrack";
        public static string Model { get; } = "Vodaworld-RNC";
        public static string ClientId { get; } = "t000000065";
        public static bool Heartbeat { get; set; }

        private static string _deviceType;
        private static string deviceZoneName { get; set; }
        private static List<string> addedZones { get; set; }

        public static string RNCIpAddress
        {
            get
            {
                return GetConfigurationValue("rnc-ipaddress");
            }
        }

        public static int UnSeenListInterval
        {
            get
            {
                return Convert.ToInt32(GetConfigurationValue("UnSeenList"));
            }
        }

        public static int KeyframeInterval
        {
            get
            {
                return Convert.ToInt32(GetConfigurationValue("KeyFrame"));
            }
        }

        public static int RemoveTimeout
        {
            get
            {
                return Convert.ToInt32(GetConfigurationValue("RemoveTimeout"));
            }
        }

        public static int RNCCheckInterval
        {
            get
            {
                return Convert.ToInt32(GetConfigurationValue("RNCCheckInterval"));
            }
        }



        public static string GetConfigurationValue(string key)
        {
            //Refresh the configuration in run time
            ConfigurationManager.RefreshSection("appSettings");
            //return the appsetting value of setting name
            return ConfigurationManager.AppSettings[key];
        }



        public static Dictionary<string, Device> GetIotDevices()
        {
            Dictionary<string, Device> temp = new Dictionary<string, Device>();
            addedZones = new List<string>();
            foreach (var device in new Innotrack.DeviceManager.Entities.Device().Read())
            {
                deviceZoneName = DeviceHelper.GetDeviceZoneName(device, out _deviceType);
                Device iotDevice = new Device()
                {
                    DeviceName = $"RFID|1:{_deviceType}|" + deviceZoneName,
                    // DeviceName = $"RFID|1:ZONE|{device.DeviceName}",
                    Properties = new Dictionary<string, DeviceProperty>()
                    {
                        { "TAGS", new DeviceProperty() { PropertyName = "TAGS", DataType = "RFIDDictionary" } },
                        { "HEARTBEAT", new DeviceProperty() { PropertyName = "HEARTBEAT", DataType = "Bool" } }
                    }
                };
                if (deviceZoneName != device.DeviceName)
                    if (!addedZones.Contains(deviceZoneName))
                        addedZones.Add(deviceZoneName);
                    else
                        continue;
                temp.Add($"RFID|1:{_deviceType}|{deviceZoneName}", iotDevice);
                //temp.Add($"RFID|1:ZONE|{device.DeviceName}", iotDevice);

            }
            temp.Add($"RFID|1:RNC|{IotGateway.GatewayId}", new Device()
            {
                DeviceName = IotGateway.GatewayId,
                Properties = new Dictionary<string, DeviceProperty>()
                    {
                        { "HEARTBEAT", new DeviceProperty() { PropertyName = "HEARTBEAT", DataType = "Bool" } }
                    }
            });
            return temp;
        }

    }



}
