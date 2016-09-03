using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FastScreenRecorder
{

    public partial class MainWindow : Window
    {
        #region Initialition Variable
        SettingsModel sm = new SettingsModel();
        ScreenShoot ss;

        ProjectInfo pi = new ProjectInfo();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }


        #region Release Video


        private void save_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            // restore project info json file

            string address = p_pi.ProjectLocation;


            string searchPattern = "_*.png";

            DirectoryInfo di = new DirectoryInfo(address);
            FileInfo[] files = di.GetFiles(searchPattern);
            ProjectInfo pinfo = new PiAction().GetPi(address);
            pinfo.Images = new List<Images>();
            foreach (var file in files)
            {
                pinfo.Images.Add(new Images
                {
                    Date = file.CreationTime,
                    imageId = int.Parse(file.Name.Substring(1, file.Name.Length - 5)),
                    ImageName = file.Name
                });
            }

            PiAction pa = new PiAction();
            pa.writePi(address, pinfo);
        }

        private void fullScreen_Click(object sender, RoutedEventArgs e)
        {
            if(p_pi != null)
            {
                Player player = new Player(p_pi, (int)slider.Value, (int)speedSlider.Value, isPlaying);
                player.Show();
            }
            
        }


        #endregion

        #region General Settings Actions


        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            folderLocation.Text = fbd.SelectedPath;

            sm.RecordLocation = fbd.SelectedPath;

            pi.ProjectLocation = sm.RecordLocation;

            if (File.Exists(pi.ProjectLocation + "/" + "ProjectInfo.json"))
            {
                pi = new PiAction().GetPi(pi.ProjectLocation);
                projectName.Text = pi.ProjectName;
                folderLocation.Text = pi.ProjectLocation;
                fpm.Text = pi.SettingsModel.FPM.ToString();
                www.Text = pi.SettingsModel.WxH.Width.ToString();
                hhh.Text = pi.SettingsModel.WxH.Height.ToString();
            }
        }

        private void fpm_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                sm.FPM = int.Parse(fpm.Text);
            }
            catch (FormatException)
            {

            }


        }

        private void openProject_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                PiAction pa = new PiAction();
                this.pi = pa.GetPi(fbd.SelectedPath);
                if (pi == null)
                    return;
                this.sm = this.pi.SettingsModel;
                UpdateFields(this.pi);
            }
        }


        #endregion

        #region Start Recording Button Action

        private void startRecordingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(folderLocation.Text))
            {
                System.Windows.Forms.MessageBox.Show("Geçerli bir dizin seçiniz");
                return;
            }
            sm.FPM = int.Parse(fpm.Text);
            sm.RecordLocation = folderLocation.Text;
            sm.ProgramStat = ProgramStatus.Active;
            sm.WxH = new System.Drawing.Size(int.Parse(www.Text), int.Parse(hhh.Text));

            pi.ProjectName = projectName.Text;
            pi.ProjectLocation = sm.RecordLocation;
            pi.SettingsModel = sm;
            if (pi.Images == null)
            {
                pi.Images = new List<Images>();
            }
            this.Hide();

            // Keyboard listeners ...
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
            drawProgramStatus();

            PiAction piAction = new PiAction(pi);
            this.pi.LastImageID = piAction.GetPi(pi.ProjectLocation).LastImageID;
            piAction.writePi(folderLocation.Text, pi);
            this.pi = piAction.GetPi(pi.ProjectLocation);
            this.sm = this.pi.SettingsModel;

            UpdateFields(pi);
            startRecording();
        }

        private void startRecording()
        {
            ss = new ScreenShoot(sm);
            ss.StartCapturing();
        }

        public void UpdateFields(ProjectInfo pi)
        {
            projectName.Text = pi.ProjectName;
            folderLocation.Text = pi.SettingsModel.RecordLocation;
            fpm.Text = pi.SettingsModel.FPM.ToString();
            www.Text = pi.SettingsModel.WxH.Width.ToString();
            hhh.Text = pi.SettingsModel.WxH.Height.ToString();
        }


        #endregion

        #region Drawing Program Status

        void drawProgramStatus()
        {
            Brush color = sm.StatusColor;
            IntPtr desktop = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                g.FillRectangle(color,
                    0,
                    Screen.PrimaryScreen.Bounds.Height - 40,
                    50,
                    40
                );
            }
            ReleaseDC(IntPtr.Zero, desktop);
        }

        #endregion

        #region Keyboard Listener

        private LowLevelKeyboardListener _listener;

        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            Console.WriteLine(e.KeyPressed.ToString());
            if (e.KeyPressed.ToString() == "Home")
            {
                this.Show();
                _listener.UnHookKeyboard();
                sm.ProgramStat = ProgramStatus.Stoped;
                ss.StopCapturing();
                drawProgramStatus();
            }

            if (e.KeyPressed.ToString() == "RightCtrl")
            {
                if (sm.ProgramStat != ProgramStatus.Pending)
                {
                    sm.ProgramStat = ProgramStatus.Pending;
                    ss.PauseCapturing();
                }

                else
                {
                    sm.ProgramStat = ProgramStatus.Active;
                    ss.ResumeCapturing();
                }


                drawProgramStatus();
            }
        }


        #endregion

        #region DllImport

        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        #endregion

        #region Play Project İmages

        private ProjectInfo p_pi;
        private PiAction p_pa;
        private int imgCount;

        private void open_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (fbd.SelectedPath != null && Directory.Exists(fbd.SelectedPath))
            {
                p_pa = new PiAction();
                p_pi = p_pa.GetPi(fbd.SelectedPath);
                imgCount = p_pi.Images.Count;
                if (imgCount > 0)
                {
                    slider.Maximum = imgCount;
                    loadImage(p_pi.Images[0].ImageName);
                    slider.IsEnabled = true;
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Geçerli Bir Proje Dizini Seçiniz");
                return;
            }

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
            try
            {
                if (p_pi.Images == null)
                {
                    System.Windows.Forms.MessageBox.Show("Proje Açılmadı");
                    return;
                }
            }
            catch (NullReferenceException){
                System.Windows.Forms.MessageBox.Show("Proje Açılmadı");
                return;
            }
            
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
            if (slider.Value >= imgCount)
                playPause();
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(100000 / (int)speedSlider.Value));
        }
        #endregion

        #region Form Buttons
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // kapat
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // minimize

        }

        #endregion

        #region Form Move Action

        System.Windows.Point p1, p2;
        bool isDragging = false;

        public IntPtr Handle { get; private set; }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            p1 = e.GetPosition(null);
            isDragging = true;
        }

        private void titleBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.GetPosition(null).X - p1.X;
                this.Top += e.GetPosition(null).Y - p1.Y;
            }
            p2 = e.GetPosition(null);
        }

        private void newProject_Click(object sender, RoutedEventArgs e)
        {
            // all fields clearing
            projectName.Text = "";
            folderLocation.Text = "";
            fpm.Text = "12";
            www.Text = "1366";
            hhh.Text = "768";
            sm = new SettingsModel();
            pi = new ProjectInfo();
        }



        private void titleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }
        #endregion


    }
}