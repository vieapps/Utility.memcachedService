using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace net.vieapps.Services.Utility.memcachedService
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			this.InitializeComponent();

			this.Installers.Add(new ServiceProcessInstaller()
			{
				Account = ServiceAccount.LocalSystem,
				Username = null,
				Password = null
			});

			this.Installers.Add(new ServiceInstaller()
			{
				StartType = ServiceStartMode.Automatic,
				ServiceName = "VIEApps-Memcached",
				DisplayName = "VIEApps Memcached",
				Description = "memcached Server for Windows (x86/x64)"
			});

			this.AfterInstall += new InstallEventHandler(this.StartServiceAfterInstall);
		}

		void StartServiceAfterInstall(object sender, InstallEventArgs e)
		{
			try
			{
				using (var controller = new ServiceController("VIEApps-Memcached"))
				{
					controller.Start();
				}
			}
			catch { }
		}
	}
}