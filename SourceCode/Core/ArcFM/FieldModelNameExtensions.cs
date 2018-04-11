using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using Miner.Interop;

namespace Core.ArcFM
{
    public static class FieldModelNameExtensions
    {
        #region Private Properties

        private static readonly log4net.ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IMMModelNameManager _modelNameManager = null;

        private static IMMModelNameManager ModelNameManager
        {
            get { return _modelNameManager ?? Miner.Geodatabase.ModelNameManager.Instance; }
        }

        #endregion

        #region Query Field Model Names

        public static bool HasFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            return modelNameManager.ContainsFieldModelName(pClass, pField, pModelName);
        }

        public static List<string> ListFieldModelNames(this IObjectClass pClass, IField pField)
        {
            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.CanReadModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to read Model Names on {0}", pClass.AliasName));

            IEnumBSTR modelNamesEnum = modelNameManager.FieldModelNames(pClass, pField);

            return modelNamesEnum.ToList();
        }

        #endregion

        #region Manage Field Model Names

        public static bool AddFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            if (pClass is null)
                throw new ArgumentException("pClass");
            if (pField is null)
                throw new ArgumentException("pField");

            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (modelNameManager.ContainsFieldModelName(pClass, pField, pModelName))
                return false;

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.AddFieldModelName(pClass, pField, pModelName);

            return modelNameManager.ContainsFieldModelName(pClass, pField, pModelName);
        }

        public static bool RemoveFieldModelName(this IObjectClass pClass, IField pField, string pModelName)
        {
            if (pClass is null)
                throw new ArgumentException("pClass");
            if (pField is null)
                throw new ArgumentException("pField");

            IMMModelNameManager modelNameManager = ModelNameManager.Instance;
            if (modelNameManager is null)
                throw new Exception("Cannot instantiate Model Name Manager");

            if (!modelNameManager.ContainsFieldModelName(pClass, pField, pModelName))
                return false;

            if (!modelNameManager.CanWriteModelNames(pClass))
                throw new Exception(String.Format("Insufficient Permissions to write Model Names on {0}", pClass.AliasName));

            modelNameManager.RemoveFieldModelName(pClass, pField, pModelName);

            return !modelNameManager.ContainsFieldModelName(pClass, pField, pModelName);
        }

        #endregion

        #region Workspace extensions

        public static IField GetField(this IWorkspace workspace, IObjectClass classObject, string fieldModelName, bool raiseError = false)
        {
            var field = ModelNameManager.FieldFromModelName(classObject, fieldModelName);

            if (field == null && raiseError)
            {
                throw new Exception(String.Format("Unable to find required field mode name {0} on the following class: {1}", fieldModelName, classObject.AliasName));
            }

            return field;
        }

        #endregion

        #region Class Extensions

        public static IRelationshipClass RelationshipFromRelatedClassModelName(this IObjectClass objectClass, string modelNameDestination)
        {
            IEnumRelationshipClass relClasses = objectClass.get_RelationshipClasses(esriRelRole.esriRelRoleAny);
            relClasses.Reset();

            IRelationshipClass relClass = relClasses.Next();
            while (relClass != null)
            {
                if (ModelNameManager.ContainsClassModelName(relClass.DestinationClass, modelNameDestination))
                {
                    break;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(relClass);
                relClass = relClasses.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(relClasses);

            if (relClass == null)
            {
                throw new NullReferenceException(String.Format("Model Name '{0}' was not assigned to an origin/destination class beloging to a relationship related to Object Class {1}", modelNameDestination, objectClass.AliasName));
            }

            return relClass;
        }

        /// <summary>
        /// All the fields on the object class with the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModelName">Field model name</param>
        /// <returns>All the fields on the object class with the given model name</returns>
        [Obsolete("It is much more efficient to use Get Field Names and cache indices when iterating multiple rows.  Only use this method if it will not be performed in any sort of loop", false)]
        public static EnumerableMmField GetFields(this IObjectClass objectClass, string fieldModelName)
        {
            return ModelNameManager.FieldsFromModelName(objectClass, fieldModelName).ToEnumerable();
        }

        /// <summary>
        /// All the fields on the object class with the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModelName">Field model name</param>
        /// <returns>All the fields on the object class with the given model name</returns>
        public static IMMEnumField GetFieldsForModel(this IObjectClass objectClass, string fieldModelName)
        {
            return ModelNameManager.FieldsFromModelName(objectClass, fieldModelName);
        }

        /// <summary>
        /// Gets the field on the object class with the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModelName">Field model name</param>
        /// <param name="raiseError">Whether or not to throw an exception if no matching field is found</param>
        /// <returns>Field with the given model name</returns>
        [Obsolete("It is much more efficient to use Get Field Name and cache indices when iterating multiple rows.  Only use this method if it will not be performed in any sort of loop", false)]
        public static IField GetField(this IObjectClass objectClass, string fieldModelName, bool raiseError = false)
        {
            var results = objectClass.GetFields(fieldModelName);
            var firstResult = results.FirstOrDefault();

            if (firstResult == null && raiseError)
                throw new Exception(String.Format("Unable to find required field model name {0} on class {1}", fieldModelName, ((IDataset)objectClass).Name));
            
            return firstResult;
        }
        
        /// <summary>
        /// All the field names on the object class with the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModelName">Field model name</param>
        /// <returns>All the field names on the object class with the given model name</returns>
        public static EnumerableMmString GetFieldNames(this IObjectClass objectClass, string fieldModelName)
        {
            return ModelNameManager.FieldNamesFromModelName(objectClass, fieldModelName).ToEnumerable();
        }

        /// <summary>
        /// Gets the field on the object class with the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModelName">Field model name</param>
        /// <param name="raiseError">Whether or not to throw an exception if no matching field is found</param>
        /// <returns>Field name with the given model name</returns>
        public static string GetFieldName(this IObjectClass objectClass, string fieldModelName, bool raiseError = false)
        {
            var results = objectClass.GetFieldNames(fieldModelName);
            var firstResult = results.FirstOrDefault();

            if (firstResult == null && raiseError)
                throw new Exception(String.Format("Unable to find required field model name {0} on class {1}", fieldModelName, ((IDataset)objectClass).Name));

            return firstResult;
        }

        /// <summary>
        /// Gets a field index lookup on the class for the given set of field model names.
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="modelNames">Field model names</param>
        /// <param name="raiseError">Whether an error should be raised if one of the model names isn't present</param>
        /// <returns>Dictionary mapping field model names to field indices</returns>
        public static IDictionary<string, int> GetFieldIndexLookup(this IObjectClass objectClass, IEnumerable<string> modelNames, bool raiseError = false)
        {
            var fieldIndexLookup = new Dictionary<string, int>();

            foreach (var fieldModel in modelNames)
            {
                bool hasName = false;

                foreach (var fieldName in objectClass.GetFieldNames(fieldModel))
                {
                    if (hasName)
                    {
                        string message = String.Format("Field model name {0} is applied to multiple fields on class {1} when it should only be applied to a single field.", fieldModel, ((IDataset)objectClass).Name);
                        
                        if (raiseError)
                            throw new Exception(message);

                        Debug.WriteLine(message);
                    }

                    hasName = true;
                    var fieldIndex = fieldIndexLookup[fieldModel] = objectClass.FindField(fieldName);

                    if (fieldIndex == -1 && raiseError)
                        throw new Exception(String.Format("Unable to find required field model name {0} on class {1}.  The field configured for the model name is no longer in the data model.", fieldModel, ((IDataset)objectClass).Name));
                }

                if (raiseError && !hasName)
                    throw new Exception(string.Format("Unable to find required field model name {0} on class {1}", fieldModel, ((IDataset)objectClass).Name));
            }

            return fieldIndexLookup;
        }

        /// <summary>
        /// Gets a field index lookup on the class for the given set of field model names.
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="modelNames">Field model names</param>
        /// <param name="fieldNameLookup">Dictionary lookup tieing model names to field names</param>
        /// <param name="raiseError">Whether an error should be raised if one of the model names isn't present</param>
        /// <returns>Dictionary mapping field model names to field indices</returns>
        public static IDictionary<string, int> GetFieldIndexLookup(this IObjectClass objectClass, IEnumerable<string> modelNames, out IDictionary<string, string> fieldNameLookup, bool raiseError=false)
        {
            var fieldIndexLookup = new Dictionary<string, int>();
            fieldNameLookup = new Dictionary<string, string>();

            foreach (var fieldModel in modelNames)
            {
                bool hasName = false;
                foreach (var fieldName in objectClass.GetFieldNames(fieldModel))
                {
                    if (hasName)
                    {
                        string message = String.Format("Field model name {0} is applied to multiple fields on class {1} when it should only be applied to a single field.", fieldModel, ((IDataset)objectClass).Name);

                        if (raiseError)
                            throw new Exception(message);

                        Debug.WriteLine(message);
                    }

                    hasName = true;

                    fieldNameLookup[fieldModel] = fieldName;
                    fieldIndexLookup[fieldModel] = objectClass.FindField(fieldName);
                }

                if (raiseError && !hasName)
                    throw new Exception(String.Format("Unable to find required model name {0} on class {1}", fieldModel, ((IDataset)objectClass).Name));
            }

            return fieldIndexLookup;
        }

        /// <summary>
        /// This method should only be used when a field model name is assigned to multiple fields on a single class.
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModel">Field model name</param>
        /// <param name="raiseError">Whether an error should be raised if one of the model names isn't present</param>
        /// <returns>List of field indicies where the field model name is applied</returns>
        public static IEnumerable<int> GetFieldIndicesForModelName(this IObjectClass objectClass, string fieldModel, bool raiseError = false)
        {
            bool hasName = false;

            foreach (var fieldName in objectClass.GetFieldNames(fieldModel))
            {
                hasName = true;
                var fieldIndex = objectClass.FindField(fieldName);

                if (fieldIndex == -1 && raiseError)
                    throw new Exception(
                        String.Format(
                            "Unable to find required field model name {0} on class {1}.  The field configured for the model name is no longer in the data model.",
                            fieldModel, ((IDataset) objectClass).Name));

                yield return fieldIndex;
            }

            if (raiseError && !hasName)
                throw new Exception(String.Format("Unable to find required field model name {0} on class {1}", fieldModel, ((IDataset) objectClass).Name));
        }
        /// <summary>
        /// This method should only be used when a field model name is assigned to multiple fields on a single class.
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="fieldModel">Field model name</param>
        /// <param name="raiseError">Whether an error should be raised if one of the model names isn't present</param>
        /// <returns>List of field indicies where the field model name is applied</returns>
        public static IEnumerable<KeyValuePair<string,int>> GetFieldIndexLookupForModelName(this IObjectClass objectClass, string fieldModel, bool raiseError = false)
        {
            bool hasName = false;

            foreach (var fieldName in objectClass.GetFieldNames(fieldModel))
            {
                hasName = true;
                var fieldIndex = objectClass.FindField(fieldName);
                if (fieldIndex == -1 && raiseError)
                    throw new Exception(
                        String.Format(
                            "Unable to find required field model name {0} on class {1}.  The field configured for the model name is no longer in the data model.",
                            fieldModel, ((IDataset)objectClass).Name));

                yield return new KeyValuePair<string, int>(fieldName, fieldIndex);
            }

            if (raiseError && !hasName)
                throw new Exception(String.Format("Unable to find required field model name {0} on class {1}", fieldModel, ((IDataset)objectClass).Name));
        }

        /// <summary>
        /// Applies the given values in the dictionary to the object
        /// </summary>
        /// <param name="gdbObject">ESRI row</param>
        /// <param name="childValues">Dictionary of field indices and field values</param>
        public static void UpdateObject(this IRow gdbObject, IDictionary<int, object> childValues)
        {
            foreach (var kvp in childValues)
                gdbObject.Value[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Retrieves the set of relationship classes involved with the given class where the other class contains the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="modelName">Class model name on the other class</param>
        /// <param name="sourceRole">Relationship role of the given class</param>
        /// <returns>Enumerable set of relationship classes that match the given criteria</returns>
        public static IEnumerable<IRelationshipClass> GetRelationshipClasses(this IObjectClass objectClass, string modelName, esriRelRole sourceRole)
        {
            var relationshipClasses = objectClass.RelationshipClasses[sourceRole].ToEnumerable();
            int thisObjectClassId = objectClass.ObjectClassID;

            return relationshipClasses.Where(relationshipClass =>
                {
                    switch (sourceRole)
                    {
                        case esriRelRole.esriRelRoleAny:
                            if ((relationshipClass.OriginClass.ObjectClassID == thisObjectClassId &&
                                ModelNameManager.ContainsClassModelName(relationshipClass.DestinationClass, modelName)) ||
                                (relationshipClass.DestinationClass.ObjectClassID == thisObjectClassId &&
                                ModelNameManager.ContainsClassModelName(relationshipClass.OriginClass, modelName)))
                                return true;
                            break;
                        case esriRelRole.esriRelRoleDestination:
                            if (ModelNameManager.ContainsClassModelName(relationshipClass.OriginClass, modelName))
                                return true;
                            break;
                        case esriRelRole.esriRelRoleOrigin:
                            if (ModelNameManager.ContainsClassModelName(relationshipClass.DestinationClass, modelName))
                                return true;
                            break;
                    }

                    return false;
                });
        }

        /// <summary>
        /// Retrieves the relationship class involved with the given class where the other class contains the given model name
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="modelName">Class model name on the other class</param>
        /// <param name="sourceRole">Relationship role of the given class</param>
        /// <param name="relatedClass">Object class on the other end of the relationship</param>
        /// <param name="raiseError">Whether an error should be raised if no class is found</param>
        /// <returns>Relationship class that match the given criteria</returns>
        public static IRelationshipClass GetRelationshipClass(this IObjectClass objectClass, string modelName, esriRelRole sourceRole, out IObjectClass relatedClass, bool raiseError = false)
        {
            var results = objectClass.GetRelationshipClasses(modelName, sourceRole);
            var firstResult = results.FirstOrDefault();
            if (firstResult == null)
            {
                if (raiseError)
                    throw new Exception(string.Format("Unable to find required class related to {1} with class model name {0}", modelName, ((IDataset)objectClass).Name));

                relatedClass = null;
                return null;
            }

            int thisObjectClassId = objectClass.ObjectClassID;
            relatedClass = firstResult.DestinationClass.ObjectClassID == thisObjectClassId ? firstResult.OriginClass : firstResult.DestinationClass;

            return firstResult;
        }

        #endregion

        #region Field Extensions

        /// <summary>
        /// Checks to see whether the given field has the specified model name assigned to it.
        /// </summary>
        /// <param name="objectClass">ArcFM object class</param>
        /// <param name="field">Field to be checked</param>
        /// <param name="fieldModelName">Field model name to look for</param>
        /// <returns>True if the field model name is assigned, false otherwise</returns>
        public static bool ContainsModelName(this IObjectClass objectClass, IField field, string fieldModelName)
        {
            return ModelNameManager.ContainsFieldModelName(objectClass, field, fieldModelName);
        }

        #endregion
    }
}
