using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using n3q.Tools;
using System.Web;
using System.Linq;

namespace n3q.WebEx.Controllers
{
    [ApiController]
    public class AvatarController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebExConfigDefinition Config { get; set; }
        private readonly IMemoryCache _cache;
        const string HttpPrefix = "http://";

        public AvatarController(ILogger<AvatarController> logger, WebExConfigDefinition config, IMemoryCache memoryCache)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            _cache = memoryCache;
        }

        class CachedResponse
        {
            public byte[] Data;
            public string ContentType;
        }

        [Route("[controller]/HttpBridge")]
        [HttpGet]
        public async Task<FileContentResult> HttpBridge(string url)
        {
            Log.Info(url);

            if (_cache.Get(url) is CachedResponse cachedResponse) {
                return new FileContentResult(cachedResponse.Data, cachedResponse.ContentType);
            }

            var client = new HttpClient();
            //data = await client.GetByteArrayAsync(url);
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) { throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase} {url}"); }
            var data = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType.MediaType;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(3600))
                        .SetSize(data.Length)
                        ;
            _cache.Set(url, new CachedResponse { Data = data, ContentType = contentType }, cacheEntryOptions);

            return new FileContentResult(data, contentType);
        }

        [Route("[controller]/InlineData")]
        [HttpGet]
        public async Task<string> InlineData(string url)
        {
            Log.Info(url);

            var originalUrl = "";
            if (Config.UpgradeAvatarXmlUrlToHttps) {
                if (url.StartsWith(HttpPrefix)) {
                    originalUrl = url;
                    url = "https://" + url.Substring(HttpPrefix.Length);
                    Log.Info($"Upgrade to https: {url}");
                }
            }

            if (_cache.Get(url) is string xml) {
                return xml;
            }

            xml = await CreateXml(url);
            if (!Has.Value(xml) && Has.Value(originalUrl)) {
                Log.Info($"Fallback to http: {originalUrl}");
                url = originalUrl;
                xml = await CreateXml(url);
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(3600))
                        .SetSize(xml.Length)
                        ;
            _cache.Set(url, xml, cacheEntryOptions);

            return xml;
        }

        private async Task<string> CreateXml(string avatarUrl)
        {
            var baseUrl = "";
            var idx = avatarUrl.LastIndexOf("/");
            if (idx >= 0) {
                baseUrl = avatarUrl.Substring(0, idx) + "/";
            }

            var client = new HttpClient();
            string inData;
            try {
                inData = await client.GetStringAsync(avatarUrl);
            } catch (Exception) {
                return "";
            }

            var outDoc = new XmlDocument();
            var outRoot = outDoc.CreateElement("config", "http://schema.bluehands.de/character-config");
            SetXmlAttribute(outDoc, outRoot, "version", "1.0");
            outDoc.AppendChild(outRoot);

            var inDoc = new XmlDocument();
            inDoc.LoadXml(inData);

            var inParams = inDoc.GetElementsByTagName("param");
            foreach (XmlNode inParam in inParams) {
                var name = inParam.Attributes["name"].Value;
                var value = inParam.Attributes["value"].Value;
                var outParam = outDoc.CreateElement("param");
                SetXmlAttribute(outDoc, outParam, "name", name);
                SetXmlAttribute(outDoc, outParam, "value", value);
                outRoot.AppendChild(outParam);
            }

            var imageClient = new HttpClient() { MaxResponseContentBufferSize = 100000 };
            var downloadUrls = new Dictionary<string, string>();

            var inSequences = inDoc.GetElementsByTagName("sequence");
            foreach (XmlNode inSequence in inSequences) {
                var outSequence = outDoc.CreateElement("sequence");

                var group = inSequence.Attributes["group"] != null ? inSequence.Attributes["group"].Value : "";
                var name = inSequence.Attributes["name"] != null ? inSequence.Attributes["name"].Value : "";
                var type = inSequence.Attributes["type"] != null ? inSequence.Attributes["type"].Value : "";
                var probability = inSequence.Attributes["probability"] != null ? inSequence.Attributes["probability"].Value : "";
                var inX = inSequence.Attributes["in"] != null ? inSequence.Attributes["in"].Value : "";
                var outX = inSequence.Attributes["out"] != null ? inSequence.Attributes["out"].Value : "";

                if (Has.Value(group)) { SetXmlAttribute(outDoc, outSequence, "group", group); }
                if (Has.Value(name)) { SetXmlAttribute(outDoc, outSequence, "name", name); }
                if (Has.Value(type)) { SetXmlAttribute(outDoc, outSequence, "type", type); }
                if (Has.Value(probability)) { SetXmlAttribute(outDoc, outSequence, "probability", probability); }
                if (Has.Value(inX)) { SetXmlAttribute(outDoc, outSequence, "in", inX); }
                if (Has.Value(outX)) { SetXmlAttribute(outDoc, outSequence, "out", outX); }

                var outAnimation = outDoc.CreateElement("animation");
                var inAnimation = inSequence.FirstChild;

                if (inAnimation.Attributes["dx"] != null) { SetXmlAttribute(outDoc, outAnimation, "dx", inAnimation.Attributes["dx"].Value); }
                if (inAnimation.Attributes["dy"] != null) { SetXmlAttribute(outDoc, outAnimation, "dy", inAnimation.Attributes["dy"].Value); }

                var inSrc = inAnimation.Attributes["src"] != null ? inAnimation.Attributes["src"].Value : "";
                if (Has.Value(inSrc)) {
                    var imageUrl = inSrc.StartsWith("http") ? inSrc : baseUrl + inSrc;
                    if (Config.UpgradeAvatarImageUrlToHttps) {
                        if (imageUrl.StartsWith(HttpPrefix)) {
                            imageUrl = Config.ImageProxyUrlTemplate.Replace("{url}", HttpUtility.UrlEncode(imageUrl));
                        }
                    }
                    SetXmlAttribute(outDoc, outAnimation, "src", imageUrl);

                    if (inAnimation.Attributes["duration"] != null) { SetXmlAttribute(outDoc, outAnimation, "duration", inAnimation.Attributes["duration"].Value); }

                    downloadUrls.Add(name, imageUrl);

                    outSequence.AppendChild(outAnimation);
                    outRoot.AppendChild(outSequence);
                }
            }

            var imageResponses = new Dictionary<string, Task<HttpResponseMessage>>();
            try {
                foreach (var pair in downloadUrls) {
                    var sequence = pair.Key;
                    var url = pair.Value;
                    Log.Info($"Download image: {url}");
                    var response = imageClient.GetAsync(url);
                    imageResponses.Add(sequence, response);
                }
                _ = await Task.WhenAll(imageResponses.Select(pair => pair.Value));
            } catch (Exception ex) {
                Log.Warning(ex);
            }

            var imageBytes = new Dictionary<string, Task<byte[]>>();
            try {
                foreach (var pair in imageResponses) {
                    var sequence = pair.Key;
                    var response = pair.Value;
                    if (response.Result.IsSuccessStatusCode) {
                        var bytes = response.Result.Content.ReadAsByteArrayAsync();
                        imageBytes.Add(sequence, bytes);
                    } else {
                        Log.Warning($"Image download failed: {sequence}: {downloadUrls[sequence]}");
                    }
                }
                _ = await Task.WhenAll(imageBytes.Select(pair => pair.Value));
            } catch (Exception ex) {
                Log.Warning(ex);
            }

            foreach (var pair in imageBytes) {
                var sequence = pair.Key;
                var bytes = pair.Value;
                var imageData = bytes.Result;

                //using var stream = new MemoryStream(imageData);
                //var info = new GifInfo.GifInfo(stream);
                //var duration = (int)Math.Round(info.AnimationDuration.TotalMilliseconds);

                var duration = GetGifDurationMSec(imageData);

                var xpath = $"//sequence[@name='{sequence}']";
                var sequenceNodes = outDoc.SelectNodes(xpath);
                if (sequenceNodes.Count > 0) {
                    var sequenceNode = sequenceNodes[0];
                    var animationNode = sequenceNode.FirstChild;

                    if (Config.AvatarProxyPreloadSequenceNames.Contains(sequence)) {
                        var outDataBase64Encoded = Convert.ToBase64String(imageData);
                        var outDataUrl = "data:image/gif;base64," + outDataBase64Encoded;
                        SetXmlAttribute(outDoc, animationNode, "src", outDataUrl);
                    }

                    SetXmlAttribute(outDoc, animationNode, "duration", duration.ToString(CultureInfo.InvariantCulture));
                }
            }

            var xml = "";
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings {
                Indent = true,
                Encoding = Encoding.UTF8,
            })) {
                outDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                xml = stringWriter.GetStringBuilder().ToString();
            }

            xml = xml.Replace(" xmlns=\"\"", "");
            xml = xml.Replace("encoding=\"utf-16\"", "encoding=\"UTF-8\"");
            return xml;
        }

        void SetXmlAttribute(XmlDocument doc, XmlNode node, string name, string value)
        {
            var attr = doc.CreateAttribute(name);
            attr.Value = value;

            if (node.Attributes[name] == null) {
                node.Attributes.Append(attr);
            } else {
                node.Attributes[name].Value = value;
            }
        }

        int GetGifDurationMSec(byte[] data)
        {
            var duration = 0;
            var frames = 0;
            //var loop = false;

            //data = System.IO.File.ReadAllBytes(@"C:\Users\wolf\Desktop\idle-1.gif");

            var signatureBytes = data.Take(4).ToArray();
            var signature = System.Text.Encoding.Default.GetString(signatureBytes);
            if (signature == "GIF8") {
                var graphicControlExtensionHeader = new byte[] { 0x21, 0xf9, 0x04 };
                for (var i = 4; i < data.Length; i++) {
                    var currentBytes = data.Skip(i).Take(3).ToArray();
                    if (currentBytes.SequenceEqual(graphicControlExtensionHeader)) {
                        var durationBytes = data.Skip(i + 4).Take(2).ToArray();
                        var delayTime = BitConverter.ToInt16(durationBytes);
                        if (delayTime < 1000) {
                            duration += delayTime;
                            frames++;
                        }
                    }
                }
            }

            return duration * 10;
        }

    }
}
