using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;

namespace Core.ArcObjects
{
    public static class PropertySetHelper
    {
        public static string ToPropertiesString(this IPropertySet propertySet, bool bracketless = false)
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            string format = bracketless ? "{0}={1};" : "[{0}]={1}";

            return propertySet.ToPropertiesString(format);
        }

        public static string ToPropertiesString(this IPropertySet propertySet, string format = "{0}={1}\r\n")
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            StringBuilder result = new StringBuilder();

            foreach (var property in propertySet.ToDictionary())
            {
                result.AppendFormat(format, property.Key, property.Value);
            }

            return result.ToString().Trim();
        }

        public static Dictionary<string, string> ToDictionary(this IPropertySet propertySet)
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            int propertyCount = propertySet.Count;
            var dictionary = new Dictionary<string, string>();

            propertySet.GetAllProperties(out object nameArray, out object valueArray);
            object[] names = (object[])nameArray;
            object[] values = (object[])valueArray;

            for (int i = 0; i < propertyCount; i++)
            {
                dictionary.Add(names[i].ToString(), values[i].ToString());
            }

            return dictionary;
        }

        public static IPropertySet PropertySetFromString(string propertySetText)
        {
            if (String.IsNullOrEmpty(propertySetText))
                throw new ArgumentNullException("propertySetText");

            IPropertySet propertySet = new PropertySetClass();

            string[] properties = propertySetText.Split(new[] { ';' });

            for (int i = 0; i < properties.Count(); i++)
            {
                if (String.IsNullOrEmpty(properties[i])) continue;

                string[] propertyParts = properties[i].Split(new[] { '=' });

                // Remove [ ]
                string name = propertyParts[0].Substring(1, propertyParts[0].Length - 2);
                string value = propertyParts[1];

                propertySet.SetProperty(name, value);
            }

            return propertySet;
        }
    }
}
