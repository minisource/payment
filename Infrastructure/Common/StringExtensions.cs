using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Infrastructure.Common
{
    public static class StringExtensions
    {
        public static string FormatMobileNumber(string inputNumber)
        {
            if(string.IsNullOrWhiteSpace(inputNumber)) 
                throw new ArgumentNullException(nameof(inputNumber), "شماره موبایل وارد نشده است");

            // Remove any non-numeric characters from the input
            string cleanedNumber = Regex.Replace(inputNumber, @"[^\d]", "");
            cleanedNumber = cleanedNumber.TrimStart('0');

            // Check if the cleaned number is empty or null
            if (string.IsNullOrEmpty(cleanedNumber))
            {
                throw new ArgumentException("شماره موبایل خود را در سامانه تکمیل کنید");
            }

            // Check if the number already starts with +98 or 0098
            if (cleanedNumber.StartsWith("98") && cleanedNumber.Length != 10)
            {
                return "+" + cleanedNumber;
            }
            else if (cleanedNumber.StartsWith("0098"))
            {
                return "+" + cleanedNumber.Substring(2);
            }

            // If none of the above conditions are met, prepend +98 to the number
            return "+98" + cleanedNumber;
        }

        public static bool ContainsNumber(this string inputText)
        {
            return !string.IsNullOrWhiteSpace(inputText) && inputText.ToEnglishNumbers().Any(char.IsDigit);
        }

        public static bool HasConsecutiveChars(this string inputText, int sequenceLength = 3)
        {
            var charEnumerator = StringInfo.GetTextElementEnumerator(inputText);
            var currentElement = string.Empty;
            var count = 1;
            while (charEnumerator.MoveNext())
            {
                if (string.Equals(currentElement, charEnumerator.GetTextElement(), StringComparison.Ordinal))
                {
                    if (++count >= sequenceLength)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 1;
                    currentElement = charEnumerator.GetTextElement();
                }
            }

            return false;
        }

        public static bool IsEmailAddress(this string inputText)
        {
            return !string.IsNullOrWhiteSpace(inputText) && new EmailAddressAttribute().IsValid(inputText);
        }

        public static bool IsNumeric(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return false;
            }

            return long.TryParse(inputText.ToEnglishNumbers(), NumberStyles.Number, CultureInfo.InvariantCulture, out _);
        }

        public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
        {
            return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
        }

        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value);
        }

        public static string ToNumeric(this int value)
        {
            return value.ToString("N0"); //"123,456"
        }

        public static string ToNumeric(this decimal value)
        {
            return value.ToString("N0");
        }

        public static string ToCurrency(this int value)
        {
            //fa-IR => current culture currency symbol => ریال
            //123456 => "123,123ریال"
            return value.ToString("N0");
        }

        public static string ToCurrency(this decimal value)
        {
            return value.ToString("N0");
        }

        public static string En2Fa(this string str)
        {
            return str.Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("2", "۲")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("5", "۵")
                .Replace("6", "۶")
                .Replace("7", "۷")
                .Replace("8", "۸")
                .Replace("9", "۹");
        }

        public static string Fa2En(this string str)
        {
            return str.Replace("۰", "0")
                .Replace("۱", "1")
                .Replace("۲", "2")
                .Replace("۳", "3")
                .Replace("۴", "4")
                .Replace("۵", "5")
                .Replace("۶", "6")
                .Replace("۷", "7")
                .Replace("۸", "8")
                .Replace("۹", "9")
                //iphone numeric
                .Replace("٠", "0")
                .Replace("١", "1")
                .Replace("٢", "2")
                .Replace("٣", "3")
                .Replace("٤", "4")
                .Replace("٥", "5")
                .Replace("٦", "6")
                .Replace("٧", "7")
                .Replace("٨", "8")
                .Replace("٩", "9");
        }

        public static string FixPersianChars(this string str)
        {
            return str.Replace("ﮎ", "ک")
                .Replace("ﮏ", "ک")
                .Replace("ﮐ", "ک")
                .Replace("ﮑ", "ک")
                .Replace("ك", "ک")
                .Replace("ي", "ی")
                .Replace(" ", " ")
                .Replace("‌", " ")
                .Replace("ھ", "ه");//.Replace("ئ", "ی");
        }

        public static string CleanString(this string? str)
        {
            if(string.IsNullOrWhiteSpace(str)) return str;
            return str.Trim().FixPersianChars().Fa2En().NullIfEmpty();
        }

        public static string NullIfEmpty(this string str)
        {
            return str?.Length == 0 ? null : str;
        }

        public static string Join(string delimeter, string[] items)
        {
            var result = items.Aggregate("", (current, obj) => current + obj + ",");
            return result.Substring(0, result.Length - 1);
        }
        public static string Join(string delimeter, long[] items)
        {
            string result = items.Aggregate("", (current, obj) => current + obj.ToString() + ",");
            return result.Substring(0, result.Length - 1);
        }

        public static string ConvertToQueryStrings(object viewModel) =>
            string.Join("&", viewModel.GetType().GetProperties()
                 .Where(p => p.GetValue(viewModel) != null)
                .Select(p => $"{p.Name}={HttpUtility.UrlEncode(p.GetValue(viewModel)?.ToString())}"));

        public static T? ConvertTo<T>(this string input, object? defaultValue = default)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue is not null ? (T?)defaultValue : default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter == null)
                return (T?)defaultValue;

            try
            {
                return (T)converter.ConvertFromString(input);
            }
            catch (Exception)
            {
                if (typeof(T) == typeof(bool))
                {
                    if (input == "0")
                        return (T)(object)false;
                    if (input == "1")
                        return (T)(object)true;
                }

                return defaultValue is not null ? (T?)defaultValue : default(T);
            }
        }

        /// <summary>
        /// چکمی کند اسلاگ طبق فرمت تعریف شده باشد و کاراکترهای نامعتبر را تبدیل به - می کند
        /// </summary>
        /// <param name="slug">آدرس دسترسی</param>
        /// <returns></returns>
        public static string GetSlug(this string slug)
        {
            string newstr = " ";
            slug = slug.Trim();
            string expression = @"(^a-zA-Z0-9-آ-ی-_-)";
            Regex regx = new Regex(expression);
            if (regx.IsMatch(slug))
            {
                newstr = slug;
            }
            else
            {
                var reg = new Regex(@"[^a-zA-Z0-9-آ-ی-_-]");
                newstr = reg.Replace(slug, "-");

            }
            newstr = Regex.Replace(newstr, @"\s+", "-");
            newstr = System.Text.RegularExpressions.Regex.Replace(newstr, @"[ ]\s{2,}", "-");
            newstr = System.Text.RegularExpressions.Regex.Replace(newstr, "[-]+$", "");
            newstr = System.Text.RegularExpressions.Regex.Replace(newstr, "^[-]", "");
            return newstr.Replace(" ", "");
        }

        public static string ToMd5(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hash) sb.Append(t.ToString("X2"));
            return sb.ToString().ToLower();
        }
    }
}
