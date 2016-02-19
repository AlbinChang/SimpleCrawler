using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.IO;
using ProgressBarForm;
using System.Threading;
using System.Collections.Concurrent;

namespace SimpleCrawler
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private HashSet<string> urls = new HashSet<string>();
        private Queue<string> urlsToBrowse = new Queue<string>();
        private string websiteCopy = "";

        private async void btnStart_Click(object sender, EventArgs e)
        {
            //更新界面按钮的属性值
            btnStart.Enabled = false;
            btnStop.Enabled = !btnStart.Enabled;

            #region  从文本框中提取url信息
            string webSite = txtWebSite.Text;
            if (string.IsNullOrWhiteSpace(webSite))
            {
                MessageBox.Show("域名为空！");
                btnStart.Enabled = true;
                btnStop.Enabled = !btnStart.Enabled;
                return;
            }
            if (!webSite.Contains("http"))
            {
                webSite = "http://" + webSite;
            }
            while (webSite[webSite.Length - 1] == '/')
            {
                webSite = webSite.Substring(0, webSite.Length - 1);
            }
            //检查地址是否发生变化
            if (websiteCopy != webSite)
            {
                dataGridViewTitleURL.Rows.Clear();
                urls.Clear();
                urlsToBrowse.Clear();
                
            }
            #endregion

            Crawler crawler = new Crawler();

            try
            {
                #region 根据网站域名提取种子url
                if (urls.Count == 0 && urlsToBrowse.Count == 0)
                {
                    List<string> hostBasedHttpURL = await crawler.GetTitleAndHostBasedHTTPURLsFromURL(webSite);

                    //第一个是Title信息，需要移除掉
                    if (hostBasedHttpURL.Count > 0)
                    {
                        //先提取信息到界面上
                        dataGridViewTitleURL.Rows.Add(hostBasedHttpURL[0], webSite);
                        dataGridViewTitleURL.Rows[dataGridViewTitleURL.Rows.Count - 2].HeaderCell.Value = (dataGridViewTitleURL.Rows.Count - 1).ToString();
                        hostBasedHttpURL.RemoveAt(0);
                    }

                    urls.Add(webSite);
                    
                    foreach (string url in hostBasedHttpURL)
                    {
                        if(urls.Add(url))
                        {
                            urlsToBrowse.Enqueue(url);
                        }
                    }
                }
                #endregion

                #region 根据已有url和初始索引位置，不断地爬取Title和url信息                          

                Method method = this.ThreadMethod;
                //开启4个线程：3个互斥锁，至少将代码划分为4个执行区域
                for (int temp = 0; temp < 4; temp++)
                {
                    this.Invoke(method);                                        
                }

                #endregion
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message);
            }
            finally
            {
                crawler.Dispose();
            }
            //最后显示爬出的url数量和Title数量
            //MessageBox.Show("url有" + urls.Count + "\n" + "title有" + (dataGridViewTitleURL.Rows.Count - 1));

            //备份一份域名
            websiteCopy = webSite;

            //更新界面按钮的属性值
            //btnStart.Enabled = true;
            //btnStop.Enabled = !btnStart.Enabled;

            lblUrls.Text = "URL：" + urls.Count;
            lblTitles.Text = "TITLE：" + (dataGridViewTitleURL.Rows.Count - 1);
        }


        private delegate void Method();

        //三个进程互斥锁
        private object queueLock = new object();
        private object UILock = new object();
        private object URLLock = new object();

        /// <summary>
        /// 异步函数：线程方法
        /// </summary>
        /// <param name="crawler"></param>
        private async void ThreadMethod()
        {
            Crawler crawler = new Crawler();

            while (urlsToBrowse.Count != 0 && btnStart.Enabled == false)
            {
                string url, title;

                //队列互斥锁
                lock (queueLock)
                {
                    //i++;           
                    //url = urls.ElementAt(i);

                    url = urlsToBrowse.Dequeue();                    
                    title = "";
                }

                //获取同一域名下的http超链接以及Title信息
                List<string> hostBasedHttpURL = await crawler.GetTitleAndHostBasedHTTPURLsFromURL(url);

                //先提取Title信息
                if (hostBasedHttpURL.Count > 0)
                {
                    title = hostBasedHttpURL[0];
                    hostBasedHttpURL.RemoveAt(0);
                }

                #region 添加Title和url到datagridview
                try
                {
                    //UI互斥锁
                    lock (UILock)
                    {
                        dataGridViewTitleURL.Rows.Add(title, url);
                        dataGridViewTitleURL.Rows[dataGridViewTitleURL.Rows.Count - 2].HeaderCell.Value = (dataGridViewTitleURL.Rows.Count - 1).ToString();
                        if (btnStopRoll.Text == "停止滚动")
                        {
                            dataGridViewTitleURL.FirstDisplayedScrollingRowIndex = dataGridViewTitleURL.Rows.Count - 1;
                        }
                        this.lblUrls.Text = "URL: " + urls.Count.ToString();
                        this.lblTitles.Text = "TITLE: " + (dataGridViewTitleURL.Rows.Count - 1).ToString();
                    }
                }
                catch (Exception)
                {
                    //do nothing。
                }
                #endregion

                foreach (string each in hostBasedHttpURL)
                {
                    //写互斥锁
                    lock (URLLock)
                    {
                        if (urls.Add(each))
                        {
                            lock (queueLock)
                            {
                                urlsToBrowse.Enqueue(each);
                            }                            
                        }    
                    }
                }
            }

            crawler.Dispose();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = !btnStart.Enabled;

        }

        private void btnStopRoll_Click(object sender, EventArgs e)
        {
            if (btnStopRoll.Text == "停止滚动")
            {
                btnStopRoll.Text = "开始滚动";
            }
            else
            {
                btnStopRoll.Text = "停止滚动";
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (btnStart.Enabled == false || btnStop.Enabled == true)
            {
                MessageBox.Show("请先停止抓取！");
                return;
            }

            dataGridViewTitleURL.Rows.Clear();
            urls.Clear();
            urlsToBrowse.Clear();

            //更新界面
            lblUrls.Text = "URL：" + urls.Count;
            lblTitles.Text = "TITLE：" + (dataGridViewTitleURL.Rows.Count - 1);

        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (btnStart.Enabled == false || btnStop.Enabled == true)
            {
                MessageBox.Show("请先停止抓取！");
                return;
            }

            #region 进度提示窗口设置
            ProgressForm progressForm = new ProgressForm();
            progressForm.Size = new Size(600, 80);
            progressForm.StartPosition = FormStartPosition.Manual;
            progressForm.Location = new Point(this.DesktopLocation.X + this.Width / 2 - progressForm.Size.Width / 2, this.DesktopLocation.Y + this.Height / 2 - progressForm.Size.Height / 2);
            progressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            progressForm.progress.Maximum = 100;
            progressForm.progress.Minimum = 0;
            progressForm.progress.Value = 0;
            progressForm.Show(this);
            this.Enabled = false;
            #endregion

            using (StreamWriter writer = new StreamWriter("TitleUrl.txt"))
            {
                for (int i = 0; i < dataGridViewTitleURL.Rows.Count - 1; i++)
                {
                    string title = dataGridViewTitleURL.Rows[i].Cells[0].Value.ToString();
                    string url = dataGridViewTitleURL.Rows[i].Cells[1].Value.ToString();
                    await writer.WriteLineAsync(title);
                    await writer.WriteLineAsync(url);

                    //更新进度条
                    progressForm.progress.Value = 50 * i / dataGridViewTitleURL.Rows.Count;
                    progressForm.Text = "任务进度：" + progressForm.progress.Value + "%";
                }
            }

            using (StreamWriter writer = new StreamWriter("Urls.txt"))
            {
                int i = 0;
                foreach (string url in urls)
                {
                    await writer.WriteLineAsync(url);
                    i++;
                    //更新进度条
                    progressForm.progress.Value = 50 + 50 * i / urls.Count;
                    progressForm.Text = "任务进度：" + progressForm.progress.Value + "%";
                }

            }

            progressForm.Close();
            MessageBox.Show("保存数据完成！");
            this.Enabled = true;
        }

        private async Task DynamicalSave(int startIndex1, int startIndex2)
        {
            using (StreamWriter writer = new StreamWriter("TitleUrl.txt", true))
            {
                for (int i = startIndex1; i < dataGridViewTitleURL.Rows.Count - 1; i++)
                {
                    string title = dataGridViewTitleURL.Rows[i].Cells[0].Value.ToString();
                    string url = dataGridViewTitleURL.Rows[i].Cells[1].Value.ToString();
                    await writer.WriteLineAsync(title);
                    await writer.WriteLineAsync(url);
                }
            }

            using (StreamWriter writer = new StreamWriter("Urls.txt", true))
            {
                for (int i = startIndex2; i < urls.Count; i++)
                {
                    string url = urls.ElementAt(i);
                    await writer.WriteLineAsync(url);
                }
            }
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            if (btnStart.Enabled == false || btnStop.Enabled == true)
            {
                MessageBox.Show("请先停止抓取！");
                return;
            }


            #region 进度提示窗口设置
            ProgressForm progressForm = new ProgressForm();
            progressForm.Size = new Size(600, 80);
            progressForm.StartPosition = FormStartPosition.Manual;
            progressForm.Location = new Point(this.DesktopLocation.X + this.Width / 2 - progressForm.Size.Width / 2, this.DesktopLocation.Y + this.Height / 2 - progressForm.Size.Height / 2);
            progressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            progressForm.progress.Maximum = 100;
            progressForm.progress.Minimum = 0;
            progressForm.progress.Value = 0;
            progressForm.Show(this);
            this.Enabled = false;
            #endregion

            //先清空原来的数据
            dataGridViewTitleURL.Rows.Clear();
            using (StreamReader reader = new StreamReader("TitleUrl.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string title = await reader.ReadLineAsync();
                    string url = await reader.ReadLineAsync();
                    dataGridViewTitleURL.Rows.Add(title, url);
                    dataGridViewTitleURL.Rows[dataGridViewTitleURL.Rows.Count - 2].HeaderCell.Value = (dataGridViewTitleURL.Rows.Count - 1).ToString();

                    //更新进度条
                    if (progressForm.progress.Value < 50)
                    {
                        progressForm.progress.Value++;
                        progressForm.Text = "任务进度：" + progressForm.progress.Value + "%";
                    }

                }
            }

            urls.Clear();
            urlsToBrowse.Clear();
            int numOfTitles = dataGridViewTitleURL.Rows.Count-1;
            using (StreamReader reader = new StreamReader("Urls.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string url = await reader.ReadLineAsync();
                    urls.Add(url);
                    numOfTitles--;
                    if (numOfTitles < 0)
                    {
                        urlsToBrowse.Enqueue(url);
                    }

                    //更新进度条
                    if (progressForm.progress.Value < 90)
                    {
                        progressForm.progress.Value = progressForm.progress.Value + 1;
                        progressForm.Text = "任务进度：" + progressForm.progress.Value + "%";
                    }

                }
            }

            if (dataGridViewTitleURL.Rows.Count > 1)
            {
                string temp = dataGridViewTitleURL.Rows[0].Cells[1].Value.ToString();
                txtWebSite.Text = "http://" + temp.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                websiteCopy = txtWebSite.Text;
            }

            progressForm.Close();
            MessageBox.Show("载入数据完成！");
            this.Enabled = true;
        }



    }
}
