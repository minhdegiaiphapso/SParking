//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Windows.Interop;

//namespace SP.Parking.Terminal.Wpf.UI
//{
//    // http://social.msdn.microsoft.com/Forums/en-US/d4e92dd4-01d7-45b5-ba9e-a623658e1bf9/is-there-any-way-to-create-a-label-with-a-transparent-background-on-top-of-a-video-control?forum=winforms
//     public class LayeredWindow : Form
//    {
//        public LayeredWindow(Func<Bitmap> bmp)
//        {
//            this.TopMost = true;
//            this.ShowInTaskbar = false;
//            this.Shown += (s, e) => this.UpdateLayered(bmp());
//        }
//        protected override CreateParams CreateParams
//        {
//            get
//            {
//                var p = base.CreateParams;
//                p.ExStyle |= 0x80000 | 0x00000080;  // WS_EX_LAYERED | WS_EX_TOOLWINDOW 
//                return p;
//            }
//        }
//        public void UpdateLayered(Bitmap bmp)
//        {
//            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
//                throw new ArgumentException("32bpp with alpha-channel required");

//            IntPtr screenDc = GetDC(new HandleRef());
//            IntPtr memDc = CreateCompatibleDC(new HandleRef(null, screenDc));

//            IntPtr hBmp = bmp.GetHbitmap(Color.FromArgb(0));
//            SelectObject(new HandleRef(null, memDc), hBmp);

//            this.Size = bmp.Size;

//            var blendFunc = new BLENDFUNCTION
//            {
//                BlendOp = 0,
//                BlendFlags = 0,
//                SourceConstantAlpha = 0xff,
//                AlphaFormat = 1
//            };
//            bool flag = UpdateLayeredWindow(
//                this.Handle,
//                screenDc,
//                new POINT { X = this.Left, Y = this.Top },   // new location
//                new POINT { X = this.Width, Y = this.Height },    // new size
//                memDc,
//                new POINT { X = 0, Y = 0 },  // source location
//                0,
//                ref blendFunc,
//                2);
//            ReleaseDC(new HandleRef(), new HandleRef(null, memDc));
//            ReleaseDC(new HandleRef(), new HandleRef(null, screenDc));
//        }

//        /// <summary>
//        /// sets the owner of a System.Windows.Forms.Form to a System.Windows.Window
//        /// </summary>
//        /// <param name="form"></param>
//        /// <param name="owner"></param>
//        public void SetOwner(System.Windows.Window owner)
//        {
//            WindowInteropHelper helper = new WindowInteropHelper(owner);
//            SetWindowLong(new HandleRef(this, this.Handle), -8, helper.Handle.ToInt32());
//        }

//        [DllImport("user32.dll")]
//        private static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

//        [DllImport("user32.dll")]
//        static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, POINT pptDst, 
//            POINT pSizeDst, IntPtr hdcSrc, POINT pptSrc, int crKey, ref BLENDFUNCTION pBlend, 
//            int dwFlags);
//        [StructLayout(LayoutKind.Sequential)]
//        struct BLENDFUNCTION
//        {
//            public byte BlendOp;
//            public byte BlendFlags;
//            public byte SourceConstantAlpha;
//            public byte AlphaFormat;
//        }
//        [DllImport("gdi32.dll")] static extern IntPtr SelectObject(HandleRef hdc, IntPtr obj);
//        [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(HandleRef hDC);
//        [DllImport("user32.dll")] static extern IntPtr GetDC(HandleRef hWnd);
//        [DllImport("user32.dll")] static extern int ReleaseDC(HandleRef hWnd, HandleRef hDC);
//        [StructLayout(LayoutKind.Sequential)] public class POINT { public int X; public int Y; }
//    }
//}
