using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.IO;
using System.Text.RegularExpressions;

namespace mjk.SFTP_VS2015
{
    class SFTPClient
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool UploadSFTPFile(string host, string username, string password, string sourcefile, string destinationpath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                try
                {
                    client.Connect();
                    log.Debug(String.Format("SFTP Connect {0}@{1}:{2} success.", username, host, port));
                    client.ChangeDirectory(destinationpath);
                    log.Debug(String.Format("SFTP change directory to {0} success.", destinationpath));

                    using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, Path.GetFileName(sourcefile));
                    }
                }
                catch (Exception ex)
                {
                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                }

                client.Disconnect();
                log.Debug(String.Format("SFTP Disconnected."));
            }

            return true;
        }

        public bool UploadSFTPFile(SftpClient client, string sourcefile, string destinationpath)
        {
            try
            {
                //log.Debug(String.Format("SFTP changing directory to {0}", destinationpath));
                client.ChangeDirectory(destinationpath);
                log.Debug(String.Format("SFTP change directory to {0} success.", destinationpath));

                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    client.BufferSize = 4 * 1024;
                    client.UploadFile(fs, Path.GetFileName(sourcefile));
                }
            }
            catch (Exception ex)
            {
                log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
            }

            return true;
        }

        public bool DeleteSFTPFile(SftpClient client, string file, string remotepath)
        {
            try
            {
                client.ChangeDirectory(remotepath);
                log.Debug(String.Format("SFTP change directory to {0} success.", remotepath));

                string deletefile = Path.Combine(remotepath, new FileInfo(file).Name).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                client.Delete(deletefile);
                log.Debug(String.Format("Remote delete file {0} completed.", deletefile));
            }
            catch (Exception ex)
            {
                log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
            }

            return true;
        }

        public bool GetSFTPFile(string host, string username, string password, string sourcepath, string destinationpath, string searchpattern, string backuppath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                try
                {
                    client.Connect();
                    log.Debug(String.Format("SFTP Connect {0}@{1}:{2} success.", username, host, port));
                    //client.ChangeDirectory(sourcepath);
                    log.Debug(String.Format("Start download file from directory {0}", sourcepath));
                    DownloadDirectory(client, sourcepath, destinationpath, searchpattern, backuppath);

                }
                catch (Exception ex)
                {
                    log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                }

                client.Disconnect();
                log.Debug(String.Format("SFTP Disconnected."));
            }

            return true;
        }

        private void DownloadDirectory(SftpClient client, string source, string destination, string pattern, string backup)
        {
            var files = client.ListDirectory(source);
            foreach (var file in files)
            {
                if (!file.IsDirectory && !file.IsSymbolicLink)
                {
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = r.Matches(file.Name);
                    int count = matches.Count;

                    log.Debug(file.Name + ", match total: " + count);
                    
                    if (count > 0)
                    {
                        var dowloadedfile = DownloadFile(client, file, destination, pattern);

                        if (dowloadedfile != null)
                        {
                            // start upload file to backup path
                            log.Debug(String.Format("Start upload file {0} to backup path {1}", dowloadedfile, backup));
                            UploadSFTPFile(client, dowloadedfile, backup);
                            log.Debug(String.Format("Upload file {0} to {1} completed.", dowloadedfile, backup));

                            // remove file from source path
                            log.Debug(String.Format("Start delete file {0} from remote path {1}", dowloadedfile, source));
                            DeleteSFTPFile(client, dowloadedfile, source);
                        }
                    }
                }
                else if (file.IsDirectory)
                {
                    log.Debug(String.Format("Ignoring directory {0}", file.Name));
                }
                else if (file.IsSymbolicLink)
                {
                    log.Debug(String.Format("Ignoring symbolic link {0}", file.FullName));
                }
                else if (file.Name != "." && file.Name != "..")
                {
                    var dir = Directory.CreateDirectory(Path.Combine(destination, file.Name));
                    DownloadDirectory(client, file.FullName, dir.FullName, pattern, backup);
                }
            }
        }

        private string DownloadFile(SftpClient client, SftpFile file, string directory, string pattern)
        {
            string destpath = null;

            try
            {
                destpath = Path.Combine(directory, file.Name);
                log.Debug(String.Format("Downloading {0} to {1}", file.FullName, destpath));
                //using (Stream fileStream = new FileStream(destpath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (Stream fileStream = File.OpenWrite(destpath))
                {
                    client.DownloadFile(file.FullName, fileStream);
                    log.Debug(String.Format("Save file {0} to {1} completed.", file.FullName, directory));
                }
            }
            catch (Exception ex)
            {
                log.Error(String.Format("{0} : {1}\n{2}", ex.Source, ex.Message, ex.StackTrace));
                destpath = null;
            }

            return destpath;
        }
    }
}
