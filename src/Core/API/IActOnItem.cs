using System;

namespace Core.API
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }
}