using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YuQ15.Pages
{
    public class HostPathItem
    {
        public HostPathItem(String hostPathString) { this.HostPathString = hostPathString; }
        public String HostPathString { get; set; }
    }

    public partial class SetHostPathPage : PhoneApplicationPage
    {
        public string newHostPathString = null;
        private string imgStr;
        public SetHostPathPage()
        {
            InitializeComponent();

            GetHostPathList();

            imgStr = (Application.Current as App).backgroundImage;
            if (!string.IsNullOrEmpty(imgStr))
            {
                setBg(imgStr);
            }
        }

        private void GetHostPathList()
        {
            List<HostPathItem> myHostPathItems = new List<HostPathItem>();
            IsolatedStorageSettings mySettings = IsolatedStorageSettings.ApplicationSettings;

            foreach (var x in mySettings)
            {
                if (x.Key != "HostPath"&&x.Key!= "backgroundImage")
                    myHostPathItems.Add(new HostPathItem((string)x.Value));
            }
            this.hostPathList.ItemsSource = myHostPathItems;
        }

        //清理缓存
        private void CleanHostPathCache(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除网站地址缓存吗？", "羽晴 | YuQ11", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                IsolatedStorageSettings.ApplicationSettings.Clear();
                
                //保存背景图片
                IsolatedStorageSettings mySetting = IsolatedStorageSettings.ApplicationSettings;
                mySetting["backgroundImage"] = PhoneApplicationService.Current.State["backgroundImage"] as string;
                mySetting.Save();

                GetHostPathList();
            }
        }

        //清除提示
        private void ClearTips(object sender, RoutedEventArgs e)
        {
            TextBox myTextBox = e.OriginalSource as TextBox;
            if (myTextBox.Text == "示例：192.168.1.2/YuQ11")
            {
                myTextBox.Text = string.Empty;
                myTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
        //增加提示
        private void AddTips(object sender, RoutedEventArgs e)
        {
            TextBox myTextBox = e.OriginalSource as TextBox;
            if (string.IsNullOrEmpty(myTextBox.Text))
            {
                myTextBox.Text = "示例：192.168.1.2/YuQ11";
                myTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        //增加网站地址
        private void EnterToInsertTheNewPath(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                InsertTheNewHostPath(null, null);
            }
        }
        private void InsertTheNewHostPath(object sender, RoutedEventArgs e)
        {
            string newHostPath = hostPathTextBox.Text.Trim().ToLower().Replace(@"\", "/");
            if (newHostPath[newHostPath.Length - 1] == '/')
            {
                newHostPath = newHostPath.Substring(0, newHostPath.Length - 1);
            }
            if (!newHostPath.Contains("http://"))
            {
                newHostPath = "http://" + newHostPath;
            }

            //测试是否可行
            newHostPathString = newHostPath;
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadStringCompleted += TestTheHostPathAvaluable;
                myWebClient.Encoding = System.Text.Encoding.UTF8;
                myWebClient.DownloadStringAsync(new Uri(newHostPath + "/Static/isYuQ11.html?t="+(new Random(DateTime.Now.Millisecond)).Next()));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "羽晴 | YuQ11", MessageBoxButton.OK);
                this.hostPathTextBox.Focus();
            }
        }
        //测试结果
        private void TestTheHostPathAvaluable(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null||e.Result!="yes")
            {
                MessageBox.Show("网站地址有误，请重新输入", "羽晴 | YuQ11", MessageBoxButton.OK);
                this.hostPathTextBox.Focus();
            }
            else
            {
                IsolatedStorageSettings mySetting = IsolatedStorageSettings.ApplicationSettings;
                if (mySetting.Count(p => (string)p.Value == newHostPathString) <= 0)
                {
                    int i = IsolatedStorageSettings.ApplicationSettings.Count;
                    string key = "Path" + i.ToString();
                    mySetting.Add(key, newHostPathString);
                    mySetting.Save();
                }
                ChangeHostPath();
            }
        }

        //选中地址
        private void HostPathList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector myLongListSelector = sender as LongListSelector;
            HostPathItem myHostPathItem = myLongListSelector.SelectedItem as HostPathItem;
            newHostPathString = myHostPathItem.HostPathString;

            ChangeHostPath();
        }
        //更改地址
        private void ChangeHostPath()
        {
            IsolatedStorageSettings mySetting = IsolatedStorageSettings.ApplicationSettings;
            mySetting["HostPath"] = newHostPathString;
            mySetting.Save();
            (Application.Current as App).HostPath = newHostPathString;

            MessageBox.Show("设置网站地址成功！");
            this.NavigationService.GoBack();
        }

        private void setBg(string imgStr)
        {
            ImageBrush imgbr = new ImageBrush();
            BitmapImage img = new BitmapImage(new Uri(imgStr, UriKind.RelativeOrAbsolute));
            imgbr.ImageSource = img;
            imgbr.Opacity = .45;
            LayoutRoot.Background = imgbr;
        }
    }
}