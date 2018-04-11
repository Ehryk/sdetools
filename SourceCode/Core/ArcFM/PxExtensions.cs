using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using ADODB;
using ESRI.ArcGIS.esriSystem;
using Miner.Interop;
using Miner.Interop.msxml2;
using Miner.Interop.Process;
using Miner.Process.Messaging;

//using Miner.Interop.msxml4;

namespace Core.ArcFM
{    
    /// <summary>
    /// This class provides a series of extension methods to the ArcFM Process Framework and its extensions (Session Manager and Workflow Manager).
    /// </summary>
    public static class PxExtensions
    {
        /// <summary>
        /// Gets the session with the specified ID
        /// </summary>
        /// <param name="smExtension">Session Manager extension</param>
        /// <param name="sessionId">ID of the session</param>
        /// <param name="readOnly">Whether the returned session should be read only</param>
        /// <returns>Specified session</returns>
        public static IMMSession GetSession(this IMMSessionManager smExtension, int sessionId, bool readOnly)
        {
            var refSessionId = sessionId;
            var refReadOnly = readOnly;
            var smNode = smExtension.GetSession(ref refSessionId, ref refReadOnly);

            if (smNode == null)
                throw new Exception("Unable to load Session with id " + sessionId);

            return smNode;
        }

        /// <summary>
        /// Retrieves the design xml for the currently open design.
        /// </summary>
        /// <param name="bonusInformation">Whether geoassociativity objects should be included.  The resulting xml does NOT conform to the vendor XSD.</param>
        /// <param name="unsavedEdits">Whether the design contains unsaved edits</param>
        /// <returns>Xml document containing the design information</returns>
        public static XmlDocument GetOpenedDesignXml(bool bonusInformation, out bool unsavedEdits)
        {
            unsavedEdits = false;

            var managerClass = Type.GetTypeFromProgID("esriSystem.ExtensionManager");
            var extensionManager = Activator.CreateInstance(managerClass) as IExtensionManager;
            Debug.Assert(extensionManager != null, "ESRI Extension Manager");
            var designTree = extensionManager.FindExtension("DesignerTopLevel");
            if (designTree == null ||
                !((ID8List)designTree).HasChildren)
            {
                return null;
            }

            //Check to see if there are unsaved edits
            unsavedEdits = ((IMMPersistentListItem)designTree).GetDirty();

            var designerTopLevel = designTree as IMMPersistentXML2;
            Debug.Assert(designerTopLevel != null, "Design Tree");

            var oldXmlDoc = new DOMDocumentClass();
            IPropertySet exportProperties = null;
            if (bonusInformation)
            {
                //This is an undocumented hidden flag that will include attribute information for GIS features in the design.
                exportProperties = new PropertySetClass();
                exportProperties.SetProperty("SaveGeoAssoc", "True");
            }
            
            designerTopLevel.SaveToDOM(mmXMLFormat.mmXMLFDesign, oldXmlDoc, exportProperties);

            var newXmlDoc = new XmlDocument();
            newXmlDoc.LoadXml(oldXmlDoc.xml);
            return newXmlDoc;
        }

        /// <summary>
        /// Retrieves the px user with the given name and/or description
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="name">User name to query with</param>
        /// <param name="description">User description to query with</param>
        /// <returns>The first user that matches the given query</returns>
        public static IMMPxUser2 GetUser(this IMMPxApplication pxApplication, string name = null, string description = null)
        {
            var allUsers = pxApplication.Users;
            allUsers.Reset();

            for (var user = (IMMPxUser2)allUsers.Next();
                user != null;
                user = (IMMPxUser2)allUsers.Next())
            {
                if ((String.Compare(user.Name, name, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.IsNullOrEmpty(name))
                    &&
                    (String.Compare(user.Description, description, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.IsNullOrEmpty(description)))
                    return user;
            }

            return null;
        }
        

        /// <summary>
        /// Retrieves the session manager extension from the current process framework instance
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <returns>Session manager extension</returns>
        public static IMMSessionManager GetSessionManager(this IMMPxApplication pxApplication)
        {
            var extension = pxApplication.FindPxExtensionByName(ProcessFrameworkConstants.SessionManagerExtension) as IMMSessionManager;
            if (extension == null)
                throw new ArgumentException("Unable to load workflow manager extension from application.  Ensure it is configured and enabled.");

            return extension;
        }


        /// <summary>
        /// Creates and initializes the specified process framework node
        /// </summary>
        /// <param name="pxApplication">Process Framework application</param>
        /// <param name="id">ID of the node</param>
        /// <param name="type">Type of the node</param>
        /// <returns>Initialized process framework node</returns>
        public static IMMPxNode InitializePxNode(this IMMPxApplication pxApplication, int id, string type)
        {
            var nodeTypeId = pxApplication.Helper.GetNodeTypeID(type);
            var newNode = ProcessorHelper.GetPxNode(pxApplication, id, nodeTypeId, type);
            return newNode;
        }

        /// <summary>
        /// Retrieves the given task for the specified node that contains a transition matching the target maximo state
        /// </summary>
        /// <param name="currentNode">Process framework node</param>
        /// <param name="maximoState">Display name of the transition to execute</param>
        /// <returns></returns>
        public static IMMPxTask GetMaximoTransition(this IMMPxNode currentNode, string maximoState)
        {
            var allTasks = ((IMMPxNode4)currentNode).AllTasks;
            allTasks.Reset();
            for (var task = (IMMPxTask2)allTasks.Next();
                task != null;
                task = (IMMPxTask2)allTasks.Next())
            {
                var taskTransition = task.Transition;
                if (taskTransition == null)
                    continue;

                if (String.Compare(taskTransition.DisplayName, maximoState, StringComparison.OrdinalIgnoreCase) == 0)
                        return (IMMPxTask)task;
            }

            return null;
        }

        /// <summary>
        /// Generates a lookup dictionary of the work functions in the process framework database
        /// </summary>
        /// <param name="pxApp">Process framework application</param>
        /// <returns>Dictionary containing the names and codes of the px work functions</returns>
        public static Dictionary<string, string> GetPxWorkFunctions(this IMMPxApplication pxApp)
        {
            return GetDictionary(pxApp.Connection, string.Format("SELECT NAME, CODE FROM {0}MM_WMS_WORK_FUNCTION", pxApp.Login.SchemaName));
        }

        /// <summary>
        /// Generates a lookup dictionary of the units of measure in the process framework database
        /// </summary>
        /// <param name="pxApp">Process framework application</param>
        /// <returns>Dictionary containing the values and codes of the px units of measure</returns>
        public static Dictionary<string, string> GetPxUnitsOfMeasure(this IMMPxApplication pxApp)
        {
            return GetDictionary(pxApp.Connection, string.Format("SELECT VALUE, CODE  FROM {0}MM_WMS_UNITS_OF_MEASURE", pxApp.Login.SchemaName));
        }

        /// <summary>
        /// Generates a dictionary lookup using the given sql query.  The first column of the result set will be used as the key, and the second will be used as the value.  Duplicate records will not result in an error.
        /// </summary>
        /// <param name="connection">Process framework conection</param>
        /// <param name="sqlQuery">SQL query used to generate lookup</param>
        /// <returns>Dictionary containing a key value pair for each record returned from the query.</returns>
        private static Dictionary<string,string> GetDictionary(this _Connection connection, string sqlQuery)
        {
            var resultLookup = new Dictionary<string, string>();

            object affectedRecords;
            using (var queryResults = new DisposableRecordset(connection.Execute(sqlQuery, out affectedRecords)))
            {
                var recordSet = queryResults.RecordSet;
                if (recordSet.EOF) 
                    return resultLookup;

                for (recordSet.MoveNext();
                    !recordSet.EOF;
                    recordSet.MoveNext())
                {
                    var key = Convert.ToString(recordSet.Fields[0].Value);
                    var value = Convert.ToString(recordSet.Fields[1].Value);
                    resultLookup[key] = value;
                }
            }

            return resultLookup;
        }

        /// <summary>
        /// Gets the work request type that corresponds to the given code
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="workRequestTypeCode">Work request code</param>
        /// <returns>Work Request type id</returns>
        public static int GetWorkRequestType(this IMMPxApplication pxApplication, string workRequestTypeCode)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID FROM {1}MM_WMS_WR_TYPE WHERE CODE ='{0}'", workRequestTypeCode, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }
        
        /// <summary>
        /// Gets the approved design ID for the given work request
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="workRequestId">Work request ID</param>
        /// <returns>ID of the approved design for the given work request.  -1 if there are no approved designs</returns>
        public static int GetApprovedDesign(this IMMPxApplication pxApplication, int workRequestId)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT DESIGN_ID FROM {1}MM_WMS_APPROVED_DESIGNS WHERE WORK_REQUEST_ID = {0}", workRequestId, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }
        
        /// <summary>
        /// Gets the administrative area id for the specified area
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="scheduleArea">Administrative area to be used as a wildstar query</param>
        /// <returns>ID of the corresponding administrative area.  -1 if there is no matching area</returns>
        public static int GetAdminArea(this IMMPxApplication pxApplication, string scheduleArea)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID FROM {1}MM_WMS_ADMINISTRATIVE_AREA WHERE VALUE LIKE '{0}%'", scheduleArea, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }

        /// <summary>
        /// Gets the facilit type ID associated with the facility type
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="facilityType">String name for the facility type</param>
        /// <returns>ID of the facility type</returns>
        public static int GetFacilityType(this IMMPxApplication pxApplication, string facilityType)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID FROM {1}MM_WMS_GIS_FACILITY_TYPE WHERE VALUE LIKE '{0}'", facilityType, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }

        /// <summary>
        /// Retrieves the work request id associated with the first work request that matches the given name
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="workRequestName">String to be used as a wildstar query against work request names</param>
        /// <returns>Id of the corresponding work request.  -1 if there was no match</returns>
        public static int GetWorkRequestByName(this IMMPxApplication pxApplication, string workRequestName)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID FROM {1}MM_WMS_WORK_REQUEST WHERE NAME LIKE '{0}%'", workRequestName, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }

        /// <summary>
        /// Get the id and node type of the px node associated with the specified version.
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="versionName">Name of the database version</param>
        /// <param name="nodeId">ID of the process framework node</param>
        /// <param name="nodeTypeId">Node Type ID of the process framework node</param>        
        public static void GetPxNodeByVersionName(this IMMPxApplication pxApplication, string versionName, out int nodeId, out int nodeTypeId)
        {
            nodeId = -1;
            nodeTypeId = -1;

            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID, NODE_TYPE_ID FROM {1}MM_PX_VERSIONS WHERE NAME LIKE '{0}%'", versionName, pxApplication.Login.SchemaName);

            object affectedRecords;
            using (var queryResults = new DisposableRecordset(connection.Execute(sqlQuery, out affectedRecords)))
            {
                var record = queryResults.RecordSet;
                if (record.EOF)
                    return;

                record.MoveFirst();
                var dbVal = record.Fields[0].Value;
                if (dbVal != DBNull.Value &&
                    dbVal != null)
                    nodeId = Convert.ToInt32(dbVal);

                dbVal = record.Fields[1].Value;
                if (dbVal != DBNull.Value &&
                    dbVal != null)
                    nodeTypeId = Convert.ToInt32(dbVal);
            }
        }

        /// <summary>
        /// Retrieves the session id associated with the first session that matches the given name
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="sessionName">String to be used as a wildstar query against work request names</param>
        /// <returns>Id of the corresponding session.  -1 if there was no match</returns>
        public static int GetSessionByName(this IMMPxApplication pxApplication, string sessionName)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT SESSION_ID FROM {1}MM_SESSION WHERE SESSION_NAME LIKE '{0}%'", sessionName, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);
        }

        public static List<string> GetSessionWorkOrder(this IMMPxApplication pxApplication, int sessionId, string className, int oid)
        {
            string featureOidColumn;
            switch (className)
            {
                case POINT_OF_DELIVERY:
                    featureOidColumn = "PODOID";
                    break;
                case SERVICE_POINT:
                    featureOidColumn = "SERVICEPOINTOID";
                    break;
                case OH_SECONDARY_FEATURE:
                    featureOidColumn = "OHSECONDARYCONDUCTOROID";
                    break;
                case UG_SECONDARY_FEATURE:
                    featureOidColumn = "UGSECONDARYCONDUCTOROID";
                    break;
                case CONDUIT_SYSTEM:
                    featureOidColumn = "CONDUITSYSTEMOID";
                    break;
                case TRANSFORMER_UNIT:
                    featureOidColumn = "TRANSFORMERUNITOID";
                    break;
                case TRANSFORMER_BANK:
                    featureOidColumn = "TRANSFORMERBANKOID";
                    break;
                default:
                    featureOidColumn = string.Empty;
                    break;
            }
            if (featureOidColumn == string.Empty)
            {
                // If the class name is not a service point, secondary, conduit, or transformer feature then return an empty string list.
                return new List<string>();
            }
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT WORKORDER FROM {0}PGE_WORK_ORDER_MANAGER WHERE SESSIONID = {1} AND {2} = {3}", pxApplication.Login.SchemaName, sessionId, featureOidColumn, oid);
            return GetAllValues(connection, sqlQuery);
        }

        public static int GetSessionTypeId(this IMMPxApplication pxApplication, string sessionType)
        {
            // This method needs to prepare an sql query to retrieve session type id from the MM_SESSION_TYPE table.
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT ID FROM {0}MM_SESSION_TYPE WHERE SESSION_TYPE = '{1}'", pxApplication.Login.SchemaName, sessionType);
            return GetSingleValue(connection, sqlQuery, -1);
        }

        public static List<string> GetSessionNames(this IMMPxApplication pxApplication, string sessionName)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT SESSION_NAME FROM {1}MM_SESSION WHERE SESSION_NAME LIKE '{0}%'", sessionName, pxApplication.Login.SchemaName);
            return GetAllValues(connection, sqlQuery);
        }

        /// <summary>
        /// Retrieves the session id associated with the most recent session that matches the given name
        /// </summary>
        /// <param name="pxApplication">Process framework application</param>
        /// <param name="sessionName">String to be used as a wildstar query against work request names</param>
        /// <returns>Id of the corresponding session.  -1 if there was no match</returns>
        public static int GetSessionByNameMostRecent(this IMMPxApplication pxApplication, string sessionName)
        {
            var connection = pxApplication.Connection;
            var sqlQuery = string.Format("SELECT SESSION_ID FROM {1}MM_SESSION WHERE SESSION_NAME LIKE '{0}%' ORDER BY SESSION_ID DESC", sessionName, pxApplication.Login.SchemaName);
            return GetSingleValue(connection, sqlQuery, -1);   
        }

        /// <summary>
        /// Retrieves a single value from the first record in the given connection.  If there are no rows returned from the query, the default value will be returned
        /// </summary>
        /// <typeparam name="T">Type of the first column</typeparam>
        /// <param name="connection">Connection to use</param>
        /// <param name="sqlQuery">Query to retrieve the value</param>
        /// <param name="defaultValue">Value to return if no results are found</param>
        /// <returns>Value from the first column of the first record in the query</returns>
        private static T GetSingleValue<T>(this _Connection connection, string sqlQuery, T defaultValue)
        {
            object affectedRecords;
            using (var queryResults = new DisposableRecordset(connection.Execute(sqlQuery, out affectedRecords)))
            {
                var record = queryResults.RecordSet;
                if (record.EOF)
                    return defaultValue;

                record.MoveFirst();
                var dbVal = record.Fields[0].Value;

                return (T)Convert.ChangeType(dbVal, typeof(T));
            }
        }

        /// <summary>
        /// This is specifically to return all string values in a record set.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        private static List<string> GetAllValues(this _Connection connection, string sqlQuery)
        {
            object affectedRecords;
            using (var queryResults = new DisposableRecordset(connection.Execute(sqlQuery, out affectedRecords)))
            {
                Recordset record = queryResults.RecordSet;
                if (record.EOF)
                    return new List<string>();

                record.MoveFirst();

                List<string> dbValList = new List<string>();

                for (int i = 0; i < Convert.ToInt32(affectedRecords); i++)
                {
                    dbValList.Add((record.Fields[0].Value).ToString());
                    record.MoveNext();
                }

                return dbValList;
            }
        }

        /// <summary>
        /// Provides a recordset that will dispose and close
        /// </summary>
        internal class DisposableRecordset:IDisposable
        {
            private readonly Recordset _recordSet;
            internal DisposableRecordset(Recordset recordSet)
            {
                _recordSet = recordSet;
            }

            public void Dispose()
            {
                if (_recordSet != null)
                    _recordSet.Close();
            }

            public Recordset RecordSet
            { get { return _recordSet; } }
        }
    }
}
