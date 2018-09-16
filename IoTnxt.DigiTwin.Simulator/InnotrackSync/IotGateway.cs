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
        public static bool Heartbeat { get; set;  }

        public static string GetConfigurationValue(string key)
        {
            //Refresh the configuration in run time
            ConfigurationManager.RefreshSection("appSettings");
            //return the appsetting value of setting name
            return ConfigurationManager.AppSettings[key];
        }

        public static Dictionary<string,Device> GetIotDevices()
        {
            Dictionary<string, Device> temp = new Dictionary<string, Device>();
            foreach (var device in  new Innotrack.DeviceManager.Entities.Device().Read())
            {
                Device iotDevice = new Device()
                {
                    DeviceName = "RFID|1:ZONE|" + device.DeviceName ,
                    Properties = new Dictionary<string, DeviceProperty>() { { "TAGS",new DeviceProperty() {PropertyName="TAGS",DataType="RFIDDictionary" } } }
                };
            temp.Add("RFID|1:ZONE|" + device.DeviceName, iotDevice);
             
            }
            return temp;
        }

    }
}
