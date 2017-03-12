using System;
using System.Text;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format. Defaults to UTF8 Encoding.
        /// </summary>
        public static byte[] ToBytes(this string input, Encoding encoding = null)
        {
            encoding = encoding ?? new UTF8Encoding();
            return encoding.GetBytes(input);
        }

        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static bool EqualsIgnoreCase(this string input, string other, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return input.Equals(other, comparison);
        }

        /// <summary>
        /// Remove non-alphanumeric characters from a string (and optionally whitespace as well)
        /// </summary>
        public static string ToAlphanumeric(this string input, bool allowWhiteSpace = false)
        {
            if (allowWhiteSpace)
                return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))));

            return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c)))));
        }

        /// <summary>
        /// Performs a given type of string comparison contains
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Performs a case sensitive comparison contains
        /// </summary>
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static int ToInt(this string s, int fallback = -1)
        {
            return ToNullableInt(s) ?? fallback;
        }

        public static int? ToNullableInt(this string s)
        {
            return int.TryParse(s, out int i) ? i : (int?)null;
        }

        public static decimal ToDecimal(this string o, decimal fallback = 0)
        {
            return ToNullableDecimal(o) ?? fallback;
        }

        public static decimal? ToNullableDecimal(this string o)
        {
            return Decimal.TryParse(o, out decimal d) ? d : (decimal?)null;
        }

        public static DateTime ToDateTime(this string s)
        {
            return ToNullableDateTime(s) ?? DateTime.MinValue;
        }

        public static DateTime? ToNullableDateTime(this string s)
        {
            return DateTime.TryParse(s, out DateTime d) ? d : (DateTime?)null;
        }

        public static bool ToBoolean(this string o, bool fallback = false)
        {
            return ToNullableBoolean(o) ?? fallback;
        }

        public static bool? ToNullableBoolean(this string o)
        {
            return Boolean.TryParse(o, out bool b) ? b : (bool?)null;
        }
    }
}
