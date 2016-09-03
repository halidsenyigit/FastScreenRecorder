using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FastScreenRecorder
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        ProjectInfo p_pi;
        public Player(ProjectInfo pi, int index, int speed, bool isPlaying)
        {
            InitializeComponent();
            this.p_pi = pi;
            slider.Maximum = p_pi.Images.Count;
            speedSlider.Value = speed;
            slider.Value = index;
            loadImage(p_pi.Images[index].ImageName);
            if (isPlaying)
                playPause();
        }


        private void loadImage(string imageName)
        {
            image.Source = new BitmapImage(new Uri(p_pi.ProjectLocation + "/" + imageName));
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                loadImage(p_pi.Images[(int)slider.Value].ImageName);
            }
            catch (ArgumentOutOfRangeException) { }
        }


        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool isPlaying = false;
        private float playSpeed;
        private void play_Click(object sender, RoutedEventArgs e)
        {
            playPause();
        }

        void playPause()
        {
            playSpeed = (int)speedSlider.Value;
            if (!isPlaying)
            {
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(100000 / playSpeed));
                dispatcherTimer.Start();
                isPlaying = true;
                play.Content = "Pause";
            }
            else
            {
                dispatcherTimer.Stop();
                isPlaying = false;
                play.Content = "Play";
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            slider.Value += 1;
            if (slider.Value >= p_pi.Images.Count)
                playPause();
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(100000 / (int)speedSlider.Value));
        }
    }
}
