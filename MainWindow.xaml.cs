using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using BaiduSearch.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BaiduSearch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<BaiduFileModel> AllDatas = new ObservableCollection<BaiduFileModel>();
        private int pages = 0;
        private HtmlParser parser = new HtmlParser();
        private HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(keyvalue.Text))
            {
                return;
            }
            var tempstr = await client.GetStringAsync("http://www.daihema.com/s/name/" + keyvalue.Text);
            IHtmlDocument tempdoc = await parser.ParseDocumentAsync(tempstr);
            var num = tempdoc.QuerySelector(".search-page")?.QuerySelectorAll(".red.sep-both05.bold")?.LastOrDefault()?.TextContent;
            if (string.IsNullOrEmpty(num) || num == "0")
            {
                MessageBox.Show("搜索结果为空","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            var allcount = Convert.ToInt32(num);
            pages = allcount % 30 == 0 ? allcount / 30 : allcount / 30 + 1;
            AllDatas.Clear();
            var key = keyvalue.Text;
            var tempvalue = 0;
            ModifyProgress(0);
            await Task.Run(() =>
            {
                for (int i = 1; i <= pages; i++)
                {
                    var str = client.GetStringAsync("http://www.daihema.com/s/name/" + key + "/" + i).Result;

                    IHtmlDocument doc = parser.ParseDocumentAsync(str).Result;
                    var results = doc.QuerySelectorAll(".row").Select(o =>
                      {
                          var tempStr = o.QuerySelector(".small").TextContent;
                          var index1 = tempStr.IndexOf("发布");
                          var data = tempStr.Substring(index1 - 10, 10);
                          if (data.Contains(" ") || data.Contains("今天"))
                          {
                              data = tempStr.Substring(index1 - 5, 5);
                          }
                          DateTime temp = default;
                          if (data.Length == 10)
                          {
                              temp = Convert.ToDateTime(data);
                          }
                          else if (data.Length == 5)
                          {
                              if (data.Contains(":"))
                              {
                                  temp = DateTime.Now.Date;
                              }
                              else
                              {
                                  temp = DateTime.Parse(DateTime.Now.Year + "-" + data);
                              }
                          }
                          return new BaiduFileModel()
                          {
                              Title = o.QuerySelector("a").GetAttribute("title"),
                              Author = o.QuerySelector(".small").LastChild.TextContent,
                              Size = o.QuerySelector(".size").TextContent,
                              Time = temp.ToShortDateString().ToString(),
                              Url=o.QuerySelector("a").GetAttribute("href"),
                          };
                      });
                    AllDatas =new ObservableCollection<BaiduFileModel>(AllDatas.Concat(results));
                    tempvalue = tempvalue + 100 / pages;
                    ModifyProgress(tempvalue);
                }
            });
            //初始化页数
            des.Content = $"共{num}条数据";
            ModifyProgress(100);
            page1.CurrentPage = "1";
            page1.TotalPage = pages+"";
            AllDatas=new ObservableCollection<BaiduFileModel>(AllDatas.OrderByDescending(o => o.Time).ToList());
            datagrid1.ItemsSource = AllDatas.Take(30);
        }



        //异步修改值
        public void ModifyProgress(double value) 
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                progress.Value = value;
            });
        }

        public string GetValue()
        {
            string temp = string.Empty;
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                temp = keyvalue.Text;
            });
            return temp;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("邮箱:1149809644@qq.com", "合作", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void page1_FirstPage(object sender, RoutedEventArgs e)
        {
            Break();
            datagrid1.ItemsSource = AllDatas?.Take(30);
            page1.CurrentPage = 1+"";
        }

        private void page1_LastPage(object sender, RoutedEventArgs e)
        {
            Break();
            datagrid1.ItemsSource = AllDatas?.Skip(30 * (pages - 1));
            page1.CurrentPage = pages + "";
        }

        private void page1_NextPage(object sender, RoutedEventArgs e)
        {
            Break();
            if (Convert.ToInt32(page1.CurrentPage) + 1 == pages)
            {
                datagrid1.ItemsSource = AllDatas?.Skip(30 * (pages - 1));
            }
            else if (Convert.ToInt32(page1.CurrentPage) + 1 > pages)
            {
                return;
            }
            else 
            {
                datagrid1.ItemsSource = AllDatas?.Skip(30 * Convert.ToInt32(page1.CurrentPage)).Take(30);
            }
            page1.CurrentPage = Convert.ToInt32(page1.CurrentPage) + 1 + "";
        }

        private void page1_PreviousPage(object sender, RoutedEventArgs e)
        {
            Break();
            if (Convert.ToInt32(page1.CurrentPage) - 1 == 0)
            {
                return;
            }
            else
            {
                datagrid1.ItemsSource = AllDatas?.Skip(30 * (Convert.ToInt32(page1.CurrentPage) - 2)).Take(30);
            }
            page1.CurrentPage = Convert.ToInt32(page1.CurrentPage) - 1+"";
        }

        public void Break() 
        {
            if (AllDatas.Count <= 0 || pages == 0) 
            {
                return;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid1.SelectedItem != null) 
            {
                var item = datagrid1.SelectedItem as BaiduFileModel;
                Clipboard.SetDataObject("http://pdd.19mi.net/go"+item.Url.Substring(item.Url.LastIndexOf('/')));
            }
        }
    }
    public class BaiduFileModel
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Size { get; set; }

        public string Time { get; set; }

        public string Url { get; set; }

    }
}
        

