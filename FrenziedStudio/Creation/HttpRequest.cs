using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Creation
{
    public class HttpRequest
    {
        public string RequestName { get; set; }
        public string Response { get; set; }
        public CookieCollection Cookies { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public string StatusCode { get; set; }
        public string Status { get; set; }

        public HttpWebRequest Request { get; set; }

        public void ProcessRequest()
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)Request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);

                    Status = "success";
                    Response = reader.ReadToEnd();
                    Cookies = GetAllCookies(Request.CookieContainer);
                    CookieContainer = Request.CookieContainer;
                    Headers = response.Headers;
                    StatusCode = response.StatusCode.ToString();
                }
            }
            catch
            {
                Status = "failed";
            }
        }

        public CookieCollection GetAllCookies(CookieContainer container)
        {
            var allCookies = new CookieCollection();
            var domainTableField = container.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            var domains = (IDictionary)domainTableField.GetValue(container);

            foreach (var val in domains.Values)
            {
                var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var values = (IDictionary)type.GetValue(val);
                foreach (CookieCollection cookies in values.Values)
                {
                    allCookies.Add(cookies);
                }
            }

            return allCookies;
        }

    }
}
