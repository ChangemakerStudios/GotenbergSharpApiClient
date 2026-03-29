//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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



namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

/// <summary>
/// Types of external resources that can be injected into URL-based PDF conversions.
/// </summary>
public enum ExtraUrlResourceType
{
    /// <summary>
    /// No resource type specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// CSS stylesheet injected via link tag (&lt;link rel="stylesheet" href="..."&gt;).
    /// </summary>
    LinkTag = 1,

    /// <summary>
    /// JavaScript file injected via script tag (&lt;script src="..."&gt;&lt;/script&gt;).
    /// </summary>
    ScriptTag = 2
}