using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace mjk.SFTP_VS2015
{
    public partial class ServiceMain : ServiceBase
    {
        SFTPProcess sftp = new SFTPProcess();
        public ServiceMain()
        {
            InitializeComponent();

            /*if (!System.Diagnostics.EventLog.SourceExists("SFTPVS2015LogSource"))
                System.Diagnostics.EventLog.CreateEventSource("SFTPVS2015LogSource", "SFTP");

            eventLog1.Source = "SFTPVS2015LogSource";
            // the event log source by which 
            //the application is registered on the computer
            eventLog1.Log = "SFTP";*/
        }

        protected override void OnStart(string[] args)
        {
            //eventLog1.WriteEntry("SFTP VS2015 service started.");
        }

        protected override void OnStop()
        {
            //eventLog1.WriteEntry("SFTP VS2015 service stoped.");
        }

        protected override void OnContinue()
        {
            //eventLog1.WriteEntry("SFTP VS2015 service is continuing in working.");
        }
    }
}
