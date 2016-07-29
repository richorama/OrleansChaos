﻿using System;
using System.Net;
using System.Threading.Tasks;

using Orleans.Runtime.Host;
using System.Reflection;
using System.IO;
using Orleans.Runtime.Configuration;
using Chaos;
using System.Collections.Generic;

namespace TestHost
{
    internal class OrleansHostWrapper : IDisposable
    {
        public bool Debug
        {
            get { return siloHost != null && siloHost.Debug; }
            set { siloHost.Debug = value; }
        }

        private SiloHost siloHost;

        public OrleansHostWrapper()
        {
            siloHost = new SiloHost("primary", ClusterConfiguration.LocalhostPrimarySilo());
        }

        public bool Run()
        {
            bool ok = false;

            try
            {
                siloHost.InitializeOrleansSilo();

                siloHost.Config.Globals.RegisterBootstrapProvider<MessageLatencyProvider>("MessageLatency", new Dictionary<string, string> { { "MaxLatency", "1000" } });
                siloHost.Config.Globals.RegisterBootstrapProvider<MessageTerminatorProvider>("MessageTerminator", new Dictionary<string, string> { { "DropOutPercentage", "25" } });

                ok = siloHost.StartOrleansSilo();
                if (!ok) throw new SystemException(string.Format("Failed to start Orleans silo '{0}' as a {1} node.", siloHost.Name, siloHost.Type));
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
                var msg = string.Format("{0}:\n{1}\n{2}", exc.GetType().FullName, exc.Message, exc.StackTrace);
                Console.WriteLine(msg);
            }

            return ok;
        }

        public bool Stop()
        {
            bool ok = false;

            try
            {
                siloHost.StopOrleansSilo();
            }
            catch (Exception exc)
            {
                siloHost.ReportStartupError(exc);
                var msg = string.Format("{0}:\n{1}\n{2}", exc.GetType().FullName, exc.Message, exc.StackTrace);
                Console.WriteLine(msg);
            }

            return ok;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            siloHost.Dispose();
            siloHost = null;
        }
    }
}
