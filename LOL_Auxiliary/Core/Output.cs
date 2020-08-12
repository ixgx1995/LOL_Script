using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOL_Auxiliary
{
    /// <summary>
    /// 图像获取和文件获取类
    /// </summary>
    public class Output
    {
        #region 构造函数

        Thread t;
        IntPtr hwnd;

        public Output(IntPtr hwnd)
        {
            this.hwnd = hwnd;
        }

        #endregion

        #region GetWindowCapture的dll引用

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(
         IntPtr hdc // handle to DC
         );
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(
         IntPtr hdc,         // handle to DC
         int nWidth,      // width of bitmap, in pixels
         int nHeight      // height of bitmap, in pixels
         );
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
         IntPtr hdc,           // handle to DC
         IntPtr hgdiobj    // handle to object
         );
        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(
         IntPtr hdc           // handle to DC
         );
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(
         IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
         IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
         UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
         );
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(
         IntPtr hwnd
         );
        #endregion

        /// <summary>
        /// 获得窗体屏幕图片
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public Bitmap GetWindowCapture()
        {
            IntPtr hscrdc = GetWindowDC(hwnd);
            Rectangle windowRect = new Rectangle();
            windowshandle.GetWindowRect(hwnd, ref windowRect);
            int width = Math.Abs(windowRect.X - windowRect.Width);
            int height = Math.Abs(windowRect.Y - windowRect.Height);
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hwnd, hmemdc, 0);
            Bitmap bmp = Image.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//删除用过的对象
            DeleteDC(hmemdc);//删除用过的对象
            return bmp;
        }

        /// <summary>
        /// 获得窗体屏幕图片(此方法有宽度和分辨率不正确性)
        /// </summary>
        public Bitmap GetScreenData()
        {
            windowshandle.Rect rect = new windowshandle.Rect();
            Bitmap QQPic = new Bitmap(1, 1);
            if (!hwnd.Equals(IntPtr.Zero))
            {
                windowshandle.GetWindowRect(hwnd, ref rect);  //获得目标窗体的大小
                QQPic = new Bitmap(rect.Right - rect.Left + 300, rect.Bottom - rect.Top + 150);
                Graphics g1 = Graphics.FromImage(QQPic);
                IntPtr hdc1 = windowshandle.GetDC(hwnd);
                IntPtr hdc2 = g1.GetHdc();  //得到Bitmap的DC
                windowshandle.BitBlt(hdc2, 0, 0, rect.Right - rect.Left + 300, rect.Bottom - rect.Top + 150, hdc1, 0, 0, windowshandle.SRCCOPY);
                g1.ReleaseHdc(hdc2);  //释放掉Bitmap的DC
            }
            return QQPic;
        }

        /// <summary>
        /// 截取指定窗口
        /// </summary>
        /// <returns></returns>
        public Bitmap GetRunAPPImage()
        {
            windowshandle.Rect rect = new windowshandle.Rect();
            Image img;
            try
            {
                windowshandle.GetWindowRect(hwnd, ref rect); //取控件的矩形
                img = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
                Graphics g = Graphics.FromImage(img);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, Screen.FromHandle(hwnd).Bounds.Size);
                return new Bitmap(img);
            }
            catch (Exception ex)
            {
                MessageBox.Show("出错:" + ex);
            }
            return null;
        }

        /// <summary>
        /// 截取图片
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public static Bitmap GetImgPart(int X, int Y,int width,int height,bool IsSave,string SaveName)
        {
            Point p = Control.MousePosition;
            Bitmap image = new Bitmap(width, height);

            Graphics imgGraphics = Graphics.FromImage(image);
            //设置截屏区域

            imgGraphics.CopyFromScreen(X, Y, 0, 0, new Size(width, height));
            if (IsSave)
                image.Save(System.Windows.Forms.Application.StartupPath + string.Format(@"\ContrastImg\{0}.jpg", SaveName), ImageFormat.Tiff);
            return image;
        }

        /// <summary>
        /// 从大图中截取一部分图片
        /// </summary>
        /// <param name="fromImagePath">源图片</param>        
        /// <param name="offsetX">从偏移X坐标位置开始截取</param>
        /// <param name="offsetY">从偏移Y坐标位置开始截取</param>
        /// <param name="toImagePath">保存图片地址</param>
        /// <param name="width">保存图片的宽度</param>
        /// <param name="height">保存图片的高度</param>
        /// <param name="name">保存的图片名字</param>
        /// <returns></returns>
        public Bitmap CaptureImage(Bitmap bmp, int offsetX, int offsetY, int width, int height,string name)
        {
            //创建新图位图
            Bitmap bitmap = new Bitmap(width, height);
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            //截取原图相应区域写入作图区
            graphic.DrawImage(bmp, 0, 0, new Rectangle(offsetX, offsetY, width, height), GraphicsUnit.Pixel);
            //从作图区生成新图
            Image saveImage = Image.FromHbitmap(bitmap.GetHbitmap());
            //保存图片
            saveImage.Save(System.Windows.Forms.Application.StartupPath + @"\ContrastImg\" + name + ".jpg", ImageFormat.Tiff);
            //释放资源   
            graphic.Dispose();
            bitmap.Dispose();
            return new Bitmap(saveImage);
        }

        /// <summary>
        /// 通过FileStream 来打开文件，这样就可以实现不锁定Image文件，到时可以让多用户同时访问Image文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadImageFile(string Name)
        {
            string path = System.Windows.Forms.Application.StartupPath + string.Format(@"\ContrastImg\{0}.jpg", Name);
            FileStream fs = File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
            fs.Close();
            Bitmap bit = new Bitmap(result);
            return bit;
        }

        /// <summary>
        /// 返回相应分辨率和大小的图片
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static Bitmap GetSetResolutionImg(Bitmap bmp,int Index)
        {
            Bitmap img2 = new Bitmap(bmp.Width - Index, bmp.Height - Index, PixelFormat.Format24bppRgb);
            img2.SetResolution(Properties.Settings.Default.Resolving_Width, Properties.Settings.Default.Resolving_Width);
            using (Graphics g = Graphics.FromImage(img2))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(0, 0, img2.Width, img2.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
                g.Dispose();
            }
            return img2;
        }

        
    }
}
