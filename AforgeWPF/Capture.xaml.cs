using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AforgeWPF
{
    /// <summary>
    /// Capture.xaml 的交互逻辑
    /// </summary>
    public partial class Capture : Window
    {
        FilterInfoCollection cameras = null;
        VideoCaptureDevice cam = null;
        public Capture()
        {
            InitializeComponent();
        }

        private void Btn_ChooseDevice_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_DevicesName.Items.Count > 0)
            {
                int index = cmb_DevicesName.SelectedIndex;
                cam=new VideoCaptureDevice(cameras[index].MonikerString);
                btn_Capture.IsEnabled = false;
                btn_OpenCam.IsEnabled = true;
            }
        }

        private void Btn_LoadPic_Click(object sender, RoutedEventArgs e)
        {
                OpenFileDialog openfiledialog = new OpenFileDialog
                {
                    Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif|所有文件|*.*"
                };

                if ((bool)openfiledialog.ShowDialog())
                {
                    picture.Source = new BitmapImage(new Uri(openfiledialog.FileName));
                }            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (cameras.Count > 0)
                {
                    for(int i = 0; i < cameras.Count; i++)
                    {
                        cmb_DevicesName.Items.Add(cameras[i].Name);
                    }
                    cmb_DevicesName.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageBox.Show("Get Camera name error");
            }
        }

        private void Btn_OpenCam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoCaptureDevice source = new VideoCaptureDevice(cameras[cmb_DevicesName.SelectedIndex].MonikerString);
                sourcePlayer.VideoSource = new AsyncVideoSource(source);
                sourcePlayer.Start();
                if (sourcePlayer.IsRunning)
                {
                    // 允许拍照
                    btn_Capture.IsEnabled = true;
                }
            }
            catch
            {
                MessageBox.Show("Open camera Error");
            }
        }

        private void Btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Bitmap bitmap = sourcePlayer.GetCurrentVideoFrame();
                picture.Source =(BitmapImage) bitmap.Clone();
                bitmap.Dispose();
            }
            catch
            {
                MessageBox.Show("Capture Error");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 停止视频
                sourcePlayer.SignalToStop();
                for (int i = 0; (i < 50) && (sourcePlayer.IsRunning); i++)
                {
                    Thread.Sleep(100);
                }
                if(sourcePlayer.IsRunning)
                    sourcePlayer.Stop();
            
        }
    }
}
