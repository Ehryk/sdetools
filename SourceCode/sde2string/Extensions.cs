using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace sde2string
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

        public static int ToInt(this string s)
        {
            return ToNullableInt(s) ?? -1;
        }
        public static int? ToNullableInt(this string s)
        {
            int i;
            return int.TryParse(s, out i) ? i : (int?)null;
        }

        public static decimal ToDecimal(this string o)
        {
            return ToNullableDecimal(o) ?? 0;
        }
        public static decimal? ToNullableDecimal(this string o)
        {
            decimal d;
            return Decimal.TryParse(o, out d) ? d : (decimal?)null;
        }

        public static DateTime ToDateTime(this string s)
        {
            return ToNullableDateTime(s) ?? DateTime.MinValue;
        }
        public static DateTime? ToNullableDateTime(this string s)
        {
            DateTime d;
            return DateTime.TryParse(s, out d) ? d : (DateTime?)null;
        }

        public static bool ToBoolean(this string o)
        {
            return ToNullableBoolean(o) ?? false;
        }
        public static bool? ToNullableBoolean(this string o)
        {
            bool b;
            return Boolean.TryParse(o, out b) ? b : (bool?)null;
        }
    }

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

        public static int ToInt(this object o)
        {
            return ToNullableInt(o) ?? -1;
        }
        public static int? ToNullableInt(this object o)
        {
            if (o is int)
                return (int)o;

            int i;
            return int.TryParse(o.ToString(), out i) ? i : (int?)null;
        }

        public static decimal ToDecimal(this object o)
        {
            return ToNullableDecimal(o) ?? 0;
        }
        public static decimal? ToNullableDecimal(this object o)
        {
            if (o is decimal)
                return (decimal)o;

            decimal d;
            return Decimal.TryParse(o.ToString(), out d) ? d : (decimal?)null;
        }

        public static DateTime ToDateTime(this object o)
        {
            return ToNullableDateTime(o) ?? DateTime.MinValue;
        }
        public static DateTime? ToNullableDateTime(this object o)
        {
            if (o is DateTime)
                return (DateTime)o;

            DateTime d;
            return DateTime.TryParse(o.ToString(), out d) ? d : (DateTime?)null;
        }

        public static bool ToBoolean(this object o)
        {
            return ToNullableBoolean(o) ?? false;
        }
        public static bool? ToNullableBoolean(this object o)
        {
            if (o is bool)
                return (bool)o;

            bool b;
            return Boolean.TryParse(o.ToString(), out b) ? b : (bool?)null;
        }
    }

    public static class ApplicationInfo
    {
        public static Version Version { get { return Assembly.GetCallingAssembly().GetName().Version; } }

        public static string Title
        {
            get
            {
                object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title.Length > 0) return titleAttribute.Title;
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static string ProductName
        {
            get
            {
                object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string Description
        {
            get
            {
                object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string CopyrightHolder
        {
            get
            {
                object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string CompanyName
        {
            get
            {
                object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    }
}
