using System;
using System.Text;
using System.Collections.Generic;
using log4net;
using ESRI.ArcGIS.esriSystem;

namespace Core.ArcObjects
{
    public static class EsriExtensions
    {
        #region Private Properties

        private static readonly log4net.ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #endregion

        #region IEnumBSTR Extensions

        public static List<string> ToList(this IEnumBSTR pEnum)
        {
            List<string> modelNames = new List<string>();
            string modelName = pEnum.Next();

            try
            {
                while (!String.IsNullOrEmpty(modelName))
                {
                    modelNames.Add(modelName);
                    modelName = pEnum.Next();
                }
            }
            catch (Exception ex)
            {
                //Iteration Over
                _log.Warn(String.Format("Iteration of {0} threw exception: {1}", pEnum, ex.Message), ex);
            }

            return modelNames;
        }

        #endregion

        #region IPropertySet Extensions

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

        #endregion
    }
}
