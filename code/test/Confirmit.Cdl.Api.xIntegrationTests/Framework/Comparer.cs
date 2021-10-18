using System;
using System.Collections.Generic;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public class Comparer<T> : IComparer<T>
        where T : class
    {
        private readonly string _sortField;

        public Comparer(string sortField)
        {
            if (sortField != "Id" && sortField != "Accessed" && sortField != "Created")
                throw new ArgumentOutOfRangeException(nameof(sortField), sortField, null);

            _sortField = sortField;
        }

        public int Compare(T x, T y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException();

            switch (_sortField)
            {
                case "Id":
                {
                    var xValue = (long) GetPropertyValue(x);
                    var yValue = (long) GetPropertyValue(y);
                    return xValue.CompareTo(yValue);
                }
                case "Accessed":
                case "Created":
                {
                    var xValue = (DateTime?) GetPropertyValue(x);
                    var yValue = (DateTime?) GetPropertyValue(y);
                    return Compare(xValue, yValue);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(_sortField), _sortField, null);
            }
        }

        private object GetPropertyValue(T src)
        {
            return src.GetType().GetProperty(_sortField)?.GetValue(src);
        }

        private static int Compare(DateTime? x, DateTime? y)
        {
            return x == null
                ? y == null ? 0 : -1
                : y == null
                    ? 1
                    : x.Value.CompareTo(y.Value);
        }
    }
}
