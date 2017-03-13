using System;
using Miner.Interop;
using Miner.Geodatabase;
using ESRI.ArcGIS.Geodatabase;

namespace Core.ArcObjects
{
    public static class ModelNameHelper
    {
        #region Class Model Names

        public static bool AddClassModelName(this IObjectClass pClass, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.AddClassModelName(pClass, pModelName);

            return pClass.HasModelName(pModelName);
        }

        public static bool RemoveClassModelName(this IObjectClass pClass, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.RemoveClassModelName(pClass, pModelName);

            return !pClass.HasModelName(pModelName);
        }

        public static bool HasModelName(this IObjectClass pClass, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            return modelNameManager.ContainsClassModelName(pClass, pModelName);
        }

        #endregion

        #region Field Model Names

        public static bool AddFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.AddFieldModelName(pClass, pField, pModelName);

            return pClass.HasFieldModelName(pField, pModelName);
        }

        public static bool RemoveFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.RemoveFieldModelName(pClass, pField, pModelName);

            return !pClass.HasFieldModelName(pField, pModelName);
        }

        public static bool HasFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            return modelNameManager.ContainsFieldModelName(pClass, pField, pModelName);
        }

        #endregion
    }
}
