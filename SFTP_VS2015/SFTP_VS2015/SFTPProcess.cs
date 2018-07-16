using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mjk.SFTP_VS2015
{
    class SFTPProcess
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private System.Collections.IDictionary appConfig = null;

        protected bool runTest = false;
        protected bool runProd = false;

        protected System.Timers.Timer timeIntervalTest = null;
        protected System.Timers.Timer timeIntervalProd = null;

        protected int ENV_TEST_TIME_INTERVAL = 5; // second
        protected int ENV_PROD_TIME_INTERVAL = 10;
        // RBTSO
        protected string t_RBTSOSrcPath = null;
        protected string t_RBTSOTargetPath = null;
        protected string t_RBTSOFile = null;
        // PGI
        protected string t_PGISrcPath = null;
        protected string t_PGISrcBackupPath = null;
        protected string t_PGITargetPath = null;
        protected string t_PGIFile = null;
        // RPT
        protected string t_RPTSrcPath = null;
        protected string t_RPTTargetPath = null;
        protected string t_RPTFile = null;

        // RBTSO Production
        protected string p_RBTSOSrcPath = null;
        protected string p_RBTSOTargetPath = null;
        protected string p_RBTSOFile = null;
        // PGI Production
        protected string p_PGISrcPath = null;
        protected string p_PGISrcBackupPath = null;
        protected string p_PGITargetPath = null;
        protected string p_PGIFile = null;
        // RPT Production
        protected string p_RPTSrcPath = null;
        protected string p_RPTTargetPath = null;
        protected string p_RPTFile = null;

        protected string t_SFTP_HOST = "127.0.0.1";
        protected int t_SFTP_PORT = 22;
        protected string t_SFTP_USER = "";
        protected string t_SFTP_PWD = "";
        // Production
        protected string SFTP_HOST = "127.0.0.1";
        protected int SFTP_PORT = 22;
        protected string SFTP_USER = "";
        protected string SFTP_PWD = "";

        // flag
        protected bool haveRBTSODir = false;
        protected bool havePGIDir = false;
        protected bool haveRPTDir = false;
        protected bool haveRBTSOProcessedDir = false;
        protected bool haveRPTProcessedDir = false;

        protected bool haveRBTSODirProd = false;
        protected bool havePGIDirProd = false;
        protected bool haveRPTDirProd = false;
        protected bool haveRBTSOProcessedDirProd = false;
        protected bool haveRPTProcessedDirProd = false;

        public void Start()
        {
            // set current directory to BaseDirectory
            // this require when running as Service otherwise it will use Windows as current directory
            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            // set thread name for easy debug
            //System.Threading.Thread.CurrentThread.Name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
            log.Info("SFTP VS2015 is start.");
            log.Info("SFTP Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version);

            // load sftp configuration
            try
            {
                appConfig = System.Configuration.ConfigurationManager.GetSection("sftp") as System.Collections.IDictionary;

                ENV_TEST_TIME_INTERVAL = int.Parse(appConfig["env_test_interval"].ToString());
                ENV_PROD_TIME_INTERVAL = int.Parse(appConfig["env_prod_interval"].ToString());

                runTest = appConfig["env_test"].ToString() == "yes" ? true : false;
                runProd = appConfig["env_prod"].ToString() == "yes" ? true : false;

                // get SFTP Server configure
                t_SFTP_HOST = appConfig["sftp_host_test"].ToString();
                t_SFTP_PORT = int.Parse(appConfig["sftp_port_test"].ToString());
                t_SFTP_USER = appConfig["sftp_user_test"].ToString();
                t_SFTP_PWD = appConfig["sftp_pwd_test"].ToString();

                SFTP_HOST = appConfig["sftp_host"].ToString();
                SFTP_PORT = int.Parse(appConfig["sftp_port"].ToString());
                SFTP_USER = appConfig["sftp_user"].ToString();
                SFTP_PWD = appConfig["sftp_pwd"].ToString();

                // get RBTSO Test configure
                t_RBTSOSrcPath = appConfig["test_rbtso_src_path"].ToString();
                t_RBTSOTargetPath = appConfig["test_rbtso_target_path"].ToString();
                t_RBTSOFile = appConfig["test_rbtso_file"].ToString();
                // get PGI Test configure
                t_PGISrcPath = appConfig["test_pgi_src_path"].ToString();
                t_PGISrcBackupPath = appConfig["test_pgi_src_path_backup"].ToString();
                t_PGITargetPath = appConfig["test_pgi_target_path"].ToString();
                t_PGIFile = appConfig["test_pgi_file"].ToString();
                // get Report Test configure
                t_RPTSrcPath = appConfig["test_rpt_src_path"].ToString();
                t_RPTTargetPath = appConfig["test_rpt_target_path"].ToString();
                t_RPTFile = appConfig["test_rpt_file"].ToString();

                // get RBTSO Production configure
                p_RBTSOSrcPath = appConfig["rbtso_src_path"].ToString();
                p_RBTSOTargetPath = appConfig["rbtso_target_path"].ToString();
                p_RBTSOFile = appConfig["rbtso_file"].ToString();
                // get PGI Production configure
                p_PGISrcPath = appConfig["pgi_src_path"].ToString();
                p_PGISrcBackupPath = appConfig["pgi_src_path_backup"].ToString();
                p_PGITargetPath = appConfig["pgi_target_path"].ToString();
                p_PGIFile = appConfig["pgi_file"].ToString();
                // get Report Production configure
                p_RPTSrcPath = appConfig["rpt_src_path"].ToString();
                p_RPTTargetPath = appConfig["rpt_target_path"].ToString();
                p_RPTFile = appConfig["rpt_file"].ToString();
            }
            catch (Exception err)
            {
                log.Error("SFTPProcess Error: Failed to load configuration. ");
                log.Error(string.Format("{0} - {1}\n{2}", err.Source, err.Message, err.StackTrace));
            }

            if (runTest)
            {
                // initial and start timeout
                timeIntervalTest = new System.Timers.Timer(ENV_TEST_TIME_INTERVAL * 1000);
                timeIntervalTest.Elapsed += new System.Timers.ElapsedEventHandler(ServiceSFTPTest);
                timeIntervalTest.Start();
            }
            
            if (runProd)
            {
                timeIntervalProd = new System.Timers.Timer(ENV_PROD_TIME_INTERVAL * 1000);
                timeIntervalProd.Elapsed += new System.Timers.ElapsedEventHandler(ServiceSFTPProduction);
                timeIntervalProd.Start();
            }

        }

        public void Stop()
        {
            log.Info("SFTP VS2015 is stopped.");
        }

        private void ServiceSFTPTest(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
            timeIntervalTest.Stop();

            haveRBTSODir = false;
            havePGIDir = false;
            haveRPTDir = false;

            try
            {
                log.Info("Start SFTP process on Test Server.");
                // START RBTSO Process
                // Get file RBTSO from src path
                // Check folder exist?

                if (!Directory.Exists(t_RBTSOSrcPath))
                {
                    log.Debug(String.Format("Directory {0} dose not exist.", t_RBTSOSrcPath));
                    try
                    {
                        Directory.CreateDirectory(t_RBTSOSrcPath);
                        log.Debug(String.Format("Create directory {0} completed.", t_RBTSOSrcPath));
                        haveRBTSODir = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("Create directory {0} failed.", t_RBTSOSrcPath));
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }
                else
                {
                    haveRBTSODir = true;
                }
                
                // get file from source path with specific file name
                if (haveRBTSODir)
                {
                    // get file
                    SFTPClient sftp = new SFTPClient();
                    try
                    {
                        // Only get files that begin with the letter "RBTSO."
                        string[] dirs = Directory.GetFiles(t_RBTSOSrcPath, t_RBTSOFile);
                        log.Debug(String.Format("The number of files starting with {1} is {0}.", dirs.Length, t_RBTSOFile));
                        foreach (string dir in dirs)
                        {
                            log.Debug(dir);
                            // upload file to target path via SFTP
                            log.Debug(String.Format("Start upload file {0} to {1}", dir, t_RBTSOTargetPath));
                            sftp.UploadSFTPFile(t_SFTP_HOST, t_SFTP_USER, t_SFTP_PWD, dir, t_RBTSOTargetPath, t_SFTP_PORT);
                            log.Debug(String.Format("Upload file {0} to {1} success.", dir, t_RBTSOTargetPath));

                            haveRBTSOProcessedDir = false;
                            // move processed file to processed directory
                            if (!Directory.Exists(t_RBTSOSrcPath+"\\processed"))
                            {
                                log.Debug(String.Format("Directory {0} dose not exist.", t_RBTSOSrcPath + "\\processed"));
                                try
                                {
                                    Directory.CreateDirectory(t_RBTSOSrcPath + "\\processed");
                                    log.Debug(String.Format("Create directory {0} completed.", t_RBTSOSrcPath + "\\processed"));
                                    haveRBTSOProcessedDir = true;
                                }
                                catch (Exception ex)
                                {
                                    log.Error(String.Format("Create directory {0} failed.", t_RBTSOSrcPath + "\\processed"));
                                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                }
                            }
                            else
                            {
                                haveRBTSOProcessedDir = true;
                            }

                            if (haveRBTSOProcessedDir)
                            {
                                try
                                {
                                    // Move the file.
                                    File.Move(dir, t_RBTSOSrcPath + "\\processed\\"+Path.GetFileName(dir));
                                    log.Debug(String.Format("{0} was moved to {1}.", dir, t_RBTSOSrcPath + "\\processed"));

                                    // See if the original exists now.
                                    if (File.Exists(dir))
                                    {
                                        log.Debug("The original file still exists, which is unexpected.");
                                    }
                                    else
                                    {
                                        log.Debug("The original file no longer exists, which is expected.");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                }
                            }

                            // delay between connect SFTP and upload file
                            log.Debug(String.Format("Wait 3 second for next upload file"));
                            System.Threading.Thread.Sleep(3000);
                        }

                        log.Debug(String.Format("Upload all files to {0} completed.", t_RBTSOTargetPath));
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }

                // END RBTSO Process

                // START PGI Process
                // Get file from SFTP

                try
                {
                    SFTPClient sftp = new SFTPClient();

                    if (!Directory.Exists(t_PGITargetPath))
                    {
                        log.Debug(String.Format("Directory {0} dose not exist.", t_PGITargetPath));
                        try
                        {
                            Directory.CreateDirectory(t_PGITargetPath);
                            log.Debug(String.Format("Create directory {0} completed.", t_PGITargetPath));
                            havePGIDir = true;
                        }
                        catch (Exception ex)
                        {
                            log.Error(String.Format("Create directory {0} failed.", t_PGITargetPath));
                            log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                        }
                    }
                    else
                    {
                        havePGIDir = true;
                    }

                    if (havePGIDir)
                    {
                        // Only get files that begin with the letter "CO."
                        sftp.GetSFTPFile(t_SFTP_HOST, t_SFTP_USER, t_SFTP_PWD, t_PGISrcPath, t_PGITargetPath, t_PGIFile, t_PGISrcBackupPath, t_SFTP_PORT);
                    }

                    log.Debug(String.Format("Download all files from {0} completed.", t_PGISrcPath));
                }
                catch (Exception ex)
                {
                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                }

                // END PGI Process

                // START RPT Process
                // Check RPT folder exist?
                if (!Directory.Exists(t_RPTSrcPath))
                {
                    log.Debug(String.Format("Directory {0} dose not exist.", t_RPTSrcPath));
                    try
                    {
                        Directory.CreateDirectory(t_RPTSrcPath);
                        log.Debug(String.Format("Create directory {0} completed.", t_RPTSrcPath));
                        haveRPTDir = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("Create directory {0} failed.", t_RPTSrcPath));
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }
                else
                {
                    haveRPTDir = true;
                }

                // get file from source path with specific file name
                if (haveRPTDir)
                {
                    // get file
                    SFTPClient sftp = new SFTPClient();
                    try
                    {
                        string[] patterns = t_RPTFile.Split('|');

                        foreach (string search in patterns)
                        {
                            // Only get files that begin with the letter "OH." and "TR."
                            string[] dirs = Directory.GetFiles(t_RPTSrcPath, search);
                            log.Debug(String.Format("The number of files starting with {1} is {0}.", dirs.Length, search));
                            foreach (string dir in dirs)
                            {
                                log.Debug(dir);
                                // upload file to target path via SFTP
                                log.Debug(String.Format("Start upload file {0} to {1}", dir, t_RPTTargetPath));
                                sftp.UploadSFTPFile(t_SFTP_HOST, t_SFTP_USER, t_SFTP_PWD, dir, t_RPTTargetPath, t_SFTP_PORT);
                                log.Debug(String.Format("Upload file {0} to {1} success.", dir, t_RPTTargetPath));

                                haveRPTProcessedDir = false;
                                // move processed file to processed directory
                                if (!Directory.Exists(t_RPTSrcPath + "\\processed"))
                                {
                                    log.Debug(String.Format("Directory {0} dose not exist.", t_RPTSrcPath + "\\processed"));
                                    try
                                    {
                                        Directory.CreateDirectory(t_RPTSrcPath + "\\processed");
                                        log.Debug(String.Format("Create directory {0} completed.", t_RPTSrcPath + "\\processed"));
                                        haveRPTProcessedDir = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(String.Format("Create directory {0} failed.", t_RPTSrcPath + "\\processed"));
                                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                    }
                                }
                                else
                                {
                                    haveRPTProcessedDir = true;
                                }

                                if (haveRPTProcessedDir)
                                {
                                    try
                                    {
                                        // Move the file.
                                        File.Move(dir, t_RPTSrcPath + "\\processed\\" + Path.GetFileName(dir));
                                        log.Debug(String.Format("{0} was moved to {1}.", dir, t_RPTSrcPath + "\\processed"));

                                        // See if the original exists now.
                                        if (File.Exists(dir))
                                        {
                                            log.Debug("The original file still exists, which is unexpected.");
                                        }
                                        else
                                        {
                                            log.Debug("The original file no longer exists, which is expected.");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                    }
                                }

                                // delay between connect SFTP and upload file
                                log.Debug(String.Format("Wait 3 second for next upload file"));
                                System.Threading.Thread.Sleep(3000);
                            }
                        }

                        log.Debug(String.Format("Upload all files to {0} completed.", t_RPTTargetPath));
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }

                // END RPT Process
            }
            catch (Exception err)
            {
                log.Error(String.Format("{0} : {1}\n{2}", err.Source, err.Message, err.StackTrace));
            }

            timeIntervalTest.Start();
        }

        private void ServiceSFTPProduction(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
            timeIntervalProd.Stop();

            haveRBTSODirProd = false;
            havePGIDirProd = false;
            haveRPTDirProd = false;

            try
            {
                log.Info("Start SFTP process on Production Server.");
                // START RBTSO Process
                // Get file RBTSO from src path
                // Check folder exist?

                if (!Directory.Exists(p_RBTSOSrcPath))
                {
                    log.Debug(String.Format("Directory {0} dose not exist.", p_RBTSOSrcPath));
                    try
                    {
                        Directory.CreateDirectory(p_RBTSOSrcPath);
                        log.Debug(String.Format("Create directory {0} completed.", p_RBTSOSrcPath));
                        haveRBTSODirProd = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("Create directory {0} failed.", p_RBTSOSrcPath));
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }
                else
                {
                    haveRBTSODirProd = true;
                }

                // get file from source path with specific file name
                if (haveRBTSODirProd)
                {
                    // get file
                    SFTPClient sftp = new SFTPClient();
                    try
                    {
                        // Only get files that begin with the letter "RBTSO."
                        string[] dirs = Directory.GetFiles(p_RBTSOSrcPath, p_RBTSOFile);
                        log.Debug(String.Format("The number of files starting with {1} is {0}.", dirs.Length, p_RBTSOFile));
                        foreach (string dir in dirs)
                        {
                            log.Debug(dir);
                            // upload file to target path via SFTP
                            log.Debug(String.Format("Start upload file {0} to {1}", dir, p_RBTSOTargetPath));
                            sftp.UploadSFTPFile(SFTP_HOST, SFTP_USER, SFTP_PWD, dir, p_RBTSOTargetPath, SFTP_PORT);
                            log.Debug(String.Format("Upload file {0} to {1} success.", dir, p_RBTSOTargetPath));

                            haveRBTSOProcessedDirProd = false;

                            // move processed file to processed directory
                            if (!Directory.Exists(p_RBTSOSrcPath + "\\processed"))
                            {
                                log.Debug(String.Format("Directory {0} dose not exist.", p_RBTSOSrcPath + "\\processed"));
                                try
                                {
                                    Directory.CreateDirectory(p_RBTSOSrcPath + "\\processed");
                                    log.Debug(String.Format("Create directory {0} completed.", p_RBTSOSrcPath + "\\processed"));
                                    haveRBTSOProcessedDirProd = true;
                                }
                                catch (Exception ex)
                                {
                                    log.Error(String.Format("Create directory {0} failed.", p_RBTSOSrcPath + "\\processed"));
                                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                }
                            }
                            else
                            {
                                haveRBTSOProcessedDirProd = true;
                            }

                            if (haveRBTSOProcessedDirProd)
                            {
                                try
                                {
                                    // Move the file.
                                    File.Move(dir, p_RBTSOSrcPath + "\\processed\\" + Path.GetFileName(dir));
                                    log.Debug(String.Format("{0} was moved to {1}.", dir, p_RBTSOSrcPath + "\\processed"));

                                    // See if the original exists now.
                                    if (File.Exists(dir))
                                    {
                                        log.Debug("The original file still exists, which is unexpected.");
                                    }
                                    else
                                    {
                                        log.Debug("The original file no longer exists, which is expected.");
                                    }

                                }
                                catch (Exception ex)
                                {
                                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                }
                            }

                            // delay between connect SFTP and upload file
                            log.Debug(String.Format("Wait 3 second for next upload file"));
                            System.Threading.Thread.Sleep(3000);
                        }

                        log.Debug(String.Format("Upload all files to {0} completed.", p_RBTSOTargetPath));
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }

                // END RBTSO Process

                // START PGI Process
                // Get file from SFTP

                try
                {
                    SFTPClient sftp = new SFTPClient();

                    if (!Directory.Exists(p_PGITargetPath))
                    {
                        log.Debug(String.Format("Directory {0} dose not exist.", p_PGITargetPath));
                        try
                        {
                            Directory.CreateDirectory(p_PGITargetPath);
                            log.Debug(String.Format("Create directory {0} completed.", p_PGITargetPath));
                            havePGIDirProd = true;
                        }
                        catch (Exception ex)
                        {
                            log.Error(String.Format("Create directory {0} failed.", p_PGITargetPath));
                            log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                        }
                    }
                    else
                    {
                        havePGIDirProd = true;
                    }

                    if (havePGIDirProd)
                    {
                        // Only get files that begin with the letter "CO."
                        sftp.GetSFTPFile(SFTP_HOST, SFTP_USER, SFTP_PWD, p_PGISrcPath, p_PGITargetPath, p_PGIFile, p_PGISrcBackupPath, SFTP_PORT);
                    }

                    log.Debug(String.Format("Download all files from {0} completed.", p_PGISrcPath));
                }
                catch (Exception ex)
                {
                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                }

                // END PGI Process

                // START RPT Process
                // Check RPT folder exist?
                if (!Directory.Exists(p_RPTSrcPath))
                {
                    log.Debug(String.Format("Directory {0} dose not exist.", p_RPTSrcPath));
                    try
                    {
                        Directory.CreateDirectory(p_RPTSrcPath);
                        log.Debug(String.Format("Create directory {0} completed.", p_RPTSrcPath));
                        haveRPTDirProd = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("Create directory {0} failed.", p_RPTSrcPath));
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }
                else
                {
                    haveRPTDirProd = true;
                }

                // get file from source path with specific file name
                if (haveRPTDirProd)
                {
                    // get file
                    SFTPClient sftp = new SFTPClient();
                    try
                    {
                        string[] patterns = p_RPTFile.Split('|');

                        foreach (string search in patterns)
                        {
                            // Only get files that begin with the letter "OH." and "TR."
                            string[] dirs = Directory.GetFiles(p_RPTSrcPath, search);
                            log.Debug(String.Format("The number of files starting with {1} is {0}.", dirs.Length, search));
                            foreach (string dir in dirs)
                            {
                                log.Debug(dir);
                                // upload file to target path via SFTP
                                log.Debug(String.Format("Start upload file {0} to {1}", dir, p_RPTTargetPath));
                                sftp.UploadSFTPFile(SFTP_HOST, SFTP_USER, SFTP_PWD, dir, p_RPTTargetPath, SFTP_PORT);
                                log.Debug(String.Format("Upload file {0} to {1} success.", dir, p_RPTTargetPath));

                                haveRPTProcessedDirProd = false;
                                // move processed file to processed directory
                                if (!Directory.Exists(p_RPTSrcPath + "\\processed"))
                                {
                                    log.Debug(String.Format("Directory {0} dose not exist.", p_RPTSrcPath + "\\processed"));
                                    try
                                    {
                                        Directory.CreateDirectory(p_RPTSrcPath + "\\processed");
                                        log.Debug(String.Format("Create directory {0} completed.", p_RPTSrcPath + "\\processed"));
                                        haveRPTProcessedDirProd = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(String.Format("Create directory {0} failed.", p_RPTSrcPath + "\\processed"));
                                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                    }
                                }
                                else
                                {
                                    haveRPTProcessedDirProd = true;
                                }

                                if (haveRPTProcessedDirProd)
                                {
                                    try
                                    {
                                        // Move the file.
                                        File.Move(dir, p_RPTSrcPath + "\\processed\\" + Path.GetFileName(dir));
                                        log.Debug(String.Format("{0} was moved to {1}.", dir, p_RPTSrcPath + "\\processed"));

                                        // See if the original exists now.
                                        if (File.Exists(dir))
                                        {
                                            log.Debug("The original file still exists, which is unexpected.");
                                        }
                                        else
                                        {
                                            log.Debug("The original file no longer exists, which is expected.");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                                    }
                                }

                                // delay between connect SFTP and upload file
                                log.Debug(String.Format("Wait 3 second for next upload file"));
                                System.Threading.Thread.Sleep(3000);
                            }
                        }

                        log.Debug(String.Format("Upload all files to {0} completed.", t_RPTTargetPath));
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                    }
                }

                // END RPT Process
            }
            catch (Exception err)
            {
                log.Error(String.Format("{0} : {1}\n{2}", err.Source, err.Message, err.StackTrace));
            }

            timeIntervalProd.Start();
        }
    }
}
