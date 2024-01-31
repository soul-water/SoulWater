using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoulWater
{
    static class 判断
    {
        /// <summary>
        /// 判断字符串是否有数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasNumber(this String str)
        {
            bool result = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsNumber(str, i))
                {
                    return true;
                }
            }
            return result;
        }

        /// <summary>
        /// 判断一个字符串中是否有字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasLetter(this string str)
        {
            return Regex.Matches(str, "[a-zA-Z]").Count > 0;
        }
        /// <summary>
        /// 判断一个字符串是否有中文
        /// </summary>
        /// <param name="CString"></param>
        /// <returns></returns>
        public static bool HASChinese(this string CString)
        {
            if (CString != null && CString.Length > 0)
            {
                char[] cs = [.. CString];
                foreach (char c in cs)
                {
                    if (Convert.ToInt32(c) >= 128)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断数组是否有相同的
        /// </summary>
        /// <param name="Input">输入数组</param>
        /// <param name="SW1">第一个相同的值</param>
        /// <returns>是否有相同的</returns>
        public static bool Checkarray( List<String> Input, out String SW1)
        {
            SW1 = null;
            for (int i = 0; i < Input.Count; i++)
            {
                //String a = Input[i];
                for (int j = i + 1; j < Input.Count; j++)
                {
                    //String b = Input[j];
                    if (Input[i] == Input[j])
                    {
                        SW1 = Input[i];
                        return true;
                    }
                }
            }
            return false;

        }
        /// <summary>
        /// 判断为字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumOrAlp(this string str)

        {

            string pattern = @"^[A-Za-z0-9]+$";  //@意思忽略转义，+匹配前面一次或多次，$匹配结尾

            Match match = Regex.Match(str, pattern);

            return match.Success;

        }
        // 去掉字符串中的数字  
        public static string RemoveNumber(this string key)
        {
            return Regex.Replace(key, @"\d", "");
        }

        //去掉字符串中的非数字
        public static string RemoveNotNumber(this string key)
        {
            return Regex.Replace(key, @"[^\d]*", "");
        }
    }
    class 排序
    {
        private static readonly int tolerance = 30;
        static List<int[]> PointDisgus(List<int[]> Points)
        {
            int n = Points.Count;
            List<int[]> Points_ = new(Points);
            Points_.Sort((elem1, elem2) => elem1[1].CompareTo(elem2[1]));

            List<int[]> OutPoints = [];
            int a = 0;

            for (int i = 0; i < n; i++)
            {
                List<int[]> PNews = [];

                if (i < a)
                {
                    continue;
                }

                for (int j = i; j < n; j++)
                {
                    if (Math.Abs(Points_[i][1] - Points_[j][1]) < tolerance)  // 设置y方向上的偏差
                    {
                        PNews.Add(Points_[j]);
                        a++;
                    }
                }

                PNews.Sort((elem1, elem2) => elem1[0].CompareTo(elem2[0]));

                foreach (var pp in PNews)
                {
                    OutPoints.Add(pp);
                }

                PNews.Clear();
            }

            return OutPoints;
        }
        public static int CompareBlockssInZOrder(BlockReference a1, BlockReference b1)
        {
            Point3d a = a1.Position;
            Point3d b = b1.Position;
            // 对点进行比较以便按照Z字顺序进行排序
            if (Math.Abs(a.Y - b.Y) <= tolerance)
            {
                if (Math.Abs(a.X - b.X) <= tolerance)
                {
                    return 0;
                }
                else if (a.X < b.X)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else if (a.Y > b.Y)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        static void Main()
        {
            // 示例用法
            List<int[]> inputPoints =
        [
            [1, 5],
            [3, 2],
            [7, 8],
            // Add more points as needed
        ];

            List<int[]> result = PointDisgus(inputPoints);

            // 输出结果
            foreach (var point in result)
            {
                Console.WriteLine($"({point[0]}, {point[1]})");
            }
        }
    }
}
