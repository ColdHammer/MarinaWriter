using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarinaWriter
{
    public partial class ColorForm : Form
    {
        public ColorForm()
        {
            InitializeComponent();
            red = 0xa3;
            green = 0xce;
            blue = 0xd2;
        }
        int red;
        int blue;
        int green;

        Bitmap bitmap;
        private void Form1_Load(object sender, EventArgs e)
        {
            setUpImage(red, green, blue, pictureBox1, ref backup);
            setUpRed(red, green, blue, pictureBox2, ref backUpRed);
            setUpGreen(red, green, blue, pictureBox3, ref backUpGreen);
            setUpBlue(red, green, blue, pictureBox4, ref backUpBlue);
            setLines(red, green, blue);
            textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
        }

        private void setUpImage(int red, int green, int blue, PictureBox pictureBox, ref Bitmap bup)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int bmw = bitmap.Width - 1;
                    int bmh = bitmap.Height - 1;
                    int r = (int)(((bmw - i) / ((float)bmw)) * (255 - red)) + red;
                    r = (int)(r * ((bmh - j) / ((float)bmh)));

                    int g = (int)(((bmw - i) / ((float)bmw)) * (255 - green)) + green;
                    g = (int)(g * ((bmh - j) / ((float)bmh)));

                    int b = (int)(((bmw - i) / ((float)bmw)) * (255 - blue)) + blue;
                    b = (int)(b * ((bmh - j) / ((float)bmh)));

                    bitmap.SetPixel(i, j, Color.FromArgb(255, r, g, b));
                }
            }
            pictureBox.Image = bitmap;
            bup = bitmap;
        }


        private void setUpRed(int red, int green, int blue, PictureBox pictureBox, ref Bitmap bup)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    
                    bitmap.SetPixel(i, j, Color.FromArgb(255, i * (255 / bitmap.Width), green, blue));

                }
            }
            pictureBox.Image = bitmap;
            bup = bitmap;
        }
        private void setUpGreen(int red, int green, int blue, PictureBox pictureBox, ref Bitmap bup)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {

                    bitmap.SetPixel(i, j, Color.FromArgb(255, red, i * (255 / bitmap.Width), blue));
                }
            }
            pictureBox.Image = bitmap;
            bup = bitmap;
        }
        private void setUpBlue(int red, int green, int blue, PictureBox pictureBox, ref Bitmap bup)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, Color.FromArgb(255, red, green, i * (255/bitmap.Width)));
                }
            }
            pictureBox.Image = bitmap;
            bup = bitmap;
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        Bitmap backup;
        Bitmap backUpRed;
        Bitmap backUpGreen;
        Bitmap backUpBlue;

        public void setLines(int r, int g, int b)
        {
            Bitmap red = (Bitmap)backUpRed.Clone();
            Bitmap green = (Bitmap)backUpGreen.Clone();
            Bitmap blue = (Bitmap)backUpBlue.Clone();


            for(int i = 0; i < red.Height; i++)
            {
                red.SetPixel((red.Width - 1) * r / 255, i, Color.Gray);
                green.SetPixel(g * (red.Width-1) / 255, i, Color.Gray);
                blue.SetPixel(b * (red.Width - 1) / 255, i, Color.Gray);
            }

            pictureBox2.Image = red;
            pictureBox3.Image = green;
            pictureBox4.Image = blue;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int y = e.Y > 0 ? e.Y : 0;
            y = y < ((PictureBox)sender).Height -1 ? y : ((PictureBox)sender).Height-1;
            int x = e.X > 0 ? e.X : 0;
            x = x < ((PictureBox)sender).Image.Width-1 ? x : ((PictureBox)sender).Image.Width-1;

            bitmap = (Bitmap)backup.Clone();
            Bitmap btmap = bitmap;
            red = bitmap.GetPixel(x, y).R;
            green = bitmap.GetPixel(x, y).G;
            blue = bitmap.GetPixel(x, y).B;
            setUpRed(red, green, blue, pictureBox2, ref backUpRed);
            setUpGreen(red, green, blue, pictureBox3, ref backUpGreen);
            setUpBlue(red, green, blue, pictureBox4, ref backUpBlue);
            textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
            setLines(red, green, blue);
            for (int i = 0; i < Math.Max(bitmap.Width, bitmap.Height); i++)
            {
                if (bitmap.Width > i)
                {
                    bitmap.SetPixel(i, y, Color.FromArgb(0xff, 0x8, 0x8, 0x8));
                }
                if (bitmap.Height > i)
                {
                    bitmap.SetPixel(x, i, Color.FromArgb(0xff, 0x8, 0x8, 0x8));
                }
            }
            pictureBox1.Image = bitmap;
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2.Image = (Bitmap)backUpRed.Clone();
            Bitmap bm = (Bitmap)((PictureBox)sender).Image;
            int x = e.X > 0 ? e.X : 0;
            x = x < bm.Width -1 ? x : bm.Width -1;

            red = (int)(x * (255.0/ (bm.Width - 1)));
            
            setUpImage(red, green, blue, pictureBox1, ref backup);
            setUpGreen(red, green, blue, pictureBox3, ref backUpGreen);
            setUpBlue(red, green, blue, pictureBox4, ref backUpBlue);
            setLines(red, green, blue);
            textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox3.Image = (Bitmap)backUpGreen.Clone();
            Bitmap bm = (Bitmap)((PictureBox)sender).Image;
            int x = e.X > 0 ? e.X : 0;
            x = x < bm.Width -1  ? x : bm.Width -1 ;

            green = (int)(x * (255.0 / (bm.Width - 1)));
            setUpImage(red, green, blue, pictureBox1, ref backup);
            setUpRed(red, green, blue, pictureBox2, ref backUpRed);
            setUpBlue(red, green, blue, pictureBox4, ref backUpBlue);
            setLines(red, green, blue);
            textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
        }

        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox4.Image = (Bitmap)backUpBlue.Clone();
            Bitmap bm = (Bitmap)((PictureBox)sender).Image;
            int x = e.X > 0 ? e.X : 0;
            x = x < bm.Width - 1 ? x : bm.Width - 1;

            blue = (int)(x * (255.0 / (bm.Width - 1)));
            setUpImage(red, green, blue, pictureBox1, ref backup);
            setUpGreen(red, green, blue, pictureBox3, ref backUpGreen);
            setUpRed(red, green, blue, pictureBox2, ref backUpRed);
            setLines(red, green, blue);
            textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                char[] characters = textBox1.Text.ToCharArray();
                char[] redf = new char[2];
                char[] greenf = new char[2];
                char[] bluef = new char[2];

                Array.Copy(characters, 1, redf, 0, 2);
                Array.Copy(characters, 3, greenf, 0, 2);
                Array.Copy(characters, 5, bluef, 0, 2);
                
                red = hexToInt(redf);
                green = hexToInt(greenf);
                blue = hexToInt(bluef);

                setUpImage(red, green, blue, pictureBox1, ref backup);
                setUpRed(red, green, blue, pictureBox2, ref backUpRed);
                setUpGreen(red, green, blue, pictureBox3, ref backUpGreen);
                setUpBlue(red, green, blue, pictureBox4, ref backUpBlue);
                textBox1.Text = String.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
            }
        }

        public int hexToInt(char[] hex)
        {
            char[] values = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            char[] valuesUp = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int amount = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                for (int j = 0; j < values.Length; j++)
                {
                    if(values[j] == hex[i] || valuesUp[j] == hex[i])
                    {
                        amount += (int)(j * (Math.Pow(16, hex.Length - 1 -i)));
                    }
                }
            }
            return amount;
        }

        Color c;

        public Color getColor()
        {
            return c;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            c = Color.FromArgb(255, red, green, blue);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
