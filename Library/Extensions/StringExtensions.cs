using System;
using System.Text;
namespace Library.Extensions
{
    public static class StringExtensions
    {
        public static string AddSpaceAfterUpperCase(this string inputString, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return string.Empty;
            }
            var result = new StringBuilder(inputString.Length * 2);
            var strTemp = inputString;
            try
            {
                result.Append(inputString[0]);
                for (var i = 1; i < inputString.Length; i++)
                {
                    if (char.IsUpper(inputString[i]))
                        if (inputString[i - 1] != ' ' && !char.IsUpper(inputString[i - 1]) ||
                            preserveAcronyms && char.IsUpper(inputString[i - 1]) && i < inputString.Length - 1 &&
                            !char.IsUpper(inputString[i + 1]))
                        {
                            result.Append(' ');
                        }
                    result.Append(inputString[i]);
                }
            }
            catch (Exception)
            {
                return strTemp;
            }
            return result.ToString();
        }
    }

}



