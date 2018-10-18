using Innotrack.Logger;
using IoTnxt.Common.Bootstrap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
    public class ConcurrentFileLogger
    {
        BlockingCollection<string> queue = new BlockingCollection<string>();
        private readonly LoggerX _logger;
        private Task executionTask;

        public ConcurrentFileLogger()
        {
            StartExecuteLogging();
        }

        public void Log(string log)
        {
            queue.Add(log);
        }

        public Task StartExecuteLogging()
        {
            while (true)
            {
                if (queue.TryTake(out var log, 100))
                {
                    _logger.WriteEventLog(log);
                }
                Task.Delay(10);
            }
        }

        public void Dispose()
        {
        }

        public ConcurrentFileLogger(LoggerX logger)
        {
            this._logger = logger ?? throw new NullReferenceException(nameof(logger));
        }
    }
}
