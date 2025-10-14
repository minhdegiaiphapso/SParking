using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using AForge.Math;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;

namespace LicensePlateRecognition
{
    public class ImagePlate
    {
        Bitmap image_input;
        Bitmap backgroundFrame;
        public Bitmap p;

        double hor_coe = 0;//he so nhan cua max value hor
        double ver_coe = 0;//he so nhan cua max value ver
        int number_coe = 0;//so hang blank
        double plate_ratio = 0; // ti le bien so xe
        int min_freq = 0;

        double[] energy_array = new double[256];
        //===================properties=========================
        private Bitmap image;
        private static Bitmap plate;
        private string backgroundImagePath;

        public Bitmap Plate
        {
            get
            {
                return plate;
            }
            set
            {
                plate = value;
            }
        }
        public Bitmap Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
            }
        }
        //====================Init============================
        public ImagePlate(Bitmap image, string backgroundImagePath)
        {
            Image = image;
            this.backgroundImagePath = backgroundImagePath;
        }
        public ImagePlate(string backgroundImagePath)
        {
            Plate = null;
            Image = null;
            this.backgroundImagePath = backgroundImagePath;
        }
        //===================method=========================
        public void Get_Plate()
        {
            image_input = Image;
            backgroundFrame = new Bitmap(backgroundImagePath); //Application.StartupPath + "\\anh\\nen.jpg"
            hor_coe = 0.6;//0.5
            ver_coe = 0.4;//0.4
            number_coe = 13;//13
            min_freq = 100;
            plate_ratio = 14;//14;  

            IFilter filt = new GrayscaleY();
            backgroundFrame = filt.Apply(backgroundFrame);
            image_input = filt.Apply(image_input);
            //IFilter f = new Threshold(180);
            //image_input = f.Apply(image_input);
            p = image_input;
            Subtract sub_img = new Subtract();
            sub_img.OverlayImage = backgroundFrame;
            Bitmap temp_img = sub_img.Apply(image_input);

            image_input = get_object(image_input, temp_img);

            image_input = fft(image_input);
            Plate = image_input;
        }

        protected unsafe Bitmap get_object(Bitmap img1, Bitmap img2)
        {
            int w = img1.Width;
            int h = img1.Height;
            BitmapData datascr1 = img1.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData datascr2 = img2.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr1 = (byte*)datascr1.Scan0.ToPointer();
            byte* scr2 = (byte*)datascr2.Scan0.ToPointer();
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++, scr1++, scr2++)
                {
                    if (*scr2 == 0)
                        *scr1 = 0;
                }
            }
            img1.UnlockBits(datascr1);
            img2.UnlockBits(datascr2);
            return img1;
        }

        private Bitmap fft(Bitmap img)
        {

            double w_ratio = 1.0 * img.Width / 512;//ti le theo chieu rong
            double h_ratio = 1.0 * img.Height / 256;//ti le theo chieu cao
            int top, bottom, left, right;

            IFilter filt = new ResizeBilinear(512, 256);
            img = filt.Apply(img);
            // create complex image from bitmap
            ComplexImage cimage = ComplexImage.FromBitmap(img);
            Complex[,] data_complex = cimage.Get_data;
            int h = data_complex.GetLength(0);
            int c = data_complex.GetLength(1);

            Complex[] data_row = new Complex[c];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < c; j++)
                    data_row[j] = data_complex[i, j];
                FourierTransform.FFT(data_row, FourierTransform.Direction.Forward);
                for (int j = 0; j < c; j++)
                    data_complex[i, j] = data_row[j];
            }

            double max = 0;
            int max_index = 0;
            for (int i = 0; i < h; i++)//h
            {
                double energy = 0;
                for (int j = min_freq; j < c; j++)//c
                {

                    energy += (data_complex[i, j].SquaredMagnitude);
                }



                //
                energy = energy * 2 / c;
                if (max < energy)
                {
                    max = energy;
                    max_index = i;
                }
                energy_array[i] = energy;
            }
            // horizon FFT

            int distance = 40;
            double thres = max * hor_coe;  //0.6
            bool f = false;
            int start, end;
            start = end = 0;
            int count = 0;
            for (int i = 0; i < h; i++)//bo 10 hang o bien{i=10,i<h-10}
            {
                if (energy_array[i] > thres)
                {
                    if (!f)
                    {
                        start = i;
                        f = true;
                    }
                    else
                        end = i;
                    count = 0;
                }
                else
                    count++;
                if (count > 7)     //video 10
                {
                    if ((end - start) < distance)
                    {
                        f = false;//tim lai start, end
                        if (i >= max_index)//tim lai nguong thres
                        {
                            max = energy_array[i];
                            for (int j = i + 1; j < h; j++)//bo bien duoi 10hang{j<h-10}
                            {
                                if (max < energy_array[j])
                                {
                                    max = energy_array[j];
                                    max_index = j;
                                }
                            }
                            thres = max * hor_coe;

                        }
                    }
                    else
                        break;
                }
            }
            top = (int)(start * h_ratio);
            bottom = (int)(end * h_ratio);
            //co resize anh truoc khi xu li FFT,luu y khi trich anh can nhan he so resize
            Bitmap extractimg = get_plate(img, start, end, 0, c - 1);

            //vertical fft
            filt = new ResizeBilinear(512, 64);
            extractimg = filt.Apply(extractimg);
            cimage = ComplexImage.FromBitmap(extractimg);
            data_complex = cimage.Get_data;
            h = data_complex.GetLength(0);
            c = data_complex.GetLength(1);
            Complex[] data_col = new Complex[h];
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < h; j++)
                    data_col[j] = data_complex[j, i];
                FourierTransform.FFT(data_col, FourierTransform.Direction.Forward);
                for (int j = 0; j < h; j++)
                    data_complex[j, i] = data_col[j];
            }
            max = 0;
            max_index = 0;
            //dinh nghia lai mang theo chieu rong w=512
            energy_array = new double[512];
            for (int i = 0; i < c; i++)
            {
                double energy = 0;
                for (int j = 0; j < h; j++)
                {
                    energy += (data_complex[j, i].SquaredMagnitude);
                }
                energy = energy * 2 / h;//c;
                if (max < energy)
                {
                    max = energy;
                    max_index = i;
                }
                energy_array[i] = energy;
            }
            distance = 50;
            thres = max * ver_coe;
            f = false;
            start = end = 0;
            count = 0;
            for (int i = 0; i < c; i++)//bo 2 bien ngang cua anh 10 hang{i=10,i<c-10}
            {
                if (energy_array[i] > thres)
                {
                    if (!f)
                    {
                        start = i;
                        f = true;
                    }
                    else
                        end = i;
                    count = 0;
                }
                else
                    count++;
                if (count > number_coe)      //15 video
                {
                    if ((end - start) < distance)//cap nhat lai start,end
                    {
                        f = false;
                        if (i >= max_index)//tim lai nguong thres
                        {
                            max = energy_array[i];
                            for (int j = i + 1; j < c; j++)//bo bien duoi 10hang{j<c-10}
                            {
                                if (max < energy_array[j])
                                {
                                    max = energy_array[j];
                                    max_index = j;
                                }
                            }
                            thres = max * ver_coe;
                        }
                    }
                    else
                        break;
                }
            }
            left = (int)(start * w_ratio);
            right = (int)(end * w_ratio);
            top = (int)(bottom - (right - left) * plate_ratio / 19); //ratio=14 danh cho load data
            //trich anh ban dau input_image
            extractimg = get_plate(image_input, top, bottom, left, right);
            return extractimg;

        }

        protected unsafe Bitmap get_plate(Bitmap img, int top, int bottom, int left, int right)
        {
            if (top < 0)
                top = 0;
            int h = bottom - top + 1;
            int w = right - left + 1;
            BitmapData dataimg = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            Bitmap dstimg = AForge.Imaging.Image.CreateGrayscaleImage(w, h);
            BitmapData datadst = dstimg.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            int stepimg = dataimg.Stride - w;
            int stepdst = datadst.Stride - w;
            int xmin = left;
            int xmax = right;
            int ymin = top;
            int ymax = bottom;
            byte* scr = (byte*)dataimg.Scan0.ToPointer() + ymin * dataimg.Stride + xmin;
            byte* dst = (byte*)datadst.Scan0.ToPointer();
            for (int i = ymin; i <= ymax; i++)
            {
                for (int j = xmin; j <= xmax; j++, scr++, dst++)
                    *dst = *scr;
                scr += stepimg;
                dst += stepdst;
            }
            img.UnlockBits(dataimg);
            dstimg.UnlockBits(datadst);
            return dstimg;

        }
    }
}
