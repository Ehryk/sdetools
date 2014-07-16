using System;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace somenamespace
{
    class test
    {
        private void SDEWorkspaceFromPropertiesExample()
                 {
                          // Sample I have stored in a database text column - this can be produced by using 
                          // This string can be built by passing a propertyset (from an existing SDE connection file) to PropertySetToString() - note the password is encoded
                          string sdeProperties = @"[SERVER]=agdc-gis3.agdc-gis.local;[INSTANCE]=sde:sqlserver:agdc-gis3.agdc-gis.local;[DBCLIENT]=sqlserver;[DB_CONNECTION_PROPERTIES]=agdc-gis3.agdc-gis.local;[DATABASE]=Baker_SDE;[IS_GEODATABASE]=true;[AUTHENTICATION_MODE]=DBMS;[USER]=sa;[PASSWORD]=System.Byte[];[CONNPROP-REV]=Rev1.0;[VERSION]=sde.DEFAULT;";

                          IPropertySet propertySet = GDBUtilities.PropertySetFromString(sdeProperties);
                          IWorkspace workspace = GDBUtilities.GetWorkspaceSDEFromConnectionPropertySet(propertySet);

                          string featureClassName = "SomeFeatureClass"
                          IFeatureClass featureClass = GetFeatureClassFromWorkspace(featureClassName, workspace);
                 }

        public static IWorkspace GetWorkspaceSDEFromConnectionPropertySet(IPropertySet propertySet)
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

        /// <summary>
        /// Get SDE Workspace from provided path to an SDE Connection File
        /// </summary>
        /// <param name="connectionFilePath"></param>
        /// <param name="windowHandle"></param>
        /// <returns></returns>
        public static IWorkspace GetWorkspaceSDEFromConnectionFile(string connectionFilePath, int windowHandle)
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

        /// <summary>
        /// Get SDE Workspace from provided SDE Connection File name (contained in project)
        /// </summary>
        /// <param name="connectionFileName"></param>
        /// <returns></returns>
        public static IWorkspace GetWorkspaceSDEFromProjectConnectionFile(string connectionFileName)
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


        public static IFeatureClass GetFeatureClassFromWorkspace(string featureClassName, IWorkspace workspace)
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

        #region PropertySet

        public static String PropertySetToString(IPropertySet propertySet)
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
                connectionProperties = String.Format("{0}[{1}]={2};", connectionProperties, names[i], values[i]);
                //string nameString = names[i].ToString();
                //string valueString = values[i].ToString();
            }
            return connectionProperties.Trim();
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
    }
}
