using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Assist_GW.DAL
{
    public static class LogInFile
    {
        public static void InsertFeedback(int instanceId, string msg, string path)
        {
            try
            {
                CreateFolderIfNotExist(path);
                var dateLog = DateTime.Now;

                string Today = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");

                string activityLogFile = "InstanceId_" + instanceId.ToString() + "_DateTime_" + Today + ".txt";
                string activityLogPath = @path + "\\" + activityLogFile;
                string allText = dateLog.ToString("dd/MM/yyyy HH:mm:ss.fff") + " AddressId: " + instanceId + " Message: " + msg + "\r\n";

                byte[] allTextBytes = Encoding.UTF8.GetBytes(allText);
                FileStream fileStream = File.Open(activityLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                fileStream.Write(allTextBytes, 0, allTextBytes.Count());

                fileStream.Close();
            }
            catch
            {
            }
        }

        #region Private Methods
        private static void CreateFolderIfNotExist(string logFilesPath)
        {
            try
            {
                Directory.GetDirectories(logFilesPath);
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    Directory.CreateDirectory(logFilesPath);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
