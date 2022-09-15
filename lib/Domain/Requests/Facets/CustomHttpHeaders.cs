using System;
using System.Collections.Generic;
using System.Linq;
using Gotenberg.Sharp.API.Client.Extensions;
using Newtonsoft.Json;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public class CustomHttpHeaders : Dictionary<string, IEnumerable<string>>
    {
        public void AddItem(string name, string value)
        {
            if (name.IsNotSet()) throw new ArgumentException("Header name is null or empty");

            this.Add(name, new[] { value });
        }

        public string ToJsonFormat()
        {
            return JsonConvert.SerializeObject(this.ToDictionary(
                entry => entry.Key,
                entry => string.Join(",", entry.Value)));
        }
    }
}