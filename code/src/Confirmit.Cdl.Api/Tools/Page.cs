using System.Collections.Generic;

namespace Confirmit.Cdl.Api.Tools
{
    public class Page<T>
    {
        private readonly int _skip;
        private readonly int _take;

        public Page(int skip, int take)
        {
            _skip = skip;
            _take = take;
        }

        public List<T> Entities { get; set; }
        public int TotalCount { get; set; }

        public int? Previous
        {
            get
            {
                if (_skip <= 0 || _take <= 0 || TotalCount <= 0)
                    return null;

                var skip = _skip - _take;
                if (skip <= 0)
                    return 0;

                if (skip >= TotalCount)
                    skip = _skip - (_skip - TotalCount) / _take * _take - _take;

                return skip;
            }
        }

        public int? Next
        {
            get
            {
                if (_take <= 0 || TotalCount <= 0 || _skip + _take >= TotalCount)
                    return null;

                return _skip + _take;
            }
        }
    }
}