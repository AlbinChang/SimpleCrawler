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

namespace SimpleCrawler
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;


            string webSite = txtWebSite.Text;

            dataGridViewTitleURL.Rows.Clear();

            if (string.IsNullOrWhiteSpace(webSite))
                return;

            HashSet<string> urls = new HashSet<string>();

            try
            {
                if(!webSite.Contains("http"))
                {
                    webSite = "http://" + webSite;
                }
                List<string> hostBasedHttpURL = await GetTitleAndHostBasedHTTPURLsFromURL(webSite);

                //第一个是Title信息，需要移除掉
                if (hostBasedHttpURL.Count > 0)
                {
                    hostBasedHttpURL.RemoveAt(0);
                }

                foreach (string url in hostBasedHttpURL)
                {
                    urls.Add(url);
                }
            }
            catch (Exception except)
            {
                if (!except.Message.Contains("发送请求") && !except.Message.Contains("响应状态代码") && !except.Message.Contains("已取消一个任务"))
                {
                    MessageBox.Show(except.Message);
                    btnStopRoll.PerformClick();
                }
            }

            for (int i = 0; i < urls.Count && btnStart.Enabled == false; i++)
            {
                string url = urls.ElementAt(i);
                string title = "";

                //获取同一域名下的http超链接
                List<string> hostBasedHttpURL = await GetTitleAndHostBasedHTTPURLsFromURL(url);

                //先提取Title信息
                if (hostBasedHttpURL.Count > 0)
                {
                    title = hostBasedHttpURL[0];
                    hostBasedHttpURL.RemoveAt(0);
                }

                try
                {
                    dataGridViewTitleURL.Rows.Add(title, url);
                    dataGridViewTitleURL.Rows[i].HeaderCell.Value = string.Format("{0}", i + 1);
                }
                catch (Exception)
                {
                    //do nothing。
                }

                foreach (string each in hostBasedHttpURL)
                {
                    urls.Add(each);
                }
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;

        }

        private string GetCharsetFromHead(string head)
        {
            string temp = "";
            int indexCharset = head.IndexOf("charset");
            if (indexCharset > 0)
            {
                for (int i = indexCharset + 8; head[i] != '>'; i++)
                {
                    if (temp == "")
                    {
                        if (head[i] != '\"' && head[i] != '\'' && head[i] != ' ' && head[i] != '\t')
                        {
                            temp += head[i];
                        }
                    }
                    else
                    {
                        if (head[i] != '\"' && head[i] != '\'' && head[i] != ' ' && head[i] != '\t')
                        {
                            temp += head[i];
                        }
                        else
                            break;
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// 函数：根据url获取同一域名下的http超链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<List<string>> GetTitleAndHostBasedHTTPURLsFromURL(string url)
        {
            string title = "";
            HttpClient client = new HttpClient();
            List<string> hostBasedHttpURL = new List<string>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                try {

                    if (response.IsSuccessStatusCode == false)
                    {
                        title = response.StatusCode.ToString();
                        hostBasedHttpURL.Insert(0, title);
                        return hostBasedHttpURL;
                    }


                    try
                    {
                        if (response.Content.Headers.ContentType.MediaType != "text/html")
                        {
                            title = "This page is " + response.Content.Headers.ContentType.MediaType;
                            hostBasedHttpURL.Insert(0, title);
                            return hostBasedHttpURL;
                        }
                    }
                    catch (Exception)
                    {
                        title = "This page is unclear of media-type";
                        hostBasedHttpURL.Insert(0, title);
                        return hostBasedHttpURL;
                    }



                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();


                    if (bytes == null || bytes.Length == 0)
                    {
                        title = "No content!";
                        hostBasedHttpURL.Insert(0, title);
                        return hostBasedHttpURL;
                    }


                    //获取网页字符集编码
                    string charset = response.Content.Headers.ContentType.CharSet;
                    if (charset == null)
                    {
                        charset = Encoding.UTF8.WebName;
                    }
                    string body = Encoding.GetEncoding(charset).GetString(bytes);
                    string[] headSplit = { "<head", "</head>", "<HEAD", "</HEAD>", "<body", "<BODY" };
                    string[] headParts = body.Split(headSplit, 3, StringSplitOptions.RemoveEmptyEntries);
                    if (headParts.Length == 1)
                    {
                        title = "No head!";
                        hostBasedHttpURL.Insert(0, title);
                        return hostBasedHttpURL;
                    }
                    string head = "";
                    if (headParts.Length == 3)
                        head = headParts[1];
                    else
                    {
                        MessageBox.Show("head标签提取出现问题！");
                        btnStopRoll.PerformClick();
                    }
                        
                    string encode = GetCharsetFromHead(head);
                    if (encode == "")
                    {
                        GetCharsetFromHead(headParts[0]);
                    }

                    //重新解码网页
                    if (encode != "" && Encoding.GetEncoding(encode) != Encoding.GetEncoding(charset))
                        body = Encoding.GetEncoding(encode).GetString(bytes);

                    //获取Title信息
                    string[] titleSplit = { "<title>", "</title>", "<TITLE>", "</TITLE>" };
                    string[] parts = body.Split(titleSplit, 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3)
                        title = "No title!";
                    else
                        title = parts[1];

                    #region 获取http超链接
                    //获取所有a标签
                    List<string> aTags = FindATags(body);
                    //获取所有a标签的超链接
                    List<string> httpURLs = new List<string>(GetHrefsFromAtags(aTags));
                    //过滤掉一些无效的超链接
                    FilterHttpURl(httpURLs);
                    //根据主机名称将所有相对URL都设置成绝对URL
                    string dnsHost = response.RequestMessage.RequestUri.DnsSafeHost;
                    SetAbsoluteHttpURL(httpURLs, "http://" + dnsHost);
                    //根据主机名称获取属于该网站的URL地址
                    hostBasedHttpURL = GetHostBasedHttpURL(httpURLs, dnsHost);
                    #endregion
                }
                finally
                {
                    //释放非托管资源
                    response.Dispose();
                }
                
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("发送请求") && !e.Message.Contains("响应状态代码") && !e.Message.Contains("已取消一个任务"))
                {
                    MessageBox.Show("异常：\n" + e.Message);
                    btnStopRoll.PerformClick();
                }                    
                else
                    title = e.Message;
            }
            finally
            {
                client.Dispose();
            }            

            //将Title信息放在列表的第一个位置
            hostBasedHttpURL.Insert(0, HttpUtility.HtmlDecode(title) );

            return hostBasedHttpURL;
        }


        /// <summary>
        /// 函数：获取属于某个网站（主机）的URL
        /// </summary>
        /// <param name="httpURL"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        List<string> GetHostBasedHttpURL(List<string> httpURL, string host)
        {
            HashSet<string> urls = new HashSet<string>();

            string[] parts = host.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2)
                host = host.Substring(host.IndexOf('.') + 1);

            foreach (string each in httpURL)
            {
                int index = each.IndexOf(host);
                if (index > 0 && index < 20)
                    urls.Add(each);
            }
            return new List<string>(urls);
        }

        /// <summary>
        /// 函数：过滤掉一些无效的超链接
        /// </summary>
        /// <param name="httpURL"></param>
        void FilterHttpURl(List<string> httpURL)
        {
            string[] noFilterStrings = { ".jsp" };
            string[] filterStrings = {  ".js", ".map", ".png", "jpeg", ".css", ".jpg", ".gif", ".xml", ".doc",".xls",".pdf",".txt",
                                                  "rss.php", "down.php","plugin.php",
                                                  "#","@",
                                                  "magnet:", "ed2k:", "ftp:", "thunder:","javascript:",
                                                  "search"};

            for (int i = 0; i < httpURL.Count; i++)
            {
                

                if (httpURL[i].Length <= 3 || httpURL[i].Length >100)
                {
                    httpURL.RemoveAt(i);
                    i--;
                    continue;
                }

                if (httpURL[i].Split(noFilterStrings,2, StringSplitOptions.None).Length > 1)
                {
                    continue;
                }

                if (httpURL[i].Split(filterStrings,2, StringSplitOptions.None).Length > 1)
                {
                    httpURL.RemoveAt(i);
                    i--;
                    continue;
                }

                if (httpURL[i].IndexOf("http") > 0)
                {
                    httpURL.RemoveAt(i);
                    i--;
                    continue;
                }

            }
        }

        void SetAbsoluteHttpURL(List<string> httpURL, string webSite)
        {
            for (int i = 0; i < httpURL.Count; i++)
            {
                if (httpURL[i].IndexOf("http") != 0)
                {
                    if (httpURL[i][0] == '/' && httpURL[i][1] == '/')
                    {
                        httpURL[i] = "http:" + httpURL[i];
                    }
                    else if (httpURL[i][0] == '/' && httpURL[i][1] != '/')
                    {
                        httpURL[i] = webSite + httpURL[i];
                    }
                    else if (httpURL[i][0] != '/')
                    {
                        httpURL[i] = webSite + "/" + httpURL[i];
                    }
                }

                if (httpURL[i][httpURL[i].Length - 1] == '/')
                    httpURL[i] = httpURL[i].Substring(0, httpURL[i].Length - 1);
                //最后 把 &amp; 替换成 &
                httpURL[i] = HttpUtility.HtmlDecode(httpURL[i]);
            }
        }


        List<string> FindATags(string responseBody)
        {
            List<string> aTags = new List<string>();

            for (int i = 1; i < responseBody.Length - 5; i++)
            {
                if (responseBody[i] == 'a' && responseBody[i - 1] == '<')
                {
                    int j = i + 1;
                    for (; j < responseBody.Length - 3; j++)
                    {
                        if (responseBody[j] == '>')
                        {
                            string tag = responseBody.Substring(i - 1, j - (i - 1) + 1);
                            aTags.Add(tag);
                            i = j;
                            break;
                        }
                    }
                }
            }
            return aTags;
        }

        /// <summary>
        /// 函数：从a标签中提取超链接，并去除空白字符。
        /// </summary>
        /// <param name="aTags"></param>
        /// <returns></returns>
        HashSet<string> GetHrefsFromAtags(List<string> aTags)
        {
            HashSet<string> hrefs = new HashSet<string>();

            foreach (string each in aTags)
            {
                string temp = "";

                int index = each.IndexOf("href");
                if (index > 0)
                {
                    for (int i = index + 4; i < each.Length - 2; i++)
                    {
                        if (each[i] == '\"' || each[i] == '\'')
                        {
                            int j = i + 1;
                            for (; each[j] != '\"' && each[j] != '\'' && each[j] != ' ' && each[j] != '>'; j++)
                            {
                                temp += each[j];                               
                            }
                            if (temp != "")
                                hrefs.Add( temp);
                            break;
                        }
                    }
                }
            }

            return hrefs;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void dataGridViewTitleURL_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (btnStopRoll.Text == "停止滚动")
            {
                dataGridViewTitleURL.FirstDisplayedScrollingRowIndex = dataGridViewTitleURL.Rows.Count - 1;
            }
            
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
    }
}
