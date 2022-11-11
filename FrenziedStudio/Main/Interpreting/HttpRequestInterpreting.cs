using FrenziedStudio.Creation;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace FrenziedStudio.Main
{
    public class HttpRequestInterpreting
    {
        public static HttpRequest CreateHttpRequest(string Name, string[] Request, RequestType rt)
        {
            HttpRequest req = new HttpRequest();

            req.RequestName = Name;

            if (rt == RequestType.Fiddler)
            {
                string firstLinePattern = @"\A(.*?) (.*?) (.*?)$";
                Regex fidReg = new Regex(firstLinePattern);

                string MethodType = fidReg.Match(Request[0]).Groups[1].Value;
                string RequestURL = fidReg.Match(Request[0]).Groups[2].Value;
                string PostBody = string.Empty;

                req.Request = WebRequest.CreateHttp(new Uri(RequestURL));
                req.Request.Method = MethodType;

                for (int i = 1; i < Request.Length - 1; i++)
                {
                    var m = Regex.Match(Request[i], @"(.*?): (.*?)$");

                    string HeaderName = m.Groups[1].Value;
                    string HeaderValue = m.Groups[2].Value;

                    if (HeaderName == "Accept")
                    {
                        req.Request.Accept = HeaderValue;
                    }
                    else if (HeaderName == "Connection")
                    {
                        if (HeaderValue.ToLower() == "keep-alive")
                        {
                            req.Request.KeepAlive = true;
                        }
                        else
                        {
                            req.Request.KeepAlive = false;
                        }
                    }
                    else if (HeaderName == "prept-Encoding")
                    {
                        if (HeaderValue == "gzip, deflate")
                        {
                            req.Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        }
                    }
                    else if (HeaderName == "Content-Length")
                    {
                        if (MethodType == "GET")
                        {
                            req.Request.ContentLength = Convert.ToInt32(HeaderValue);
                        }
                        else if (MethodType == "POST")
                        {
                            req.Request.ContentLength = PostBody.Length;
                        }
                    }
                    else if (HeaderName == "Expect")
                    {
                        req.Request.Expect = HeaderValue;
                    }
                    else if (HeaderName == "Date")
                    {
                        req.Request.Date = Convert.ToDateTime(HeaderValue);
                    }
                    else if (HeaderName == "Host")
                    {
                        req.Request.Host = HeaderValue;
                    }
                    else if (HeaderName == "If-Modified-Since")
                    {
                        req.Request.IfModifiedSince = Convert.ToDateTime(HeaderValue);
                    }
                    else if (HeaderName == "Referer")
                    {
                        req.Request.Referer = HeaderValue;
                    }
                    else if (HeaderName == "Transfer-Encoding")
                    {
                        req.Request.TransferEncoding = HeaderValue;
                    }
                    else if (HeaderName == "User-Agent")
                    {
                        req.Request.UserAgent = HeaderValue;
                    }
                    else
                    {
                        req.Request.Headers.Add(m.Groups[1].Value, m.Groups[2].Value);
                    }
                }

                return req;
            }

            req.Request.CookieContainer = new CookieContainer();

            return req;
        }

        public enum RequestType
        {
            Fiddler,
            Burpsuite
        }
    }
}
