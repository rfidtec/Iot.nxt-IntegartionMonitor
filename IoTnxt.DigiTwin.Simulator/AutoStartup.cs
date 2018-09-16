using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using IoTnxt.Common.Bootstrap;
using IoTnxt.Data.DbHelper;
using IoTnxt.Data.DbHelper.Abstractions;
using IoTnxt.DAPI.RedGreenQueue.Adapter;
using IoTnxt.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IoTnxt.DigiTwin.Simulator
{
    public class AutoStartup : IAutoStartup
    {
        public AutoStartup(
            ILogger<AutoStartup> logger,
            RedGreenQueueAdapter redq,
            IDbHelperProvider provider,
            IOptions<SimulatorOptions> options,
            Gateway1Simulator sim1)
        {
            //SimulateMassSendIntegrity(logger, redq, provider, options);
            //SimulateSinglePropertyChanges(logger, redq, provider, options);
        }

        public void SimulateSinglePropertyChanges(ILogger<AutoStartup> logger,
            RedGreenQueueAdapter redq,
            IDbHelperProvider provider,
            IOptions<SimulatorOptions> options)
        {
            logger.LogTrace("Simulation started");

            var cnt = 1000;

            for (var i = 0; i < cnt; i++)
            {
                try
                {
                    var siteKey = "AFRICA.ZAF.GP.MID.VC.DS";
                    var groupId = "RECTIFIER.1";
                    var deviceId = "RECTIFIER.1";
                    var properties = new[] { "BATTEMP", "BM501", "BM502" };

                    var baseline = 0;
                    foreach (var prop in properties)
                    {
                        var val = baseline * 1000000 + (double)i;
                        baseline++;

                        if (i % properties.Length != baseline - 1)
                            continue;

                        logger.LogTrace($"{siteKey}.{groupId}.{deviceId}.{prop} => {val}");

                        redq.SendEdge2Notification(siteKey, groupId, DateTime.UtcNow, null, false, null,
                            null, DateTime.UtcNow, false,
                            (deviceId, prop, val)
                        );
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Sending simulation message");
                    i--;
                    Thread.Sleep(1000);
                    continue;
                }

                Thread.Sleep(1000);
            }

            logger.LogTrace("Simulation completed");
        }

        public void SimulateMassSendIntegrity(ILogger<AutoStartup> logger,
            RedGreenQueueAdapter redq,
            IDbHelperProvider provider,
            IOptions<SimulatorOptions> options)
        {
            logger.LogTrace("Simulation started");
            var db = provider.Get("Data") ?? throw new ArgumentException("DbReference not found: Data");

            var tblname = "AgentHistory_TESTTEMP";
            logger.LogTrace($"Dropping {tblname}");
            db.Tables().DropObject(tblname);
            var cnt = 100000;

            for (int i = 0; i < cnt; i++)
            {
                try
                {
                    var siteKey = "AFRICA.ZA.GP.CENTURION.IOTNXT.MICHIELDEV";
                    var groupId = "TESTTEMP.1";
                    var deviceId = "TEMP";

                    if (i % 1000 == 0)
                        logger.LogTrace($"{siteKey}.{groupId}.{deviceId} => {i}");
                    redq.SendEdge2Notification(siteKey, groupId, DateTime.Today.AddSeconds(i), null, false, new Hashtable
                    {
                        ["Raptor"] = "SimulatedMac1",
                        ["Version"] = "Simulatorv1"
                    }, DateTime.Today.AddSeconds(i - 1), DateTime.UtcNow, false,
                        (deviceId + ".1", "VALUE", i),
                        (deviceId + ".2", "VALUE", i),
                        (deviceId + ".3", "VALUE", i),
                        (deviceId + ".4", "VALUE", i),
                        (deviceId + ".5", "VALUE", i),
                        (deviceId + ".6", "VALUE", i),
                        (deviceId + ".7", "VALUE", i),
                        (deviceId + ".8", "VALUE", i),
                        (deviceId + ".9", "VALUE", i),
                        (deviceId + ".10", "VALUE", i)
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Sending simulation message");
                    i--;
                    Thread.Sleep(1000);
                }
            }

            Thread.Sleep(1000);

            while (true)
            {
                try
                {
                    var count = db.Scalar<int>("SELECT COUNT(*) FROM (SELECT DISTINCT [TEMP.1], [TEMP.2], [TEMP.3], [TEMP.4], [TEMP.5], [TEMP.6], [TEMP.7], [TEMP.8], [TEMP.9], [TEMP.10] FROM AgentHistory_TESTTEMP) IQ");

                    if (count == cnt)
                    {
                        logger.LogInformation("Success");
                        break;
                    }

                    if (count > cnt)
                    {
                        logger.LogCritical("Failure");
                        break;
                    }

                    logger.LogInformation($"History count: {count}");
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Checking on results of simulation");
                }
            }

            logger.LogTraceEx("Simulation completed");
        }

        public void Dispose()
        {
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }

    public class SimulatorOptions
    {
    }
}
