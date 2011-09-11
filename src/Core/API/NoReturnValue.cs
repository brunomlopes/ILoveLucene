namespace Core.API
{
    public static class NoReturnValue
    {
        private class NullTypedItem : IItem
        {
            public string Text
            {
                get { return null; }
            }

            public string Description
            {
                get { return null; }
            }

            public object Item
            {
                get { return null; }
            }
        }
        public static readonly IItem Object = new NullTypedItem();
    }
}