using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using AForge.Imaging;
using AForge.Math;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;

namespace LicensePlateRecognition
{
    public class LicensePlate
    {
        //bien so
        int numline = 2;//so dong        
        int[] numword_eachline;//mang chua so ky tu moi dong

        //image
        Bitmap input_image;
        int input_width;
        int input_height;
        Bitmap process_image;
        //xac dinh hang,cot
        int[,] his_hor = null;
        int[] his_ver = null;
        //xac dinh top,bottom,left,right
        int[, ,] num_hor = null;    //hang,so ki tu,left+right
        int[,] num_ver = null;     //hang,top+bottom

        public Bitmap p;
        //===================properties=========================
        private Bitmap plate;
        private static Bitmap[] ImageArr;

        public Bitmap  Plate
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
        //====================Init============================
        public  LicensePlate(Bitmap plate)
        {
            Plate = plate;
            
        }
        public  LicensePlate()
        {
            Plate = null;
            ImageArray = null;
        }

        //================method=============================
        public void Split()
        {
            input_image = Plate;


            process_imginput(input_image);


            //define array
            numline = 2;
            his_ver = new int[input_height];
            his_hor = new int[numline, input_width];
            num_ver = new int[numline, 2];
            num_hor = new int[numline, 35, 2];//cap phat mang max=35 phan tu


            define_line(process_image, 10);


            numword_eachline = new int[numline];
            for (int i = 0; i < numline; i++)
                numword_eachline[i] = 4;


            define_pos(process_image, 7);
            //xac dinh chinh xac so ki tu moi hang

            Bitmap[] imgArr = new Bitmap[8];
            ImageArray = new Bitmap[8];
            int k = 0;
            for (int i = 0; i < numline; i++)
                for (int j = 0; j < numword_eachline[i]; j++)
                {
                    imgArr[k] = extractImg(process_image, num_ver[i, 0], num_ver[i, 1], num_hor[i, j, 0], num_hor[i, j, 1]);
                    k++;
                }

            //tien xu ly, resize cac ky tu
            FiltersSequence filts = new FiltersSequence();
            filts.Add(new Dilatation());
            filts.Add(new Erosion());
            IFilter resize = new ResizeBilinear(10, 20);

            for (int i = 0; i < imgArr.Length; i++)
                if (imgArr[i].Width < 3 || imgArr[i].Height < 5) imgArr[i] = resize.Apply(imgArr[i]);

            for (int i = 0; i < imgArr.Length; i++)
            {
                //resize

                imgArr[i] = filts.Apply(imgArr[i]);
                BlobCounter blobs = new BlobCounter(imgArr[i]);
                Blob[] words = blobs.GetObjects(imgArr[i]);
                foreach (Blob word in words)
                    imgArr[i] = word.Image;

                imgArr[i] = resize.Apply(imgArr[i]);


                ImageArray[i] = imgArr[i];
            }


        }

        private void define_line(Bitmap img, int thres)
        {
            int i, j;
            bool f;
            //vertical analyse
            int w = img.Width;
            int h = img.Height;
            /*
            BitmapData dataimg = img.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* scr = (byte*)dataimg.Scan0.ToPointer();
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++, scr++)
                    his_ver[i] += *scr;
            }
            */

            for (i = 0; i < img.Height; i++)
            {
                for (j = 0; j < img.Width; j++)
                {
                    Color cr = img.GetPixel(j, i);
                    his_ver[i] += Convert.ToInt32(cr.R) / 255;
                }
            }

            //get top,bottom

            i = 5;//bo bien ngang tren cua bien so
            j = 0;
            f = false;
            while (i < h)
            {
                if (!f && his_ver[i] > thres)
                {
                    num_ver[j, 0] = i;
                    f = true;
                }
                if (f && ((his_ver[i] < thres) || i == h - 6))//bo bien ngang duoi bien so
                {
                    if ((i - num_ver[j, 0]) > 30)//check chieu cao cua ki tu
                    {
                        num_ver[j, 1] = i;
                        j++;
                    }
                    f = false;
                }
                i++;
                if (j == numline)
                    break;
            }

        }

        private void define_pos(Bitmap img, int thres)
        {
            const int min_width = 20;//chieu rong toi thieu cac chu tru so 1
            const int min_pulse = 40;//so pixel toi thieu de kiem tra so 1
            const int min_1 = 5;//do rong toi thieu neu ki tu la so 1

            int i, j;
            bool f = false;
            bool check_max = false;
            for (i = 0; i < numline; i++)
            {
                histogram_ver(img, i, num_ver[i, 0], num_ver[i, 1]);
            }
            //get left,right      

            for (int k = 0; k < numline; k++)
            {
                i = 27;
                j = 0;

                while (i < img.Width - 27)
                {
                    if (his_hor[k, i] > thres && !f)
                    {
                        num_hor[k, j, 0] = i;
                        f = true;
                    }
                    if (f && his_hor[k, i] > min_pulse)
                        check_max = true;
                    if (f && his_hor[k, i] < thres)
                    {
                        if ((i - num_hor[k, j, 0]) > min_width || (check_max && (i - num_hor[k, j, 0]) > min_1))
                        {
                            num_hor[k, j, 1] = i - 1;
                            j++;
                        }
                        f = false;
                        check_max = false;
                    }
                    i++;
                    if (j == numword_eachline[k] || i == img.Width - 27)
                        break;
                }
            }

        }

        protected unsafe Bitmap extractImg(Bitmap scr_img, int top, int bottom, int left, int right)
        {
            if (top < 0)
                top = 0;
            int width = right - left + 1;
            int height = bottom - top + 1;
            if (width <= 0 || height <= 0) { width = 1; height = 1; }
            BitmapData scrdata = scr_img.LockBits(new Rectangle(0, 0, scr_img.Width, scr_img.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            Bitmap dst_img = AForge.Imaging.Image.CreateGrayscaleImage(width, height);
            BitmapData dstdata = dst_img.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            //copy pixel
            int xmin = left;
            int xmax = right;
            int ymin = top;
            int ymax = bottom;
            int scrstep = scrdata.Stride - width;
            int dststep = dstdata.Stride - width;
            byte* scr = (byte*)scrdata.Scan0.ToPointer() + ymin * scrdata.Stride + xmin;
            byte* dst = (byte*)dstdata.Scan0.ToPointer();

            for (int i = ymin; i <= ymax; i++)
            {
                for (int j = xmin; j <= xmax; j++, scr++, dst++)
                {
                    *dst = *scr;
                }
                scr += scrstep;
                dst += dststep;
            }
            scr_img.UnlockBits(scrdata);
            dst_img.UnlockBits(dstdata);
            return dst_img;
        }

        private void histogram_ver(Bitmap img, int row, int top, int bottom)
        {
            Point[] values = new Point[img.Width];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = top; j <= bottom; j++)
                {
                    Color cr = img.GetPixel(i, j);
                    his_hor[row, i] += Convert.ToInt16(cr.R) / 255;
                }
                values[i] = new Point(i, bottom - his_hor[row, i]);
            }

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

        private void process_imginput(Bitmap img)
        {
            //grayscale
            IFilter way_filt;//= new GrayscaleY();
            //process_image = way_filt.Apply(img);
            //resize
            way_filt = new ResizeBilinear(300, 200);
            process_image = way_filt.Apply(img);
            //process_image = get_plate(process_image, 10, 210, 30, 330);
            input_image = way_filt.Apply(input_image);
            p = process_image;
            //threshold
            way_filt = new Threshold(125);
            process_image = way_filt.Apply(process_image);


            //K-means
            process_image = kmean(process_image);


            //invert
            way_filt = new Invert();
            process_image = way_filt.Apply(process_image);



            //way_filt = new Median();
            //process_image = way_filt.Apply(process_image);

            //way_filt = new AdaptiveSmooth();
            //process_image = way_filt.Apply(process_image);
            //filter k
            BlobsFiltering filter = new BlobsFiltering();
            filter.MinHeight = 25;//50
            filter.MinWidth = 10;
            filter.MaxHeight = 100;
            filter.ApplyInPlace(process_image);
            //p = process_image;
            input_width = process_image.Width;
            input_height = process_image.Height;


        }

        protected unsafe Bitmap kmean(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            BitmapData datascr = img.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            int[,] label = new int[h, w];
            int dim = 3;
            int[] counts = new int[dim];
            double[] c = new double[dim];
            double[] c1 = new double[dim];
            double old_error = 0;
            double error = 0;
            c1[0] = c[0] = 120;
            c1[1] = c[1] = 180;
            c1[2] = c[2] = 210;

            int step = datascr.Stride - w;
            int end, start;

            for (int p = 0; p < 8; p++)
            {
                start = p * h / 8;
                end = start + h / 8;
                do
                {
                    byte* scr = (byte*)datascr.Scan0.ToPointer() + datascr.Stride * start;
                    old_error = error;
                    error = 0;
                    for (int i = 0; i < dim; i++)
                    {
                        counts[i] = 0;
                        c1[i] = 0;
                    }
                    for (int i = start; i < end; i++)
                    {
                        for (int j = 0; j < w; j++, scr++)
                        {
                            double min_dist = double.MaxValue;
                            for (int k = 0; k < dim; k++)
                            {
                                double dist = 0;
                                dist = Math.Pow(*scr - c[k], 2);
                                if (dist < min_dist)
                                {
                                    min_dist = dist;
                                    label[i, j] = k;
                                }
                            }
                            c1[label[i, j]] += *scr;
                            counts[label[i, j]]++;
                            error += min_dist;
                        }

                        scr += step;
                    }
                    for (int q = 0; q < dim; q++)
                    {
                        c[q] = (counts[q] != 0) ? c1[q] / counts[q] : c1[q];
                    }
                    double temp = Math.Abs(error - old_error);

                } while (Math.Abs(error - old_error) > 0.001);
            }
            Bitmap dstimg = AForge.Imaging.Image.CreateGrayscaleImage(w, h);
            BitmapData datadst = dstimg.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* dst = (byte*)datadst.Scan0.ToPointer();
            byte* org = (byte*)datascr.Scan0.ToPointer();
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++, org++, dst++)
                {
                    if (label[i, j] == 0)//==0
                        *dst = 0;//*org;
                    else
                        *dst = 255;//=255
                }
                dst += step;
                org += step;
            }
            img.UnlockBits(datascr);
            dstimg.UnlockBits(datadst);
            return dstimg;
        }
    }
}
