namespace Plugins.Commands
{
    public interface ICommand
    {
        void Act();
        string Text { get; }
        string Description { get; }
    }
}