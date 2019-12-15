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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public ViedoCaptureWindow()
        {
            InitializeComponent();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                for (int i = 0; i < videoDevices.Count; i++)
                {
                    VideoCaptureDevice cap = new VideoCaptureDevice(videoDevices[i].MonikerString);
                    foreach (var capability in cap.VideoCapabilities)
                    {
                        System.Drawing.Size s = new System.Drawing.Size(640, 480);
                        if (capability.FrameSize.Equals(s))
                        {
                            cap.VideoResolution = capability;
                        }
                    }

                    list_Cam.Add(cap);
                }
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
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (sourcePlayer_PC.IsRunning && sourcePlayer_Link.IsRunning && sourcePlayer_Usb.IsRunning)
            {
                writer = new VideoFileWriter();
                //打开写入流                
                writer.Open(@"D:\EmguCV\aaa.avi", list_Cam[0].VideoResolution.FrameSize.Width, list_Cam[0].VideoResolution.FrameSize.Height, 30, (VideoCodec)3, 3000000);
                //开始录制
                sourcePlayer_PC.NewFrame += SourcePlayer_PC_NewFrame;
                writer1 = new VideoFileWriter();
                //打开写入流
                writer1.Open(@"D:\EmguCV\bbb.avi", list_Cam[2].VideoResolution.FrameSize.Width, list_Cam[2].VideoResolution.FrameSize.Height, 30, (VideoCodec)3, 3000000);
                sourcePlayer_Usb.NewFrame += SourcePlayer_Usb_NewFrame;
                writer2 = new VideoFileWriter();
                //打开写入流
                writer2.Open(@"D:\EmguCV\ccc.avi", list_Cam[1].VideoResolution.FrameSize.Width, list_Cam[1].VideoResolution.FrameSize.Height, 30, (VideoCodec)3, 3000000);
                sourcePlayer_Link.NewFrame += SourcePlayer_Link_NewFrame;
            }
        }

        private void SourcePlayer_Link_NewFrame(object sender, ref Bitmap image)
        {
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
            sourcePlayer_PC.Stop();
            sourcePlayer_Usb.Stop();
            sourcePlayer_Link.Stop();
        }
    }
}
