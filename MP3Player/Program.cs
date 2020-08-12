using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace MP3Player
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string inputFile = "";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);//初始化路径
            ofd.Filter = " MP3文件|*.mp3";//过滤选项设置，文本文件，所有文件。
            ofd.FilterIndex = 0;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;
            ofd.Title = "请选择MP3文件";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                inputFile = ofd.FileName;

                Mp3Play m3p = new Mp3Play(inputFile);
                m3p.StarReceive();
                m3p.StarPlay();
            }
            Console.WriteLine("音乐播放中。。。。");
            Console.ReadKey();
        }
    }
}
