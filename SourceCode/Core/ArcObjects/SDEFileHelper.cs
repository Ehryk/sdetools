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

        public static string GetConnectionStringFromSDEFile(string path, bool bracketless)
        {
            return GetPropertySetFromSDEFile(path).ToPropertiesString(bracketless);
        }

        public static IPropertySet GetPropertySetFromSDEFile(string path)
        {
            return GetWorkspaceFromSDEFile(path).ConnectionProperties;
        }

        public static Dictionary<string, string> GetDictionaryFromSDEFile(string path)
        {
            return GetWorkspaceFromSDEFile(path).ConnectionProperties.ToDictionary();
        }

        public static IWorkspace GetWorkspaceFromSDEFile(string path)
        {
            return WorkspaceFromSDEFile(path);
        }

        public static IFeatureWorkspace GetFeatureWorkspaceFromSDEFile(string path)
        {
            return FeatureWorkspaceFromSDEFile(path);
        }

        public static Dictionary<string, string> DirectParseSDEFile(string path, Encoding encoding = null)
        {
            string contents;

            if (encoding == null)
                contents = File.ReadAllText(path);
            else
                contents = encoding.GetString(File.ReadAllBytes(path));

            return GetSDEDictionary(contents);
        }

        public static Dictionary<string, string> GetSDEDictionary(string contents)
        {
            return new Dictionary<string, string>();
        }

        #endregion

        #region Workspace Helpers

        private static IFeatureWorkspace FeatureWorkspaceFromSDEFile(string path)
        {
            return WorkspaceFromSDEFile(path) as IFeatureWorkspace;
        }

        private static IWorkspace WorkspaceFromSDEFile(string path)
        {
            SdeWorkspaceFactory pWSFactory = new SdeWorkspaceFactory();
            IWorkspace pWSpace = pWSFactory.OpenFromFile(path, 0);

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

        private static IWorkspace GetWorkspaceFromConnectionPropertySet(IPropertySet propertySet)
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            String connectionProperties = null;
            try
            {
                // for error reference - convert to string
                connectionProperties = propertySet.ToPropertiesString(true);

                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);

                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache = (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                return workspaceFactory.Open(propertySet, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(
                         String.Format("Failed to Open SDE Workspace from connection properties [{0}]", connectionProperties),
                         ex);
            }
        }

        private static IWorkspace GetWorkspaceSDEFromConnectionFile(string connectionFilePath, int windowHandle)
        {
            try
            {
                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache =
                         (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                return workspaceFactory.OpenFromFile(connectionFilePath, windowHandle);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new Exception(
                         String.Format(
                                  "Failed to Open SDE Workspace from Connection File Path. Verify the SDE file [{0}] exists.",
                                 connectionFilePath), ex);
            }
        }

        private static IWorkspace GetWorkspaceSDEFromProjectConnectionFile(string connectionFileName)
        {
            if (connectionFileName == null)
                throw new ArgumentNullException("connectionFileName");

            try
            {
                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);

                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache = (IWorkspaceFactorySchemaCache)workspaceFactory;
                workspaceFactorySchemaCache.EnableSchemaCaching();

                string connectionFilePath = string.Format("Data/SDEConnections/{0}", connectionFileName);
                return workspaceFactory.OpenFromFile(connectionFilePath, 0);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw new Exception(
                         String.Format(
                                  "Failed to Open SDE Workspace from [Project] Connection File. Verify the SDE file [{0}] is included in the project build output.",
                                 connectionFileName), ex);
            }
        }

        private static IFeatureClass GetFeatureClassFromWorkspace(string featureClassName, IWorkspace workspace)
        {
            IFeatureClass featureClass;

            if (workspace is Sde4Workspace)
            {
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                featureClass = featureWorkspace.OpenFeatureClass(featureClassName);
            }
            else
            {
                throw new ArgumentNullException("workspace",
                         string.Format(
                                  "The workspace [{0}] is not an SDE database. Cannot retrieve the feature class [{1}]",
                                 workspace.PathName, featureClassName));
            }

            return featureClass;
        }

        #endregion
    }
}
