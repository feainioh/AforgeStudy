using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AforgeWPF
{
    /// <summary>
    /// AforgeCaputureControl.xaml 的交互逻辑
    /// </summary>
    public partial class AforgeCaputureControl : System.Windows.Controls.UserControl
    {

        #region 变量
        private FilterInfoCollection cameras = null;
        private VideoCaptureDevice cam = null;
        private VideoFileWriter writer;     //写入到视频

        private string _saveDir=@"D:\Video";
        /// <summary>
        /// 保存目录
        /// </summary>
        public string SaveDir
        {
            get { return _saveDir; }
            set { _saveDir = value; }
        }

        #endregion
        public AforgeCaputureControl()
        {
            InitializeComponent();
        }

        private void Btn_OpenDevice_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btn_OpenDevice.Content == "打开相机")
            {
                try
                {
                    if (cmb_DevicesName.Items.Count > 0)
                    {
                        int index = cmb_DevicesName.SelectedIndex;
                        cam = new VideoCaptureDevice(cameras[index].MonikerString);
                        //分辨率
                        foreach (var capability in cam.VideoCapabilities)
                        {
                            if (capability.FrameSize.Width ==640&&capability.FrameSize.Height == 480)
                            {
                                cam.VideoResolution = capability;
                                break;
                            }
                        }
                        if ((bool)cb_IsViedo.IsChecked)
                        {
                            if (!string.IsNullOrEmpty(txt_SaveDir.Text))
                            {
                                if (!Directory.Exists(txt_SaveDir.Text))
                                    Directory.CreateDirectory(txt_SaveDir.Text);

                                cb_IsViedo.IsEnabled = false;
                                txt_SaveDir.IsEnabled = false;
                                btn_ChooseDir.IsEnabled = false;
                                sourcePlayer.VideoSource = new AsyncVideoSource(cam);
                                sourcePlayer.Start();
                                //保存路径
                                string path = string.Format(@"{0}\{1}.avi", txt_SaveDir.Text, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                                //保存到本地
                                writer = new VideoFileWriter();
                                //打开写入流                
                                writer.Open(path, cam.VideoResolution.FrameSize.Width, cam.VideoResolution.FrameSize.Height, 30, (VideoCodec)3, 3000000);
                                //开始录制
                                sourcePlayer.NewFrame += SourcePlayer_NewFrame;
                                lb_Msg.Content = "开始录像,保存地址:" + path;
                                btn_OpenDevice.Content = "关闭相机";
                            }
                            else
                            {
                                lb_Msg.Content = "请选择录像需要保存的文件夹";
                            }


                        }
                    }
                    else
                    {
                        lb_Msg.Content = "当前未识别到有效的相机";
                    }
                }
                catch (Exception ex)
                {
                    lb_Msg.Content = ex.Message;
                }
            }
            else
            {
                StopCap();
                btn_OpenDevice.Content = "打开相机";
                cb_IsViedo.IsEnabled = true;
                txt_SaveDir.IsEnabled = true;
                btn_ChooseDir.IsEnabled = true;
            }
        }

        private void SourcePlayer_NewFrame(object sender, ref System.Drawing.Bitmap image)
        {            
            writer.WriteVideoFrame(image);
            //image.Dispose();
            Thread.Sleep(10);
        }

        private void Btn_ChooseDir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();  //选择文件夹


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_SaveDir.Text = openFileDialog.SelectedPath;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txt_SaveDir.Text = SaveDir;
                cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (cameras.Count > 0)
                {
                    for (int i = 0; i < cameras.Count; i++)
                    {
                        cmb_DevicesName.Items.Add(cameras[i].Name);
                    }
                    cmb_DevicesName.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                lb_Msg.Content = "Get Camera name error:" + ex.Message;
            }
        }


        #region 方法
        private void StopCap()
        {
            // 停止视频
            sourcePlayer.SignalToStop();
            for (int i = 0; (i < 50) && (sourcePlayer.IsRunning); i++)
            {
                Thread.Sleep(100);
            }
            if (sourcePlayer.IsRunning)
                sourcePlayer.Stop();

            lb_Msg.Content = "停止录像";
        }
        #endregion
    }
}
