using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Imaging;

namespace Noise
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.ImageLocation = open.FileName;
            }
        }

        //灰階
        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        gray = (P[2] * 299 + P[1] * 587 + P[0] * 114) / 1000;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox1.Image = copy;
        }

        //胡椒鹽
        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int result;
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    result = rand.Next(1, 10);
                    if (result == 1)
                        copy.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    else if (result == 2)
                        copy.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            pictureBox2.Image = copy;

            //
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    result = copy.GetPixel(x, y).R;
                    histo[result]++;
                }
            Series Series1 = new Series("amount",5000);
            Series1.ChartType = SeriesChartType.Area;
            Series1.Color = Color.Blue;

            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }
            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);

            int max = 0;
            for(int i = 0; i < 256; i++)
                if (histo[i] > max)
                    max = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = max;
        }

        //高斯
        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            int u = 20;
            double a = 2;
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            decimal[] p = new decimal[511];
            decimal[] ps = new decimal[511];
            for (int i = -255; i <= 255; i++)
            {
                p[i + 255] = (decimal)(Math.Exp((-(i - u) * (i - u)) / (2 * a * a) / (2 * a * a)) / (Math.Sqrt(2 * Math.PI) * a));
            }

            for (int i = 0; i < 511; i++)
            {
                if (i == 0)
                    ps[i] = p[i];
                else
                    ps[i] = ps[i - 1] + p[i];
            }
            decimal max = ps[510];
            for (int i = 0; i < 511; i++)
            {
                ps[i] /= max;
            }

            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            decimal result;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        result = (decimal)rand.NextDouble();
                        int i;
                        for (i = 0; i < 511; i++)
                        {
                            if (ps[i] > result)
                                break;
                        }
                        gray = P[2] + i - 255;
                        if (gray > 255) gray = 255;
                        else if (gray < 0) gray = 0;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox2.Image = copy;

            //
            int resul;
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    resul = copy.GetPixel(x, y).R;
                    histo[resul]++;
                }
            Series Series1 = new Series("amount", 5000);
            Series1.Color = Color.Blue;
            Series1.ChartType = SeriesChartType.Area;
            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }

            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);


            int maxx = 0;
            for (int i = 0; i < 256; i++)
                if (histo[i] > maxx)
                    maxx = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = maxx;
        }

        //Uniform noise
        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            int[,] noise = new int[copy.Width, copy.Height];
            int a = 0;
            int b = 30;

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int result;

            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        result = rand.Next(a, b);
                        gray = P[2] + result;
                        if (gray > 255)
                            gray = 255;
                        else if (gray < 0)
                            gray = 0;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox2.Image = copy;

            //
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    result = copy.GetPixel(x, y).R;
                    histo[result]++;
                }
            Series Series1 = new Series("amount", 5000);
            Series1.Color = Color.Blue;
            Series1.ChartType = SeriesChartType.Area;
            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }
            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);
            int max = 0;
            for (int i = 0; i < 256; i++)
                if (histo[i] > max)
                    max = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = max;
        }

        //Rayleigh noise
        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            double u = 15;
            double sig = 80;
            double b = 4 * sig / (4 - Math.PI);
            double a = u - Math.Sqrt(Math.PI * b / 4);
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            decimal[] p = new decimal[511];
            decimal[] ps = new decimal[511];
            for (int i = -255; i <= 255; i++)
            {
                double za = i - a;
                if (za < 0)
                    p[i + 255] = 0;
                else
                    p[i + 255] = (decimal)((2 / b) * za * Math.Exp((-za * za) / b));
            }

            for (int i = 0; i < 511; i++)
            {
                if (i == 0)
                    ps[i] = p[i];
                else
                    ps[i] = ps[i - 1] + p[i];
            }
            decimal max = ps[510];
            for (int i = 0; i < 511; i++)
            {
                ps[i] /= max;
            }

            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            decimal result;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        result = (decimal)rand.NextDouble();
                        int i;
                        for (i = 0; i < 511; i++)
                        {
                            if (ps[i] > result)
                                break;
                        }
                        gray = P[2] + i - 255;
                        if (gray > 255) gray = 255;
                        else if (gray < 0) gray = 0;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox2.Image = copy;

            //
            int resul;
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    resul = copy.GetPixel(x, y).R;
                    histo[resul]++;
                }
            Series Series1 = new Series("amount", 5000);
            Series1.Color = Color.Blue;
            Series1.ChartType = SeriesChartType.Area;
            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }
            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);
            int maxx = 0;
            for (int i = 0; i < 256; i++)
                if (histo[i] > maxx)
                    maxx = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = (double)maxx;
        }

        //Gamma noise
        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            double u = 10;
            double sig = 20;
            double a = u / sig;
            double b = a * u + 0.5;
            a = b / u;
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            decimal[] p = new decimal[511];
            decimal[] ps = new decimal[511];

            decimal powab = (decimal)Math.Pow(a, b);
            decimal mom = 1;
            for (int j = (int)b - 1; j > 0; j--)
            {
                mom *= j;
            }
            for (int i = -255; i <= 255; i++)
            {
                if (i < 0)
                    p[i + 255] = 0;
                else
                {
                    decimal son = (decimal)(((double)powab * Math.Pow(i, b - 1)) * Math.Exp(-a * i));
                    p[i + 255] = son / mom;
                }

            }

            for (int i = 0; i < 511; i++)
            {
                if (i == 0)
                    ps[i] = p[i];
                else
                    ps[i] = ps[i - 1] + p[i];
            }
            decimal max = ps[510];
            for (int i = 0; i < 511; i++)
            {
                ps[i] /= max;
            }

            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            decimal result;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        result = (decimal)rand.NextDouble();
                        int i;
                        for (i = 0; i < 511; i++)
                        {
                            if (ps[i] > result)
                                break;
                        }
                        gray = P[2] + i - 255;
                        if (gray > 255) gray = 255;
                        else if (gray < 0) gray = 0;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox2.Image = copy;

            //
            int resul;
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    resul = copy.GetPixel(x, y).R;
                    histo[resul]++;
                }
            Series Series1 = new Series("amount", 5000);
            Series1.Color = Color.Blue;
            Series1.ChartType = SeriesChartType.Area;
            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }
            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);
            int maxx = 0;
            for (int i = 0; i < 256; i++)
                if (histo[i] > maxx)
                    maxx = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = (double)maxx;
        }

        //Exponential noise
        private void button8_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap(pictureBox1.Image);
            double u = 15;
            double sig = 400;
            double a = 1 / u;
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            decimal[] p = new decimal[511];
            decimal[] ps = new decimal[511];
            for (int i = -255; i <= 255; i++)
            {
                if (i < 0)
                    p[i + 255] = 0;
                else
                    p[i + 255] = (decimal)(a * Math.Exp(-a * i));
            }

            for (int i = 0; i < 511; i++)
            {
                if (i == 0)
                    ps[i] = p[i];
                else
                    ps[i] = ps[i - 1] + p[i];
            }
            decimal max = ps[510];
            for (int i = 0; i < 511; i++)
            {
                ps[i] /= max;
            }

            //指標法將影像RGB抽取出來存放在陣列中
            Rectangle recta = new Rectangle(0, 0, copy.Width, copy.Height);
            BitmapData BmData = copy.LockBits(recta, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr Scan = BmData.Scan0;
            int Offset = BmData.Stride - copy.Width * 3;
            int gray;
            decimal result;
            unsafe
            {
                byte* P = (byte*)(void*)Scan;
                for (int y = 0; y < copy.Height; y++, P += Offset)
                {
                    for (int x = 0; x < copy.Width; x++, P += 3)
                    {
                        result = (decimal)rand.NextDouble();
                        int i;
                        for (i = 0; i < 511; i++)
                        {
                            if (ps[i] > result)
                                break;
                        }
                        gray = P[2] + i - 255;
                        if (gray > 255) gray = 255;
                        else if (gray < 0) gray = 0;
                        P[2] = P[1] = P[0] = (byte)gray;
                    }
                }
            }
            copy.UnlockBits(BmData);
            pictureBox2.Image = copy;

            //
            int resul;
            int[] histo = new int[256];
            for (int x = 0; x < copy.Width; x++)
                for (int y = 0; y < copy.Height; y++)
                {
                    resul = copy.GetPixel(x, y).R;
                    histo[resul]++;
                }
            Series Series1 = new Series("amount", 5000);
            Series1.Color = Color.Blue;
            Series1.ChartType = SeriesChartType.Area;
            for (int i = 0; i < 256; i++)
            {
                Series1.Points.AddXY(i, histo[i]);
            }
            this.chart1.Series.Clear();
            this.chart1.Series.Add(Series1);
            int maxx = 0;
            for (int i = 0; i < 256; i++)
                if (histo[i] > maxx)
                    maxx = histo[i];
            chart1.ChartAreas[0].AxisY.Maximum = (double)maxx;
        }
    }
}
