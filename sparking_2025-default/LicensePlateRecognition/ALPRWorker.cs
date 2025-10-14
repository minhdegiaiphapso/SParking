using System.Drawing;
using System.IO;

namespace LicensePlateRecognition
{
    public class ALPRWorker : IALPRWorker
    {
        ImagePlate _imagePlate;
        LicensePlate _licensePlate;
        NeuronNetwork _neuronNetwork;

        public ALPRWorker(string backgroundImagePath = @"Resource\background_image.jpg")
        {
            _imagePlate = new ImagePlate(backgroundImagePath);
            _licensePlate = new LicensePlate();
            _neuronNetwork = new NeuronNetwork();
            //_neuronNetwork.LoadNetworkChar(@"Resource\character_weight.ann");
            //_neuronNetwork.LoadNetworkNum(@"Resource\number_weight.ann");
        }

        public string LicensePlateRecognize(Bitmap image)
        {
            _imagePlate.Image = image;
            // Get plate image from original image.
            _imagePlate.Get_Plate();
            _licensePlate.Plate = _imagePlate.Plate;
            // Split character images from plate image.
            _licensePlate.Split();
            // Recognize text from character images using ANN.
            _neuronNetwork.ImageArray = _licensePlate.ImageArray;
            _neuronNetwork.recognition();
            // Return result
            return _neuronNetwork.ResultText;
        }

        public string LicensePlateRecognize(string filepath)
        {
            Bitmap image = new Bitmap(filepath);
            return LicensePlateRecognize(image);
        }

        public string LicensePlateRecognize(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData, 0, imageData.Length))
            {
                Bitmap image = new Bitmap(ms);
                return LicensePlateRecognize(image);
            }
        }
    }
}
