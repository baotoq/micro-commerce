using System.Collections.Generic;

namespace Catalog.API.Common
{
    public class CursorPaged<T> : List<T>
    {
        public long PreviousPage { get; set; }
        public long NextPage { get; set; }

        public CursorPaged()
        {
        }

        public CursorPaged(IEnumerable<T> collection) : base(collection)
        {
        }
    }
}
