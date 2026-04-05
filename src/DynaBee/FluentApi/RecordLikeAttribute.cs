namespace DynaBee.FluentApi
{
    /// <summary>
    /// Marker attribute indicating a generated type is intended to behave as a record-like model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class RecordLikeAttribute : Attribute
    {
    }
}