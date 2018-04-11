using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ESRI.ArcGIS.Geodatabase;

namespace PGE.ArcFM.Common.Wrappers
{
    public class TableWrapper
    {
        private readonly Dictionary<string, int> _fieldIndexDictionary;
        private readonly Dictionary<string, string> _fieldAliasDictionary;
        private readonly IDictionary<int, IDictionary<int, IDictionary<object, string>>> _fieldDomainDictionary;
        public string TableName { get; private set; }
        private readonly ITable _esriTable;

        public TableWrapper(ITable esriTable, ICollection<string> fieldNames)
        {
            if (esriTable == null)
                throw new ArgumentNullException("esriTable");
            if (fieldNames == null)
                throw new ArgumentNullException("fieldNames");

            _esriTable = esriTable;
            TableName = ((IDataset) esriTable).Name;

            var fieldInfoCollection =
                fieldNames.Select(fieldName =>
                {
                    var fieldIndex = _esriTable.FindField(fieldName);
                    var esriField = fieldIndex < 0 ?
                        null :
                        _esriTable.Fields.Field[fieldIndex];
                    return new { FieldName = fieldName, FieldIndex = fieldIndex, EsriField = esriField };
                })
                    .Where(fieldInfo => fieldInfo.EsriField != null)
                    .ToArray();

            _fieldIndexDictionary = fieldInfoCollection
                .ToDictionary(fieldInfo => fieldInfo.FieldName, fieldInfo => fieldInfo.FieldIndex);
            
            _fieldAliasDictionary = fieldInfoCollection
                .ToDictionary(fieldInfo => fieldInfo.FieldName, fieldInfo => fieldInfo.EsriField.AliasName);

            _fieldDomainDictionary =
                ((IObjectClass) _esriTable).GetSubtypeFieldDomainDictionary(_fieldIndexDictionary.Values);
        }

        public DataView GetRows(string filter, string sort = null)
        {
            if(string.IsNullOrEmpty(filter))
                throw new ArgumentNullException("filter");

            var newTable = GetEmptyDataTable();

            foreach (var esriRow in _esriTable.Where(filter))
            {
                var newRow = GetRow(esriRow, newTable);
                newTable.Rows.Add(newRow);
            }
            
            return new DataView(newTable, string.Empty, sort, DataViewRowState.CurrentRows);
        }

        public DataRow GetRow(IRow esriRow, DataTable dataTable = null)
        {
            if (dataTable == null)
                dataTable = GetEmptyDataTable();

            var newRow = dataTable.NewRow();
            foreach (var fieldInfo in _fieldIndexDictionary)
            {
                newRow[fieldInfo.Key] = ((IObject)esriRow).GetDomainedValue(fieldInfo.Value, _fieldDomainDictionary);
                //var rowValue = esriRow.Value[fieldInfo.Value];
                //
                //Dictionary<object, string> domainDictionary;
                //string domainAlias;
                //newRow[fieldInfo.Key] = _fieldDomainDictionary.TryGetValue(fieldInfo.Key, out domainDictionary) &&
                //                        domainDictionary.TryGetValue(rowValue, out domainAlias)
                //    ? domainAlias
                //    : Convert.ToString(rowValue);
            }

            return newRow;
        }

        private DataTable GetEmptyDataTable()
        {
            var newTable = new DataTable(TableName);
            foreach (var fieldInfo in _fieldIndexDictionary)
                newTable.Columns.Add(fieldInfo.Key);

            return newTable;
        }

        public void LoadDataGridView(DataGridView dataGridView, string filter, string sortColumn = "")
        {
            var dataTable = GetRows(filter);

            dataGridView.Columns.Clear();
            dataGridView.DataSource = dataTable;

            foreach (DataGridViewColumn dataColumn in dataGridView.Columns)
            {
                string columnAlias;
                if (_fieldAliasDictionary.TryGetValue(dataColumn.Name, out columnAlias))
                    dataColumn.HeaderText = columnAlias;
                if(!string.IsNullOrEmpty(sortColumn) &&
                    dataColumn.Name.Equals(sortColumn, StringComparison.InvariantCultureIgnoreCase))
                    dataGridView.Sort(dataColumn, ListSortDirection.Ascending);
            }
        }
        public void LoadDataGridView(DataGridView dataGridView, string filter, string[] sortColumns = null)
        {
            var dataTable = sortColumns == null
                ? GetRows(filter)
                : GetRows(filter,string.Join(",", sortColumns));

            dataGridView.Columns.Clear();
            dataGridView.DataSource = dataTable;

            foreach (DataGridViewColumn dataColumn in dataGridView.Columns)
            {
                string columnAlias;
                if (_fieldAliasDictionary.TryGetValue(dataColumn.Name, out columnAlias))
                    dataColumn.HeaderText = columnAlias;
            }
        }
    }
}
