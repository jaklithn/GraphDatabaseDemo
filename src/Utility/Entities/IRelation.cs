namespace Utility.Entities
{
    public interface IRelation
    {
        string FromId { get; }
        string ToId { get; }
    }
}