using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncTest
{
    public partial class Form1 : Form
    {
        Dictionary<int, PictureBox> pbs = new Dictionary<int, PictureBox>();
        List<Thread> threads = new List<Thread>();
        PictureBox selected;
        int neesPic = 25;
        public Form1()
        {
            InitializeComponent();
        }

        private void setGiphyImage()
        {
            Image iig = Image.FromFile("C:/Users/朱靜宜/Downloads/giphy.gif");

            for (int count = 0; count < neesPic; count++)
            {
                PictureBox pb = new PictureBox();
                pb.Name = "pi" + count;
                pb.Dock = DockStyle.Fill;
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Image = (Image)iig.Clone();
                int column = count / 5;
                int row = count % 5;
                this.tableLayoutPanel1.Controls.Add(pb, column, row);
            }
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            pbs.Clear();
            threads.Clear();
            this.tableLayoutPanel1.Controls.Clear();
            this.setGiphyImage();
            Stopwatch ss = new Stopwatch();
            ss.Start();

            for (int i = 0; i < neesPic; i++)
            {
                string vv = i.ToString();
                Thread t = new Thread(() => WaitMe(vv));
                t.IsBackground = true;
                threads.Add(t);
            }
            foreach (Thread t in threads)
            {
                t.Start();
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }
            for (int count = 0; count < pbs.Count; count++)
            {
                int column = count / 5;
                int row = count % 5;
                this.tableLayoutPanel1.Controls.RemoveByKey("pi" + count);
                this.tableLayoutPanel1.Controls.Add(pbs[count], column, row);
            }
            ss.Stop();
            pbs.Clear();
            GC.Collect();
            MessageBox.Show($"multi thread :{ss.ElapsedMilliseconds} ms");
        }

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            pbs.Clear();
            this.tableLayoutPanel1.Controls.Clear();
            this.setGiphyImage();

            Stopwatch ss = new Stopwatch();
            ss.Start();
            Task[] taskHS = new Task[neesPic];
            for (int i = 0; i < neesPic; i++)
            {
                string vv = i.ToString();
                taskHS[int.Parse(vv)] = Task.Run(() => WaitMe(vv));
            }

            await Task.WhenAll(taskHS);

            for (int count = 0; count < pbs.Count; count++)
            {
                int column = count / 5;
                int row = count % 5;
                this.tableLayoutPanel1.Controls.RemoveByKey("pi" + count);
                this.tableLayoutPanel1.Controls.Add(pbs[count], column, row);
            }
            ss.Stop();
            pbs.Clear();
            GC.Collect();
            MessageBox.Show($"normal :{ss.ElapsedMilliseconds} ms");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pbs.Clear();
            this.tableLayoutPanel1.Controls.Clear();
            this.setGiphyImage();

            Stopwatch ss = new Stopwatch();
            ss.Start();

            Parallel.For(0, neesPic, (int i) =>
              {
                  string vv = i.ToString();
                  WaitMe(vv);
              });
            for (int count = 0; count < pbs.Count; count++)
            {
                int column = count / 5;
                int row = count % 5;
                this.tableLayoutPanel1.Controls.RemoveByKey("pi" + count);
                this.tableLayoutPanel1.Controls.Add(pbs[count], column, row);
            }
            ss.Stop();
            pbs.Clear();
            GC.Collect();
            MessageBox.Show($"parallel.for :{ss.ElapsedMilliseconds} ms");

        }

        private async Task WaitMe(string count)
        {
            Random ran = new Random();
            string nums = ran.Next(4).ToString();
            Bitmap curBitmap = (Bitmap)Image.FromFile($"C:/Users/CC/Downloads/{nums}.jpg").Clone();
            Stopwatch ss = new Stopwatch();
            ss.Start();
            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                byte temp = 0;
                // 啟動不安全模式
                unsafe
                {
                    byte* ptr = (byte*)(bmpData.Scan0.ToPointer());
                    for (int i = 0; i < bmpData.Height; i++)
                    {
                        for (int j = 0; j < bmpData.Width; j++)
                        {
                            temp = (byte)(0.299 * ptr[2] + 0.587 * ptr[1] + 0.114 * ptr[0]);
                            ptr[0] = ptr[1] = ptr[2] = temp;
                            ptr += 3;
                        }
                        ptr += bmpData.Stride - bmpData.Width * 3;
                    }
                }
                curBitmap.UnlockBits(bmpData);
            }

            ss.Stop();
            Console.WriteLine($"picture-{count} process:{ss.ElapsedMilliseconds}");
            PictureBox pb = new PictureBox();
            pb.Name = count;
            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.Image = curBitmap;
            pb.DoubleClick += new EventHandler(this.openpic);

            pbs.Add(int.Parse(count), pb);

        }

        private void openpic(object sender, EventArgs e)
        {
            PictureBox pp = sender as PictureBox;
            Form form = new Form();
            form.FormClosing += new FormClosingEventHandler(this.cometopapa);
            form.Size = pp.Size;
            selected = pp;
            form.Controls.Add(pp);
            form.ShowDialog();
        }

        private void cometopapa(object sender, FormClosingEventArgs e)
        {
            selected.Parent = this.tableLayoutPanel1;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            List<string> result = new List<string>();
            Task[] taskHS = new Task[neesPic];
            Stopwatch ss = new Stopwatch();
            ss.Start();
            for (int i = 0; i < neesPic; i++)
            {
                string vv = i.ToString();
                taskHS[int.Parse(vv)] = Task.Run<string>(() => getFromU(neesPic));
            }

            await Task.WhenAll(taskHS);

            foreach(Task<string> tt in taskHS)
            {
                result.Add(tt.Result);
            }
            ss.Stop();
            MessageBox.Show("UUUU"+ss.ElapsedMilliseconds);
        }

        private async Task<string> getFromU(int count)
        {
            string result = "";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"http://127.0.0.1:52407/getData?count={count}");
                response.EnsureSuccessStatusCode();
                result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
        }
    }
}
