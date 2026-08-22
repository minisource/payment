using System;
using System.Collections.Generic;
using System.Globalization;

namespace Infrastructure.Common
{
    public static class DateTimeHelper
    {
        public static string ConvertDateTimeToString(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : null;
        }
        public static DateTime? ConvertStringToDateTime(string value)
        {
            if (DateTime.TryParseExact(value, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;

        }

        public static string ConvertToLocalDateTime(this DateTime utcDateTime)
        {
            var localDate = utcDateTime.ConvertToLocalDate();
            var localTime = utcDateTime.ConvertToLocalTime();

            return $"{localDate} {localTime}";
        }

        /// <summary>
        /// Persian full datetime
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns>day, nameofmonth, year , ساعت houre:minute</returns>
        public static string ConvertToDateTimeFullString(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var persianCalendar = new PersianCalendar();

            var year = persianCalendar.GetYear(localDateTime).ToString("0000");
            var month = persianCalendar.GetMonth(localDateTime);
            var persianMonth = GetPersianMonth(month);
            var day = persianCalendar.GetDayOfMonth(localDateTime).ToString("00");

            var convertedDate = $"{day} {persianMonth} {year} ساعت {localDateTime.ToString("HH:mm")}";

            return convertedDate;
        }

        public static string ConvertToLocalDate(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var persianCalendar = new PersianCalendar();

            var year = persianCalendar.GetYear(localDateTime).ToString("0000");
            var month = persianCalendar.GetMonth(localDateTime).ToString("00");
            var day = persianCalendar.GetDayOfMonth(localDateTime).ToString("00");

            var persianDate = $"{year}/{month}/{day}";

            return persianDate;
        }

        public static string ConvertToLocalDateWithFormat(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var persianCalendar = new PersianCalendar();

            var month = persianCalendar.GetMonth(localDateTime);
            var persianMonth = GetPersianMonth(month);
            var day = persianCalendar.GetDayOfMonth(localDateTime);
            var persianDay = GetPersianDay(persianCalendar.GetDayOfWeek(localDateTime));

            var persianDate = $"{persianDay} {day} {persianMonth}";

            return persianDate;
        }

        public static string ConvertToLocalTime(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var localTime = localDateTime.ToString("HH:mm:ss");

            return localTime;
        }

        public static DateTime? ConvertToUtcDateTime(this string persianDateTime)
        {
            if (string.IsNullOrEmpty(persianDateTime)) return null;

            persianDateTime = PersianToEnglish(persianDateTime);

            persianDateTime = persianDateTime.Replace("T", " ");

            var localDateTime = DateTime.Parse(persianDateTime, new CultureInfo("fa-IR"));
            var utcDateTime = localDateTime.ToUniversalTime();

            return utcDateTime;
        }

        public static string ConvertToLocalDateTime(this DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue) return null;

            return utcDateTime.Value.ConvertToLocalDateTime();
        }

        public static string ConvertToLocalDateTimeDifferenceNow(this DateTime utcDateTime)
        {
            var now = DateTime.UtcNow;

            var different = now - utcDateTime;
            var totalDays = (int)different.TotalDays;
            if (totalDays == 0)
            {
                var totalHours = (int)different.TotalHours;
                if (totalHours != 0) return $"{totalHours} ساعت پیش";
                var totalMinutes = (int)different.TotalMinutes;
                if (totalMinutes != 0) return $"{totalMinutes} دقیقه پیش";
                var totalSeconds = (int)different.TotalSeconds;
                return $"{totalSeconds} ثانیه پیش";
            }

            if (totalDays < 2) return $"{totalDays} روز" + " پیش";

            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(utcDateTime);
            var yearNow = persianCalendar.GetYear(now);
            var totalYears = yearNow - year;
            if (totalYears < 1) return utcDateTime.ConvertToLocalDateYearMonth();
            if (totalYears >= 1) return utcDateTime.ConvertToLocalFullDate();

            return utcDateTime.ConvertToLocalDate();
        }

        public static string ConvertToLocalDateYearMonth(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var persianCalendar = new PersianCalendar();

            var month = persianCalendar.GetMonth(localDateTime);
            var persianMonth = GetPersianMonth(month);
            var day = persianCalendar.GetDayOfMonth(localDateTime);

            var persianDate = $" {day} {persianMonth}";

            return persianDate;
        }

        public static string ConvertToLocalFullDate(this DateTime utcDateTime)
        {
            var localDateTime = utcDateTime.ToLocalTime();

            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(localDateTime).ToString("0000");
            var month = persianCalendar.GetMonth(localDateTime);
            var persianMonth = GetPersianMonth(month);
            var day = persianCalendar.GetDayOfMonth(localDateTime);

            var persianDate = $" {day} {persianMonth} {year}";

            return persianDate;
        }
        public static string TimeAgo(this DateTime dateTime)
        {
            string result = string.Empty;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = string.Format("{0} ثانیه قبل", timeSpan.Seconds);
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ?
                    string.Format("{0} دقیقه قبل", timeSpan.Minutes) :
                    "حدود یک دقیقه قبل";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ?
                    string.Format("{0} ساعت قبل", timeSpan.Hours) :
                    "حدود یک ساعت قبل";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    string.Format("{0} روز قبل", timeSpan.Days) :
                    "دیروز";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    string.Format("{0} ماه قبل", timeSpan.Days / 30) :
                    "حدود یک ماه قبل";
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    string.Format("{0} سال قبل", timeSpan.Days / 365) :
                    "حدود یک سال قبل";
            }

            return result;
        }

        public static DateTime UnixTimestampToDateTime(long unixTimeStamp)
        {
            try
            {
                return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(unixTimeStamp);
            }
            catch
            {
                return DateTime.MaxValue;
            }
        }

        public static long DateTimeToUnixTimestamp(this DateTime idateTime)
        {
            try
            {
                idateTime = new DateTime(idateTime.Year, idateTime.Month, idateTime.Day, idateTime.Hour, idateTime.Minute, idateTime.Second);
                TimeSpan unixTimeSpan = idateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).ToLocalTime();
                return long.Parse(unixTimeSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                return 0;
            }
        }

        public static long? DateTimeToUnixTimestamp(this DateTime? idateTime)
        {
            try
            {
                if (idateTime.HasValue)
                    return idateTime.Value.DateTimeToUnixTimestamp();

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static string GetPersianDate(this string date, bool reverse = false)
        {
            System.Globalization.PersianCalendar p = new System.Globalization.PersianCalendar();
            DateTime dt = new DateTime();
            if (!DateTime.TryParse(date, out dt)) dt = DateTime.Now;
            string Returndate = string.Empty;
            if (reverse)
            {
                Returndate = ((p.GetDayOfMonth(dt) > 9) ? p.GetDayOfMonth(dt).ToString() : "0" + p.GetDayOfMonth(dt).ToString());
                Returndate += @"\" + ((p.GetMonth(dt) > 9) ? p.GetMonth(dt).ToString() : "0" + p.GetMonth(dt).ToString());
                Returndate += @"\" + p.GetYear(dt).ToString();
                return Returndate;
            }
            Returndate = p.GetYear(dt).ToString();
            Returndate += @"\" + ((p.GetMonth(dt) > 9) ? p.GetMonth(dt).ToString() : "0" + p.GetMonth(dt).ToString());
            Returndate += @"\" + ((p.GetDayOfMonth(dt) > 9) ? p.GetDayOfMonth(dt).ToString() : "0" + p.GetDayOfMonth(dt).ToString());

            return Returndate;
        }

        public static DateTime ConvertShamsiToMiladi(this DateTime shamsiDate)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            DateTime miladiDate = persianCalendar.ToDateTime(shamsiDate.Year, shamsiDate.Month, shamsiDate.Day, 0, 0, 0, 0);

            return miladiDate;
        }

        #region PrivateMethods

        private static string GetPersianMonth(int month)
        {
            var months = new List<string>(new[]
            {
                "فروردين", "اردیبهشت", "خرداد",
                "تیر", "مرداد", "شهریور",
                "مهر", "آبان", "آذر",
                "دی", "بهمن", "اسفند"
            });
            return months[month - 1];
        }

        private static string GetPersianDay(DayOfWeek day)
        {
            var Day = new List<string>(new[]
            {
                "یکشنبه", "دوشنبه", "سه شنبه",
                "چهار شنبه", "پنج شنبه", "جمعه",
                "شنبه"
            });
            return Day[(int)day];
        }

        public static string PersianToEnglish(string input)
        {
            var lettersDictionary = new Dictionary<char, char>
            {
                ['۰'] = '0',
                ['۱'] = '1',
                ['۲'] = '2',
                ['۳'] = '3',
                ['۴'] = '4',
                ['۵'] = '5',
                ['۶'] = '6',
                ['۷'] = '7',
                ['۸'] = '8',
                ['۹'] = '9'
            };
            foreach (var item in input)
                if (lettersDictionary.TryGetValue(item, out var englishDigit))
                    input = input.Replace(item, englishDigit);
            return input;
        }

        #endregion PrivateMethods
    }
}