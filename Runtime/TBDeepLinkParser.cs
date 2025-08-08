using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace TextBuddy.core
{
    /// <summary>
    /// Parses and provides access to deep link data from a given URL.
    /// </summary>
    public class TBDeepLinkParser
    {
        private readonly Uri _uri;
        private readonly bool _isParsed;

        /// <summary>
        /// Initializes the parser with a deep link URL.
        /// </summary>
        /// <param name="url">The deep link URL to parse.</param>
        public TBDeepLinkParser(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
            {
                _uri = parsedUri;
                _isParsed = true;
            }
            else
            {
                TBLoger.Warning($"[DeepLinkParser] Invalid URL provided: {url}");
                _isParsed = false;
            }
        }

        /// <summary>
        /// Returns the scheme name 
        /// </summary>
        public string Scheme => _isParsed ? _uri.Scheme : null;

        /// <summary>
        /// Returns the host name (e.g., 'textbuddy') from the deep link URL.
        /// </summary>
        public string HostName => _isParsed ? _uri.Host : null;

        /// <summary>
        /// Returns the endpoint path (e.g., '/confirm') from the deep link URL.
        /// </summary>
        public string Path => _isParsed ? _uri.AbsolutePath : null;

        /// <summary>
        /// Parses the query string parameters into a dictionary.
        /// </summary>
        /// <returns>Dictionary of query parameters, or null if parsing failed.</returns>
        public Dictionary<string, string> ParseQuery()
        {
            if (!_isParsed || string.IsNullOrEmpty(_uri.Query))
                return null;

            try
            {
                return _uri.Query
                           .TrimStart('?')
                           .Split('&', StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Split('=', 2))
                           .Where(pair => pair.Length == 2 && !string.IsNullOrWhiteSpace(pair[0]))
                           .ToDictionary(
                               pair => pair[0],
                               pair => UnityWebRequest.UnEscapeURL(pair[1])
                           );
            }
            catch (Exception ex)
            {
                TBLoger.Warning($"[DeepLinkParser] Failed to parse query: {ex.Message}");
                return null;
            }
        }
    }
}
