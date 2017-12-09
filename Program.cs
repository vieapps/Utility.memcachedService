using System;
using System.Windows.Forms;

namespace net.vieapps.Services.Utility.Memcached
{
	static class Program
	{
		internal static bool AsService = true;
		internal static ServicePresenter Form = null;

		static void Main(string[] args)
		{
			Program.AsService = !Environment.UserInteractive;
			if (Program.AsService)
				System.ServiceProcess.ServiceBase.Run(new ServiceRunner());
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Program.Form = new ServicePresenter();
				Application.Run(Program.Form);
			}
		}
	}
}