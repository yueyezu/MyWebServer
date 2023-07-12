/*********************************************************************************
 * 机器名称：YUEYEZ-PC
 * 公司名称：励图高科
 * 文件名：  MyWebLibrary
 * 创建人：  胡勇超
 * 创建时间：2015/3/11 19:34:35
 * 描述：对于web简单的服务器创建的一些类库
 ********************************************************************************/
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace MyWebServer
{
    /// <summary>
    /// 服务器参数
    /// </summary>
    public class WebServerParamter
    {
        public string ListenerUrl { get; set; }
        public string WebRootFolder { get; set; }
    }

    /// <summary>
    /// request参数
    /// </summary>
    public class WebServerRequestParamter
    {
        public HttpListenerContext Context { get; set; }
        public string WebRootFolder { get; set; }
    }

    /// <summary>
    /// 创建web服务器的类
    /// </summary>
    public class MyWebLibrary
    {
        /// <summary>
        /// 启动服务器的线程，启动监听
        /// </summary>
        private static Thread _webthread = null;

        private HttpListener listerner;

        /// <summary>
        /// lock
        /// </summary>
        private object lockWebData = new object();
        /// <summary>
        /// 是否关闭该服务器的标识变量
        /// </summary>
        private static bool IsStop = true;

        /// <summary>
        /// 获取当前服务器的运行状态
        /// </summary>
        /// <returns></returns>
        public bool IsRun()
        {
            return !IsStop;
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Stop()
        {
            lock (lockWebData)
            {
                IsStop = true;
                _webthread.Abort();
                _webthread = null;
                if (listerner.IsListening)
                {
                    listerner.Abort();
                }
            }
        }

        /// <summary>
        /// 根据路径和网站的目录来启动服务器
        /// </summary>
        /// <param name="addressstr">访问的路径，例如：http://localhost:778</param>
        /// <param name="folder">本地路径,作为网站根目录</param>
        public void Begin(string addressstr, string folder)
        {
            IsStop = false;

            if (!addressstr.EndsWith("/"))
                addressstr += "/";
            if (!folder.EndsWith("\\"))
                folder += "\\";

            WebServerParamter para = new WebServerParamter()
            {
                ListenerUrl = addressstr,
                WebRootFolder = folder
            };
            _webthread = new Thread(doWork);
            _webthread.Start(para);
        }

        /// <summary>
        /// 开启新的线程来启动服务器
        /// </summary>
        /// <param name="obj"></param>
        private void doWork(object obj)
        {
            WebServerParamter para = obj as WebServerParamter;
            listerner = new HttpListener();

            listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证 Anonymous匿名访问
            listerner.Prefixes.Add(para.ListenerUrl);
            StatusAppend("WebServer Start Successed.......");
            try
            {
                listerner.Start();
                while (true)
                {
                    if (IsStop)
                    {
                        StatusAppend("WebServer Stop Successed.......");
                        break;
                    }
                    //等待请求连接,没有请求则GetContext处于阻塞状态
                    HttpListenerContext ctx = listerner.GetContext();
                    ThreadPool.QueueUserWorkItem(ProcessHttpClient,
                        new WebServerRequestParamter()
                        {
                            Context = ctx,
                            WebRootFolder = para.WebRootFolder
                        });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (listerner.IsListening)
                {
                    listerner.Stop();
                }
            }
        }

        /// <summary>
        /// 处理请求的函数
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessHttpClient(object obj)
        {
            WebServerRequestParamter para = obj as WebServerRequestParamter;
            HttpListenerContext context = para.Context;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            //do something as you want
            //关闭输出流，释放相应资源
            if (IsStop)
            {
                StatusAppend("WebServer Stop Successed.......");
                return;
            }

            StatusAppend("Request:" + request.Url);
            response.StatusCode = 200;//设置返回给客服端http状态代码
            string url = request.RawUrl;

            string filepath = Path.Combine(para.WebRootFolder, request.Url.AbsolutePath.TrimStart('/').Replace("/", "\\"));

            if (!File.Exists(filepath))
            {
                //使用Writer输出http响应代码
                using (StreamWriter writer = new StreamWriter(response.OutputStream))
                {
                    response.ContentEncoding = Encoding.UTF8;
                    writer.WriteLine("<html><head><title>启动结果</title><meta http-equiv=\"X-UA-Compatible\" content=\"IE=7\"><meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\"></head><body>");
                    writer.WriteLine("<div><p>未找到相应的文件!</p></div>");
                    writer.WriteLine("</body></html>");
                    writer.Close();
                    response.Close();
                }
            }
            else
            {
                try
                {
                    using (Stream outstream = response.OutputStream)
                    {
                        using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            int _cachesize = 128;
                            byte[] bytes = new byte[_cachesize];
                            int numBytesToRead = (int)fs.Length;
                            //   response.ContentLength64 = numBytesToRead;
                            while (numBytesToRead > 0)  //一次读取_cachesize个字符，循环读取输出文件，知道全部输出为止。
                            {
                                // Read may return anything from 0 to numBytesToRead.
                                int n = fs.Read(bytes, 0, _cachesize);
                                outstream.Write(bytes, 0, n);
                                // Break when the end of the file is reached.
                                if (n == 0)
                                    break;
                                numBytesToRead -= n;
                            }
                        }
                        outstream.Close();
                    }
                }
                catch (Exception e)
                {
                    StatusAppend("Request:" + request.Url + "   error!" + e.Message);
                }
            }
        }

        /// <summary>
        /// 记录运行日志
        /// </summary>
        /// <param name="str"></param>
        private void StatusAppend(string str)
        {
            //这里暂时不实现可以作为事件来实现，考虑到跨线程的情况


        }

        //private static void CancellingAWorkItem()
        //{
        //    CancellationTokenSource cts = new CancellationTokenSource();
        //    // Pass the CancellationToken and the number-to-count-to into the operation  
        //    ThreadPool.QueueUserWorkItem(o => Count(cts.Token, 1000));
        //    Console.WriteLine("Press <Enter> to cancel the operation.");
        //    Console.ReadLine();
        //    cts.Cancel();  // If Count returned already, Cancel has no effect on it    
        //    // Cancel returns immediately, and the method continues running here...   
        //    Console.ReadLine();  // For testing purposes 
        //}
        //private static void Count(CancellationToken token, Int32 countTo)
        //{
        //    for (Int32 count = 0; count < countTo; count++)
        //    {
        //        if (token.IsCancellationRequested)
        //        {
        //            Console.WriteLine("Count is cancelled");
        //            break; // Exit the loop to stop the operation      
        //        }
        //        Console.WriteLine(count);
        //        Thread.Sleep(200);   // For demo, waste some time    
        //    }
        //    Console.WriteLine("Count is done");
        //}
    }
}