// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class ConfigBuilder
    {
        public ConfigBuilder(HtmlConversionBuilder parent)
            =>  this.Builder = parent;
        
        [UsedImplicitly]
        public HtmlConversionBuilder Builder { get; }

        [UsedImplicitly]
        public ConfigBuilder TimeOut(float value)
        {
            this.Builder.ConfigInstance.TimeOut = value;
            return this;
       }
        
        [UsedImplicitly]
        public ConfigBuilder ChromeRpccBufferSize(int value)
        {
            this.Builder.ConfigInstance.ChromeRpccBufferSize = value;
            return this;
        }

        [UsedImplicitly]
        public ConfigBuilder ResultFileName(string value)
        {
            if(value.IsNotSet()) throw new ArgumentException("ResultFileName was null || empty");
            this.Builder.ConfigInstance.ResultFileName = value;
            return this;
        }
        
        [UsedImplicitly]
        public ConfigBuilder WebHook(string value)
        {
            if(value.IsNotSet()) throw new ArgumentException("WebHook was null || empty");
            if(!Uri.IsWellFormedUriString(value, UriKind.Absolute)) throw new ArgumentException("WebHook was not well formed. See https://docs.microsoft.com/en-us/dotnet/api/system.uri.iswellformeduristring?view=netstandard-2.0");
            this.Builder.ConfigInstance.WebHook =new Uri(value);
            return this;
        }
        
        [UsedImplicitly]
        public ConfigBuilder WebHook(Uri value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(!value.IsAbsoluteUri) throw new ArgumentException("WebHook must be absolute");
            this.Builder.ConfigInstance.WebHook = value;
            return this;
        }

        [UsedImplicitly]
        public ConfigBuilder WebHookTimeOut(float value)
        {
            this.Builder.ConfigInstance.WebHookTimeOut = value;
            return this;
        }
        
    }
}