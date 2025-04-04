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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public sealed class MergeBuilder() : BaseMergeBuilder<MergeRequest, MergeBuilder>(new MergeRequest())
{
    /// <summary>
    ///     Convert the resulting PDF into the given PDF/A format.
    /// </summary>
    public MergeBuilder SetPdfFormat(LibrePdfFormats format)
    {
        this.Request.PdfFormat = format;
        return this;
    }

    /// <summary>
    /// 	Flatten the resulting PDF.
    /// </summary>
    public MergeBuilder SetFlatten(bool enableFlatten = true)
    {
        this.Request.EnableFlatten = enableFlatten;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    public MergeBuilder SetPdfUa(bool enablePdfUa = true)
    {
        this.Request.EnablePdfUa = enablePdfUa;
        return this;
    }
}