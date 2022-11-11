using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class CompareLineData
    {
        public static bool CompareTwoValues(string value1, string value2, string comparison)
        {
            // Returns true if any of the comparisons are met based on the comparison attribute.
            // Equals - two values are equal. | Contains - value1 contains value 2

            switch (comparison)
            {
                case "equals":
                    if (value1 == value2) { return true; }
                    break;

                case "notequals":
                    if (value1 != value2) { return true; }
                    break;

                case "contains":
                    if (value1.Contains(value2)) { return true; }
                    break;

                case "notcontains":
                    if (!value1.Contains(value2)) { return true; }
                    break;

                case "bigger":
                    if (Convert.ToInt32(value1) > Convert.ToInt32(value2)) { return true; }
                    break;

                case "biggerequal":
                    if (Convert.ToInt32(value1) >= Convert.ToInt32(value2)) { return true; }
                    break;

                case "smaller":
                    if (Convert.ToInt32(value1) < Convert.ToInt32(value2))
                    {
                        return true;
                    }
                    break;

                case "smallerequal":
                    if (Convert.ToInt32(value1) <= Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthbigger":
                    if (value1.Length > Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthsmaller":
                    if (value1.Length < Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthbiggerequal":
                    if (value1.Length <= Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthsmallerequal":
                    if (value1.Length <= Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthequals":
                    if (value1.Length == Convert.ToInt32(value2)) { return true; }
                    break;

                case "lengthnotequals":
                    if (value1.Length != Convert.ToInt32(value2)) { return true; }
                    break;

                case "empty":
                    if (string.IsNullOrEmpty(value1)) { return true; }
                    break;

                case "notempty":
                    if (!string.IsNullOrEmpty(value1)) { return true; }
                    break;
            }

            return false;
        }
    }
}
