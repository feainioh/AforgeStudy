/**************************************************************************
*   
*   =================================
*   CLR版本    ：4.0.30319.42000
*   命名空间    ：AforgeWPF
*   文件名称    ：ViedoCaptureWindow.cs
*   =================================
*   创 建 者    ：LQZ
*   创建日期    ：2019-12-6 14:15:19 
*   功能描述    ：
*   =================================
*   修改者    ：
*   修改日期    ：
*   修改内容    ：
*   =================================
*  
***************************************************************************/
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;

namespace AforgeWPF
{
    /// <summary>
    /// ViedoCaptureWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ViedoCaptureWindow : Window
    {
        VideoCaptureDevice cam = null;
        private FilterInfoCollection videoDevices;  //摄像头设备
        private VideoCaptureDevice videoSource;     //视频的来源选择
        private VideoSourcePlayer videoSourcePlayer;    //AForge控制控件
        private VideoFileWriter writer;     //写入到视频
        VideoFileWriter writer1;
        VideoFileWriter writer2;
        private bool is_record_video = false;   //是否开始录像
        List<VideoCaptureDevice> list_Cam = new List<VideoCaptureDevice>();
        private ScreenCaptureStream videoStreamer;
        List<VideoCapabilities> list_Cap = new List<VideoCapabilities>();

        public ViedoCaptureWindow()
        {
            InitializeComponent();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                cmb_VideoCapabilities.Items.Clear();
                //获取基础分辨率
                VideoCaptureDevice baseVideo = new VideoCaptureDevice(videoDevices[0].MonikerString);
                foreach(var baseCap in baseVideo.VideoCapabilities)
                {
                    if (!list_Cap.Contains(baseCap))
                    {
                        list_Cap.Add(baseCap);
                    }
                }
                //遍历剩余相机的分辨率
                for (int i = 0; i < videoDevices.Count; i++)
                {
                    VideoCaptureDevice cap = new VideoCaptureDevice(videoDevices[i].MonikerString);
                    foreach (var capability in cap.VideoCapabilities)
                    {
                        if (!list_Cap.Contains(capability))
                        {
                            list_Cap.Add(capability);
                            cmb_VideoCapabilities.Items.Add(capability.FrameSize);
                        }   
                    }
                    list_Cam.Add(cap);
                }
                cmb_VideoCapabilities.SelectedIndex = 0;
                btn_Stop.IsEnabled = false;
                btn_Start.IsEnabled = false;
            }
        }

        private void Btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (list_Cam.Count >= 3)
            {
                sourcePlayer_PC.VideoSource = new AsyncVideoSource(list_Cam[0]);
                sourcePlayer_Usb.VideoSource = new AsyncVideoSource(list_Cam[2]);
                sourcePlayer_Link.VideoSource = new AsyncVideoSource(list_Cam[1]);
                lb_PC.Content = videoDevices[0].MonikerString;
                lb_usb.Content = videoDevices[2].MonikerString;
                lb_link.Content = videoDevices[1].MonikerString;
                sourcePlayer_PC.Start();
                sourcePlayer_Usb.Start();
                sourcePlayer_Link.Start();

                btn_Open.IsEnabled = false;
                btn_Start.IsEnabled = true;
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            //帧率
            int frameRate = int.Parse(txt_FrameRate.Text);
            if (sourcePlayer_PC.IsRunning && sourcePlayer_Link.IsRunning && sourcePlayer_Usb.IsRunning)
            {
                writer = new VideoFileWriter();
                //打开写入流                
                writer.Open(@"D:\EmguCV\aaa.avi", list_Cam[0].VideoResolution.FrameSize.Width, list_Cam[0].VideoResolution.FrameSize.Height, frameRate, (VideoCodec)3);
                //开始录制
                sourcePlayer_PC.NewFrame += SourcePlayer_PC_NewFrame;
                writer1 = new VideoFileWriter();
                //打开写入流
                writer1.Open(@"D:\EmguCV\bbb.avi", list_Cam[2].VideoResolution.FrameSize.Width, list_Cam[2].VideoResolution.FrameSize.Height, frameRate, (VideoCodec)3);
                sourcePlayer_Usb.NewFrame += SourcePlayer_Usb_NewFrame;
                writer2 = new VideoFileWriter();
                //打开写入流
                writer2.Open(@"D:\EmguCV\ccc.avi", list_Cam[1].VideoResolution.FrameSize.Width, list_Cam[1].VideoResolution.FrameSize.Height, frameRate, (VideoCodec)3);
                sourcePlayer_Link.NewFrame += SourcePlayer_Link_NewFrame;

                btn_Start.IsEnabled = false;
                btn_Stop.IsEnabled = true;
            }
        }

        private void SourcePlayer_Link_NewFrame(object sender, ref Bitmap image)
        {
            //写入文件流
            writer2.WriteVideoFrame(image);
            Thread.Sleep(10);
        }

        private void SourcePlayer_Usb_NewFrame(object sender, ref Bitmap image)
        {
            writer1.WriteVideoFrame(image);
            Thread.Sleep(10);
        }

        private void SourcePlayer_PC_NewFrame(object sender, ref System.Drawing.Bitmap image)
        {
            writer.WriteVideoFrame(image);
            Thread.Sleep(10);
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            //停止摄像头
            sourcePlayer_PC.Stop();
            sourcePlayer_Usb.Stop();
            sourcePlayer_Link.Stop();


            btn_Stop.IsEnabled = false;
            btn_Start.IsEnabled = true;
        }

        private void Cmb_VideoCapabilities_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach(VideoCaptureDevice cam in list_Cam)
            {
                try
                {
                    //设置分辨率
                    cam.VideoResolution = list_Cap[cmb_VideoCapabilities.SelectedIndex];
                }
                catch
                {
                    //设置默认分辨率
                    cam.VideoResolution = list_Cap[0];
                }
            }
        }
    }
}
