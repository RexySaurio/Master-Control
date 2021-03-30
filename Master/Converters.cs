using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Master
{
    public class Converters
    {
        public static void CMDOutput(string txt)
        {
            txt = txt.Replace("<CMDOUT>", "");
            MasterConsole.ConsoleMessage(txt, CONSOLE_MSG.cmdClient);
        }

        public static int ParseValidNumber(string text)
        {

            int toReturn = -1; //! -1 is invalid

            //? Contains letters?
            if (!Check.ContainsLetters(text))
            {
                try
                {
                    toReturn = int.Parse(text);
                }
                catch (Exception) { }

            }
            
            return toReturn;
        }

        /// <summary>
        /// Get Text between two chars
        /// </summary>
        /// <param name="input">Input to extract</param>
        /// <param name="start">Initial char</param>
        /// <param name="end">End char</param>
        /// <returns> Strign between two chars</returns>
        public static string TextBetween(string input, string start, string end)
        {
            int posFrom = input.IndexOf(start);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(end, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
    }

    public class Check
    {
        /// <summary>
        /// Get Lenght without count spaceses
        /// </summary>
        /// <param name="arr"> Array to check </param>
        /// <returns> true/false </returns>
        public static int FixedLenght(string[] arr)
        {
            int count = 0;
            foreach(string s in arr)
            {
                if(s != " ")
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if the string array contains specified character
        /// </summary>
        /// <param name="arr"> String array </param>
        /// <param name="pt"> Char </param>
        /// <returns> True/False </returns>
        public static bool ArrayContains(string[] arr, char pt)
        {
            string st = "";
            foreach(string cn in arr)
            {
                st += cn;
            }

            foreach(char c in st)
            {
                if(c == pt)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the string array contains specified
        /// </summary>
        /// <param name="arr"> String array </param>
        /// <param name="pt"> String </param>
        /// <returns> True/False </returns>
        public static bool ArrayContains(string[] arr, string pt)
        {
            foreach (string cn in arr)
            {
                if (cn == pt)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check Texts Similarity by percentage
        /// </summary>
        /// <param name="s"> String to check </param>
        /// <param name="t"> String to compare </param>
        /// <returns> true/false </returns>
        public static bool Similarity(string s, string t)
        {

            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return false;
                return false;
            }

            if (string.IsNullOrEmpty(t))
            {
                return false;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            int Coinc =  d[n, m];

            float perc = (Coinc * 100)/t.Length;

            if(perc < 50)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the string contains Letters  Upper-Lower
        /// </summary>
        /// <param name="text"> String </param>
        /// <returns> True/False </returns>
        public static bool ContainsLetters(string text)
        {
            string lett = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ";

            bool toReturn = false;
            foreach (char c in text)
            {
                if (lett.Contains(c))
                {
                    toReturn = true;
                    break;
                }
            }

            return toReturn;
        }
    }

    public class Auxiliars
    {
        /// <summary>
        /// Generate random ID
        /// </summary>
        /// <param name="lenght">Lenght id</param>
        /// <returns> random string </returns>
        public static string RandomID(int lenght)
        {
            string toReturn = string.Empty;

            string letts = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for(int i=0; i < lenght; i++)
            {
                int sw = Status.rand.Next(0,1);

                if(sw == 0)
                {
                    // Letter
                    toReturn += letts[Status.rand.Next(0, letts.Length - 1)];
                }
                else
                {
                    // Number
                    toReturn += Status.rand.Next(0,9).ToString();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get bytes from file
        /// </summary>
        /// <returns> File in bytes </returns>
        public static byte[] GetFileBytes(string filePath)
        {
            byte[] bts = new byte[1024];

            //? Exist?
            if (File.Exists(filePath))
            {
                bts = File.ReadAllBytes(filePath);
            }
            else
            {
                MasterConsole.ConsoleMessage("The file not exists", CONSOLE_MSG.error);
            }


            return bts;
        }
    }
}
