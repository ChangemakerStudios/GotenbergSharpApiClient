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

using Gotenberg.Sharp.API.Client.Domain.Builders;

namespace Gotenberg.Sharp.Api.Client.Tests.Domain.Builders;

public sealed class HtmlRequestBuilderTests
{
    [Fact]
    public Task ToApiRequestMessage_PaperDimensionWithDecimal_NumbersUsesDotAsDecimalSeparator_Test()
    {
        // Arrange.
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<h1>Hello</h1>"))
            .WithPageProperties(pp => pp.SetPaperHeight(12.2).SetPaperWidth(8.5));

        // Act.
        var request = builder.Build().CreateApiRequest().ToApiRequestMessage();

        // Assert.
        Assert.NotNull(request.Content);
        return Verify(request.Content.ReadAsStringAsync())
            .ScrubLines(x => x.StartsWith("----------------------------"));
    }
}