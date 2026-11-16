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

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class GotenbergServerSelectionTests
{
    private static GotenbergServer Server(string tag) => GotenbergServer.FromImageTag(tag);

    [TestCase("7", null, null, null)]
    [TestCase("8", "8.0.0", null, null)]
    [TestCase("7", "8.0.0", null, "Needs Gotenberg 8.0.0 or later")]
    [TestCase("8.3.0", "8.4.0", null, "Needs Gotenberg 8.4.0 or later")]
    [TestCase("8.4.0", "8.4.0", null, null)]
    [TestCase("8", "8.15.0", "9.0.0", null)]
    [TestCase("8.40.0", "8.15.0", "9.0.0", null)]
    [TestCase("8.14.0", "8.15.0", "9.0.0", "Needs Gotenberg 8.15.0 or later")]
    [TestCase("9", "8.15.0", "9.0.0", "Removed in Gotenberg 9.0.0")]
    [TestCase("9.0.0", "8.15.0", "9.0.0", "Removed in Gotenberg 9.0.0")]
    [TestCase("9.5.1", null, "9.0.0", "Removed in Gotenberg 9.0.0")]
    [TestCase("latest", null, "9.0.0", "Removed in Gotenberg 9.0.0")]
    [TestCase("8.99.0", null, "9.0.0", null)]
    public void IgnoreReason_AppliesFloorAndExclusiveCeiling(string tag, string? since, string? until, string? expected)
    {
        GotenbergServers.IgnoreReason(Server(tag).Version, since, until).Should().Be(expected);
    }

    [Test]
    public void IgnoreReason_CeilingIsExclusive()
    {
        GotenbergServers.IgnoreReason(new Version(8, 9, 9), null, "9.0.0").Should().BeNull();
        GotenbergServers.IgnoreReason(new Version(9, 0, 0), null, "9.0.0").Should().NotBeNull();
    }
}
