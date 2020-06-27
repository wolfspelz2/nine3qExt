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

namespace n3q.WebEx.Controllers
{
    [ApiController]
    public class AvatarController : ControllerBase
    {
        public ICallbackLogger Log { get; set; }
        public WebExConfig Config { get; set; }
        private readonly IMemoryCache _cache;

        public AvatarController(ILogger<AvatarController> logger, WebExConfig config, IMemoryCache memoryCache)
        {
            Log = new FrameworkCallbackLogger(logger);
            Config = config;
            _cache = memoryCache;
        }

        [Route("[controller]/InlineData")]
        [HttpGet]
        public async Task<string> InlineData(string url)
        {
            if (_cache.Get(url) is string xml) {
                return xml;
            }

            xml = await CreateXml(url);

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
            var inData = await client.GetStringAsync(avatarUrl);

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
            var imageDownloads = new Dictionary<string, Task<byte[]>>();

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
                    SetXmlAttribute(outDoc, outAnimation, "src", imageUrl);

                    if (inAnimation.Attributes["duration"] != null) { SetXmlAttribute(outDoc, outAnimation, "duration", inAnimation.Attributes["duration"].Value); }

                    var imageDataTask = imageClient.GetByteArrayAsync(imageUrl);
                    imageDownloads.Add(name, imageDataTask);

                    outSequence.AppendChild(outAnimation);
                    outRoot.AppendChild(outSequence);
                }
            }

            _ = await Task.WhenAll(imageDownloads.Values);

            foreach (var pair in imageDownloads) {
                var sequenceName = pair.Key;
                var imageData = pair.Value.Result;

                using var stream = new MemoryStream(imageData);
                var info = new GifInfo.GifInfo(stream);
                var duration = (int)Math.Round(info.AnimationDuration.TotalMilliseconds);

                var xpath = $"//sequence[@name='{sequenceName}']";
                var sequenceNodes = outDoc.SelectNodes(xpath);
                if (sequenceNodes.Count > 0) {
                    var sequenceNode = sequenceNodes[0];
                    var animationNode = sequenceNode.FirstChild;

                    if (Config.AvatarProxyPreloadSequenceNames.Contains(sequenceName)) {
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

        private void SetXmlAttribute(XmlDocument doc, XmlNode node, string name, string value)
        {
            var attr = doc.CreateAttribute(name);
            attr.Value = value;

            if (node.Attributes[name] == null) {
                node.Attributes.Append(attr);
            } else {
                node.Attributes[name].Value = value;
            }
        }
    }
}
