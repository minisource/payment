using System.Globalization;

namespace Infrastructure.Common;

//
// Summary:
//     Converts English digits of a given number to their equivalent Persian digits.
public static class PersianNumbersUtils
{
    //
    // Summary:
    //     Converts English digits of a given number to their equivalent Persian digits.
    public static string ToPersianNumbers(this int number, string format = "")
    {
        return (!string.IsNullOrEmpty(format) ? number.ToString(format, CultureInfo.InvariantCulture) : number.ToString(CultureInfo.InvariantCulture)).ToPersianNumbers();
    }

    //
    // Summary:
    //     Converts English digits of a given number to their equivalent Persian digits.
    public static string ToPersianNumbers(this long number, string format = "")
    {
        return (!string.IsNullOrEmpty(format) ? number.ToString(format, CultureInfo.InvariantCulture) : number.ToString(CultureInfo.InvariantCulture)).ToPersianNumbers();
    }

    //
    // Summary:
    //     Converts English digits of a given number to their equivalent Persian digits.
    public static string ToPersianNumbers(this int? number, string format = "")
    {
        if (!number.HasValue)
        {
            number = 0;
        }

        return (!string.IsNullOrEmpty(format) ? number.Value.ToString(format, CultureInfo.InvariantCulture) : number.Value.ToString(CultureInfo.InvariantCulture)).ToPersianNumbers();
    }

    //
    // Summary:
    //     Converts English digits of a given number to their equivalent Persian digits.
    public static string ToPersianNumbers(this long? number, string format = "")
    {
        if (!number.HasValue)
        {
            number = 0L;
        }

        return (!string.IsNullOrEmpty(format) ? number.Value.ToString(format, CultureInfo.InvariantCulture) : number.Value.ToString(CultureInfo.InvariantCulture)).ToPersianNumbers();
    }

    //
    // Summary:
    //     Converts English digits of a given string to their equivalent Persian digits.
    //
    // Parameters:
    //   data:
    //     English number
    public static string ToPersianNumbers(this string data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        char[] array = data!.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            switch (array[i])
            {
                case '0':
                case '٠':
                    array[i] = '۰';
                    break;
                case '1':
                case '١':
                    array[i] = '۱';
                    break;
                case '2':
                case '٢':
                    array[i] = '۲';
                    break;
                case '3':
                case '٣':
                    array[i] = '۳';
                    break;
                case '4':
                case '٤':
                    array[i] = '۴';
                    break;
                case '5':
                case '٥':
                    array[i] = '۵';
                    break;
                case '6':
                case '٦':
                    array[i] = '۶';
                    break;
                case '7':
                case '٧':
                    array[i] = '۷';
                    break;
                case '8':
                case '٨':
                    array[i] = '۸';
                    break;
                case '9':
                case '٩':
                    array[i] = '۹';
                    break;
            }
        }

        return new string(array);
    }

    //
    // Summary:
    //     Converts Persian and Arabic digits of a given string to their equivalent English
    //     digits.
    //
    // Parameters:
    //   data:
    //     Persian number
    public static string ToEnglishNumbers(this string data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        char[] array = data!.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            switch (array[i])
            {
                case '٠':
                case '۰':
                    array[i] = '0';
                    break;
                case '١':
                case '۱':
                    array[i] = '1';
                    break;
                case '٢':
                case '۲':
                    array[i] = '2';
                    break;
                case '٣':
                case '۳':
                    array[i] = '3';
                    break;
                case '٤':
                case '۴':
                    array[i] = '4';
                    break;
                case '٥':
                case '۵':
                    array[i] = '5';
                    break;
                case '٦':
                case '۶':
                    array[i] = '6';
                    break;
                case '٧':
                case '۷':
                    array[i] = '7';
                    break;
                case '٨':
                case '۸':
                    array[i] = '8';
                    break;
                case '٩':
                case '۹':
                    array[i] = '9';
                    break;
            }
        }

        return new string(array);
    }
}