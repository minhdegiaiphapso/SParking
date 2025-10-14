using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using NLog;
using SP.Parking.Terminal.Core.Models;
//using Cirrious.CrossCore;
//using NLog;
//using NLog;
//using Cirrious.CrossCore;

namespace SP.Parking.Terminal.Core.Services
{
    public class StorageService : IStorageService
    {
        private Random _random = new Random();
        private IWebClient _webClient;
        private IHostSettings _hostSettings;

        public StorageService(IHostSettings hostSettings, IWebClient webClient)
        {
            _hostSettings = hostSettings;
            _webClient = webClient;
        }

        public string BuildFilePath(string cardID, bool isFront, bool isCheckIn, DateTime time)
        {
            string direction = isCheckIn ? "in" : "out";
            string position = isFront ? "front" : "back";
            string dateFolder = string.Format("{0:0000}{1:00}{2:00}", time.Year, time.Month, time.Day);
            string timeFolder = string.Format("{0:00}{1:00}", time.Hour, 15 * (int)(time.Minute / 15));
            string filename = string.Format("{0:00}{1:00}_{2}_{3}.jpg", time.Minute, time.Second, cardID, position);
            return string.Format(@"{0}/{1}/{2}/{3}", dateFolder, direction, timeFolder, filename);
        }

        public string BuildFilePath(string cardID, bool isFront, bool isCheckIn)
        {
            var now = TimeMapInfo.Current.LocalTime;
            return BuildFilePath(cardID, isFront, isCheckIn, now);
            //return BuildFilePath(cardID, isFront, isCheckIn, DateTime.Now);
        }

        public string CreateImageSavePath(string path)
        {
            string rs = _hostSettings.StoragePath + "\\" + path;
            rs = rs.Replace('/', '\\');
            rs = rs.Replace(@"\\", @"\");
            return rs;
        }

        private async void DoSaveImageToDisk(string imagePath, byte[] imageData, Action<Exception> complete)
        {
            Exception exception = null;
            try
            {
                string savePath = CreateImageSavePath(imagePath);
                string directoryPath = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize: 4096, useAsync: true))
                {
                    await fs.WriteAsync(imageData, 0, imageData.Length);
                }
            }
            catch (Exception ex) { exception = ex; }
            if(complete != null)
                complete(exception);
        }

        public string SaveImage(string imagePath, byte[] imageData, Action<Exception> complete = null)
        {
            DoSaveImageToDisk(imagePath, imageData, complete);
            return imagePath;
        }

        public void SaveImage(List<string> lstImagePath, List<byte[]> lstImageData, Action<List<Exception>> complete = null)
        {
            DoSaveImagesToDisk(lstImagePath, lstImageData, complete);
        }

        private void DoSaveImagesToDisk(List<string> lstImagePath, List<byte[]> lstImageData, Action<List<Exception>> complete)
        {
            List<Exception> lstException = new List<Exception>();

            lock (this)
            {
                for (int i = 0; i < lstImagePath.Count; i++)
                {
                    string savePath = CreateImageSavePath(lstImagePath[i]);
                    var key = Guid.NewGuid();
                    string tmpSavePath = savePath + key + ".tmp";

                    try
                    {
                        string directoryPath = Path.GetDirectoryName(savePath);

                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        using (FileStream fs = new FileStream(tmpSavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize: 4096))
                        {
                            fs.Write(lstImageData[i], 0, lstImageData[i].Length);
                        }

                        if (File.Exists(savePath))
                            File.Delete(savePath);

                        File.Move(tmpSavePath, savePath);

                        lstException.Add(null);
                    }
                    catch (Exception ex)
                    {
                        lstException.Add(ex);
                    }
                }
            }
            if (complete != null)
                complete(lstException);
        }

        public bool SaveImageSync(string imagePath, byte[] imageData)
        {
            try
            {
                string savePath = CreateImageSavePath(imagePath);
                string directoryPath = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                File.WriteAllBytes(savePath, imageData);
                return true;
            }
            catch { return false; }
        }

        public void SaveFile(string filepath, string content)
        {
            string directoryPath = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filepath, content);
        }

        public string LoadFile(string filepath)
        {
            if (File.Exists(filepath))
                return File.ReadAllText(filepath);
            else
                return null;
        }

        private async void DoLoadImageFromDisk(string imagePath, Action<byte[], Exception> complete)
        {
            byte[] data = null;
            Exception exception = null;
            try
            {
                using (FileStream sourceStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 4096, useAsync: true))
                {
                    using (MemoryStream ms = new MemoryStream((int)sourceStream.Length))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await ms.WriteAsync(buffer, 0, numRead);
                        }
                        data = ms.ToArray();
                    }
                }
            }
            catch (Exception ex) { exception = ex; }
            complete(data, exception);
        }

        private void DoLoadImageFromHost(string imagePath, string imageHost, Action<byte[], Exception> complete)
        {
            if (string.IsNullOrEmpty(imageHost))
                _webClient.DownloadData("images/" + imagePath, complete);
            else
            {
                imageHost += ":9191";
                _webClient.DownloadData(imageHost, "images/" + imagePath, complete);
            }
        }

        public void DoubleLoadImage(string imagePath, string imageHost, Action<byte[], Exception> complete)
        {
            if (complete == null) return;
            string savePath = CreateImageSavePath(imagePath);

            if (File.Exists(savePath))
            {
                DoLoadImageFromDisk(savePath, complete);
            }
            else
            {
                bool finish = false;
                DoLoadImageFromHost(imagePath, string.Empty, (result, ex) => {
                    if (!finish)
                    {
                        finish = true;
                        if (complete != null)
                            complete(result, ex);
                    }
                });

                DoLoadImageFromHost(imagePath, imageHost, (result, ex) => {
                    if (!finish)
                    {
                        finish = true;
                        if (complete != null)
                            complete(result, ex);
                    }
                });
            }
        }

        public void DoubleLoadImage(string imagePath, string[] imageHosts, Action<byte[], Exception> complete)
        {
            try
            {
                string host = null;

                if (imageHosts != null && imageHosts.Length > 0)
                {
                    int idx = _random.Next(imageHosts.Length);
                    if (idx >= 0)
                        host = imageHosts[idx];
                }

                DoubleLoadImage(imagePath, host, complete);
            }
            catch (Exception ex)
            {
                if (complete != null)
                    complete(null, ex);
            }
        }

        public void LoadImage(string imagePath, string imageHost, Action<byte[], Exception> complete)
        {
            if (complete == null) return;
            string savePath = CreateImageSavePath(imagePath);
            
            if (File.Exists(savePath))
            {
                DoLoadImageFromDisk(savePath, complete);
            }
            else
            {
                DoLoadImageFromHost(imagePath, imageHost, complete);
            }
        }

        public void LoadImage(string imagePath, string[] imageHosts, Action<byte[], Exception> complete)
        {
            try
            {
                string host = null;

                if (imageHosts != null && imageHosts.Length > 0)
                {
                    int idx = _random.Next(imageHosts.Length);
                    if (idx >= 0)
                        host = imageHosts[idx];
                }

                LoadImage(imagePath, host, complete);
            }
            catch(Exception ex)
            {
                if (complete != null)
                    complete(null, ex);
            }
        }
    }
}
