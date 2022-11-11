using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public static class ConvertToXPath
    {
        public static string Convert(string value, ECT elemtype)
        {
            switch (elemtype)
            {
                case ECT.id:
                    return string.Format("//*[@id='{0}']", value);
                case ECT.name:
                    return string.Format("//*[@name='{0}']", value);
                case ECT.classname:
                    return string.Format("//*[@class='{0}']", value);
                case ECT.linktext:
                    return string.Format("//*[@href='{0}']", value);
                case ECT.xpath:
                    return value;
            }

            return string.Empty;
        }
    }

    public enum ECT
    {
        id,
        name,
        classname,
        linktext,
        xpath
    }
}
