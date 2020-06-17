using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using n3q.Tools;

namespace n3q.Runtime.Controllers
{
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(ILogger<IdentityController> logger)
        {
            _logger = logger;
            _ = _logger;
        }

        /*
            https://avatar.weblin.sui.li/identity/?avatarUrl=http://avatar.zweitgeist.com/gif/0812/gingerbreadwoman/config.xml&nickname=nickname
            http://localhost:5001/Identity/Generated?avatarUrl=http://avatar.zweitgeist.com/gif/0812/gingerbreadwoman/config.xml&nickname=nickname&digest=123&imageUrl=https://www.galactic-developments.de/img/book-menu.png

            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE identity-xml>
            <identity xmlns="http://schema.bluehands.de/digest-container" digest="">
                <item id="avatar" contenttype="avatar" digest="" src="http://avatar.zweitgeist.com/gif/0812/gingerbreadwoman/idle.gif" order="1"/>
                <item id="avatar2" contenttype="avatar2" digest="" src="http://avatar.zweitgeist.com/gif/0812/gingerbreadwoman/config.xml" mimetype="avatar/gif" order="1"/>
                <item id="properties" contenttype="properties" digest="" encoding="plain" mimetype="text/plain" order="1"> 
            <![CDATA[KickVote=true
            Nickname=nickname]]></item>
            </identity>
        */
        [Route("[controller]/Generated")]
        [HttpGet]
        public async Task<string> Generated(string avatarUrl, string nickname, string imageUrl, string digest)
        {
            await Task.CompletedTask;

            if (!Has.Value(imageUrl)) {
                var idx = avatarUrl.LastIndexOf("/");
                if (idx >= 0) {
                    imageUrl = avatarUrl.Substring(0, idx) + "/idle.gif";
                }
            }

            var digest_XmlEncoded = WebUtility.HtmlEncode(digest);
            var imageUrl_XmlEncoded = WebUtility.HtmlEncode(imageUrl);
            var avatarUrl_XmlEncoded = WebUtility.HtmlEncode(avatarUrl);
            var nickname_XmlEncoded = WebUtility.HtmlEncode(nickname);

#pragma warning disable format
            var xml =
$@"<?xml version='1.0' encoding='UTF-8'?>
<!DOCTYPE identity-xml>
<identity xmlns='http://schema.bluehands.de/digest-container' digest='{digest_XmlEncoded}'>
    <item id='avatar' contenttype='avatar' digest='{digest_XmlEncoded}' src='{imageUrl_XmlEncoded}' order='1'/>
    <item id='avatar2' contenttype='avatar2' digest='{digest_XmlEncoded}' src='{avatarUrl_XmlEncoded}' mimetype='avatar/gif' order='1'/>
    <item id='properties' contenttype='properties' digest='{digest_XmlEncoded}' encoding='plain' mimetype='text/plain' order='1'><![CDATA[Nickname={nickname_XmlEncoded}]]></item>
</identity>
".Replace("'", "\"");
#pragma warning restore format

            //var xmlDoc = new XmlDocument();
            //var rootNode = xmlDoc.CreateElement("identity", "http://schema.bluehands.de/digest-container");
            //AddXmlAttribute(xmlDoc, rootNode, "digest", digest);
            //xmlDoc.AppendChild(rootNode);

            //{
            //    var itemNode = xmlDoc.CreateElement("item", "");
            //    AddXmlAttribute(xmlDoc, itemNode, "id", "avatar");
            //    AddXmlAttribute(xmlDoc, itemNode, "contenttype", "avatar");
            //    AddXmlAttribute(xmlDoc, itemNode, "digest", digest);
            //    AddXmlAttribute(xmlDoc, itemNode, "src", imageUrl);
            //    AddXmlAttribute(xmlDoc, itemNode, "order", "1");
            //    rootNode.AppendChild(itemNode);
            //}

            //{
            //    var itemNode = xmlDoc.CreateElement("item");
            //    AddXmlAttribute(xmlDoc, itemNode, "id", "avatar2");
            //    AddXmlAttribute(xmlDoc, itemNode, "contenttype", "avatar2");
            //    AddXmlAttribute(xmlDoc, itemNode, "digest", digest);
            //    AddXmlAttribute(xmlDoc, itemNode, "src", avatarUrl);
            //    AddXmlAttribute(xmlDoc, itemNode, "order", "1");
            //    rootNode.AppendChild(itemNode);
            //}

            //{
            //    var itemNode = xmlDoc.CreateElement("item");
            //    AddXmlAttribute(xmlDoc, itemNode, "id", "properties");
            //    AddXmlAttribute(xmlDoc, itemNode, "contenttype", "properties");
            //    AddXmlAttribute(xmlDoc, itemNode, "digest", digest);
            //    AddXmlAttribute(xmlDoc, itemNode, "mimetype", "text/plain");
            //    AddXmlAttribute(xmlDoc, itemNode, "order", "1");
            //    itemNode.InnerText = $"Nickname={nickname}"; 
            //    rootNode.AppendChild(itemNode);
            //}

            //var xml = "";
            //using (var stringWriter = new StringWriter())
            //using (var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings {
            //    Indent = true,
            //    Encoding = Encoding.UTF8,
            //})) {
            //    xmlDoc.WriteTo(xmlTextWriter);
            //    xmlTextWriter.Flush();
            //    xml = stringWriter.GetStringBuilder().ToString();
            //}

            //xml = xml.Replace(" xmlns=\"\"", "");
            //xml = xml.Replace("encoding=\"utf-16\"", "encoding=\"UTF-8\"");

            return xml;
        }

        private void AddXmlAttribute(XmlDocument doc, XmlElement node, string name, string value)
        {
            var attr = doc.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);
        }
    }
}
