using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
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
        private FilterInfoCollection videoDevices;  //摄像头设备
        private VideoCaptureDevice videoSource;     //视频的来源选择
        private VideoSourcePlayer videoSourcePlayer;    //AForge控制控件
        private VideoFileWriter writer;     //写入到视频
        private bool is_record_video = false;   //是否开始录像
        System.Timers.Timer timer_count;
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
            catch(Exception ex)
            {
                MessageBox.Show("Get Camera name error:"+ex.Message);
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
            catch(Exception ex)
            {
                MessageBox.Show("Open camera Error:"+ ex.Message);
            }
        }

        private void Btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Bitmap bitmap = sourcePlayer.GetCurrentVideoFrame();
                //IntPtr ip = bitmap.GetHbitmap();//从GDI+ Bitmap创建GDI位图对象

                //BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                //BitmapSizeOptions.FromEmptyOptions());
                //picture.Source =bitmapSource;
                //bitmap.Dispose();
                int width = 640;    //录制视频的宽度
                int height = 480;   //录制视频的高度
                int fps = 9;

                //创建一个视频文件
                String video_format = this.cmb_DevicesName.Text.Trim(); //获取选中的视频编码
                if (this.videoSource.IsRunning && this.videoSourcePlayer.IsRunning)
                {
                    if (-1 != video_format.IndexOf("MPEG"))
                    {
                        writer.Open("test.avi", width, height, fps, VideoCodec.MPEG4);
                    }
                    else if (-1 != video_format.IndexOf("WMV"))
                    {
                        writer.Open("test.wmv", width, height, fps, VideoCodec.WMV1);
                    }
                    else
                    {
                        writer.Open("test.mkv", width, height, fps, VideoCodec.Default);
                    }
                }
                else
                    MessageBox.Show("没有视频源输入，无法录制视频。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                timer_count.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                this.is_record_video = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Capture Error:"+ex.Message);
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
