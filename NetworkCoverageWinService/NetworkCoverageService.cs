using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;

namespace NetworkCoverageWinService
{
    public partial class NetworkCoverageService : ServiceBase
    {
        private int eventId = 1;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private System.Timers.Timer TimerObj = new System.Timers.Timer();
        public NetworkCoverageService()
        {
            try
            {
                // Debugger.Launch();
                InitializeComponent();
            }
            catch (Exception ex)
            {
                this.WriteToFile("NetworkCoverageService Construction Error: " + ex.ToString());
            }
        }
        protected override void OnStart(string[] args)
        {
            //SetTimeInterval();
            int inSecond = Convert.ToInt32(ConfigurationManager.AppSettings["timerIntervalInSeconds"].ToString());
            timer = new System.Timers.Timer(inSecond * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Start();

        }
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            //Serve 1
            string Serve1sourceRemotePath = @"Server1sourceRemotePath";
            string Serve1destLocalPath = @"C:Serve1destLocalPath\Serve1\";
            string Serve1processedRawPath = @"C:\Serve1processedRawPath\processed-raw-files\Serve1";
            CallingSFTPConnection("IP number", 22, "username", "password", Serve1sourceRemotePath, Serve1destLocalPath, Serve1processedRawPath);
            CallWebAPIFormatLTEFile();
            //WriteToFile("SFTP & setFormatLTE method completed on:" + string.Format(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
            timer.Start();
        }

        private void CallingSFTPConnection(string ip, int port, string userName, string password, string sourceRemotePath, string destLocalPath, string processedRawPath)
        {
            //this.WriteToFile("service has been started with path "+ processedRawCDRPath + " Process on: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            using (SftpClient client = new SftpClient(new PasswordConnectionInfo(ip, port, userName, password)))
            {
                client.Connect();
                WriteToFile("SFTP connection successfully made on:" + string.Format(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")) + " with IP : " + ip);
                DownloadDirectory(client, sourceRemotePath, destLocalPath, processedRawPath);
                client.Disconnect();
            }
        }
        private void DownloadDirectory(SftpClient sftpClient, string sourceRemotePath, string destLocalPath, string processedRawCDRPath)
        {
            Directory.CreateDirectory(destLocalPath);
            IEnumerable<SftpFile> files = sftpClient.ListDirectory(sourceRemotePath);
            foreach (SftpFile file in files)
            {
                if ((file.Name != ".") && (file.Name != ".."))
                {
                    string sourceFilePath = sourceRemotePath + "/" + file.Name;
                    string destFilePath = Path.Combine(destLocalPath, file.Name);
                    string processedRawCDRPathWithFileName = Path.Combine(processedRawCDRPath, file.Name);
                    if (file.IsDirectory)
                    {
                        DownloadDirectory(sftpClient, sourceFilePath, destFilePath, processedRawCDRPathWithFileName);
                    }
                    else
                    {
                        try
                        {
                            Boolean st = File.Exists(processedRawCDRPathWithFileName);
                            if (!st)
                            {
                                using (Stream fileStream = File.Create(destFilePath))
                                {
                                    sftpClient.DownloadFile(sourceFilePath, fileStream);
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                        catch (Exception ex)
                        {
                            this.WriteToFile("Error while fetching file from core server. Failing in file: " + file.Name);
                            Console.WriteLine(ex.Message);
                        }

                    }
                }
            }
            this.WriteToFile("Successfully Fetched all the files");
        }
        protected async void CallWebAPIFormatLTEFile()
        {
            HttpClient client = new HttpClient();
            try
            {
                string baseApiAddress = ConfigurationManager.AppSettings["baseAPIAddress"];

                HttpResponseMessage response = await client.GetAsync(baseApiAddress);
                if (response.IsSuccessStatusCode)
                {
                    this.WriteToFile("Successfully Done, StatusCode: " + (int)response.StatusCode + " ( " + response.ReasonPhrase.ToString() + ") ");

                }
                else
                {
                    this.WriteToFile("Failed, StatusCode: " + (int)response.StatusCode + " ( " + response.ReasonPhrase.ToString() + ") ");
                }

            }
            catch (Exception ex)
            {
                this.WriteToFile("Called CallWebAPIFormatLTEFile Function Service Error: " + ex.ToString());

            }
            client.Dispose();
        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            this.WriteToFile("Monitoring the System, Information - " + EventLogEntryType.Information + " EventId - " + eventId++);
        }
        protected override void OnStop()
        {
            timer.Enabled = false;
        }
        protected override void OnContinue()
        {
            this.WriteToFile("In OnContinue.");
        }
        protected void WriteToFile(string text)
        {
            string path = ConfigurationManager.AppSettings["logFile"].ToString();
            path = path + "DemoService" + DateTime.Now.ToString("MMddyyyy") + ".txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
        protected void Obfuscate(string fileName, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            File.WriteAllText(fileName, Convert.ToBase64String(bytes));
        }
        protected string Deobfuscate(string fileName)
        {
            var bytes = Convert.FromBase64String(File.ReadAllText(fileName));
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            return Encoding.UTF8.GetString(bytes);
        }

    }
}
