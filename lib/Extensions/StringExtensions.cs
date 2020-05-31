namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class StringExtensions
    {
        public static bool IsSet(this string value) => !value.IsNotSet();

        public static bool IsNotSet(this string value) => string.IsNullOrWhiteSpace(value);
    }
}