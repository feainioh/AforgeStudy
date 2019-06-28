using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AforgeWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource ImagePlay;
        BitmapSource ImageStop;

        FilterInfoCollection cameras = null;
        VideoCaptureDevice cam = null;
        Dictionary<int, string> cam_resource = new Dictionary<int, string>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            if (sourcePlayer.IsRunning)
            {   // 停止视频
                sourcePlayer.SignalToStop();
                sourcePlayer.WaitForStop();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (cameras.Count > 0)
                {
                    sourcePlayer.VideoSource= new VideoCaptureDevice(cameras[0].MonikerString);
                    //cam.NewFrame += Cam_NewFrame;
                    for(int i = 0; i < cameras.Count; i++)
                    {
                        cmb_Devices.Items.Add(cameras[i].Name);
                    }
                    cmb_Devices.SelectedIndex = 0;
                }
                else
                {
                    button_Play.IsEnabled = false;
                    button_Capture.IsEnabled = false;
                }
            }
            catch
            {
                MessageBox.Show("error happened");
            }
        }

        private void Btn_OpenCamera_Click(object sender, RoutedEventArgs e)
        {
            sourcePlayer.Start();
            if (sourcePlayer.IsRunning)
            {
                if (sourcePlayer.IsRunning)
                {
                    // 改变按钮为“停止”状态
                    image_Play.Source = ImageStop;
                    label_Play.Content = "停止";

                    // 允许拍照
                    button_Capture.IsEnabled = true;
                }
            }
        }

        private void Button_Capture_Click(object sender, RoutedEventArgs e)
        {
            string filePath = @"C:\1";
            string fileName = null;
            if (sourcePlayer.VideoSource == null) return;
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            try
            {
                //sourcePlayer.Start();
                Image bitmap =sourcePlayer.GetCurrentVideoFrame();
                if (fileName == null) fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                bitmap.Save(filePath + @"\" + fileName + "-cap.jpg", ImageFormat.Jpeg);
                
                bitmap.Dispose();
                //sourcePlayer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
