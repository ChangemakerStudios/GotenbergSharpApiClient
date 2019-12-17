// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class ConfigBuilder : ConversionBuilderFacade
    {
        public ConfigBuilder(HttpMessageConfig config) => this.Config = config;
        
        [UsedImplicitly]
        public ConfigBuilder TimeOut(float value)
        {
            this.Config.TimeOut = value;
            return this;
       }
        
        [UsedImplicitly]
        public ConfigBuilder ChromeRpccBufferSize(int value)
        {
            this.Config.ChromeRpccBufferSize = value;
            return this;
        }

        [UsedImplicitly]
        public ConfigBuilder ResultFileName(string value)
        {
            if(value.IsNotSet()) throw new ArgumentException("ResultFileName was null || empty");
            this.Config.ResultFileName = value;
            return this;
        }
        
        [UsedImplicitly]
        public ConfigBuilder WebHook(string value)
        {
            if(value.IsNotSet()) throw new ArgumentException("WebHook was null || empty");
            if(!Uri.IsWellFormedUriString(value, UriKind.Absolute)) throw new ArgumentException("WebHook was not well formed. See https://docs.microsoft.com/en-us/dotnet/api/system.uri.iswellformeduristring?view=netstandard-2.0");
            this.Config.WebHook =new Uri(value);
            return this;
        }
        
        [UsedImplicitly]
        public ConfigBuilder WebHook(Uri value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!value.IsAbsoluteUri) throw new ArgumentException("WebHook must be absolute");
            this.Config.WebHook = value;
            return this;
        }

        [UsedImplicitly]
        public ConfigBuilder WebHookTimeOut(float value)
        {
            this.Config.WebHookTimeOut = value;
            return this;
        }
        
    }
}