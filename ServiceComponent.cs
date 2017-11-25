using System;
using System.Configuration;
using System.Diagnostics;

namespace net.vieapps.Services.Utility.memcachedService
{
	public class ServiceComponent
	{
		internal string _arguments = null;
		Process _process = null;
		bool _isTerminatedByService = false;

		public ServiceComponent() { }

		internal void Start(string[] args)
		{
			// initialize log
			if (Program.AsService)
				Helper.InitializeLog();

			// prepare arguments
			if (string.IsNullOrWhiteSpace(this._arguments))
				this._arguments = args != null && args.Length > 0
					? string.Join(" ", args)
					: ConfigurationManager.AppSettings.Get("Arguments");

			if (string.IsNullOrWhiteSpace(this._arguments))
				this._arguments = "-p 36429 -m 1024 -L";

			// start the process
			this._process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "memcached.exe",
					Arguments = this._arguments,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					ErrorDialog = false,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					RedirectStandardInput = true
				},
				EnableRaisingEvents = true
			};

			this._process.OutputDataReceived += this.OnOutput;
			this._process.ErrorDataReceived += this.OnOutput;
			this._process.Exited += this.OnExit;

			try
			{
				this._process.Start();
				this._process.BeginOutputReadLine();
				this._process.BeginErrorReadLine();

				Helper.WriteLog("memcached Server is started..." + "\r\n" + $"- Arguments: {this._arguments}\r\n- Server PID: {this._process.Id}\r\n- Service PID: {Process.GetCurrentProcess().Id}");
			}
			catch (Exception ex)
			{
				Helper.WriteLog($"Error occured while starting memcached Server [{this._arguments}]", ex);
			}
		}

		internal void Stop()
		{
			// stop the process
			this._process.OutputDataReceived -= this.OnOutput;
			this._process.ErrorDataReceived -= this.OnOutput;
			this._process.Exited -= this.OnExit;

			try
			{
				this._isTerminatedByService = true;
				this._process.StandardInput.Close();
				this._process.Refresh();
				if (!this._process.HasExited)
				{
					if (!this._process.WaitForExit(567))
					{
						this._process.Kill();
						Helper.WriteLog($"memcached Server is killed.\r\n\t-Time: {this._process.ExitTime}\r\n\t-Code: {this._process.ExitCode}");
					}
					else
						Helper.WriteLog($"memcached Server is stoped.\r\n\t-Time: {this._process.ExitTime}\r\n\t-Code: {this._process.ExitCode}");
				}
				else
					Helper.WriteLog($"memcached Server is stoped.\r\n\t-Time: {this._process.ExitTime}\r\n\t-Code: {this._process.ExitCode}");
			}
			catch (Exception ex)
			{
				Helper.WriteLog("Error occured while stopping memcached Server", ex);
			}

			this._process.Dispose();
			this._process = null;

			// close log
			if (Program.AsService)
				Helper.DisposeLog();
		}

		void OnOutput(object sender, DataReceivedEventArgs args)
		{
			Helper.WriteLog(args.Data);
		}

		void OnExit(object sender, EventArgs args)
		{
			if (!this._isTerminatedByService)
			{
				Helper.WriteLog($"memcached Server is stoped suddently.\r\n\t-Time: {this._process.ExitTime}\r\n\t-Code: {this._process.ExitCode}");
				Helper.WriteLog("Restarting...");

				this._process.Start();
				this._process.BeginOutputReadLine();
				this._process.BeginErrorReadLine();
			}
		}
	}
}