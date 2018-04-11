using System;
using System.Collections.Generic;
using log4net;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using Miner.Interop;
using Miner.Geodatabase;

namespace Core.ArcFM
{
    public static class ClassModelNameExtensions
    {
        #region Private Properties

        private static readonly log4net.ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IMMModelNameManager _modelNameManager = null;

        private static IMMModelNameManager ModelNameManager
        {
            get { return _modelNameManager ?? Miner.Geodatabase.ModelNameManager.Instance; }
        }

        #endregion

        #region Query Class Model Names

        public static bool ContainsModelName(this IObjectClass pClass, string pModelName)
        {
            if (ModelNameManager == null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!ModelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            return ModelNameManager.ContainsClassModelName(pClass, pModelName);
        }

        public static List<string> ListModelNames(this IObjectClass pClass)
        {
            if (ModelNameManager == null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!ModelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            IEnumBSTR modelNamesEnum = ModelNameManager.ClassModelNames(pClass);

            return modelNamesEnum.ToList();
        }

        public static EnumerableMmObjectClass GetClasses(this IWorkspace pWorkspace, string pClassModelName)
        {
            return ModelNameManager.ObjectClassesFromModelNameWS(pWorkspace, pClassModelName).ToEnumerable();
        }

        public static IObjectClass GetClass(this IWorkspace pWorkspace, string pClassModelName, bool pRaiseError = false, bool pAllowMultiple = false)
        {
            var classes = pWorkspace.GetClasses(pClassModelName);

            if (classes.Count() == 0 && raiseError)
                throw new Exception(String.Format("Unable to find required class model name {0} on any classes in this geodatabase.", pClassModelName));
            else if (classes.Count() > 1)
                threw new Exception(String.Format("Multiple classes found with class model name {0}", pClassModelName));

            return classes.FirstOrDefault();
        }

        #endregion

        #region Manage Class Model Names

        public static bool AddClassModelName(this IObjectClass pClass, string pModelName)
        {
            if (pClass is null)
                throw new ArgumentException("pClass");

            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (modelNameManager.ContainsClassModelName(pClass, pModelName))
                return false;

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.AddClassModelName(pClass, pModelName);
            
            return modelNameManager.ContainsClassModelName(pClass, pModelName);
        }

        public static bool RemoveClassModelName(this IObjectClass pClass, string pModelName)
        {
            if (pClass is null)
                throw new ArgumentException("pClass");

            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.ContainsClassModelName(pClass, pModelName))
                return false;

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.RemoveClassModelName(pClass, pModelName);

            return !modelNameManager.ContainsClassModelName(pClass, pModelName);
        }

        #endregion
    }
}
