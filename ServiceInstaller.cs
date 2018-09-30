using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace net.vieapps.Services.Utility.Memcached
{
	[RunInstaller(true)]
	public partial class ServiceInstaller : Installer
	{
		public ServiceInstaller()
		{
			this.InitializeComponent();

			this.Installers.Add(new ServiceProcessInstaller()
			{
				Account = ServiceAccount.LocalSystem,
				Username = null,
				Password = null
			});

			this.Installers.Add(new System.ServiceProcess.ServiceInstaller()
			{
				StartType = ServiceStartMode.Automatic,
				ServiceName = "VIEApps-Memcached",
				DisplayName = "VIEApps Memcached",
				Description = "memcached Server for Windows"
			});

			this.AfterInstall += (sender, args) =>
			{
				try
				{
					using (var controller = new ServiceController("VIEApps-Memcached"))
					{
						controller.Start();
					}
				}
				catch { }
			};
		}
	}
}