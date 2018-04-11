using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Miner.Interop;

namespace PGE.ArcFM.Common
{
    /// <summary>
    /// This extensions class providess a quick way to convert from ESRI and Miner objects to their enumerable variants
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Provides a row enumeration against the given object in the geodatabase using the given non-spatial and spatial parameters
        /// </summary>
        /// <param name="table">Table to query</param>
        /// <param name="filter">SQL component to query with </param>
        /// <param name="shape">Spatial component to query with</param>
        /// <param name="spatialRelationship">ESRI spatial relationship to be used with the spatial component</param>
        /// <param name="recyling">Whether the cursor should be recycling.  While this reduces the memory footprint it can result in odd behavior.  Use at your own risk</param>
        /// <param name="subFields">Which fields to return in the cursor</param>
        /// <returns>An enumerable selection cursor</returns>
        public static EnumerableCursor Where(this ITable table, string filter,
            IGeometry shape = null, esriSpatialRelEnum spatialRelationship = esriSpatialRelEnum.esriSpatialRelIntersects,
            bool recyling = true, string subFields = "*")
        {
            var queryFilter =
                shape == null ?
                (IQueryFilter)(string.IsNullOrEmpty(filter) ? null :
                    new QueryFilterClass { WhereClause = filter, SubFields = subFields }) :
                new SpatialFilterClass { WhereClause = filter, Geometry = shape, SpatialRel = spatialRelationship, SubFields = subFields };

            return table.Search(queryFilter, recyling).ToEnumerable();
        }

        public static EnumerableCursor WhereEditable(this ITable table, string filter)
        {
            return table.Where(filter, null, esriSpatialRelEnum.esriSpatialRelIntersects, false);
        }

        /// <summary>
        /// Provides a row enumeration against the given object in the geodatabase using the given non-spatial and spatial parameters
        /// </summary>
        /// <param name="objectClass">Table to query</param>
        /// <param name="filter">SQL component to query with </param>
        /// <param name="shape">Spatial component to query with</param>
        /// <param name="spatialRelationship">ESRI spatial relationship to be used with the spatial component</param>
        /// <param name="recyling">Whether the cursor should be recycling.  While this reduces the memory footprint it can result in odd behavior.  Use at your own risk</param>
        /// <returns>An enumerable selection cursor</returns>
        public static EnumerableCursor Where(this IObjectClass objectClass, string filter, IGeometry shape = null, esriSpatialRelEnum spatialRelationship = esriSpatialRelEnum.esriSpatialRelIntersects, bool recyling = true)
        {
            return ((ITable) objectClass).Where(filter, shape, spatialRelationship, recyling);
        }

        public static EnumerableCursor Where(this IObjectClass objectClass, string filter, string subFields = "*", bool recyling = true)
        {
            return ((ITable)objectClass).Where(filter, null, esriSpatialRelEnum.esriSpatialRelIntersects, recyling, subFields);
        }
        public static EnumerableCursor WhereEditable(this IObjectClass objectClass, string filter, IGeometry shape = null, esriSpatialRelEnum spatialRelationship = esriSpatialRelEnum.esriSpatialRelIntersects)
        {
            return ((ITable)objectClass).Where(filter, shape, spatialRelationship, false);
        }

        /// <summary>
        /// Provides an enumerable version of the selection cursor
        /// </summary>
        /// <param name="cursor">Selection Cursor</param>
        /// <returns>Enumerable set of rows</returns>
        public static EnumerableCursor ToEnumerable(this ICursor cursor)
        {
            return new EnumerableCursor(cursor);
        }

        /// <summary>
        /// Provides an enumerable version of the difference cursor
        /// </summary>
        /// <param name="differenceCursor">Selection Cursor</param>
        /// <returns>Enumerable set of rows</returns>
        public static EnumerableDifferenceCursor ToEnumerable(this IDifferenceCursor differenceCursor)
        {
            return new EnumerableDifferenceCursor(differenceCursor);
        }

        /// <summary>
        /// Provides a field enumeration
        /// </summary>
        /// <param name="mmFields">Field list</param>
        /// <returns>Enumerable set of fields</returns>
        public static EnumerableMmField ToEnumerable(this IMMEnumField mmFields)
        {
            return new EnumerableMmField(mmFields);
        }
        
        /// <summary>
        /// Provides a string enumeration (intended for field names)
        /// </summary>
        /// <param name="mmFields">List of string</param>
        /// <returns>Enumerable set of strings</returns>
        public static EnumerableMmString ToEnumerable(this IEnumBSTR mmFields)
        {
            return new EnumerableMmString(mmFields);
        }

        /// <summary>
        /// Provides a object class enumeration
        /// </summary>
        /// <param name="mmClasses">Object class list</param>
        /// <returns>Enumerable set of object classes</returns>
        public static EnumerableMmObjectClass ToEnumerable(this IMMEnumObjectClass mmClasses)
        {
            return new EnumerableMmObjectClass(mmClasses);
        }

        /// <summary>
        /// Provides a relationship enumeration
        /// </summary>
        /// <param name="relationshpis">Relationship list</param>
        /// <returns>Enumerable set of relationships</returns>
        public static EnumerableRelationship ToEnumerable(this IEnumRelationship relationshpis)
        {
            return new EnumerableRelationship(relationshpis);
        }

        /// <summary>
        /// Provides a relationship class enumeration
        /// </summary>
        /// <param name="relationshipClasses">Relationship class list</param>
        /// <returns>Enumerable set of relationship classes</returns>
        public static EnumerableRelationshipClass ToEnumerable(this IEnumRelationshipClass relationshipClasses)
        {
            return new EnumerableRelationshipClass(relationshipClasses);
        }

        /// <summary>
        /// Provides an object enumeration (intended for use with object sets)
        /// </summary>
        /// <param name="set">Set object</param>
        /// <returns>Enumerable set of objects</returns>
        public static EnumerableSet ToEnumerable(this ISet set)
        {
            return new EnumerableSet(set);
        }

        /// <summary>
        /// Provides a feature class enumeration
        /// </summary>
        /// <param name="featureClasses">Feature class list</param>
        /// <returns>Enumerable set of feature classes</returns>
        public static EnumerableFeatureClasses ToEnumerable(this IEnumFeatureClass featureClasses)
        {
            return new EnumerableFeatureClasses(featureClasses);
        }

        /// <summary>
        /// Provides a featre enumeration
        /// </summary>
        /// <param name="features">feature list</param>
        /// <returns>Enumerable set of feature</returns>
        public static EnumerableFeatures ToEnumerable(this IEnumFeature features)
        {
            return new EnumerableFeatures(features);
        }

        /// <summary>
        /// Provides a layer enumeration
        /// </summary>
        /// <param name="layers">layer list</param>
        /// <returns>Enumerable set of layers</returns>
        public static EnumerableLayers ToEnumerable(this IEnumLayer layers)
        {
            return new EnumerableLayers(layers);
        }

        private const string FeatureLayerClassId = "{40A9E885-5533-11D0-98BE-00805F7CED21}";

        public static IEnumLayer GetFeatureLayers(this IMap currentMap, string layerTypeId = FeatureLayerClassId)
        {
            var filterUid = new UIDClass { Value = layerTypeId };
            return currentMap.Layers[filterUid];
        }

        public static IEnumerable<IStandaloneTable> GetStandaloneTables(this IMap currentMap)
        {
            var standaloneTableCollection = (IStandaloneTableCollection) currentMap;
            int tableCount = standaloneTableCollection.StandaloneTableCount;
            for (var i = 0; i < tableCount; i++)
                yield return standaloneTableCollection.StandaloneTable[i];
        }

        public static IDictionary<int, ICollection<IFeatureLayer>> GetSelectedFeatureLayers(this IMap currentMap, ICollection<int> objectClassIds)
        {
            var layerLookup = new Dictionary<int, ICollection<IFeatureLayer>>(objectClassIds.Count);

            var allFeatureLayers = currentMap.GetFeatureLayers();
            allFeatureLayers.Reset();

            for (var layer = allFeatureLayers.Next();
                layer != null;
                layer = allFeatureLayers.Next())
            {
                if (!layer.Valid)
                    continue;

                var featureLayer = layer as IFeatureLayer;
                if (featureLayer == null)
                    continue;

                var featureClass = featureLayer.FeatureClass;
                if (featureClass == null)
                    continue;

                var objectClassId = featureClass.ObjectClassID;
                if (objectClassIds.Contains(objectClassId))
                {
                    ICollection<IFeatureLayer> featureLayers;
                    if (!layerLookup.TryGetValue(objectClassId, out featureLayers))
                        layerLookup[objectClassId] = new List<IFeatureLayer> { featureLayer };
                    else
                        featureLayers.Add(featureLayer);
                }

            }

            return layerLookup;
        }

        public static IDictionary<string, IFeatureLayer> GetFeatureLayers(this IMap currentMap, ICollection<string> featureLayerNameCollection)
        {
            return currentMap.GetFeatureLayers().ToEnumerable().Cast<IFeatureLayer>()
                .Where(layer => 
                    featureLayerNameCollection.FirstOrDefault(layerName => layer.Name.ToUpper().Contains(layerName)) != null &&
                    layer.FeatureClass != null)
                .ToDictionary(layer => featureLayerNameCollection.FirstOrDefault(layerName => layer.Name.ToUpper().Contains(layerName)));
        }

        public static IDictionary<string, IFeatureLayer> GetFeatureLayersByTableName(this IMap currentMap, ICollection<string> tableNameCollection)
        {
            return currentMap.GetFeatureLayers().ToEnumerable().Cast<IFeatureLayer>()
                .Where(layer =>
                    tableNameCollection.FirstOrDefault(tableName => ((IDataset)layer.FeatureClass).BrowseName.ToUpper().Contains(tableName)) != null &&
                    layer.FeatureClass != null)
                .ToDictionary(layer => tableNameCollection.FirstOrDefault(tableName => ((IDataset)layer.FeatureClass).BrowseName.ToUpper().Contains(tableName)));
        }

        public static IDictionary<string, IStandaloneTable> GetStandaloneTables(this IMap currentMap, ICollection<string> tableNameCollection)
        {
            return currentMap.GetStandaloneTables()
                .Where(table =>
                    tableNameCollection.FirstOrDefault(tableName => table.Name.ToUpper().Contains(tableName)) != null &&
                    table.Table != null)
                .ToDictionary(table => tableNameCollection.FirstOrDefault(tableName => table.Name.ToUpper().Contains(tableName)));
        }
    }
}
