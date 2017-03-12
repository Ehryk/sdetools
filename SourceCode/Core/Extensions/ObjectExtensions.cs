using System;
using System.Text;

namespace Core.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static string GetString(this byte[] hashBytes, bool uppercase = true)
        {
            StringBuilder hashString = new StringBuilder();

            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return uppercase ? hashString.ToString().ToUpper() : hashString.ToString();
        }

        public static int ToInt(this object o, int fallback = -1)
        {
            return ToNullableInt(o) ?? fallback;
        }
        public static int? ToNullableInt(this object o)
        {
            if (o is int)
                return (int)o;

            return int.TryParse(o.ToString(), out int i) ? i : (int?)null;
        }

        public static decimal ToDecimal(this object o, decimal fallback = 0)
        {
            return ToNullableDecimal(o) ?? fallback;
        }
        public static decimal? ToNullableDecimal(this object o)
        {
            if (o is decimal)
                return (decimal)o;

            return Decimal.TryParse(o.ToString(), out decimal d) ? d : (decimal?)null;
        }

        public static DateTime ToDateTime(this object o)
        {
            return ToNullableDateTime(o) ?? DateTime.MinValue;
        }
        public static DateTime? ToNullableDateTime(this object o)
        {
            if (o is DateTime)
                return (DateTime)o;

            return DateTime.TryParse(o.ToString(), out DateTime d) ? d : (DateTime?)null;
        }

        public static bool ToBoolean(this object o, bool fallback = false)
        {
            return ToNullableBoolean(o) ?? fallback;
        }
        public static bool? ToNullableBoolean(this object o)
        {
            if (o is bool)
                return (bool)o;

            return Boolean.TryParse(o.ToString(), out bool b) ? b : (bool?)null;
        }
    }
}
