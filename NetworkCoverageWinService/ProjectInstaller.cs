using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ServiceProcess;
using System.IO;
using System.Configuration;

namespace NetworkCoverageWinService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            ServiceProcessInstaller process = new ServiceProcessInstaller();

                process.Account = ServiceAccount.LocalSystem;

                ServiceInstaller serviceAdmin = new ServiceInstaller();

                serviceAdmin.StartType = ServiceStartMode.Manual;
                serviceAdmin.ServiceName = "NetworkCoverageService";
                serviceAdmin.DisplayName = "Network Coverage Service Display Name";
                Installers.Add(process);
                Installers.Add(serviceAdmin);
        }

        protected void WriteToFile2(string text)
        {
           // string path =  ConfigurationManager.AppSettings["logFile"].ToString() ;
            string path = "C:\\CDRStest\\LogFile\\ServiceLog2.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }

    }
}
