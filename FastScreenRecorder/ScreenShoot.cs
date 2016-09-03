using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastScreenRecorder
{
    class ScreenShoot
    {
        SettingsModel sm;
        int imageName = 0;
        Thread t;
        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        Bitmap savedPicture;
        PiAction pa;
        ProjectInfo pi;
        public ScreenShoot(SettingsModel sm)
        {
            this.sm = sm;
            pa = new PiAction();
            pi = pa.GetPi(sm.RecordLocation);
            imageName = pi.LastImageID;
        }
        private void TakeFoto()
        {
            imageName++;
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                               Screen.PrimaryScreen.Bounds.Height,
                               PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            savedPicture = new Bitmap(bmpScreenshot, sm.WxH);
            // Save the screenshot to the specified path that the user has chosen.
            savedPicture.Save(sm.RecordLocation + "/_" + imageName + ".png", ImageFormat.Png);

            pi.LastImageID = imageName;
            pi.Images.Add(new Images
            {
                Date = System.DateTime.Now,
                imageId = imageName,
                ImageName = "_" + imageName + ".png"
            });
            pa.writePi(pi.ProjectLocation, pi);
        }

        public void StartCapturing()
        {
            t = new Thread(AsyncFunc);
            t.Start();
            Console.WriteLine("Thread started running");
        }

        public void PauseCapturing()
        {
            _pauseEvent.Reset();
            Console.WriteLine("Thread paused");
        }

        public void StopCapturing()
        {
            // Signal the shutdown event
            _shutdownEvent.Set();
            Console.WriteLine("Thread Stopped ");
            // Make sure to resume any paused threads
            _pauseEvent.Set();
            // Wait for the thread to exit
            t.Join();
        }

        public void ResumeCapturing()
        {
            _pauseEvent.Set();
            Console.WriteLine("Thread resuming ");
        }

        private void AsyncFunc()
        {
            while (true)
            {
                _pauseEvent.WaitOne(Timeout.Infinite);
                if (_shutdownEvent.WaitOne(0))
                    break;

                Console.WriteLine("Thread is running");
                TakeFoto();
                Thread.Sleep(60 * 1000 / this.sm.FPM);
            }
        }

        public void SetSetingsModel(SettingsModel sm)
        {
            this.sm = sm;
        }
    }
}
