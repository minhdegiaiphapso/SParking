using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    /// <summary>
    /// Storage service interface
    /// Save or load image manager
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Build file path to save image
        /// </summary>
        /// <param name="cardID">Card ID</param>
        /// <param name="isFront">Is front image or not</param>
        /// <param name="isCheckIn">Is check in or check out process</param>
        /// <param name="time">Time to generate path of saved image</param>
        /// <returns>Result path</returns>
        string BuildFilePath(string cardID, bool isFront, bool isCheckIn, DateTime time);

        /// <summary>
        /// Build file path to save image
        /// </summary>
        /// <param name="cardID">Card ID</param>
        /// <param name="isFront">Is front image or not</param>
        /// <param name="isCheckIn">Is check in or check out process</param>
        /// <returns>Result path</returns>
        string BuildFilePath(string cardID, bool isFront, bool isCheckIn);

        /// <summary>
        /// Save Image data to image path
        /// </summary>
        /// <param name="imagePath">Image path to save</param>
        /// <param name="imageData">Image data to save</param>
        /// <param name="complete">Result callback</param>
        /// <returns>Path of saved image</returns>
        string SaveImage(string imagePath, byte[] imageData, Action<Exception> complete = null);

        /// <summary>
        /// Save a list of files
        /// </summary>
        /// <param name="lstImagePath">List of file paths to save</param>
        /// <param name="lstImageData">List of file data to save</param>
        /// <param name="complete">Result callback</param>
        void SaveImage(List<string> lstImagePath, List<byte[]> lstImageData, Action<List<Exception>> complete = null);

        /// <summary>
        /// Save Image data to image path synchronously
        /// </summary>
        /// <param name="imagePath">Image path to save</param>
        /// <param name="imageData">Image data to save</param>
        /// <returns>Flag indicates save successfully or not</returns>
        bool SaveImageSync(string imagePath, byte[] imageData);
        
        /// <summary>
        /// Load image from a host
        /// </summary>
        /// <param name="imagePath">Image path to load</param>
        /// <param name="imageHost">Image host to get image</param>
        /// <param name="complete">Result callback</param>
        void LoadImage(string imagePath, string imageHost, Action<byte[], Exception> complete);

        /// <summary>
        /// Load image from a random selected host from a list of hosts
        /// </summary>
        /// <param name="imagePath">Image path to load</param>
        /// <param name="imageHosts">List of image hosts are available to get image</param>
        /// <param name="complete">Result callback</param>
        void LoadImage(string imagePath, string[] imageHosts, Action<byte[], Exception> complete);

        /// <summary>
        /// Load image from a random selected host from a list of hosts or server
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <param name="imageHosts">The image hosts.</param>
        /// <param name="complete">The complete.</param>
        void DoubleLoadImage(string imagePath, string[] imageHosts, Action<byte[], Exception> complete);

        /// <summary>
        /// Save a text file to disk
        /// </summary>
        /// <param name="filepath">Path of file</param>
        /// <param name="content">Content string of file</param>
        void SaveFile(string filepath, string content);

        /// <summary>
        /// Load a text file from disk
        /// </summary>
        /// <param name="filepath">Path of file</param>
        /// <returns>Content string of file</returns>
        string LoadFile(string filepath);

        /// <summary>
        /// Create absolute path to save image
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <returns>Result absolute path</returns>
        string CreateImageSavePath(string path);
    }
}
