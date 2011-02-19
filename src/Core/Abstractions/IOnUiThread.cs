using System;

namespace Core.Abstractions
{
    public interface IOnUiThread
    {
        void Execute(Action action);
    }
}