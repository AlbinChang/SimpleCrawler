using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace SimpleCrawler
{
    class Crawler:IDisposable
    {

        HttpClient client;

        public Crawler()
        {
            client  = new HttpClient();
        }


        /// <summary>
        /// 函数；从head标签中提取字符集信息
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
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

            temp = temp.ToLower();

            if (temp.Contains("gb"))
            {
                temp = "gbk";
            }

            return temp;
        }

        /// <summary>
        /// 函数：根据url获取同一域名下的http超链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<string>> GetTitleAndHostBasedHTTPURLsFromURL(string url)
        {            

            string title = "";

            List<string> hostBasedHttpURL = new List<string>();
            
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                
                try
                {

                    //if (response.IsSuccessStatusCode == false)
                    //{
                    //    title = response.StatusCode.ToString();
                    //    hostBasedHttpURL.Insert(0, title);
                    //    return hostBasedHttpURL;
                    //}

                    ////please add redirect code here
                    //if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
                    //{
                    //    title = response.StatusCode.ToString();
                    //    hostBasedHttpURL.Insert(0, title);
                    //    return hostBasedHttpURL;
                    //}

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
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
                        //MessageBox.Show("head标签提取出现问题！");
                        throw new Exception("head标签提取出现问题！");
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
                if (e.Message == "发送请求时出错。" && e.InnerException != null)
                    title = url + "->" + e.Message + ":" + e.InnerException.Message;
                else
                    title = url + "->" + e.Message;
                //MessageBox.Show(e.Message);
            }                        
            finally
            {
                //client.CancelPendingRequests();
            }            
            
            title = HttpUtility.HtmlDecode(title);
            title = title.Trim();
            title = title.Replace("\r", "");
            title = title.Replace("\n", "");
            //将Title信息放在列表的第一个位置
            hostBasedHttpURL.Insert(0, title);

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

            foreach (string each in httpURL)
            {
                int index = each.IndexOf(host);
                if (index > 0 && index < 20)
                    urls.Add(each);
            }
            return new List<string>(urls);
        }


        public string[] noFilterStrings = { ".jsp" };   //反过滤
        public string[] filterStrings = {  ".js", ".map", ".png", "jpeg", ".css", ".jpg", ".gif", ".xml", ".doc",".xls",".pdf",".txt",  //文件后缀过滤
                                                  "rss.php","plugin.php", "down.php",   //php过滤
                                                  "/pic/","/divideload/",     //百度百科过滤
                                                  "#","@","<",">","..",         //特殊字符过滤
                                                  "magnet:", "ed2k:", "ftp:", "thunder:","javascript:",     //非http连接过滤
                                                  "\r","\n","\t"," ",       //空白字符过滤
                                                  "search", "play"};    //迅播网站过滤
        /// <summary>
        /// 函数：过滤掉一些无效的超链接
        /// </summary>
        /// <param name="httpURL"></param>
        void FilterHttpURl(List<string> httpURL)
        {            
            for (int i = 0; i < httpURL.Count; i++)
            {
                if (httpURL[i].Length <= 3 || httpURL[i].Length > 100)
                {
                    httpURL.RemoveAt(i);
                    i--;
                    continue;
                }

                if (httpURL[i].Split(noFilterStrings, 2, StringSplitOptions.None).Length > 1)
                {
                    continue;
                }

                if (httpURL[i].Split(filterStrings, 2, StringSplitOptions.None).Length > 1)
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

                while (httpURL[i][httpURL[i].Length - 1] == '/')
                {
                    httpURL[i] = httpURL[i].Substring(0, httpURL[i].Length - 1);
                }
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
                            for (; each[j] != '\"' && each[j] != '\'' && each[j] != ' ' && each[j] != '>' && j < each.Length; j++)
                            {
                                temp += each[j];
                            }
                            if (temp != "")
                                hrefs.Add(temp);
                            break;
                        }
                    }
                }
            }

            return hrefs;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                client.Dispose();
                client = null;

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Crawler() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
