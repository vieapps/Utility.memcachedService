using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace net.vieapps.Services.memcachedService
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
				DisplayName = "VIEApps NGX Memcached",
				Description = "memcached Server for Windows x64",
				ServiceName = "VIEAppsMemcached"
			});

			this.AfterInstall += new InstallEventHandler(this.StartServiceAfterInstall);
		}

		void StartServiceAfterInstall(object sender, InstallEventArgs e)
		{
			try
			{
				using (var serviceController = new ServiceController("VIEAppsMemcached"))
				{
					serviceController.Start();
				}
			}
			catch { }
		}
	}
}