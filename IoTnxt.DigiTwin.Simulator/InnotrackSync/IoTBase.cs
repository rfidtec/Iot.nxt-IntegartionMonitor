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
using Innotrack.DeviceManager.Data.Entities;
using DALX.Core.Sql.Filters;
using DALX.Core;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class IoTBase
    {

        #region Properties
        public readonly IRedGreenQueueAdapter _redq;
        public Gateway1Simulator Gateway1Simulator { get; set; }
       public IntegrationProcessLog integrationLog { get; set; }
        #endregion

        #region Constructors

        public IoTBase(IRedGreenQueueAdapter redq)
        {
            this._redq = redq;
            integrationLog = new IntegrationProcessLog();
           
        }
        #endregion

        #region Methods
        public bool IsRNCAlive(string ip)
        {
            PingManager pingManager = new PingManager();
            bool alive = pingManager.PingDevice(ip);

            return alive;
        }

        public async Task SendNotification(List<(string, string, object)> list)
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
                         list.ToArray());
        }

        public async Task SendNotificationMock(List<(string, string, object)> list)
        {

            Console.WriteLine(JArray.FromObject(list));
            await Task.Delay(15);
        }

        public void UpdateLastUpdatedLog(InterfaceType interfaceType)
        {
            //Create Log for this interface with a successful update.
            integrationLog.Interface = InterfaceType.tagreads.ToString();
            integrationLog.LastUpdated = DateTime.Now;
            if (!integrationLog.Read(new QueryFilter("Interface", interfaceType.ToString(), FilterOperator.Equals)).Any())
                integrationLog.Create();
         //   else
               // integrationLog.Update();
          
        }
        public void UpdateLastCheckedLog(InterfaceType interfaceType)
        {
            //Create Log for this interface with a successful update.
            integrationLog.Interface = InterfaceType.tagreads.ToString();
            integrationLog.LastChecked = DateTime.Now;
            if (!integrationLog.Read(new QueryFilter("Interface", interfaceType.ToString(), FilterOperator.Equals)).Any())
                integrationLog.Create();
          //  else
               // integrationLog.Update();

        }

        #endregion
    }
}
