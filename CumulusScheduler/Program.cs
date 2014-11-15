using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called <program>.exe.config in the application base
// directory (i.e. the directory containing <program>.exe)

namespace CumulusScheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Debug.WriteLine("Program.Main: Environment.UserInteractive '{0}'", Environment.UserInteractive);
            if (Environment.UserInteractive)
            {
                var service = new CumulusSchedulerService();
                service.OnStartInteractive(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new CumulusSchedulerService() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
