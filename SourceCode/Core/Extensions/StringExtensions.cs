using System;
using System.Text;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format. Defaults to UTF8 Encoding.
        /// </summary>
        public static byte[] ToBytes(this string pInput, Encoding pEncoding = null)
        {
            pEncoding = pEncoding ?? new UTF8Encoding();
            return pEncoding.GetBytes(pInput);
        }

        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static bool EqualsIgnoreCase(this string pInput, string pOther, StringComparison pComparison = StringComparison.OrdinalIgnoreCase)
        {
            return pInput.Equals(pOther, pComparison);
        }

        /// <summary>
        /// Remove non-alphanumeric characters from a string (and optionally whitespace as well)
        /// </summary>
        public static string ToAlphanumeric(this string pInput, bool pAllowWhiteSpace = false)
        {
            if (pAllowWhiteSpace)
                return new string(Array.FindAll(pInput.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))));

            return new string(Array.FindAll(pInput.ToCharArray(), (c => (char.IsLetterOrDigit(c)))));
        }

        /// <summary>
        /// Performs a given type of string comparison contains
        /// </summary>
        public static bool Contains(this string pSource, string pToCheck, StringComparison pComparison)
        {
            return pSource.IndexOf(pToCheck, pComparison) >= 0;
        }

        /// <summary>
        /// Performs a case sensitive comparison contains
        /// </summary>
        public static bool ContainsIgnoreCase(this string pSource, string pToCheck)
        {
            return pSource.IndexOf(pToCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static int ToInt(this string s, int pDefault = -1)
        {
            return ToNullableInt(s) ?? pDefault;
        }

        public static int? ToNullableInt(this string s)
        {
            return Int32.TryParse(s, out int i) ? i : (int?)null;
        }

        public static long ToLong(this string s, long pDefault = -1)
        {
            return ToNullableLong(s) ?? pDefault;
        }

        public static long? ToNullableLong(this string s)
        {
            return Int64.TryParse(s, out long i) ? i : (long?)null;
        }

        public static decimal ToDecimal(this string s, decimal pDefault = 0)
        {
            return ToNullableDecimal(s) ?? pDefault;
        }

        public static decimal? ToNullableDecimal(this string s)
        {
            return Decimal.TryParse(s, out decimal d) ? d : (decimal?)null;
        }

        public static DateTime ToDateTime(this string s)
        {
            return ToNullableDateTime(s) ?? DateTime.MinValue;
        }

        public static DateTime? ToNullableDateTime(this string s)
        {
            return DateTime.TryParse(s, out DateTime d) ? d : (DateTime?)null;
        }

        public static bool ToBoolean(this string s, bool pDefault = false)
        {
            return ToNullableBoolean(s) ?? pDefault;
        }

        public static bool? ToNullableBoolean(this string s)
        {
            return Boolean.TryParse(s, out bool b) ? b : (bool?)null;
        }
    }
}
