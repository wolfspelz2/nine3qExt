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
        public ICallbackLogger Log { get; set; }

        public IdentityController(ILogger<IdentityController> logger)
        {
            Log = new FrameworkCallbackLogger(logger);
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

            Log.Info($"{digest} {nickname} {avatarUrl} {imageUrl}");

            if (!Has.Value(imageUrl)) {
                var idx = avatarUrl.LastIndexOf("/");
                if (idx >= 0) {
                    imageUrl = avatarUrl.Substring(0, idx) + "/idle.gif";
                }
            }

            var useImageUrl = Has.Value(imageUrl) && imageUrl != "{imageUrl}";

            var digest_XmlEncoded = WebUtility.HtmlEncode(digest);
            var imageUrl_XmlEncoded = WebUtility.HtmlEncode(imageUrl);
            var avatarUrl_XmlEncoded = WebUtility.HtmlEncode(avatarUrl);
            var nickname_XmlEncoded = WebUtility.HtmlEncode(nickname);

#pragma warning disable format
            var xml =
$@"<?xml version='1.0' encoding='UTF-8'?>
<!DOCTYPE identity-xml>
<identity xmlns='http://schema.bluehands.de/digest-container' digest='{digest_XmlEncoded}'>
"

+ (useImageUrl ? $@"    <item id='avatar' contenttype='avatar' digest='{digest_XmlEncoded}' src='{imageUrl_XmlEncoded}' order='1'/>
" : "")

+$@"    <item id='avatar2' contenttype='avatar2' digest='{digest_XmlEncoded}' src='{avatarUrl_XmlEncoded}' mimetype='avatar/gif' order='1'/>
    <item id='properties' contenttype='properties' digest='{digest_XmlEncoded}' encoding='plain' mimetype='text/plain' order='1'><![CDATA[Nickname={nickname_XmlEncoded}]]></item>
</identity>
"
.Replace("'", "\"");
#pragma warning restore format

            return xml;
        }

        //private void AddXmlAttribute(XmlDocument doc, XmlElement node, string name, string value)
        //{
        //    var attr = doc.CreateAttribute(name);
        //    attr.Value = value;
        //    node.Attributes.Append(attr);
        //}
    }
}
