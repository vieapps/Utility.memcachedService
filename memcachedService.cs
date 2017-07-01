#region Related components
using System;
using System.ServiceProcess;
using System.Configuration;
using System.Diagnostics;
#endregion

namespace net.vieapps.Services.Utility.memcachedService
{
	public partial class memcachedService : ServiceBase
	{
		EventLog _log = null;
		Process _process = null;
		bool _isTerminatedByService = false;

		public memcachedService()
		{
			this.InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			// initialize log
			string logName = "Application";
			string logSource = "VIEApps Memcached";

			if (!EventLog.SourceExists(logSource))
				EventLog.CreateEventSource(logSource, logName);

			this._log = new EventLog(logSource)
			{
				Source = logSource,
				Log = logName
			};

			// start the memcached Server
			this.StartMemcachedServer();
		}

		protected override void OnStop()
		{
			// stop the memcached Server
			this.StopMemcachedServer();

			// close log
			this._log.Close();
			this._log.Dispose();
		}

		#region Start/Stop memcached Server
		void StartMemcachedServer()
		{
			var arguments = ConfigurationManager.AppSettings.Get("Arguments");
			if (string.IsNullOrWhiteSpace(arguments))
				arguments = "-p 16429 -m 1024 -L";

			this._process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "memcached.exe",
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					RedirectStandardInput = true
				},
				EnableRaisingEvents = true
			};

			this._process.OutputDataReceived += this.OnMemcachedOutput;
			this._process.ErrorDataReceived += this.OnMemcachedError;
			this._process.Exited += this.OnMemcachedExit;

			try
			{
				this._process.Start();
				this._process.BeginOutputReadLine();
				this._process.BeginErrorReadLine();

				this._log.WriteEntry("memcached Server x64 is started..." + "\r\n" + "- Arguments" + arguments + "]" + "\r\n" + "- Server PID: " + this._process.Id.ToString() + "\r\n" + "- Service PID: " + Process.GetCurrentProcess().Id.ToString());
			}
			catch (Exception ex)
			{
				this._log.WriteEntry("Error occured while starting memcached Server x64 [" + arguments + "]" + "\r\n\r\n" + "Message: " + ex.Message + " [" + ex.GetType().ToString() + "\r\n\r\n" + "Details: " + ex.StackTrace, EventLogEntryType.Error);
			}
		}

		void StopMemcachedServer()
		{
			if (this._process != null)
				try
				{
					this._isTerminatedByService = true;

					this._process.Refresh();

					if (this._process.HasExited)
					{
						this._log.WriteEntry(string.Format("memcached Server x64 is stoped at {1}. Exit code: {0}", this._process.ExitCode, this._process.ExitTime), EventLogEntryType.Warning);
						return;
					}

					// close procses
					this._process.StandardInput.Close();
					this._process.CloseMainWindow();
					this._process.WaitForExit(500);

					if (!this._process.HasExited)
					{
						this._process.Kill();
						this._log.WriteEntry("memcached Server x64 was killed", EventLogEntryType.Warning);
					}
					else
						this._log.WriteEntry("memcached Server x64 was stoped", EventLogEntryType.Information);
				}
				catch (Exception ex)
				{
					this._log.WriteEntry("Error occured while stopping memcached Server x64" + "\r\n\r\n" + "Message: " + ex.Message + " [" + ex.GetType().ToString() + "\r\n\r\n" + "Details: " + ex.StackTrace, EventLogEntryType.Error);
				}
				finally
				{
					this._process.OutputDataReceived -= this.OnMemcachedOutput;
					this._process.ErrorDataReceived -= this.OnMemcachedError;
					this._process.Exited -= this.OnMemcachedExit;
					this._process.Dispose();
					this._process = null;
				}
		}
		#endregion

		#region Handle the events of the process
		void OnMemcachedOutput(object sender, DataReceivedEventArgs e)
		{
			try
			{
				this._log.WriteEntry(e.Data, EventLogEntryType.Information);
			}
			catch (Exception ex)
			{
				this._log.WriteEntry("Error occured while writting information of memcached Server x64" + "\r\n\r\n" + "Message: " + ex.Message + " [" + ex.GetType().ToString() + "\r\n\r\n" + "Details: " + ex.StackTrace, EventLogEntryType.Error);
			}
		}

		void OnMemcachedError(object sender, DataReceivedEventArgs e)
		{
			try
			{
				this._log.WriteEntry(e.Data, EventLogEntryType.Error);
			}
			catch (Exception ex)
			{
				this._log.WriteEntry("Error occured while writting information of memcached Server x64" + "\r\n\r\n" + "Message: " + ex.Message + " [" + ex.GetType().ToString() + "\r\n\r\n" + "Details: " + ex.StackTrace, EventLogEntryType.Error);
			}
		}

		void OnMemcachedExit(object sender, EventArgs e)
		{
			if (!this._isTerminatedByService)
			{
				this._log.WriteEntry("memcached Server x64 is exited with unknown reason. Now re-start....", EventLogEntryType.Warning);
				this.StartMemcachedServer();
			}
		}
		#endregion

	}
}