using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace Kyapture
{
    class Program
    {
        static Point mouseDownLocation;
        static Rectangle rectangle = new Rectangle(0, 0, 0, 0);
        static List<Blackout> blackouts = new List<Blackout>();

        static void Main(string[] args)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                overlay(screen);
            }
        }

        static void overlay(Screen screen)
        {
            Blackout form = new Blackout();
            form.FormBorderStyle = FormBorderStyle.None;
            form.Bounds = screen.Bounds;
            form.TopMost = true;
            form.BackColor = Color.Black;
            form.TransparencyKey = Color.Transparent;
            form.Opacity = 0.5;
            form.Paint += Form_Paint;
            form.MouseDown += Form_MouseDown;
            form.MouseUp += Form_MouseUp;
            form.MouseMove += Form_MouseMove;
            form.StartPosition = FormStartPosition.Manual;
            Application.EnableVisualStyles();
            new Thread(() =>
            {
                Application.Run(form);
            }).Start();
            Thread.Sleep(50);
            blackouts.Add(form);
        }

        private static async void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (rectangle.Size.Height * rectangle.Size.Width > 1000)
            {
                foreach (Blackout blackout in blackouts)
                {
                    blackout.Invoke((MethodInvoker)delegate ()
                    {
                        blackout.Hide();
                    });
                }
                Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(rectangle.Left + ((Blackout)sender).Location.X, rectangle.Top + ((Blackout)sender).Location.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                byte[] data;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    saveJPG(bitmap, memoryStream, 70);
                    data = memoryStream.ToArray();
                }
                HttpResponseMessage response = await new HttpClient().PostAsync("http://saneweb.crackedsidewalks.com/upload/", new ByteArrayContent(data));
                String text = await response.Content.ReadAsStringAsync();
                if (!text.Equals("FAIL"))
                {
                    Process.Start("http://saneweb.crackedsidewalks.com/k/?x=" + text);
                }
                Application.Exit();
                Environment.Exit(0);
            }
        }
        public static void saveJPG(Bitmap bitmap, Stream stream, long level)
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, level);
            bitmap.Save(stream, getEncoder(ImageFormat.Jpeg), encoderParameters);
        }


        private static ImageCodecInfo getEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        private static void Form_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, rectangle);
            e.Graphics.DrawRectangle(new Pen(Color.GhostWhite), rectangle);
        }

        private static void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rectangle = new Rectangle(Math.Min(e.X, mouseDownLocation.X), Math.Min(e.Y, mouseDownLocation.Y), Math.Abs(e.X - mouseDownLocation.X), Math.Abs(e.Y - mouseDownLocation.Y));
                ((Blackout)sender).Invalidate();
            }
        }

        private static void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Application.Exit();
                Environment.Exit(0);
            }
            mouseDownLocation = e.Location;
        }
    }
}
