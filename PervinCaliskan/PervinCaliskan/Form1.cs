using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PervinCaliskan
{
    public partial class Form1 : Form
    {

        private string fileo;

        public Form1()
        {
            InitializeComponent();
        }


        public struct Coord
        {
            public int x, y;
        }

        private void Work(Bitmap srcb, Bitmap outb, PictureBox yeni)
        {
            int w = srcb.Width, h = srcb.Height;
            Coord[] coord = new Coord[w * h];

            FastBitmap fsb = new FastBitmap(srcb);
            FastBitmap fob = new FastBitmap(outb);
            fsb.LockImage();
            fob.LockImage();
            ulong seed = 0;
            int numpix = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; numpix++, x++)
                {
                    coord[numpix].x = x;
                    coord[numpix].y = y;
                    uint color = fsb.GetPixel(x, y);
                    seed += color;
                    fob.SetPixel(x, y, color);
                }
            fsb.UnlockImage();
            fob.UnlockImage();

            Application.DoEvents();

            int half = numpix / 2;
            int limit = half;
            XorShift rng = new XorShift(seed);
            progressBar.Visible = true;
            progressBar.Maximum = limit;

            fob.LockImage();
            while (limit > 0)
            {
                int p = (int)(rng.next() % (uint)limit);
                int q = (int)(rng.next() % (uint)limit);
                uint color = fob.GetPixel(coord[p].x, coord[p].y);
                fob.SetPixel(coord[p].x, coord[p].y, fob.GetPixel(coord[half + q].x, coord[half + q].y));
                fob.SetPixel(coord[half + q].x, coord[half + q].y, color);
                limit--;
                if (p < limit)
                {
                    coord[p] = coord[limit];
                }
                if (q < limit)
                {
                    coord[half + q] = coord[half + limit];
                }
                if ((limit & 0xfff) == 0)
                {
                    progressBar.Value = limit;
                    fob.UnlockImage();
                    yeni.Refresh();
                    fob.LockImage();
                }
            }
            fob.UnlockImage();
            yeni.Refresh();
            progressBar.Visible = false;
        }

        void DupImage(PictureBox s, PictureBox d)
        {
            if (d.Image != null)
                d.Image.Dispose();
            d.Image = new Bitmap(s.Image.Width, s.Image.Height);
        }

        void GetImagePB(PictureBox pb, string file)
        {
            Bitmap bms = new Bitmap(file, false);
            Bitmap bmp = bms.Clone(new Rectangle(0, 0, bms.Width, bms.Height), PixelFormat.Format32bppArgb);
            bms.Dispose();
            if (pb.Image != null)
                pb.Image.Dispose();
            pb.Image = bmp;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\temp\\";
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string file = openFileDialog.FileName;

                    GetImagePB(pbInput, file);
                    pbInput.Tag = file;
                    DupImage(pbInput, pbOutput);
                    Work(pbInput.Image as Bitmap, pbOutput.Image as Bitmap, pbOutput);
                    file = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".scr.png";
                    pbOutput.Image.Save(file);
                    pbOutput.Refresh();
                    fileo = file;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }
        }

        private void pbInput_Click(object sender, EventArgs e)
        {

        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void buton1_Click(object sender, EventArgs e)
        {

            if (fileo != null)
            {
                try
                {
                    string file = fileo;
                    GetImagePB(pbOutput, file);
                    pictureBox1.Tag = file;
                    DupImage(pbOutput, pictureBox1);
                    Work(pbOutput.Image as Bitmap, pictureBox1.Image as Bitmap, pictureBox1);
                    file = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".scr.png";
                    pictureBox1.Image.Save(file);
                    pictureBox1.Refresh();


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }
        }


        private void pbOutput_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\temp\\";
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string file = openFileDialog.FileName;
                    GetImagePB(pbOutput, file);
                    pbInput.Tag = file;
                    DupImage(pbOutput, pictureBox1);
                    Work(pbOutput.Image as Bitmap, pictureBox1.Image as Bitmap, pbOutput);
                    file = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".scr.png";
                    pictureBox1.Image.Save(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }
        }

    }



    unsafe public class FastBitmap
    {
        private Bitmap workingBitmap = null;
        private int width = 0;
        private BitmapData bitmapData = null;
        private Byte* pBase = null;

        public FastBitmap(Bitmap inputBitmap)
        {
            workingBitmap = inputBitmap;
        }

        public BitmapData LockImage()
        {
            Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

            width = (int)(bounds.Width * 4 + 3) & ~3;


            bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            pBase = (Byte*)bitmapData.Scan0.ToPointer();
            return bitmapData;
        }

        private uint* pixelData = null;

        public uint GetPixel(int x, int y)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            return *pixelData;
        }

        public uint GetNextPixel()
        {
            return *++pixelData;
        }

        public void GetPixelArray(int x, int y, uint[] Values, int offset, int count)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            while (count-- > 0)
            {
                Values[offset++] = *pixelData++;
            }
        }

        public void SetPixel(int x, int y, uint color)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            *pixelData = color;
        }

        public void SetNextPixel(uint color)
        {
            *++pixelData = color;
        }

        public void UnlockImage()
        {
            workingBitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }
    }

    public class XorShift
    {
        private ulong x;

        public XorShift(ulong seed)
        {
            x = seed;
        }

        public ulong next()
        {
            x ^= x >> 12; // a
            x ^= x << 25; // b
            x ^= x >> 27; // c
            return x * 7456821369425831467L;
        }
    }
}
