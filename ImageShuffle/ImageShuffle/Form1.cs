using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using Point = OpenCvSharp.Point;

namespace ImageShuffle
{
    public partial class Form1 : Form
    {
        Mat mat, dst;
        private string tate, yoko, selecttime, choicecost, tradecost;
        Image image;
        Mat[,] mats;
        Mat[,] angle;
        Mat[,] roi_dst;

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();

            AutoScroll = true;
        }




        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog yomikomi = new OpenFileDialog();
            if (yomikomi.ShowDialog() == DialogResult.OK)
            {
                mat = Cv2.ImRead(yomikomi.FileName);
            }

            dst = new Mat();
            Cv2.Resize(mat, dst, new OpenCvSharp.Size(mat.Width, mat.Height));
            pictureBox1.Size = new System.Drawing.Size(mat.Width, mat.Height);

            image = BitmapConverter.ToBitmap(mat);
            pictureBox1.Image = image;

            List<int> wdivisor = new List<int>();
            List<int> hdivisor = new List<int>();

            for (int i = 1; i <= mat.Width; i++)
            {
                if (mat.Width % i == 0)
                {
                    wdivisor.Add(i);
                }
            }

            for (int j = 1; j <= mat.Height; j++)
            {
                if (mat.Height % j == 0)
                {
                    hdivisor.Add(j);
                }
            }

            List<int> divisorlist = wdivisor.FindAll(hdivisor.Contains);
            divisorlist.Reverse();
            string wcut0 = (mat.Width / divisorlist[0]).ToString();
            string hcut0 = (mat.Height / divisorlist[0]).ToString();
            label3.Text = hcut0 + "*" + wcut0;
            if (divisorlist.Count >= 2)
            {
                string wcut1 = (mat.Width / divisorlist[1]).ToString();
                string hcut1 = (mat.Height / divisorlist[1]).ToString();
                label4.Text = hcut1 + "*" + wcut1;
            }

            if (divisorlist.Count >= 3)
            {
                string wcut2 = (mat.Width / divisorlist[2]).ToString();
                string hcut2 = (mat.Height / divisorlist[2]).ToString();
                label5.Text = hcut2 + "*" + wcut2;
            }



        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tate = textBox1.Text;
            int tatenum = Convert.ToInt32(tate);

            string yoko = textBox2.Text;
            int yokonum = Convert.ToInt32(yoko);

            List<int> matposnum = new List<int>();
            List<int> lotls = new List<int>();
            System.Random r = new System.Random();

            mats = new Mat[tatenum, yokonum];
            roi_dst = new Mat[tatenum, yokonum];
            angle = new Mat[tatenum, yokonum];

            int rectw = mat.Width / yokonum;
            int recth = mat.Height / tatenum;

            string selecttime = textBox3.Text;

            List<int> lotate = new List<int>();

            Mat rotate = new Mat();

            for (int k = 0; k < tatenum; k++)
            {
                for (int m = 0; m < yokonum; m++)
                {
                    //断片画像を「縦分割数＊横分割数」個用意する
                    mats[k, m] = mat.Clone(new Rect(m * rectw, k * recth, rectw, recth));
                    //並び替え用intリストに「00，01，02，10，11，12...」代入
                    matposnum.Add(k * 10 + m);
                    //角度リストに0～4にランダムに代入
                    lotls.Add(r.Next(0, 5));
                }
            }

            //並び替え用リスト内をシャッフル
            matposnum = matposnum.OrderBy(a => Guid.NewGuid()).ToList();
            lotls[0] = 0;

            for (int o = 0; o < tatenum; o++)
            {
                for (int p = 0; p < yokonum; p++)
                {

                    //断片画像を貼り付け先のどこに張り付けるかを指定
                    roi_dst[o, p] = new Mat(dst, new Rect(p * rectw, o * recth, rectw, recth));
                    //matsの断片画像それぞれをランダムに回転
                    if (lotls[o * yokonum + p] == 0)
                    {
                        mats[o, p] = mats[o, p];
                    }

                    if (lotls[o * yokonum + p] == 1)
                    {
                        Cv2.Rotate(mats[o, p], mats[o, p], RotateFlags.Rotate90Counterclockwise);
                    }

                    if (lotls[o * yokonum + p] == 2)
                    {
                        Cv2.Rotate(mats[o, p], mats[o, p], RotateFlags.Rotate180);
                    }

                    if (lotls[o * yokonum + p] == 3)
                    {
                        Cv2.Rotate(mats[o, p], mats[o, p], RotateFlags.Rotate90Clockwise);
                    }

                    //断片画像をdstに結合、出力
                    Cv2.CopyTo(mats[matposnum[o * yokonum + p] / 10, matposnum[o * yokonum + p] % 10], roi_dst[o, p]);
                }

                Cv2.ImShow("tes", dst);


            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            tate = textBox1.Text;
            yoko = textBox2.Text;
            selecttime = textBox3.Text;
            choicecost = textBox4.Text;
            tradecost = textBox5.Text;

            string binary = "P6\n";
            string split = "# " + yoko + " " + tate + "\n";
            string selections = "# " + selecttime + "\n";
            string cost = "# " + choicecost + " " + tradecost + "\n";
            string pixel = dst.Width + " " + dst.Height + "\n";
            string rgb = "255\n";

            byte[] header1 = System.Text.Encoding.UTF8.GetBytes(binary);
            byte[] header2 = System.Text.Encoding.UTF8.GetBytes(split);
            byte[] header3 = System.Text.Encoding.UTF8.GetBytes(selections);
            byte[] header4 = System.Text.Encoding.UTF8.GetBytes(cost);
            byte[] header5 = System.Text.Encoding.UTF8.GetBytes(pixel);
            byte[] header6 = System.Text.Encoding.UTF8.GetBytes(rgb);
            byte[] matbytes = new byte[dst.Total() * 3];
            Mat dst2 = dst.CvtColor(ColorConversionCodes.BGR2RGB);
            Marshal.Copy(dst2.Data, matbytes, 0, matbytes.Length);


            using (FileStream fs =
                File.Create(@"C:\Users\setup\Pictures\" + textBox6.Text + ".ppm"))
            {
                fs.Write(header1, 0, header1.Length);
                fs.Write(header2, 0, header2.Length);
                fs.Write(header3, 0, header3.Length);
                fs.Write(header4, 0, header4.Length);
                fs.Write(header5, 0, header5.Length);
                fs.Write(header6, 0, header6.Length);
                fs.Write(matbytes, 0, matbytes.Length);
            }

            MessageBox.Show("保存しました");
        }
    }
}