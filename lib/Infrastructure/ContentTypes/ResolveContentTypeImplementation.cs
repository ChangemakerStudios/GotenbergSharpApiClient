﻿//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;

using MimeMapping;

namespace Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

public class ResolveContentTypeImplementation : IResolveContentType
{
    public string GetContentType(
        string fileName,
        string defaultContentType = "application/octet-stream")
    {
        if (fileName.IsNotSet())
            throw new ArgumentException("file name is either null or empty");

        return MimeUtility.GetMimeMapping(fileName) ?? defaultContentType;
    }
}