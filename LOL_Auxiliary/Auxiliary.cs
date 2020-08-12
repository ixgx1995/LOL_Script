using Script;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOL_Auxiliary
{
    public class Auxiliary
    {
        System.Timers.Timer timer = new System.Timers.Timer() { Enabled = false, Interval = 500 };
        Thread t;
        object FightWildObj = new object();//线程锁
        /// <summary>
        /// 角色
        /// </summary>
        public enum role
        {
            待选择,
            我方打野,
            我方上单,
            我方中单,
            我方下路,
            我方辅助,
            敌方打野,
            敌方上单,
            敌方中单,
            敌方下路,
            敌方辅助
        }
        /// <summary>
        /// 区域
        /// </summary>
        public enum Region
        { 
            没查到位置,
            我方上路,
            我方下路,
            我方中路,
            我方上路野区,
            我方下路野区,
            我方高地,

            敌方上路,
            敌方下路,
            敌方中路,
            敌方上路野区,
            敌方下路野区,
            敌方高地,

            上路,
            下路,
            上河道,
            下河道
        }
        Bitmap Minimap;//小地图

        //我
        public List<Point> We_Up_Road_Point;//我上
        public List<Point> We_Down_Road_Point;//我下
        List<Point> We_Middle_Road_Point;//我中
        List<Point> We_Up_Wild_Point;//我上野
        List<Point> We_Down_Wild_Point;//我下野
        List<Point> We_Highland_Point;//我高

        //敌
        List<Point> Enemy_Up_Road_Point;//敌上
        List<Point> Enemy_Down_Road_Point;//敌下
        List<Point> Enemy_Middle_Road_Point;//敌中
        List<Point> Enemy_Up_Wild_Point;//敌上野
        List<Point> Enemy_Down_Wild_Point;//敌下野
        List<Point> Enemy_Highland_Point;//敌高

        //公用
        List<Point> Up_Road_Point;//上
        List<Point> Down_Road_Point;//下
        List<Point> Up_River_Point;//上河
        List<Point> Down_River_Point;//上河

        bool IsMinimap_Init = false;//是否进行小地图坐标分配（设置此值会自动进行）
        Output output;//输出类
        Voice voice;//语音类
        Point Fight_Wild_Point = Point.Empty;

        role Fight_Wild_Role = role.待选择;

        PictureBox pb1, pb2;
        public Auxiliary(IntPtr hwnd, PictureBox pb1, PictureBox pb2)
        {
            this.pb1 = pb1;
            this.pb2 = pb2;
            output = new Output(hwnd);
            voice = new Voice();
            Hook h =new Hook();
            h.HookKey_StartEnd(Play);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerEvent);
        }

        private void Play(int KeysCode, int MouseXY, Hook.KeyMouseState state)
        {
            //启动
            if (KeysCode == (int)Keys.F10)
            {
                Run();
            }
            //结束
            if (KeysCode == (int)Keys.F12)
            {
                End();
            }
            //
            if (KeysCode == (int)Keys.NumLock)
            {
                Point p = Control.MousePosition;
                int Width = 100;
                int Height = 100;
                Output.GetImgPart(p.X - Width / 2, p.Y - Height / 2, Width, Height, true, "打野");
                voice.Play("截取打野图片成功！");
            }
        }


        /// <summary>
        /// 初始化并运行
        /// </summary>
        public void Run()
        {
            IsMinimap_Init = true;
            timer.Enabled = true;
            if(t!=null)
            {
                if (t.ThreadState != ThreadState.Stopped)
                {
                    t.Abort();
                }
            }
            t = new Thread(Fight_Wild);
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// 结束运行
        /// </summary>
        public void End()
        {
            IsMinimap_Init = false;
            timer.Enabled = false;
            if (t != null)
            {
                if (t.ThreadState != ThreadState.Stopped)
                {
                    t.Abort();
                }
            }
        }

        /// <summary>
        /// 定时事件（专门用来截取小地图和分配坐标）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            //截取小地图 1920 - 250, 1080 - 300, 300, 250
            Minimap = Output.GetImgPart(Properties.Settings.Default.Resolving_Width - 245, Properties.Settings.Default.Resolving_Height - 285, 220, 220, false, "");

            //进行小地图坐标分配
            if (Minimap != null || Minimap.Width > 0 || Minimap.Height > 0)
            {
                if (IsMinimap_Init)
                {
                    Minimap_Init();
                    IsMinimap_Init = false;
                }
            }
        }

        /// <summary>
        /// 打野匹配
        /// </summary>
        /// <param name="bmp">要匹配的图片</param>
        /// <returns></returns>
        public bool Fight_WildMatching(Bitmap bmp)
        {
            //如果匹配值存在就不进行匹配
            if (Properties.Settings.Default.Matching_Index != 0)
            {
                bmp = Output.GetSetResolutionImg(bmp, Properties.Settings.Default.Matching_Index);
                bmp.Save(System.Windows.Forms.Application.StartupPath + string.Format(@"\ContrastImg\{0}.jpg", "打野", "打野"), ImageFormat.Tiff);//保存打野小图片
                return true;
            }

            bool matching = false;
            Bitmap tmpMinimap = Minimap;
            int MaxPiex = bmp.Width > bmp.Height ? bmp.Height : bmp.Width;//该图片可以操作的最大像素点
            //匹配
            for ( int i = 0; i < MaxPiex - 1; i++)
			{
                bmp = Output.GetSetResolutionImg(bmp, i);
                if (bmp != null && tmpMinimap != null)
                {
                    Point p = Images.ContainsGetPoint(Images.InitImage(bmp), Images.InitImage(tmpMinimap));
                    bool IsSuccess = Images.ContainsImg(Images.InitImage(bmp), Images.InitImage(tmpMinimap));
                    if (IsSuccess)
                    {
                        //匹配成功
                        matching = true;
                        break;
                    }
                }
			}
            bmp.Save(System.Windows.Forms.Application.StartupPath + string.Format(@"\ContrastImg\{0}.jpg", "打野", "打野"), ImageFormat.Tiff);//保存打野小图片
            return matching;
        }

        /// <summary>
        /// 打野
        /// </summary>
        public void Fight_Wild()
        {
            int index = Properties.Settings.Default.Matching_Index;
            Thread.Sleep(3000);
            Output output = new Output(IntPtr.Zero);
            Output.GetImgPart(1920 / 2 + 105, 1080 / 2 - 205, 32, 32, true, "打野");
            lock (FightWildObj)
            {
                Region OldRegion = Region.没查到位置;
                while (true)
                {
                    Bitmap bmp = Output.ReadImageFile("打野");//打野图片
                    if (Fight_Wild_Role != role.敌方打野)//匹配打野
                    {
                        if (Fight_WildMatching(bmp))
                        {
                            Fight_Wild_Role = role.敌方打野;
                            voice.Play("打野完成匹配");//播放匹配成功声音
                            MessageBox.Show("打野完成匹配");
                        }
                        else//失败跳出线程
                        {
                            voice.Play("打野匹配失败");//播放匹配失败声音
                            break;
                        }
                    }
                    else//进行处理
                    {

                        Point p = Images.ContainsGetPoint(Images.InitImage(bmp), Images.InitImage(Minimap));
                        Region region = GetRegion(p);
                        if (p == new Point(-1, -1))
                        {
                            index++;
                            bmp = Output.GetSetResolutionImg(bmp, index);
                            bmp.Save(System.Windows.Forms.Application.StartupPath + string.Format(@"\ContrastImg\{0}.jpg", "打野", "打野"), ImageFormat.Tiff);//保存打野小图片
                        }
                        pb1.Image = bmp;
                        pb2.Image = Minimap;
                        /*MessageBox.Show("x:" + p.X + "   " + "y:" + p.Y+"   "+ region.ToString());*/
                        
                        if (OldRegion != region)
                        {
                            if (region != Region.没查到位置)
                            {
                                voice.Play(Fight_Wild_Role.ToString() + "在" + region);
                                MessageBox.Show(Fight_Wild_Role.ToString() + "在" + region);
                            }
                            if (region == Region.没查到位置)
                            {
                                region = GetRegion(Fight_Wild_Point);
                                voice.Play(Fight_Wild_Role.ToString() + "消失在" + region);
                                MessageBox.Show(Fight_Wild_Role.ToString() + "消失在" + region);
                            }
                        }
                        Fight_Wild_Point = p;
                        OldRegion = region;
                    }
                    Thread.Sleep(Properties.Settings.Default.Tips_Speed);
                }
            }
        }

        /// <summary>
        /// 小地图坐标分配
        /// </summary>
        public void Minimap_Init()
        {
            List<Point> MinimapPoint = new List<Point>();

            // 路10 
            int Road_length = 15;
            // 河10 
            int River_Length = 7;
            // 高地宽30
            int Highland_Width_Length = 50;
            // 高地高
            int Highland_Hegiht_Length = 50;
            // 河间距
            //int Road_Interval_Length = (int)Math.Sqrt(Math.Pow(Road_length,2)+Math.Pow(Road_length,2));//勾股定理求出河与上下间距

            int width = Minimap.Width;
            int height = Minimap.Height;

            //循环把小地图的像素点放入到集合里 以便后面定位操作
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MinimapPoint.Add(new Point(x, y));
                }
            }

            #region 计算各位置坐标集
            {
                Predicate<Point> match;//每个区域的公式对象
                //我上
                match = s => s.X <= Road_length && s.Y > Road_length && s.Y <= height - Highland_Hegiht_Length;
                We_Up_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                //敌上
                match = s => s.X > Road_length && s.Y <= Road_length && s.X <= width - Highland_Width_Length;
                Enemy_Up_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                //上
                match = s => s.X <= Road_length && s.Y <= Road_length;
                Up_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                //我下
                match = s => s.X < width - Road_length && s.Y >= height - Road_length && s.X >= Highland_Width_Length;
                We_Down_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                //敌下
                match = s => s.X >= width - Road_length && s.Y < height - Road_length && s.Y >= Highland_Hegiht_Length;
                Enemy_Down_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                //下
                match = s => s.X >= width - Road_length && s.Y >= height - Road_length;
                Down_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                // 上河
                //match = s => s.X + s.Y >= Road_length * 2 && s.X + s.Y <= (width + height) / 2 - Road_length * 2
                //    && Math.Abs(s.X - s.Y) <= s.Y * (width / (height - 1) - 1) + River_Length / 2;
                match = s => s.X + s.Y >= Road_length * 2 && s.X + s.Y <= (width + height) / 2 - Road_length / 2
                    && s.X == Math.Abs(s.Y + Road_length / 2);
                Up_River_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                // 下河
                //match = s => s.X + s.Y >= (width + height) / 2 + Road_length * 2 && s.X + s.Y <= width + height - Road_length * 2
                //    && Math.Abs(s.X - s.Y) >= s.Y * (width / (height - 1) - 1) - River_Length / 2;
                match = s => s.X + s.Y >= (width + height) / 2 + Road_length * 2 && s.X + s.Y <= width + height - Road_length * 2
                    && s.X == Math.Abs(s.Y + Road_length / 2);
                Down_River_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                // 我中
                match = s => s.X >= Highland_Width_Length && s.Y <= height - Highland_Hegiht_Length
                    && s.X <= width / 2 && s.Y >= height / 2
                    && s.X + s.Y >= height - Road_length / 2 + (height - s.Y - 1) * (width / (height - 1) - 1);
                We_Middle_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);
                // 敌中
                match = s => s.X <= width - Highland_Width_Length && s.Y >= Highland_Hegiht_Length
                    && s.X >= width / 2 && s.Y <= height / 2
                    && s.X + s.Y <= height + Road_length / 2 + (height - s.Y - 1) * (width / (height - 1) - 1);
                Enemy_Middle_Road_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                // 我高
                match = s => s.X < Highland_Width_Length && s.Y > height - Highland_Hegiht_Length;
                We_Highland_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                // 敌高
                match = s => s.X > width - Highland_Width_Length && s.Y < Highland_Hegiht_Length;
                Enemy_Highland_Point = MinimapPoint.FindAll(match);
                MinimapPoint.RemoveAll(match);

                // 我上野
                We_Up_Wild_Point = MinimapPoint.FindAll(s => s.X - s.Y < s.Y * (width / (height - 1) - 1) && s.X + s.Y < height + (height - s.Y - 1) * (width / (height - 1) - 1));

                // 我下野
                We_Down_Wild_Point = MinimapPoint.FindAll(s => s.X - s.Y < s.Y * (width / (height - 1) - 1) && s.X + s.Y > height + (height - s.Y - 1) * (width / (height - 1) - 1));

                // 敌上野
                Enemy_Up_Wild_Point = MinimapPoint.FindAll(s => s.X - s.Y > s.Y * (width / (height - 1) - 1) && s.X + s.Y < height + (height - s.Y - 1) * (width / (height - 1) - 1));

                // 敌下野
                Enemy_Down_Wild_Point = MinimapPoint.FindAll(s => s.X - s.Y > s.Y * (width / (height - 1) - 1) && s.X + s.Y > height + (height - s.Y - 1) * (width / (height - 1) - 1));
            }
            #endregion
        }

        /// <summary>
        /// 返回坐标对应位置
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Region GetRegion(Point p)
        {
            //我
            if (We_Up_Road_Point.Contains(p)) return Region.我方上路;//我上
            if (We_Down_Road_Point.Contains(p)) return Region.我方下路;//我下
            if (We_Middle_Road_Point.Contains(p)) return Region.我方中路;//我中
            if (We_Up_Wild_Point.Contains(p)) return Region.我方上路野区;//我上野
            if (We_Down_Wild_Point.Contains(p)) return Region.我方下路野区;//我下野
            if (We_Highland_Point.Contains(p)) return Region.我方高地;//我高

            //敌
            if (Enemy_Up_Road_Point.Contains(p)) return Region.敌方上路;//敌上
            if (Enemy_Down_Road_Point.Contains(p)) return Region.敌方下路;//敌下
            if (Enemy_Middle_Road_Point.Contains(p)) return Region.敌方中路;//敌中
            if (Enemy_Up_Wild_Point.Contains(p)) return Region.敌方上路野区;//敌上野
            if (Enemy_Down_Wild_Point.Contains(p)) return Region.敌方下路野区;//敌下野
            if (Enemy_Highland_Point.Contains(p)) return Region.敌方高地;//敌高

            //公用
            if (Up_Road_Point.Contains(p)) return Region.上路;//上
            if (Down_Road_Point.Contains(p)) return Region.下路;//下
            if (Up_River_Point.Contains(p)) return Region.上河道;//上河
            if (Down_River_Point.Contains(p)) return Region.下河道;//上河
            return Region.没查到位置;
        }
    }
}
