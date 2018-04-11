using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using Miner.Interop;

namespace PGE.ArcFM.Common
{
    /// <summary>
    /// Provides an enumerable class for ESRI Select cursors
    /// </summary>
    public class EnumerableCursor : IEnumerator<IRow>, IEnumerable<IRow>
    {
        private ICursor _enumerable;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Input select cursor</param>
        public EnumerableCursor(ICursor enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        /// <summary>
        /// The current row in the enumerator
        /// </summary>
        public IRow Current { get; private set; }

        /// <summary>
        /// Advances the cursor one object
        /// </summary>
        /// <returns>True if the cursor could advance, false if the enumeration is complete</returns>
        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            Current = null;
            Current = _enumerable.NextRow();

            return Current != null;
        }

        /// <summary>
        /// Not implemented.  Cursors cannot be reset or re-evaluated.
        /// </summary>
        [Obsolete("Cursors cannot be reset or re-evaluated.", true)]
        public void Reset()
        {
            throw new NotImplementedException("Forward-only cursors do not support resetting");
        }

        /// <summary>
        /// Resets the enumeration and disposes the underlying cursor
        /// </summary>
        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IRow> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for ESRI Difference cursors
    /// </summary>
    public class EnumerableDifferenceCursor : IEnumerator<IRow>, IEnumerable<IRow>
    {
        private IDifferenceCursor _enumerable;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Input select cursor</param>
        public EnumerableDifferenceCursor(IDifferenceCursor enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        /// <summary>
        /// The current row in the enumerator
        /// </summary>
        public IRow Current { get; private set; }

        //The current ObjectID of the enumerator.  This is used when iterating a delete cursor.
        public int CurrentObjectId { get; private set; }

        /// <summary>
        /// Advances the cursor one object
        /// </summary>
        /// <returns>True if the cursor could advance, false if the enumeration is complete</returns>
        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            int objectId;
            IRow differenceRow;
            _enumerable.Next(out objectId, out differenceRow);
            Current = differenceRow;
            CurrentObjectId = objectId;
            return Current != null || CurrentObjectId > 0;
        }

        /// <summary>
        /// Not implemented.  Cursors cannot be reset or re-evaluated.
        /// </summary>
        [Obsolete("Cursors cannot be reset or re-evaluated.", true)]
        public void Reset()
        {
            throw new NotImplementedException("Forward-only cursors do not support resetting");
        }

        /// <summary>
        /// Resets the enumeration and disposes the underlying cursor
        /// </summary>
        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IRow> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for Miner object lists
    /// </summary>
    public class EnumerableMmObjectClass : IEnumerator<IObjectClass>, IEnumerable<IObjectClass>
    {
        private IMMEnumObjectClass _enumerable;
        public EnumerableMmObjectClass(IMMEnumObjectClass enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public IObjectClass Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IObjectClass> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for Miner string lists
    /// </summary>
    public class EnumerableMmString : IEnumerator<string>, IEnumerable<string>
    {
        private IEnumBSTR _enumerable;
        public EnumerableMmString(IEnumBSTR enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public string Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        public IEnumerator<string> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for Miner field lists
    /// </summary>
    public class EnumerableMmField : IEnumerator<IField>, IEnumerable<IField>
    {
        private IMMEnumField _enumerable;
        public EnumerableMmField(IMMEnumField enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IField Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        public IEnumerator<IField> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for ESRI relationship class lists
    /// </summary>
    public class EnumerableRelationshipClass : IEnumerator<IRelationshipClass>, IEnumerable<IRelationshipClass>
    {
        private IEnumRelationshipClass _enumerable;
        public EnumerableRelationshipClass(IEnumRelationshipClass enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public IRelationshipClass Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IRelationshipClass> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for ESRI relationships
    /// </summary>
    public class EnumerableRelationship : IEnumerator<IRelationship>, IEnumerable<IRelationship>
    {
        private IEnumRelationship _enumerable;
        public EnumerableRelationship(IEnumRelationship enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public IRelationship Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IRelationship> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for ESRI feature sets
    /// </summary>
    public class EnumerableSet : IEnumerator<object>, IEnumerable<object>
    {
        private ISet _enumerable;
        public EnumerableSet(ISet enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public object Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<object> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for a collection of feature classes
    /// </summary>
    public class EnumerableFeatureClasses : IEnumerator<IFeatureClass>, IEnumerable<IFeatureClass>
    {
        private IEnumFeatureClass _enumerable;
        public EnumerableFeatureClasses(IEnumFeatureClass featureClasses)
        {
            _enumerable = featureClasses;
        }

        public IFeatureClass Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            
            //RWK::This makes me a little uncomfortable, but this object comes directly from datasets.  Marshalling this enumerable causes problems when you later try to access the datasets.
            //while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IFeatureClass> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }

    /// <summary>
    /// Provides an enumerable class for ESRI feature enumerations
    /// </summary>
    public class EnumerableFeatures : IEnumerator<IFeature>, IEnumerable<IFeature>
    {
        private IEnumFeature _enumerable;
        public EnumerableFeatures(IEnumFeature enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public IFeature Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }


    /// <summary>
    /// Provides an enumerable class for ESRI layers
    /// </summary>
    public class EnumerableLayers : IEnumerator<ILayer>, IEnumerable<ILayer>
    {
        private IEnumLayer _enumerable;
        public EnumerableLayers(IEnumLayer enumerable)
        {
            if (enumerable == null)
                throw new Exception("Enumerable is null or disposed");

            _enumerable = enumerable;
        }

        public ILayer Current { get; private set; }

        public bool MoveNext()
        {
            if (_enumerable == null)
                throw new Exception("Enumerable is null or disposed");
            Current = _enumerable.Next();
            return Current != null;
        }

        public void Reset()
        {
            _enumerable.Reset();
        }

        public void Dispose()
        {
            Current = null;

            if (_enumerable == null) return;
            while (Marshal.ReleaseComObject(_enumerable) > 0) { }

            _enumerable = null;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<ILayer> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }
}
