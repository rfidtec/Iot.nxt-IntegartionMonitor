using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Innotrack.DeviceManager.Pinger;
using System.Collections;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
  public  class IoTBase
    {

        #region Properties
        public readonly ILogger<Gateway1Simulator> _logger;
        public readonly IRedGreenQueueAdapter _redq;
        public List<(string,string,object)> lst = new List<(string, string, object)>();
        #endregion

        #region Constructors

        public IoTBase(IRedGreenQueueAdapter redq)
        {
            this._redq = redq;
        }
        #endregion

        #region Methods
        public bool IsRNCAlive(string ip)
        {
            bool alive = PingManager.PingDevice(ip);

            return alive;
        }

        public async Task SendNotification()
        {
            await _redq.SendGateway1NotificationAsync(
                         IotGateway.ClientId,
                         IotGateway.GatewayId,
                         DateTime.UtcNow,
                         new Hashtable { ["SentBy"] = "Innotrack.Gateway1" },
                         null,
                         DateTime.UtcNow,
                         true,
                         false,
                         lst.ToArray());
        }
        #endregion
    }
}
