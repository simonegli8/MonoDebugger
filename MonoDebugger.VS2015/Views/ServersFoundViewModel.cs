﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using MonoDebugger.VS2015.MonoClient;
using MonoDebugger.VS2015.Settings;

namespace MonoDebugger.VS2015.Views
{
    public class ServersFoundViewModel:IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public ServersFoundViewModel()
        {
            Servers = new ObservableCollection<MonoServerInformation>();
            UserSettings settings = UserSettingsManager.Instance.Load();
            ManualIp = settings.LastIp;
            LookupServers(cts.Token);
        }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
            {
               // dispose managed resources
               cts.Dispose();
            }
          // free native resources
      }

      public void Dispose()
      {
         Dispose(true);
	     GC.SuppressFinalize(this);
      }

        public ObservableCollection<MonoServerInformation> Servers { get; set; }
        public MonoServerInformation SelectedServer { get; set; }
        public string ManualIp { get; set; }

        private async void LookupServers(CancellationToken token)
        {
            var discovery = new MonoServerDiscovery();

            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                MonoServerInformation server = await discovery.SearchServer(token);
                if (server != null)
                {
                    MonoServerInformation exists = Servers.FirstOrDefault(x => Equals(x.IpAddress, server.IpAddress));
                    if (exists == null)
                    {
                        Servers.Add(server);
                        server.LastMessage = DateTime.Now;
                    }
                    else
                    {
                        exists.LastMessage = DateTime.Now;
                    }
                }

                foreach (
                    MonoServerInformation deadServer in
                        Servers.Where(x => ((DateTime.Now - x.LastMessage).TotalSeconds > 5)).ToList())
                    Servers.Remove(deadServer);
            }
        }

        public void StopLooking()
        {
            UserSettings settings = UserSettingsManager.Instance.Load();
            settings.LastIp = ManualIp;
            UserSettingsManager.Instance.Save(settings);

            cts.Cancel();
        }
    }
}