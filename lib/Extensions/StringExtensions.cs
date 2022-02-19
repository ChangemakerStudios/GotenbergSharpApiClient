namespace Gotenberg.Sharp.API.Client.Extensions
{
    internal static class StringExtensions
    {
        internal static bool IsSet(this string value)
            => !value.IsNotSet();

        internal static bool IsNotSet(this string value)
            => string.IsNullOrWhiteSpace(value);
    }
}