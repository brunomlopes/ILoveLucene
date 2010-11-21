namespace Core.Abstractions
{
    public interface IBackgroundStartTask
    {
        bool Executed { get; }
        void Execute();
    }
}