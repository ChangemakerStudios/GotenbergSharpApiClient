// Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public static class HtmlConversionBehaviorBuilderExtensions
{
    /// <summary>
    ///     Adds a cookie to store in the Chromium cookie jar.
    /// </summary>
    /// <param name="builder">HtmlConversionBehaviorBuilder</param>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    /// <param name="domain">Cookie domain</param>
    /// <param name="path">Optional cookie path</param>
    /// <param name="secure">Optional secure flag</param>
    /// <param name="httpOnly">Optional HTTP-only flag</param>
    /// <param name="sameSite">Optional SameSite attribute ("Strict", "Lax", or "None")</param>
    /// <returns></returns>
    public static HtmlConversionBehaviorBuilder AddCookie(this HtmlConversionBehaviorBuilder builder,
        string name,
        string value,
        string domain,
        string? path = null,
        bool? secure = null,
        bool? httpOnly = null,
        string? sameSite = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var cookie = new Cookie
        {
            Name = name,
            Value = value,
            Domain = domain,
            Path = path,
            Secure = secure,
            HttpOnly = httpOnly,
            SameSite = sameSite
        };

        return builder.AddCookie(cookie);
    }

    /// <summary>
    ///     Adds multiple cookies to store in the Chromium cookie jar.
    /// </summary>
    /// <param name="builder">HtmlConversionBehaviorBuilder</param>
    /// <param name="cookies">The cookies to add</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HtmlConversionBehaviorBuilder AddCookies(this HtmlConversionBehaviorBuilder builder, IEnumerable<Cookie> cookies)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (cookies == null)
        {
            throw new ArgumentNullException(nameof(cookies));
        }

        foreach (var c in cookies)
        {
            builder.AddCookie(c);
        }

        return builder;
    }
}