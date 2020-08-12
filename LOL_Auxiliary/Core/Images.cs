using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace LOL_Auxiliary
{
    public static class Images
    {
        /// <summary>
        /// 判断图像是否存在
        /// </summary>
        /// <param name="template"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static bool ContainsImg(this Bitmap template, Bitmap bmp)
        {
            // create template matching algorithm's instance // (set similarity threshold to 92.1%)
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.85f); // find all matchings with specified above similarity
            TemplateMatch[] matchings = tm.ProcessImage(bmp,template); // highlight found matchings

            return matchings.Length > 0;
        }
        /// <summary>
        /// 判断图像是否存在另外的图像中，并返回坐标(可精确区域)
        /// </summary>
        /// <param name="template"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Point ContainsGetPointPlus(this Bitmap template, Bitmap bmp,int x,int y,int width,int height)
        {
            //搜索区域
            Rectangle r = new Rectangle(x, y, width, height);

            // create template matching algorithm's instance // (set similarity threshold to 92.1%)
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.85f); // find all matchings with specified above similarity
            TemplateMatch[] matchings = tm.ProcessImage(bmp, template, r); // highlight found matchings
            BitmapData data = template.LockBits(new Rectangle(0, 0, template.Width, template.Height), ImageLockMode.ReadWrite, template.PixelFormat);
            Point p = new Point(-1,-1);

            if (matchings.Length > 0)
            {
                Drawing.Rectangle(data, matchings[0].Rectangle, Color.White);
                p = matchings[0].Rectangle.Location;
                template.UnlockBits(data);
            }

            return p;
        }

        /// <summary>
        /// 判断图像是否存在另外的图像中，并返回坐标
        /// </summary>
        /// <param name="template"></param>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Point ContainsGetPoint(this Bitmap template, Bitmap bmp)
        {
            // create template matching algorithm's instance // (set similarity threshold to 92.1%)
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.85f); // find all matchings with specified above similarity
            TemplateMatch[] matchings = tm.ProcessImage(bmp, template); // highlight found matchings
            BitmapData data = template.LockBits(new Rectangle(0, 0, template.Width, template.Height), ImageLockMode.ReadWrite, template.PixelFormat);
            Point p = new Point(-1, -1);

            if (matchings.Length > 0)
            {
                Drawing.Rectangle(data, matchings[0].Rectangle, Color.White);
                p = matchings[0].Rectangle.Location;
                template.UnlockBits(data);
            }

            return p;
        }

        /// <summary>
        /// 初始化图片格式
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap InitImage(Bitmap img)
        {
            Bitmap bnew = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(bnew);

            g.DrawImage(img, 0, 0);

            g.Dispose();

            return bnew;
        }
    }
}
