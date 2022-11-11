using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace FrenziedStudio.External
{
    public class InterpretedLine
    {
        public InterpretedLine(string ActionValue)
        {
            string urlPattern = @"\b(?:(?:[a-z][\w-]+:(?:\/{1,3}|[a-z0-9%])|www\d{0,3}[.]|
                [a-z0-9.\-]+[.][a-z]{2,4}\/)(?:[^\s()<>]+|((?:[^\s()<>]+|(?:([^\s()<>]+)))*))+(?:((?:[^\s()
                <>]+|(?:([^\s()<>]+)))*)|[^\s`!()[]{};:'"".,<>?«»“”‘’]))";

            Url = Regex.Match(ActionValue.Replace("->", " "), urlPattern).Value;

            HasUrl = !string.IsNullOrEmpty(Url);

            if (!Url.Contains("www") && !Url.Contains("http") && !Url.Contains("https")) { HasUrl = false; }

            if (ActionValue.Contains("safereq:"))
            {
                string safeReqPattern = @"safereq:(.*?):";

                SafeKey = Regex.Match(ActionValue, safeReqPattern).Groups[1].Value;

                Url = Regex.Replace(Url, safeReqPattern, "");

                FullUrl = "safereq:" + SafeKey + ":" + Url;

                SafeRequest = true;
            }
            else if (ActionValue.Contains("hidereq:"))
            {
                Url = Url.Replace("hidereq:", "");

                FullUrl = "hidereq:" + Url;

                HideRequest = true;
            }
            else if (ActionValue.Contains("normalreq:"))
            {
                Url = Url.Replace("normalreq:", "");

                FullUrl = "normalreq:" + Url;
            }
            else
            {
                HasUrl = false;
            }
        }

        public bool HasUrl = false;
        public string Url { get; set; }
        public string FullUrl { get; set; }
        public bool SafeRequest = false;
        public string SafeKey { get; set; }
        public bool HideRequest = false;
    }

    public static class InterpretedLineActions
    {
        public static string SendRequest(InterpretedLine iu)
        {
            if (iu.SafeRequest)
            {
                SafeRequest.SafeRequest sr = new SafeRequest.SafeRequest(iu.SafeKey);
                SafeRequest.Response endpointResp = sr.Request(iu.Url);
                return endpointResp.message;
            }
            else
            {
                HttpWebRequest req = WebRequest.CreateHttp(iu.Url); 
                req.Method = "GET";
                req.Timeout = 20000; 
                req.ReadWriteTimeout = 20000;
                if (iu.HideRequest) { req.Proxy = null; }
                using (var sr = new StreamReader(req.GetResponse()
                    .GetResponseStream())) { return sr.ReadToEnd(); }
            }
        }
    }
}
