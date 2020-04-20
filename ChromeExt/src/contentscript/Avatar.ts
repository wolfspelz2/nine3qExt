const $ = require('jquery');
import { App } from './App';
import { Entity } from './Entity';
import { Platform } from './Platform';
import { IObserver, IObservable } from './ObservableProperty';

import imgDefaultAvatar from '../assets/DefaultAvatar.png';

export class Avatar implements IObserver
{
    private elem: HTMLImageElement;
    private imageUrl: string;
    private currentState: string = '';
    private hasAnimation = false;
    private animationsUrl: string;
    private animationsUrlBase: string;

    constructor(private app: App, private entity: Entity, private display: HTMLElement)
    {
        this.elem = <HTMLImageElement>$('<img class="n3q-avatar" />')[0];
        // var url = 'https://www.virtual-presence.org/images/wolf.png';
        // var url = app.getAssetUrl('default-avatar.png');
        var url = imgDefaultAvatar;
        this.elem.src = url;
        display.appendChild(this.elem);
    }

    update(key: string, value: any): void
    {
        switch (key) {
            case 'ImageUrl': {
                if (!this.hasAnimation) {
                    this.setImage(value);
                }
            } break;
            case 'AnimationsUrl': {
                // this.hasAnimation = true;
            } break;
        }
    }

    setImage(url: string): void
    {
        this.imageUrl = url;
        this.elem.src = this.imageUrl;
    }

    setAnimations(url: string): void
    {
        this.animationsUrl = url;
        var idx = url.lastIndexOf('/');
        this.animationsUrlBase = url.substring(0, idx + 1);

        // jQuery.getJSON(this.app.getAnimationsProxyUrl() + '?url=' + encodeURI(url), data => this.onAnimationData(data));
        Platform.fetchUrl(url, (ok, status, statusText, data) =>
        {
            if (ok) {
                // this.onAnimationData(data);
            }
        });
    }

    onAnimationData(data: any[]): void
    {
        // this.animations = data;
        // this.defaultGroup = this.getDefaultGroup();

        // //this.currentAction = 'wave';
        // //this.currentState = 'moveleft';

        // this.startNextAnimation();
    }

    // public async Task<string> AnimationsXml2Json(string url, string xml)
    // {
    //     var doc = new XmlDocument();
    //     doc.LoadXml(xml);
    //     var nodes = doc.DocumentElement.ChildNodes;

    //     var json = new JsonTree.Node(JsonTree.Node.Type.List);

    //     var keys = new Dictionary<string, List<string>> {
    //         { "param", new List<string> { "name", "value" }},
    //         { "sequence", new List<string> { "group", "name", "type", "probability", "in", "out" }},
    //     };

    //     foreach (XmlNode node in nodes) {
    //         if (keys.ContainsKey(node.Name)) {

    //             var dict = new JsonTree.Node(JsonTree.Node.Type.Dictionary);
    //             foreach (var key in keys[node.Name]) {
    //                 var attr = node.Attributes.GetNamedItem(key);
    //                 if (attr != null) {
    //                     dict.AsDictionary.Add(attr.Name, new JsonTree.Node(JsonTree.Node.Type.String, attr.Value));
    //                 }
    //             }

    //             if (node.Name == "sequence") {
    //                 var src = node.FirstChild.Attributes["src"].Value;
    //                 dict.AsDictionary.Add("src", new JsonTree.Node(JsonTree.Node.Type.String, src));

    //                 var srcUrl = src;
    //                 if (!srcUrl.StartsWith("http")) {
    //                     var idx = url.LastIndexOf('/');
    //                     var baseUrl = url.Substring(0, idx + 1);
    //                     srcUrl = baseUrl + src;
    //                 }
    //                 var stream = await new HttpClient().GetStreamAsync(srcUrl);
    //                 var image = Image.FromStream(stream);
    //                 var decoder = new GifDecoder(image);
    //                 var duration = decoder.AnimationLength;
    //                 dict.AsDictionary.Add("duration", new JsonTree.Node(JsonTree.Node.Type.String, duration.ToString(CultureInfo.InvariantCulture)));
    //                 var loop = decoder.IsLooped;
    //                 dict.AsDictionary.Add("loop", new JsonTree.Node(JsonTree.Node.Type.String, loop ? "true" : "false"));
    //             }

    //             var item = new JsonTree.Node(JsonTree.Node.Type.Dictionary);
    //             item.AsDictionary.Add(node.Name, dict);
    //             json.AsArray.Add(item);
    //         }
    //     }

    //     var jsonText = json.ToJson();
    //     return jsonText;
    // }

    setState(state: string): void
    {
        this.currentState = state;
        // this.startNextAnimation();
    }
}
