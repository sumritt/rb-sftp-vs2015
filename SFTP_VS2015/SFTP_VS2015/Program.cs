using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace mjk.SFTP_VS2015
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            /*ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceMain()
            };
            ServiceBase.Run(ServicesToRun);*/
            System.Threading.Thread.CurrentThread.Name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;

            if (args.Length > 0)
            {
                // check if start in console mode
                if (args[0].ToLower() == "-console")
                {
                    StartConsoleMode();
                }
                else
                {
                    System.Console.WriteLine("Unknown command line");
                }
            }
            else
            {
                // start in service mode
                StartServiceMode();
            }
        }

        static public void StartConsoleMode()
        {
            log.Info("Starting TBS Service Management in console mode. type 'exit' and press ENTER to exit.");

            SFTPProcess process = new SFTPProcess();

            process.Start();

            while (System.Console.ReadLine() != "exit")
            {
                // do nothing here
            }

            process.Stop();
        }

        static public void StartServiceMode()
        {
            log.Info("Starting TBS Service Management in service mode");

            System.ServiceProcess.ServiceBase[] serviceToRun;

            serviceToRun = new System.ServiceProcess.ServiceBase[] { new ServiceMain() };
            System.ServiceProcess.ServiceBase.Run(serviceToRun);
        }
    }
}
