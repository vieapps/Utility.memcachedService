using System;
using System.Configuration;
using System.Diagnostics;

namespace net.vieapps.Services.Utility.Memcached
{
	public class ServiceComponent
	{
		internal string Arguments { get; set; } = null;
		Process Process { get; set; } = null;
		bool IsTerminatedByService { get; set; } = false;

		internal void Start(string[] args)
		{
			// initialize log
			if (Program.AsService)
				Helper.InitializeLog();

			// prepare arguments
			if (string.IsNullOrWhiteSpace(this.Arguments))
				this.Arguments = args != null && args.Length > 0
					? string.Join(" ", args)
					: ConfigurationManager.AppSettings.Get("Arguments");

			if (string.IsNullOrWhiteSpace(this.Arguments))
				this.Arguments = "-p 36429 -m 1024 -L";

			// start the process
			this.Process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "memcached.exe",
					Arguments = this.Arguments,
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

			this.Process.OutputDataReceived += this.OnOutput;
			this.Process.ErrorDataReceived += this.OnOutput;
			this.Process.Exited += this.OnExit;

			try
			{
				this.Process.Start();
				this.Process.BeginOutputReadLine();
				this.Process.BeginErrorReadLine();

				Helper.WriteLog("memcached Server is started..." + "\r\n" + $"- Arguments: {this.Arguments}\r\n- Server PID: {this.Process.Id}\r\n- Service PID: {Process.GetCurrentProcess().Id}");
			}
			catch (Exception ex)
			{
				Helper.WriteLog($"Error occured while starting memcached Server [{this.Arguments}]", ex);
			}
		}

		internal void Stop()
		{
			// stop the process
			this.Process.OutputDataReceived -= this.OnOutput;
			this.Process.ErrorDataReceived -= this.OnOutput;
			this.Process.Exited -= this.OnExit;

			try
			{
				this.IsTerminatedByService = true;
				this.Process.StandardInput.Close();
				this.Process.Refresh();
				if (!this.Process.HasExited)
				{
					if (!this.Process.WaitForExit(567))
					{
						this.Process.Kill();
						Helper.WriteLog($"memcached Server is killed.\r\n\t-Time: {this.Process.ExitTime}\r\n\t-Code: {this.Process.ExitCode}");
					}
					else
						Helper.WriteLog($"memcached Server is stoped.\r\n\t-Time: {this.Process.ExitTime}\r\n\t-Code: {this.Process.ExitCode}");
				}
				else
					Helper.WriteLog($"memcached Server is stoped.\r\n\t-Time: {this.Process.ExitTime}\r\n\t-Code: {this.Process.ExitCode}");
			}
			catch (Exception ex)
			{
				Helper.WriteLog("Error occured while stopping memcached Server", ex);
			}

			this.Process.Dispose();
			this.Process = null;

			// close log
			if (Program.AsService)
				Helper.DisposeLog();
		}

		void OnOutput(object sender, DataReceivedEventArgs args) => Helper.WriteLog(args.Data);

		void OnExit(object sender, EventArgs args)
		{
			if (!this.IsTerminatedByService)
			{
				Helper.WriteLog($"memcached Server is stoped suddently.\r\n\t-Time: {this.Process.ExitTime}\r\n\t-Code: {this.Process.ExitCode}");
				Helper.WriteLog("Restarting...");

				this.Process.Start();
				this.Process.BeginOutputReadLine();
				this.Process.BeginErrorReadLine();
			}
		}
	}
}