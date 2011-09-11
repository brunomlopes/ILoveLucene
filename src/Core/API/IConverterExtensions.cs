namespace Core.API
{
    public static class IConverterExtensions
    {
        public static string GetId(this IConverter self)
        {
            return self.GetType().FullName;
        }
    }
}