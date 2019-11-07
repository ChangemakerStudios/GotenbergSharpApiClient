// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class MultiFormHeaderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFormHeaderAttribute"/> class.
        /// </summary>
        /// <param name="contentDisposition">The content disposition.</param>
        /// <param name="name">The name.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="mediaType">The media type</param>
        public MultiFormHeaderAttribute(string name = "files", 
                                        string fileName = null, 
                                        string contentDisposition = "form-data",
                                        string mediaType = "text/html")
        {
            ContentDisposition = contentDisposition;
            Name = name;
            FileName = fileName;
            MediaType = mediaType;
        }

        /// <summary>
        /// Gets or sets the content disposition.
        /// </summary>
        /// <value>
        /// The content disposition.
        /// </value>
        public string ContentDisposition {get;}

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name {get;}
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName {get;}

        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public string MediaType { get; }
    }
}