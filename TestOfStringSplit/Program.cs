using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOfStringSplit
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "abcde";
            string[] split1 = { "a" };
            string[] split2 = { "f" };
            string[] split3 = { "a", "f" };
            string[] split4 = { "e" };

            string[] parts1 = str.Split(split1, 2, StringSplitOptions.None);
            string[] parts2 = str.Split(split2, 2, StringSplitOptions.None);
            string[] parts3 = str.Split(split3, 2, StringSplitOptions.None);
            string[] parts4 = str.Split(split4, 2, StringSplitOptions.None);
        }
    }
}
