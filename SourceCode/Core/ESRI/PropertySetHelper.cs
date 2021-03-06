﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;

namespace Core.ArcObjects
{
    public static class PropertySetHelper
    {
        public static IPropertySet PropertySetFromString(string pPropertySetText)
        {
            if (String.IsNullOrEmpty(pPropertySetText))
                throw new ArgumentNullException("pPropertySetText");

            IPropertySet propertySet = new PropertySetClass();

            string[] properties = pPropertySetText.Split(new[] { ';' });

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
