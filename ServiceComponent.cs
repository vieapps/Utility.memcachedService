using System;
using System.Configuration;
using System.Diagnostics;

namespace net.vieapps.Services.Utility.memcachedService
{
	public class ServiceComponent
	{
		public ServiceComponent() { }

		internal string _arguments = null;
		Process _process = null;
		bool _isTerminatedByService = false;

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
				this._arguments = "-p 16429 -m 1024 -L";

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

				Helper.WriteLog("memcached Server is started..." + "\r\n" + "- Arguments: " + this._arguments + "\r\n" + "- Server PID: " + this._process.Id.ToString() + "\r\n" + "- Service PID: " + Process.GetCurrentProcess().Id.ToString());
			}
			catch (Exception ex)
			{
				Helper.WriteLog("Error occured while starting memcached Server [" + this._arguments + "]", ex);
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
						Helper.WriteLog(string.Format("memcached Server is killed.\r\n\t-Time: {0}\r\n\t-Code: {1}", this._process.ExitTime, this._process.ExitCode));
					}
					else
						Helper.WriteLog(string.Format("memcached Server is stoped.\r\n\t-Time: {0}\r\n\t-Code: {1}", this._process.ExitTime, this._process.ExitCode));
				}
				else
					Helper.WriteLog(string.Format("memcached Server is stoped.\r\n\t-Time: {0}\r\n\t-Code: {1}", this._process.ExitTime, this._process.ExitCode));
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

		void OnOutput(object sender, DataReceivedEventArgs e)
		{
			Helper.WriteLog(e.Data);	
		}

		void OnExit(object sender, EventArgs e)
		{
			if (!this._isTerminatedByService)
			{
				Helper.WriteLog(string.Format("memcached Server is stoped suddently.\r\n\t-Time: {0}\r\n\t-Code: {1}", this._process.ExitTime, this._process.ExitCode));
				Helper.WriteLog("Restart the process...");

				this._process.Start();
				this._process.BeginOutputReadLine();
				this._process.BeginErrorReadLine();
			}
		}

	}
}