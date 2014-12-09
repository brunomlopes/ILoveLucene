using System;

namespace Core.API
{
    /// <summary>
    /// Represents an action that is invalid
    /// </summary>
    [Serializable]
    public class InvalidActionException : ApplicationException
    {
        public InvalidActionException(string message) : base(message)
        {
        }
    }
}