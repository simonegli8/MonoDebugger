using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;

namespace MonoDebugger.SharedLib.Server {
	internal class MonoWebProcess : MonoProcess {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		public string Url { get; private set; }
		public Runtimes Runtime { get; set; } = Runtimes.Net4;
		public int Port { get; set; } = -1;
		public string Path { get; set; } = "/";
		public bool Https { get; set; } = false;

		public MonoWebProcess() : base() { }
		public MonoWebProcess(WebsiteParameters par) : this() {
			Runtime = par.Runtime;
			if (!string.IsNullOrEmpty(par.Url)) {
				var url = new Uri(par.Url);
				Port = url.Port;
				Path = url.AbsolutePath;
				Https = string.Compare(url.Scheme, "https", true) == 0;
			}
		}

		internal override Process Start(string workingDirectory) {
			string monoBin = (Runtime == Runtimes.Net4 ? MonoUtils.GetMonoXsp4() : MonoUtils.GetMonoXsp2());
			string args = GetProcessArgs();

			if (Port != -1) args += " --port=" + Port.ToString();
			if (Path != "/") args += " --application=" + Path + ":.";
			if (Https) args += " --https --https-client-accept";

			ProcessStartInfo procInfo = GetProcessStartInfo(workingDirectory, monoBin);

			procInfo.CreateNoWindow = true;
			procInfo.UseShellExecute = false;
			procInfo.EnvironmentVariables["MONO_OPTIONS"] = args;
			procInfo.RedirectStandardOutput = true;

			_proc = Process.Start(procInfo);
			Task.Run(() => {
				while (!_proc.StandardOutput.EndOfStream) {
					string line = _proc.StandardOutput.ReadLine();

					if (line.StartsWith("Listening on address")) {
						string url = line.Substring(line.IndexOf(":") + 2).Trim();
						if (url == "0.0.0.0")
							Url = "localhost";
						else
							Url = url;
					} else if (line.StartsWith("Listening on port")) {
						string port = line.Substring(line.IndexOf(":") + 2).Trim();
						port = port.Substring(0, port.IndexOf(" "));
						Url += ":" + port;

						if (line.Contains("non-secure"))
							Url = "http://" + Url;
						else
							Url = "https://" + Url;

						RaiseProcessStarted();
					}


					logger.Trace(line);
				}
			});

			return _proc;
		}
	}
}