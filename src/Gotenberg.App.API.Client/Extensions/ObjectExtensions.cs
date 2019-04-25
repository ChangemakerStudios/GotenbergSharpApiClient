// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Gotenberg.App.API.Sharp.Client.Extensions
{
    public static class ObjectExtensions
    {
        static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver()};

        /// <summary>
        ///  Converts the specified instance to a dictionary via JsonConvert serialize/deserialize
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <returns>A read only dictionary representation of the instance</returns>
        /// <exception cref="ArgumentNullException">instance</exception>
        public static IReadOnlyDictionary<K,V> ToDictionary<K,V>(this object instance)
        {
            if(instance == null) throw new ArgumentNullException(nameof(instance));

            var json = JsonConvert.SerializeObject(instance, _serializerSettings);
            return JsonConvert.DeserializeObject<Dictionary<K,V>>(json, _serializerSettings);
        }
    }
}