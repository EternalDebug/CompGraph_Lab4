using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace Laba3Gr_1
{
    public partial class Form1 : Form
    {
        int x1 = 100;
        int y1 = 100;
        int x2 = 300;
        int y2 = 300;
        int x3 = 100;
        int y3 = 300;

        UInt32 a = 0xFF0000;
        UInt32 b = 0x00FF00;
        UInt32 c = 0x0000FF;

        Bitmap image;

        public Form1()
        {
            InitializeComponent();

            
        }

        public void Raster()
        {
            for (int y = 0; y < pictureBox1.Height; y++)
                for (int x = 0; x < pictureBox1.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb((int)ShadeBackgroundPixel(x, y)));
        }

        UInt32 ShadeBackgroundPixel(int x, int y)
        {
            UInt32 pixelValue; //цвет пикселя с координатами (x, y)

            double l1, l2, l3;
            pixelValue = 0xFFFFFFFF; //присваиваем цвет фона - белый
            
               l1 = ((y2 - y3) * ((double)(x) - x3) + (x3 - x2) * ((double)(y) - y3)) /
                    ((y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3));
               l2 = ((y3 - y1) * ((double)(x) - x3) + (x1 - x3) * ((double)(y) - y3)) /
                    ((y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3));
               l3 = 1 - l1 - l2;
               if (l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 && l3 >= 0 && l3 <= 1)
               {
                   pixelValue = (UInt32)0xFF000000 |
                       ((UInt32)(l1 * ((a & 0x00FF0000) >> 16) + l2 * ((b & 0x00FF0000) >> 16) + l3 * ((c & 0x00FF0000) >> 16)) << 16) |
                       ((UInt32)(l1 * ((a & 0x0000FF00) >> 8) + l2 * ((b & 0x0000FF00) >> 8) + l3 * ((c & 0x0000FF00) >> 8)) << 8) |
                       (UInt32)(l1 * (a & 0x000000FF) + l2 * (b & 0x000000FF) + l3 * (c & 0x000000FF));
               }
            

            return pixelValue;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        int clck = 0;

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (clck == 0)
            {
                x1 = e.Location.X;
                y1 = e.Location.Y;
                clck++;
            }
            else if (clck == 1)
            {
                x2 = e.Location.X;
                y2 = e.Location.Y;
                clck++;
            }
            else if (clck == 2)
            {
                x3 = e.Location.X;
                y3 = e.Location.Y;
                clck++;
            }
            else
                clck = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Raster();
            pictureBox1.Image = image;
        }


        //Применяет афинное преобразование к полигону
        private List<Point> func(List<Point> inp, double[,] mat)
        {
            List<Point> result = new List<Point>();
            for (int p =0; p< inp.Count(); p++)
            {
                double[] pt = new double[3] {inp[p].X, inp[p].Y, 1 };

                double[] res = new double[3] { 0,0,0 };

                for (int i =0; i< 3; i++)
                {
                    //double r = 0;
                    for (int j =0; j<3; j++)
                    {
                        res[i] += pt[j] * mat[j, i];
                    }
                }
                Point po = new Point((int)res[0], (int)res[1]);
                result.Add(po);
            }
            return result;
        }

        //Смещение по x и y в афинных координатах
        private List<Point> dXdY(List<Point> inp, int dx = 0, int dy = 0)
        {
            double[,] mat = new double[3, 3] { { 1, 0, 0 },{ 0, 1, 0 },{ dx, dy, 1 } };

            return (func(inp, mat));
        }

        //Поворот вокруг точки. Базовый угол = Пи/2
        private List<Point> Povorot_tchk(List<Point> inp, Point tchk, double angle = 3.141592/2)
        {
            double[,] mat = new double[3, 3] { { Math.Cos(angle), Math.Sin(angle), 0 }, { -Math.Sin(angle), Math.Cos(angle), 0 }, { -tchk.X * Math.Cos(angle) + tchk.Y * Math.Sin(angle) + tchk.X, -tchk.X * Math.Sin(angle) - tchk.Y * Math.Cos(angle) + tchk.Y, 1 } };

            return (func(inp, mat));
        }

        //То же самое, но пошагово. Если в лекции была опечатка в той матрице, то это должно работать
        private List<Point> Povorot_tchk_2(List<Point> inp, Point tchk, double angle = 3.141592 / 2)
        {
            List<Point> res = inp;

            res = dXdY(res, -tchk.X, -tchk.Y);
            double[,] mat = new double[3, 3] { { Math.Cos(angle), Math.Sin(angle), 0 }, { -Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 1 } };
            res = func(res, mat);
            res = dXdY(res, tchk.X, tchk.Y);

            return res;
        }

        private List<Point> MSTB_tchk(List<Point> inp, Point tchk, double mX = 1, double mY = 1)
        {
            List<Point> res = inp;

            res = dXdY(res, -tchk.X, -tchk.Y);
            double[,] mat = new double[3, 3] { { mX, 0, 0 }, { 0, mY, 0 }, { 0, 0, 1 } };
            res = func(res, mat);
            res = dXdY(res, tchk.X, tchk.Y);

            return res;
        }

        //Поворот ребра на 90 градусов относительно центра этого ребра. Ребро - лист из 2 точек
        private List<Point> Povorot_line(List<Point> inp)
        {
            Point tchk = new Point((int)((inp[0].X + inp[1].X)/2), (int)((inp[0].Y + inp[1].Y) / 2));
            return Povorot_tchk_2(inp,tchk);
        }
    }
}
