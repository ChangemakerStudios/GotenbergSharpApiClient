using System;
using System.Collections.Generic;
using Gotenberg.Sharp.API.Client.Extensions;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public class CustomHttpHeaders : Dictionary<string, IEnumerable<string>>
    {
        public void AddItem(string name, string value)
        {
            if (name.IsNotSet()) throw new ArgumentException("Header name is null or empty");

            this.Add(name, new[] { value });
        }
    }
}