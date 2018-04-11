using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace PGE.ArcFM.Common
{
    public static class WorkspaceExtensions
    {
        public static IObjectClass GetObjectClass(this IWorkspace pWorkspace, string pName)
        {
            if (pWorkspace is null)
                throw new ArgumentException("pWorkspace");

            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)pWorkspace;
            IFeatureClass featureClass = featureWorkspace.OpenFeatureClass(pName);
            return featureClass;
        }

        public static IWorkspace GetWsFromObject(this IObject pObject)
        {
            IWorkspace retVal = null;

            IObjectClass pObjectClass = pObject.Class;
            IDataset pDataset = pObjectClass as IDataset;

            if (pDataset != null)
                retVal = pDataset.Workspace;

            return retVal;
        }
        
        public static IWorkspace GetWsFromObjectClass(this IObjectClass pObjectClass)
        {
            IWorkspace retVal = null;

            IDataset pDataset = pObjectClass as IDataset;

            if (pDataset != null)
                retVal = pDataset.Workspace;

            return retVal;
        }
    }
}
