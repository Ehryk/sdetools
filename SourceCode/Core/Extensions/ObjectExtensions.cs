using System;
using System.Text;

namespace Core.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static string GetString(this byte[] pHashBytes, bool pUppercase = true)
        {
            StringBuilder hashString = new StringBuilder();

            foreach (byte b in pHashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return pUppercase ? hashString.ToString().ToUpper() : hashString.ToString();
        }

        public static int ToInt(this object o, int pDefault = -1)
        {
            return ToNullableInt(o) ?? pDefault;
        }

        public static int? ToNullableInt(this object o)
        {
            if (o is int)
                return (int)o;

            return Int32.TryParse(o.ToString(), out int i) ? i : (int?)null;
        }

        public static long ToLong(this object o, long pDefault = -1)
        {
            return ToNullableLong(o) ?? pDefault;
        }

        public static long? ToNullableLong(this object o)
        {
            if (o is long)
                return (long)o;

            return Int64.TryParse(o.ToString(), out long i) ? i : (long?)null;
        }

        public static decimal ToDecimal(this object o, decimal pDefault = 0)
        {
            return ToNullableDecimal(o) ?? pDefault;
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

        public static bool ToBoolean(this object o, bool pDefault = false)
        {
            return ToNullableBoolean(o) ?? pDefault;
        }

        public static bool? ToNullableBoolean(this object o)
        {
            if (o is bool)
                return (bool)o;

            if (o is string)
            {
                switch (((string)o).ToUpper())
                {
                    case "TRUE":
                    case "YES":
                    case "Y":
                    case "1":
                    case "T":
                        return true;

                    case "FALSE":
                    case "NO":
                    case "N":
                    case "0":
                    case "F":
                        return false;
                }
            }

            return Boolean.TryParse(o.ToString(), out bool b) ? b : (bool?)null;
        }
    }
}
