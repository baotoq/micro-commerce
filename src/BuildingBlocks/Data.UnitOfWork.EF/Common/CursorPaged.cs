using System.Collections.Generic;

namespace Data.UnitOfWork.EF.Common
{
    public class CursorPaged<T, TToken>
    {
        public List<T> Data { get; set; }
        public TToken PreviousPageToken { get; set; }
        public TToken NextPageToken { get; set; }
    }
}
