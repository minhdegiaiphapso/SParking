using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using AForge;
using AForge.Imaging;
using AForge.Math;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using System.IO;
using System.Drawing.Imaging;

namespace LicensePlateRecognition
{
    public class NeuronNetwork
    {
        //network
        const int num_in_node = 200;   //200 ngo vao
        const int numlayers = 3;       //3 lop
        const int num_hide_node = 100;      //50 neuron o lop hiden
        //maximum layer
        const int max_layer = 200;
        float alpha = 0.05F;
        Random rnd = new Random();

        int[] layer = new int[numlayers];
        float[, ,] weight = new float[numlayers, max_layer, max_layer];//[vi tri lop hien tai,vi tri noron lop hien tai,vi tri trong so tu noron lop truoc do]
        float[, ,] input_set = null;

        //mang chua ngo vao va ra tuong ung tai mot thoi diem
        float[] current_input = new float[num_in_node];
        //ngo ra tai moi lop
        float[,] node_output = new float[numlayers, max_layer];
        float[,] errors = new float[numlayers, max_layer];

        //thong so nhan dang
        const int char_output = 21;
        const int num_output = 10;
        const int char_hide = 100;//so noron lop an cho mang neuron chu
        const int num_hide = 100;//so noron lop an cho mang neuron so
        int[] layer_char = new int[numlayers];
        int[] layer_num = new int[numlayers];
        float[, ,] weight_char = new float[numlayers, max_layer, max_layer];
        float[, ,] weight_num = new float[numlayers, max_layer, max_layer];
        float[,] outnode_char = new float[numlayers, max_layer];
        float[,] outnode_num = new float[numlayers, max_layer];
        bool flag_0;
        bool flag_2;
        bool flag_5;

        //sample nhan dang
        char[] sample_char = new char[21];
        char[] sample_num = new char[10];
        string character = "ABCDEFGHKLMNPRSTUVXYZ";
        string number = "0123456789";

        //==================properties========================
        private Bitmap[] ImageArr;
        private string LicenseText;

        public string ResultText
        {
            get
            {
                return LicenseText;
            }
            set
            {
                LicenseText = value;
            }
        }
        public Bitmap[] ImageArray
        {
            get
            {
                return ImageArr;
            }
            set
            {
                ImageArr = value;
            }
        }
        //==================properties========================

        //==================init========================
        public NeuronNetwork()
        {
            LicenseText = null;
            ImageArr = new Bitmap[8];
            form_network_char();
            form_network_num();
            character.CopyTo(0, sample_char, 0, 21);
            number.CopyTo(0, sample_num, 0, 10);
        }
        //==================init========================

        //==================method============================
        public void LoadNetworkChar(string filepath)
        {
            string line;
            char[] w_char = new char[20];
            string weight_text = "";
            int title_length, weight_length;
            StreamReader network_load_file_stream = new StreamReader(filepath);
            string network_file_name = Path.GetFileNameWithoutExtension(filepath);

            for (int i = 0; i < 7; i++)
                network_load_file_stream.ReadLine();
            //lay gia tri alpha
            weight_text = "";
            line = network_load_file_stream.ReadLine();
            title_length = ("Sigmoid Slope	= ").Length;
            weight_length = line.Length - title_length;
            line.CopyTo(title_length, w_char, 0, weight_length);
            for (int counter = 0; counter < weight_length; counter++)
                weight_text = weight_text + w_char[counter].ToString();
            alpha = (float)Convert.ChangeType(weight_text, typeof(float));
            for (int i = 1; i < numlayers; i++)
                for (int j = 0; j < layer_char[i]; j++)
                    for (int k = 0; k < layer_char[i - 1]; k++)
                    {
                        weight_text = "";
                        line = network_load_file_stream.ReadLine();
                        title_length = ("Weight[" + i.ToString() + " , " + j.ToString() + " , " + k.ToString() + "] = ").Length;
                        weight_length = line.Length - title_length;
                        line.CopyTo(title_length, w_char, 0, weight_length);
                        for (int counter = 0; counter < weight_length; counter++)
                            weight_text = weight_text + w_char[counter].ToString();
                        weight_char[i, j, k] = (float)Convert.ChangeType(weight_text, typeof(float));
                    }
            network_load_file_stream.Close();
        }

        public void LoadNetworkNum(string filepath)
        {
            string line;
            char[] w_char = new char[20];
            string weight_text = "";
            int title_length, weight_length;
            StreamReader network_load_file_stream = new StreamReader(filepath);
            string network_file_name = Path.GetFileNameWithoutExtension(filepath);

            for (int i = 0; i < 7; i++)
                network_load_file_stream.ReadLine();
            //lay gia tri alpha
            weight_text = "";
            line = network_load_file_stream.ReadLine();
            title_length = ("Sigmoid Slope	= ").Length;
            weight_length = line.Length - title_length;
            line.CopyTo(title_length, w_char, 0, weight_length);
            for (int counter = 0; counter < weight_length; counter++)
                weight_text = weight_text + w_char[counter].ToString();
            alpha = (float)Convert.ChangeType(weight_text, typeof(float));
            for (int i = 1; i < numlayers; i++)
                for (int j = 0; j < layer_num[i]; j++)
                    for (int k = 0; k < layer_num[i - 1]; k++)
                    {
                        weight_text = "";
                        line = network_load_file_stream.ReadLine();
                        title_length = ("Weight[" + i.ToString() + " , " + j.ToString() + " , " + k.ToString() + "] = ").Length;
                        weight_length = line.Length - title_length;
                        line.CopyTo(title_length, w_char, 0, weight_length);
                        for (int counter = 0; counter < weight_length; counter++)
                            weight_text = weight_text + w_char[counter].ToString();
                        weight_num[i, j, k] = (float)Convert.ChangeType(weight_text, typeof(float));
                    }
            network_load_file_stream.Close();
        }

        //-------------------------------------------------------------------------
        public void recognition()
        {
            input_set = new float[1, num_in_node, 1];
            //detect letter
            string result = "";

            for (int i = 0; i < ImageArray.Length; i++)
            {
                get_pixel(ImageArray[i], 0, 0);
                get_input(0, 0);
                if (i == 4)
                {
                    result += " ";
                }
                if (i == 2)
                {
                    result += "-";
                    cal_output_char();
                    result += detect_char(i);

                }
                else
                {
                    cal_output_num();
                    result += detect_num(i);
                }
            }
            LicenseText = result;


        }

        //==================function============================
        private void form_network_char()
        {
            layer_char[0] = num_in_node;
            layer_char[numlayers - 1] = char_output;
            for (int i = 1; i < numlayers - 1; i++)
                layer_char[i] = char_hide;
        }
        private void form_network_num()
        {
            layer_num[0] = num_in_node;
            layer_num[numlayers - 1] = num_output;
            for (int i = 1; i < numlayers - 1; i++)
                layer_num[i] = num_hide;
        }

        //-------------------------------------------------------------------------
        protected unsafe void get_pixel(Bitmap img, int line, int letter)
        {
            BitmapData imgdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr = (byte*)imgdata.Scan0.ToPointer();

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++, scr++)
                {
                    if (*scr == 255)
                        input_set[line, i * 10 + j, letter] = 1;
                    else
                        input_set[line, i * 10 + j, letter] = 0;
                }

            }
            img.UnlockBits(imgdata);
        }
        //-------------------------------------------------------------------------
        private void get_input(int set_line, int set_num)
        {
            for (int i = 0; i < num_in_node; i++)
                current_input[i] = input_set[set_line, i, set_num];
        }
        //-------------------------------------------------------------------------
        private void cal_output_char()
        {
            float f_net;
            int num_weight;
            for (int i = 0; i < numlayers; i++)
                for (int j = 0; j < layer_char[i]; j++)
                {
                    f_net = 0.0F;
                    if (i == 0) num_weight = 1; //lop input
                    else num_weight = layer_char[i - 1];

                    for (int k = 0; k < num_weight; k++)
                    {
                        if (i == 0)
                            f_net = current_input[j];
                        else
                            f_net += outnode_char[i - 1, k] * weight_char[i, j, k];
                    }
                    outnode_char[i, j] = sigmoid(f_net);
                }
        }
        //-------------------------------------------------------------------------
        private void cal_output_num()
        {
            float f_net;
            int num_weight;
            for (int i = 0; i < numlayers; i++)
                for (int j = 0; j < layer_num[i]; j++)
                {
                    f_net = 0.0F;
                    if (i == 0) num_weight = 1; //lop input
                    else num_weight = layer_num[i - 1];

                    for (int k = 0; k < num_weight; k++)
                    {
                        if (i == 0)
                            f_net = current_input[j];
                        else
                            f_net += outnode_num[i - 1, k] * weight_num[i, j, k];
                    }
                    outnode_num[i, j] = sigmoid(f_net);
                }
        }
        //-------------------------------------------------------------------------
        private float sigmoid(float f_net)
        {
            //float result=(float)(1/(1+Math.Exp (-1*slope*f_net)));		//Unipolar
            float result = (float)((2 / (1 + Math.Exp(-1 * alpha * f_net))) - 1);		//Bipolar			
            return result;
        }

        //-------------------------------------------------------------------------
        private float sigmoid_derivative(float result)
        {
            //float derivative=(float)(alpha*result*(1-result));					//Unipolar
            float derivative = (float)(alpha * 0.5F * (1 - Math.Pow(result, 2)));			//Bipolar			
            return derivative;
        }
        //-------------------------------------------------------------------------
        private char detect_char(int index1)
        {
            int index = 0;
            float max = 0;


            for (int i = 0; i < char_output; i++)
            {
                if (max < outnode_char[numlayers - 1, i])
                {
                    max = outnode_char[numlayers - 1, i];
                    index = i;
                }
            }
            if (max > 0)
                return sample_char[index];
            else
                return '?';

        }
        //-------------------------------------------------------------------------
        private char detect_num(int index1)
        {
            int index = 0;
            float max = 0;

            for (int i = 0; i < num_output; i++)
            {
                if (max < outnode_num[numlayers - 1, i])
                {
                    max = outnode_num[numlayers - 1, i];
                    index = i;
                }
            }
            if (index == 5 || index == 6)
            {
                confusion_5_6(ImageArray[index1]);
                if (flag_5)
                    index = 5;
                else
                    index = 6;
            }
            else if (index == 7)
            {
                confusion_2_7(ImageArray[index1]);
                if (flag_2)
                    index = 2;
            }
            else if (index == 0 || index == 8)
            {
                confusion_0_8(ImageArray[index1]);
                if (flag_0)
                    index = 0;
                else
                    index = 8;
            }
            if (max > 0)
                return sample_num[index];
            else
                return '?';


        }
        //-------------------------------------------------------------------------
        protected unsafe void confusion_5_6(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            BitmapData dataimg = img.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr = (byte*)dataimg.Scan0.ToPointer() + 12 * dataimg.Stride;
            flag_5 = false;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < w / 2; j++, scr++)
                {
                    if (*scr == 255)
                    {
                        flag_5 = false;
                        break;
                    }
                    flag_5 = true;
                }
                if (flag_5)
                    break;
            }

            img.UnlockBits(dataimg);
        }
        //-------------------------------------------------------------------------
        protected unsafe void confusion_2_7(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            int count = 0;
            BitmapData dataimg = img.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr = (byte*)dataimg.Scan0.ToPointer() + 17 * dataimg.Stride;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < w; j++, scr++)
                {
                    if (*scr == 255)
                    {
                        count++;
                    }

                }
                if (count >= 5)
                {
                    flag_2 = true;
                    break;
                }
                else
                {
                    count = 0;
                    flag_2 = false;
                }
            }
            img.UnlockBits(dataimg);
        }
        //-------------------------------------------------------------------------
        protected unsafe void confusion_0_8(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            BitmapData dataimg = img.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr = (byte*)dataimg.Scan0.ToPointer() + 5 * dataimg.Stride + 5;
            for (int i = 0; i < 10; i++)//do cao can quet
            {

                if (*scr == 255)
                {
                    flag_0 = false;
                    break;
                }
                scr += dataimg.Stride;
                flag_0 = true;
            }
            img.UnlockBits(dataimg);
        }

        //==================function============================
    }
}
