//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

public sealed class UrlHeaderFooterBuilder(HeaderFooterDocument headerFooterDocument)
{
    #region header

    
    public UrlHeaderFooterBuilder SetHeader(ContentItem header)
    {
        headerFooterDocument.Header =
            header ?? throw new ArgumentNullException(nameof(header));
        return this;
    }

    
    public UrlHeaderFooterBuilder SetHeader(string header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    
    public UrlHeaderFooterBuilder SetHeader(byte[] header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    
    public UrlHeaderFooterBuilder SetHeader(Stream header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    #endregion

    #region footer

    
    public UrlHeaderFooterBuilder SetFooter(ContentItem footer)
    {
        headerFooterDocument.Footer =
            footer ?? throw new ArgumentNullException(nameof(footer));
        return this;
    }

    
    public UrlHeaderFooterBuilder SetFooter(string footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    
    public UrlHeaderFooterBuilder SetFooter(byte[] footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    
    public UrlHeaderFooterBuilder SetFooter(Stream footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    #endregion
}