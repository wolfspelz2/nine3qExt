import './contentscript.scss';
const $ = require('jquery');
import { Log } from './Log';
import { App } from './App';
import { Platform } from './Platform';

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElement: HTMLElement = $('<div id="n3q-id-page" />')[0];

// var logElement: HTMLElement = $('<div class="n3q-log" />')[0];
// pageElement.append(logElement);

$('body').append(pageElement);

new App(pageElement).start();

let data : string = '<?xml version="1.0" encoding="UTF-8"?>\n<!DOCTYPE identity-xml [\n<!ATTLIST item id ID #IMPLIED>\n]>\n<identity xmlns="http://schema.bluehands.de/digest-container" digest="49129562173829b47fbdf856b222ef3b7dabb25d">\n<item id="avatar" contenttype="avatar" digest="86adeaada15ea4f2d7d11cd06dfe356d37626abl" src="https://files.zweitgeist.com/3b/79/18/0016b364f66c3e2ea5a407ea7d61b6995e.gif" order="1"/>\n<item id="avatar2" contenttype="avatar2" digest="1" src="https://storage.zweitgeist.com/index.php/295/avatar2" mimetype="avatar/gif" order="1"/>\n<item id="profilepage" contenttype="profilepage" digest="bea72819090fcc3eda2153341a717732bf43e25f" src="https://www.weblin.com/profile.php?cid=295" order="1"/>\n<item id="properties" contenttype="properties" digest="70df0a6021cc20ff2192bada3001f10c06bd2ghh" encoding="plain" mimetype="text/plain" order="1"><![CDATA[KickVote=true\nNickname=Planta Velociä\nGender=male\nProfilePage=https://www.weblin.com/profile.php?cid=295\nNicknameLink=http://www.virtual-presence.org\nPoints=10430\nPoints.Check=27395d80656f560cf99b13a4e9e1157b87e8d60b\nCommunityID=zweitgeist\nCommunityFaction=_default\nCommunityTag=https://files.zweitgeist.com/cf/c5/57/d9bb67aa26f67b648de24fe1f8fd4525b7.png\nCommunityName=Planta\'s Weblog\nCommunityPage=http://blog.wolfspelz.de/\n]]></item>\n<Signature xmlns="http://www.w3.org/2000/09/xmldsig#"><SignedInfo><CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"></CanonicalizationMethod><SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1"></SignatureMethod><Reference URI="#avatar"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"></Transform></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"></DigestMethod><DigestValue>XaGomLa3xpEvyuYun0wV/MCe5kI=</DigestValue></Reference><Reference URI="#avatar2"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"></Transform></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"></DigestMethod><DigestValue>b7RDuY+gEy2misGo3vNysw4U0YQ=</DigestValue></Reference><Reference URI="#profilepage"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"></Transform></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"></DigestMethod><DigestValue>xcsGCazIVQZ+geSMsEPAzKSaOZY=</DigestValue></Reference><Reference URI="#properties"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"></Transform></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"></DigestMethod><DigestValue>x+Een8d1ObWusPBzcuLy1xanLSU=</DigestValue></Reference></SignedInfo><SignatureValue>mPQFPRSfnio1bj+veFNPXgM4iofbPLmCswyzyErf1jFVkWjtxkttZ7J3r638zd04mfeKjReuCF96dwsLQyDRYsNUqODl0H3sBFeaw0tmHA3fC3+eNezeGvZEiaPZc/wMGQfsXgdobsSjv5gRpYgwMcuF/CbuwDDW+IRAr7Zkt8s=</SignatureValue></Signature></identity>'; 
let doc = $.parseXML(data);
console.log(doc);