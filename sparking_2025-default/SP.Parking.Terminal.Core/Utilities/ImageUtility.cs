using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Utilities
{
    public static class ImageUtility
    {
        public static byte[] ToByteArray(this System.Drawing.Image image, ImageFormat format)
        {
            if (image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static void Watermark(Bitmap bmp, string text)
        {
            if (bmp == null || string.IsNullOrEmpty(text))
                return;

            using (Font font = new Font("Arial", 16, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel))
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    using (var gp = new GraphicsPath())
                    {
                        using (var pen = new System.Drawing.Pen(ColorTranslator.FromHtml("#FFFB00"), 1))
                        {
                            pen.LineJoin = LineJoin.Round;
                            var sf = new StringFormat();
                            sf.Alignment = StringAlignment.Far;
                            sf.LineAlignment = StringAlignment.Far;
                            gp.AddString(text, font.FontFamily, (int)font.Style, font.SizeInPoints,
                                         new Rectangle(0, 0, bmp.Width, bmp.Height), sf);
                            Rectangle fr = new Rectangle(0, bmp.Height - font.Height, bmp.Width, font.Height);
                            using (System.Drawing.Drawing2D.LinearGradientBrush b = new System.Drawing.Drawing2D.LinearGradientBrush(fr,
                                                    ColorTranslator.FromHtml("#FFFB00"),
                                                    ColorTranslator.FromHtml("#FFFB00"),
                                                    90))
                            {
                                gr.SmoothingMode = SmoothingMode.AntiAlias;
                                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                gr.DrawPath(pen, gp);
                                gr.FillPath(b, gp);
                            }
                            sf.Dispose();
                        }
                    }
                }
            }
        }
    }
}
