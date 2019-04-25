// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    public static class StringExtensions
    {
        // ReSharper disable once UnusedMember.Global
        public static bool IsSet(this string value) => !value.IsNotSet();

        public static bool IsNotSet(this string value) => string.IsNullOrWhiteSpace(value);
    }
}