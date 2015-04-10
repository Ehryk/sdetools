using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Framework;

namespace sde2string
{
    static class GDBUtilities
    {
        #region Connection String Helpers

        public static string GetConnectionStringFromSDEFile(string path, bool bracketless)
        {
            return PropertySetToString(GetPropertySetFromSDEFile(path), bracketless);
        }

        public static IPropertySet GetPropertySetFromSDEFile(string path)
        {
            return GetWorkspaceFromSDEFile(path).ConnectionProperties;
        }

        public static IWorkspace GetWorkspaceFromSDEFile(string path)
        {
            return WorkspaceFromSDEFile(path);
        }

        public static IFeatureWorkspace GetFeatureWorkspaceFromSDEFile(string path)
        {
            return FeatureWorkspaceFromSDEFile(path);
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

        private static IFeatureWorkspace WorkspaceFromConnectionString(string pConnectionString)
        {
            //Parse Connection String
            SqlConnectionStringBuilder cs = new SqlConnectionStringBuilder();
            List<string> sections = pConnectionString.Split(';').ToList();
            sections.RemoveAll(s => s.ContainsIgnoreCase("Provider"));
            cs.ConnectionString = String.Join(";", sections.ToArray());
            string instance = String.Format("sde:sqlserver:{0}", cs.DataSource);

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
            IWorkspace pWSpace = pWSFactory.Open(propertySet, 0);                                                             // GET THE WORKSPACE

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
            IWorkspace pWSpace = pWSFactory.Open(propertySet, 0);                                                             // GET THE WORKSPACE

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
                connectionProperties = PropertySetToString(propertySet);

                var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                var workspaceFactory = (IWorkspaceFactory2)Activator.CreateInstance(factoryType);
                // Enable Schema Cache
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache =
                         (IWorkspaceFactorySchemaCache)workspaceFactory;
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
                IWorkspaceFactorySchemaCache workspaceFactorySchemaCache =
                         (IWorkspaceFactorySchemaCache)workspaceFactory;
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

        #region PropertySet Helpers

        public static string PropertySetToString(IPropertySet propertySet, bool bracketless = false)
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            Int32 propertyCount = propertySet.Count;

            object[] nameArray = new object[1];
            object[] valueArray = new object[1];
            propertySet.GetAllProperties(out nameArray[0], out valueArray[0]);
            object[] names = (object[])nameArray[0];
            object[] values = (object[])valueArray[0];

            // TODO - CONCAT 2 ARRAYS PROPERLY
            String connectionProperties = "";
            for (int i = 0; i < propertyCount; i++)
            {
                if (bracketless)
                    connectionProperties = String.Format("{0}{1}={2};", connectionProperties, names[i], values[i]);
                else
                    connectionProperties = String.Format("{0}[{1}]={2};", connectionProperties, names[i], values[i]);
                //string nameString = names[i].ToString();
                //string valueString = values[i].ToString();
            }
            return connectionProperties.Trim();
        }

        public static Dictionary<string, object> PropertySetToDictionary(IPropertySet propertySet)
        {
            if (propertySet == null)
                throw new ArgumentNullException("propertySet");

            Int32 propertyCount = propertySet.Count;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            object[] nameArray = new object[1];
            object[] valueArray = new object[1];
            propertySet.GetAllProperties(out nameArray[0], out valueArray[0]);
            object[] names = (object[])nameArray[0];
            object[] values = (object[])valueArray[0];

            for (int i = 0; i < propertyCount; i++)
            {
                dictionary.Add(names[i].ToString(), values[i]);
            }
            return dictionary;
        }

        public static IPropertySet PropertySetFromString(String propertySetText)
        {
            if (String.IsNullOrEmpty(propertySetText))
                throw new ArgumentNullException("propertySetText");

            IPropertySet propertySet = new PropertySetClass();

            String[] properties = propertySetText.Split(new[] { ';' });

            for (int i = 0; i < properties.Count(); i++)
            {
                if (String.IsNullOrEmpty(properties[i])) continue;

                String[] propertyParts = properties[i].Split(new[] { '=' });
                // Remove [ ]
                String name = propertyParts[0].Substring(1, propertyParts[0].Length - 2);
                String value = propertyParts[1];

                propertySet.SetProperty(name, value);
            }

            return propertySet;
        }

        #endregion

        #region License Helpers

        public static IAoInitialize CheckoutESRILicense(esriLicenseProductCode pESRIProdCode)
        {
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.Desktop);                                            // SET A REFERENCE TO THE ESRI DESKTOP APPLICATION

            var license = new AoInitializeClass();                                                                             // INSTANTIATE THE LICENSE                                                                          

            esriLicenseStatus pLicenseStatus = (esriLicenseStatus)license.IsProductCodeAvailable(pESRIProdCode);           // DETERMINE THE STATUS OF THE REQUESTED LICENSE
            if (pLicenseStatus == esriLicenseStatus.esriLicenseCheckedOut) { return license; }                                     // RETURN IF A LICENSE IS ALREADY CHECKED OUT

            if (pLicenseStatus == esriLicenseStatus.esriLicenseAvailable)                                                       // DETERMINE IF A LICENSE IS AVAILABLE
            {
                pLicenseStatus = (esriLicenseStatus)license.Initialize(pESRIProdCode);
                if (pLicenseStatus == esriLicenseStatus.esriLicenseCheckedOut || pLicenseStatus == esriLicenseStatus.esriLicenseAlreadyInitialized) 
                    return license; 
            }
            
            return null;
        }

        public static void ReturnESRILicense(IAoInitialize pESRILicense)
        {
            if (pESRILicense != null)
            {
                pESRILicense.Shutdown();                                                                // RELEASE THE LICENSE
                Marshal.ReleaseComObject(pESRILicense);
            }
        }

        #endregion
    }
}
