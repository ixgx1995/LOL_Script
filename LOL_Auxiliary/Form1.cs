using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOL_Auxiliary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Voice v = new Voice();
        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Resolving_Width = Convert.ToInt32(textBox1.Text);
            Properties.Settings.Default.Resolving_Height = Convert.ToInt32(textBox2.Text);
            Properties.Settings.Default.Save();

            
            Thread.Sleep(2000);
            Output output = new Output(IntPtr.Zero);
            Output.GetImgPart(1920 - 250, 1080 - 300, 300, 250, true, "小地图");
            v.Play("我是一只小猫咪！");
            
            /*
            Thread.Sleep(2000);
            Output output = new Output(IntPtr.Zero);
            Output.GetImgPart(1920 / 2 + 105, 1080 / 2 - 205, 30, 30, true, "打野");
            v.Play("我是一只小猫咪！");
            */
        }

        Auxiliary a;
        private void Form1_Load(object sender, EventArgs e)
        {
            a = new Auxiliary(IntPtr.Zero,pictureBox1,pictureBox2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = "";
            for (int i = 0; i < a.We_Down_Road_Point.Count; i++)
            {
                text += " " + a.We_Down_Road_Point[i];
                if (i % 10 == 0)
                {
                    text += "\r\n";
                }
            }
            textBox3.Text = text;
        }
    }
}
