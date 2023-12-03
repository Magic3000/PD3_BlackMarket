using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.ConsoleColor;

namespace PD3_BlackMarket
{
    public enum RequestMethod
    {
        GET = 0,
        POST = 1,
        PUT = 2
    }
    
    // god damn one day I found the time to rework this one
    internal class MyWebRequest
    {
        public HttpWebRequest request;
        private string _url;
        public MyWebRequest(string url, RequestMethod method, List<string> headers = null,
            string contentType = "", string json = "", string accept = "application/json")
        {
            _url = url;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            request.ContentType = contentType;
            request.Accept = accept;
            headers.ForEach(x => request.Headers.Add(x));

            if (!string.IsNullOrWhiteSpace(json))
            {
                byte[] request_bytes = Encoding.UTF8.GetBytes(json);
                request.ContentLength = request_bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(request_bytes, 0, request_bytes.Length);
                requestStream.Close();
            }
            else
                request.ContentLength = 0;
        }

        public MyWebRequest(string url, RequestMethod method, Dictionary<string, string> headers = null,
            string contentType = "", Dictionary<string, string> data = null, string accept = "application/json")
        {
            _url = url;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            request.ContentType = contentType;
            request.Accept = accept;
            headers.Keys.ToList().ForEach(x =>
            {
                if (x == "Host")
                {
                    request.Host = headers[x];
                    //$"Host set: {request.Host}"._sout(Green);
                }
                else if (x == "Content-Type")
                {
                    request.ContentType = headers[x];
                    //$"Content-Type set: {request.ContentType}"._sout(Green);
                }
                else if (x == "User-Agent")
                {
                    request.UserAgent = headers[x];
                    //$"User-Agent set: {request.UserAgent}"._sout(Green);
                }
                else if (x == "Accept")
                {
                    request.Accept = headers[x];
                    //$"Accept set: {request.Accept}"._sout(Green);
                }
                else
                    request.Headers.Add(x, headers[x]);
            });

            var dataList = new List<string>();
            foreach (var kvp in data)
            {
                dataList.Add($"{kvp.Key}={kvp.Value}");
            }
            var dataStr = string.Join("&", dataList);
            if (!string.IsNullOrWhiteSpace(dataStr))
            {
                //$"Writing data: {dataStr}"._sout(Green);
                byte[] request_bytes = Encoding.UTF8.GetBytes(dataStr);
                request.ContentLength = request_bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(request_bytes, 0, request_bytes.Length);
                //$"Writed {request.ContentLength} bytes"._sout();
                requestStream.Close();
            }
            else
                request.ContentLength = 0;
        }

        public MyWebRequest(string url, RequestMethod method, Dictionary<string, string> headers = null,
            string contentType = "", string json = "", string accept = "application/json")
        {
            _url = url;
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            request.ContentType = contentType;
            request.Accept = accept;
            //headers.Keys.ToList().ForEach(x => request.Headers.Add(x, headers[x]));
            headers.Keys.ToList().ForEach(x =>
            {
                if (x == "Host")
                {
                    request.Host = headers[x];
                    //$"Host set: {request.Host}"._sout(Green);
                }
                else if (x == "Content-Type")
                {
                    request.ContentType = headers[x];
                    //$"Content-Type set: {request.ContentType}"._sout(Green);
                }
                else if (x == "User-Agent")
                {
                    request.UserAgent = headers[x];
                    //$"User-Agent set: {request.UserAgent}"._sout(Green);
                }
                else if (x == "Accept")
                {
                    request.Accept = headers[x];
                    //$"Accept set: {request.Accept}"._sout(Green);
                }
                else
                    request.Headers.Add(x, headers[x]);
            });

            if (!string.IsNullOrWhiteSpace(json))
            {
                byte[] request_bytes = Encoding.UTF8.GetBytes(json);
                request.ContentLength = request_bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(request_bytes, 0, request_bytes.Length);
                requestStream.Close();
            }
            else
                request.ContentLength = 0;
        }

        public string GetResponse()
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = response.GetResponseStream();
                using StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.UTF8);
                string result = responseStreamReader.ReadToEnd();
                responseStreamReader.Close();
                return result;
            }
            catch (Exception exc) { $"GetResponse for {_url} exception: {exc.Message}"._sout(Red); return exc.Message; }
        }

        public string GetResponseDebug(bool log = true)
        {
            string result = "NULL";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = response.GetResponseStream();
                using StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.UTF8);
                result = responseStreamReader.ReadToEnd();
                if (log)
                    Console.WriteLine(result);
            }
            catch (WebException e)
            {
                try
                {
                    using var exceptionStreamReader = new StreamReader(e.Response.GetResponseStream());
                    $"ResponseDebug error: {exceptionStreamReader.ReadToEnd()}"._sout(Red);
                }
                catch (Exception ex2)
                {
                    $"ResponseDebug exception: {ex2.Message}"._sout(Red);
                }
            }
            return result;
        }

        public T GetResponse<T>(bool log = false)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream responseStream = response.GetResponseStream();
                using StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.UTF8);
                var a = responseStreamReader.ReadToEnd();
                if (log) Console.WriteLine(a);
                //Console.WriteLine(a);
                T result = JsonConvert.DeserializeObject<T>(a);
                responseStreamReader.Close();
                return result;
            }
            catch (WebException e)
            {
                try
                {
                    using var exceptionStreamReader = new StreamReader(e.Response.GetResponseStream());
                    $"GetResponse error: {exceptionStreamReader.ReadToEnd()}"._sout(Red);
                }
                catch (Exception ex2)
                {
                    $"GetResponse exception: {ex2.Message}"._sout(Red);
                }
                return default(T);
            }
        }

        public async Task<T> GetResponseAsync<T>()
        {
            try
            {
                using var webResponse = await request.GetResponseAsync();
                using var responseStreamReader = new StreamReader((webResponse as HttpWebResponse).GetResponseStream());
                return JsonConvert.DeserializeObject<T>(responseStreamReader.ReadToEnd());
            }
            catch (WebException e)
            {
                try
                {
                    using var exceptionStreamReader = new StreamReader(e.Response.GetResponseStream());
                    $"GetResponseAsync error: {exceptionStreamReader.ReadToEnd()}"._sout(Red);
                }
                catch (Exception ex2)
                {
                    $"GetResponseAsync exception: {ex2.Message}"._sout(Red);
                }
            }
            return default(T);
        }
    }
}
