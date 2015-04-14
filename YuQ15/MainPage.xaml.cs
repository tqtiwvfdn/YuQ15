using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace YuQ15
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 主页的 Url
        private string MainUri;
        private string imgStr;
        PhotoChooserTask myTask;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            //图片选择器
            myTask = new PhotoChooserTask();
            myTask.Completed += MyTask_Completed;

            imgStr = (Application.Current as App).backgroundImage;
            if (string.IsNullOrEmpty(imgStr))
            {
                ImageMenuItem_Click(null, null);
            }
            else
            {
                setBg(imgStr);
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {

            MainUri = (Application.Current as App).HostPath;

            if (string.IsNullOrEmpty(MainUri))
            {
                try
                {
                    this.NavigationService.Navigate(new Uri("/Pages/SetHostPathPage.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                // 在此处添加 URL
                Browser.Navigate(new Uri(MainUri, UriKind.RelativeOrAbsolute));
            }
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Browser.InvokeScript("eval", "document.body.style.backgroundImage='url("+ MainUri + "/Images/bg.jpg)';");
            Browser.Opacity = 1;
        }

        // 在 Web 浏览器的导航堆栈而不是应用程序中向后导航。
        private void BackApplicationBar_Click(object sender, EventArgs e)
        {
            Browser.GoBack();
        }

        // 在 Web 浏览器的导航堆栈而不是应用程序中向前导航。
        private void ForwardApplicationBar_Click(object sender, EventArgs e)
        {
            Browser.GoForward();
        }

        // 导航到初始“主页”。
        private void HomeMenuItem_Click(object sender, EventArgs e)
        {
            Browser.Navigate(new Uri(MainUri, UriKind.RelativeOrAbsolute));
        }

        // 处理导航故障。
        private void Browser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            MessageBox.Show("无法导航到此页面，请检查 Internet 连接");
            this.NavigationService.Navigate(new Uri("/Pages/SetHostPathPage.xaml", UriKind.Relative));
        }

        private void RefreshBarIconButton_Click(object sender, EventArgs e)
        {
            try
            {
                Browser.InvokeScript("eval", "history.go()");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Browser_Navigating(object sender, NavigatingEventArgs e)
        {
            Browser.Opacity = 0;
        }



        public static String GetImageStr(String imgFilePath)
        {
            string type;
            switch (imgFilePath.Substring(imgFilePath.LastIndexOf('.') + 1))
            {
                case "jpg":
                case "jpeg":
                    type = "JPEG";
                    break;
                case "bmp":
                    type = "BITMAP";
                    break;
                default:
                    type = "PNG";
                    break;
            }
            // 将图片文件转化为字节数组字符串，并对其进行Base64编码处理
            byte[] data = null;
            // 读取图片字节数组
            try
            {
                FileStream fileStream = new FileStream(imgFilePath, FileMode.Open, FileAccess.Read);
                data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);
                fileStream.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            // 对字节数组Base64编码
            return "data:image/" + type + ";base64," + Convert.ToBase64String(data);
        }

        //设置网站地址
        private void SettingMenuItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/SetHostPathPage.xaml", UriKind.Relative));
        }

        //设置背景图
        private void ImageMenuItem_Click(object sender, EventArgs e)
        {
            if (myTask != null)
            {
                myTask.Show();
            }
        }

        private void MyTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                IsolatedStorageSettings mySetting = IsolatedStorageSettings.ApplicationSettings;
                mySetting["backgroundImage"] = e.OriginalFileName;
                PhoneApplicationService.Current.State["backgroundImage"] = e.OriginalFileName;
                (Application.Current as App).backgroundImage = e.OriginalFileName;
                mySetting.Save();
                setBg(e.OriginalFileName);
            }
        }
        private void setBg(string imgStr)
        {
            ImageBrush imgbr = new ImageBrush();
            BitmapImage img = new BitmapImage(new Uri(imgStr, UriKind.RelativeOrAbsolute));
            imgbr.ImageSource = img;
            LayoutRoot.Background = imgbr;
        }
    }
}
