using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using Core.Extensions;
using System.IO;

namespace Core.ArcObjects
{
    public static class SDEFileHelper
    {
        #region SDE File Helpers

        public static string GetConnectionStringFromSDEFile(string pPath, bool pBracketless)
        {
            return GetPropertySetFromSDEFile(pPath).ToPropertiesString(pBracketless);
        }

        public static IPropertySet GetPropertySetFromSDEFile(string pPath)
        {
            return GetWorkspaceFromSDEFile(pPath).ConnectionProperties;
        }

        public static Dictionary<string, string> GetDictionaryFromSDEFile(string pPath)
        {
            return GetWorkspaceFromSDEFile(pPath).ConnectionProperties.ToDictionary();
        }

        public static IWorkspace GetWorkspaceFromSDEFile(string pPath)
        {
            return WorkspaceFromSDEFile(pPath);
        }

        public static IWorkspace GetWorkspaceFromSDEFile(string pPath, string pFeatureClass)
        {
            return WorkspaceFromSDEFile_FeatureClass(pPath, pFeatureClass);
        }

        public static IFeatureWorkspace GetFeatureWorkspaceFromSDEFile(string pPath)
        {
            return FeatureWorkspaceFromSDEFile(pPath);
        }

        public static Dictionary<string, string> DirectParseSDEFile(string pPath, Encoding pEncoding = null)
        {
            string contents;

            if (pEncoding == null)
                contents = File.ReadAllText(pPath);
            else
                contents = pEncoding.GetString(File.ReadAllBytes(pPath));

            return GetSDEDictionary(contents);
        }

        public static Dictionary<string, string> GetSDEDictionary(string contents)
        {
            return new Dictionary<string, string>();
        }

        #endregion

        #region Workspace Helpers

        private static IFeatureWorkspace FeatureWorkspaceFromSDEFile(string pPath)
        {
            return WorkspaceFromSDEFile(pPath) as IFeatureWorkspace;
        }

        private static IWorkspace WorkspaceFromSDEFile(string pPath)
        {
            SdeWorkspaceFactory pWSFactory = new SdeWorkspaceFactory();
            IWorkspace pWSpace = pWSFactory.OpenFromFile(pPath, 0);

            return pWSpace;
        }

        private static IWorkspace WorkspaceFromSDEFile_FeatureClass(string pPath, string pFeatureClass)
        {
            SdeWorkspaceFactory wsFactory = new SdeWorkspaceFactory();
            IWorkspace ws = wsFactory.OpenFromFile(pPath, 0);

            IFeatureClass feature = (IFeatureClass)ws.GetObjectClass(pFeatureClass);
            ws = ((IDataset)feature).Workspace;

            return ws;
        }

        private static IWorkspace WorkspaceFromSDEFile_IWorkspaceFactory2(string pPath)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory2 workspaceFactory2 = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);

            IWorkspace pWSpace = workspaceFactory2.OpenFromFile(pPath, 0);

            return pWSpace;
        }

        private static IFeatureWorkspace WorkspaceFromConnectionString(string pConnectionString, string pProvider = "sqlserver")
        {
            //Parse Connection String
            SqlConnectionStringBuilder cs = new SqlConnectionStringBuilder();
            List<string> sections = pConnectionString.Split(';').ToList();
            sections.RemoveAll(s => s.ContainsIgnoreCase("Provider"));
            cs.ConnectionString = String.Join(";", sections.ToArray());
            string instance = String.Format("sde:{0}:{1}", pProvider, cs.DataSource);

            if (cs.IntegratedSecurity)
                return GetWorkspaceFromConnectionOSAuth(cs.DataSource, instance, cs.InitialCatalog);

            if (!cs.IntegratedSecurity)
                return GetWorkspaceFromConnectionDBAuth(cs.DataSource, instance, cs.InitialCatalog, cs.UserID, cs.Password);

            return null;
        }

        private static IFeatureWorkspace GetWorkspaceFromConnectionOSAuth(string pServer, string pInstance, string pDatabase, string pClient = "sqlserver", string pConnProp = "Rev1.0", bool pIsGeodatabase = true)
        {
            IPropertySet propertySet = new PropertySetClass();

            propertySet.SetProperty("SERVER", pServer);
            propertySet.SetProperty("INSTANCE", pInstance);

            propertySet.SetProperty("DBCLIENT", pClient);
            propertySet.SetProperty("DB_CONNECTION_PROPERTIES", pServer);
            propertySet.SetProperty("IS_GEODATABASE", pIsGeodatabase);
            propertySet.SetProperty("CONNPROP-REV", pConnProp);

            propertySet.SetProperty("DATABASE", pDatabase);
            propertySet.SetProperty("VERSION", "sde.DEFAULT");

            //Operating System Authentication
            propertySet.SetProperty("AUTHENTICATION_MODE", "OSA");

            SdeWorkspaceFactory pWSFactory = new SdeWorkspaceFactory();
            IWorkspace pWSpace = pWSFactory.Open(propertySet, 0);

            return pWSpace as IFeatureWorkspace;
        }

        private static IFeatureWorkspace GetWorkspaceFromConnectionDBAuth(string pServer, string pInstance, string pDatabase, string pUserID, string pPassword, string pClient = "sqlserver", string pConnProp = "Rev1.0", bool pIsGeodatabase = true)
        {
            IPropertySet propertySet = new PropertySetClass();
            propertySet.SetProperty("SERVER", pServer);
            propertySet.SetProperty("INSTANCE", pInstance);

            propertySet.SetProperty("DBCLIENT", pClient);
            propertySet.SetProperty("DB_CONNECTION_PROPERTIES", pServer);
            propertySet.SetProperty("IS_GEODATABASE", pIsGeodatabase);
            propertySet.SetProperty("CONNPROP-REV", pConnProp);

            propertySet.SetProperty("DATABASE", pDatabase);
            propertySet.SetProperty("VERSION", "sde.DEFAULT");

            //Database Authentication
            propertySet.SetProperty("AUTHENTICATION_MODE", "DMBS");
            propertySet.SetProperty("USER", pUserID);
            propertySet.SetProperty("PASSWORD", pPassword);

            SdeWorkspaceFactory pWSFactory = new SdeWorkspaceFactory();
            IWorkspace pWSpace = pWSFactory.Open(propertySet, 0);

            return pWSpace as IFeatureWorkspace;
        }

        private static IWorkspace GetWorkspaceFromConnectionPropertySet(IPropertySet pPropertySet)
        {
            if (pPropertySet == null)
                throw new ArgumentNullException("pPropertySet");

            String connectionProperties = null;
            try
            {
                // for error reference - convert to string
                connectionProperties = pPropertySet.ToPropertiesString(true);

                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);

                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache = (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                return workspaceFactory.Open(pPropertySet, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(
                         String.Format("Failed to Open SDE Workspace from connection properties [{0}]", connectionProperties),
                         ex);
            }
        }

        private static IWorkspace GetWorkspaceSDEFromConnectionFile(string pPath, int pWindowHandle)
        {
            try
            {
                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache = (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                return workspaceFactory.OpenFromFile(pPath, pWindowHandle);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new Exception(
                         String.Format(
                                  "Failed to Open SDE Workspace from Connection File Path. Verify the SDE file [{0}] exists.",
                                 pPath), ex);
            }
        }

        private static IWorkspace GetWorkspaceSDEFromProjectConnectionFile(string pFileName, string pPath = "Data/SDEConnections/")
        {
            if (pFileName == null)
                throw new ArgumentNullException("pFileName");

            try
            {
                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);

                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache = (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                string fullPath = String.Concat(pPath, pFileName);
                return workspaceFactory.OpenFromFile(fullPath, 0);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new Exception(
                         String.Format(
                                  "Failed to Open SDE Workspace from [Project] Connection File. Verify the SDE file [{0}] is included in the project build output.",
                                 pFileName), ex);
            }
        }

        private static IFeatureClass GetFeatureClassFromWorkspace(string pFeatureClassName, IWorkspace pWorkspace)
        {
            IFeatureClass featureClass;

            if (pWorkspace is Sde4Workspace)
            {
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)pWorkspace;
                featureClass = featureWorkspace.OpenFeatureClass(pFeatureClassName);
            }
            else
            {
                throw new ArgumentNullException("workspace",
                         string.Format(
                                  "The workspace [{0}] is not an SDE database. Cannot retrieve the feature class [{1}]",
                                 pWorkspace.PathName, pFeatureClassName));
            }

            return featureClass;
        }

        #endregion
    }
}
