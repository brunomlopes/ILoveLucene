namespace Core.Abstractions
{
    public interface IFindDefaultActionForItemStrategy
    {
        /// <summary>
        /// Returns the default action for the item. 
        /// If it doesn't exist, return null
        /// </summary>
        IActOnItem DefaultForItem(IItem item);
    }
}