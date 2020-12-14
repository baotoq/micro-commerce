namespace Shared.MediatR.Models
{
    public abstract class CursorPagedQuery<TToken>
    {
        public virtual TToken PageToken { get; set; }

        public int PageSize { get; set; } = 20;
    }
}
