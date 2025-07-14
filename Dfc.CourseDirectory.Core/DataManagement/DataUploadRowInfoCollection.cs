using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public abstract class DataUploadRowInfoCollection<TData, TRow> : IReadOnlyList<TRow>
        where TRow : DataUploadRowInfo<TData>
    {
        private readonly TRow[] _rows;

        protected DataUploadRowInfoCollection(IEnumerable<TRow> rows)
        {
            _rows = rows.ToArray();
        }

        public TRow this[int index] => _rows[index];

        public int Count => _rows.Length;

        public IEnumerator<TRow> GetEnumerator() => ((IReadOnlyList<TRow>)_rows).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public abstract class DataUploadRowInfo<T>
    {
        protected DataUploadRowInfo(T data, int rowNumber)
        {
            if (rowNumber < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(rowNumber));
            }

            Data = data ?? throw new ArgumentNullException(nameof(data));
            RowNumber = rowNumber;
        }

        public T Data { get; }
        public int RowNumber { get; }
    }
}
