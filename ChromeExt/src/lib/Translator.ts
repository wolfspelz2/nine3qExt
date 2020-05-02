import * as $ from 'jquery';
import { Config } from './Config';
import { Utils } from './Utils';

interface ITranslationResponse
{
    key: string;
    lang: string;
    translatedText: string;
    isTranslated: boolean;
    timestamp: number;
}

class HtmlElementPartApplier
{
    constructor(public elem: HTMLElement, public what: string) { }
    apply(translated: string): void { }
}

class HtmlElementPartAttributeApplier extends HtmlElementPartApplier
{
    constructor(public elem: HTMLElement, public what: string, public attrName: string) { super(elem, what); }

    apply(translated: string): void
    {
        $(this.elem).attr(this.attrName, translated);
    }
}

class HtmlElementTextPartApplier extends HtmlElementPartApplier
{
    constructor(public elem: HTMLElement, public what: string) { super(elem, what); }

    apply(translated: string): void
    {
        $(this.elem).text(translated);
    }
}

export class Translator
{
    translationStatus: { [id: string]: boolean; } = {};

    constructor(private translations: any, private language: string, private translationService: string)
    {
    }

    translateElem(elem: HTMLElement): void
    {
        var translate: string = $(elem).data('translate');
        if (translate != null) {

            var parts = translate.split(' ');
            for (var i = 0; i < parts.length; i++) {
                var cmd = parts[i];
                var cmdParts = cmd.split(':');
                var what = cmdParts[0];
                var applier: HtmlElementPartApplier = null;

                switch (what) {
                    case 'attr':
                        var attrName = cmdParts[1];
                        var context = cmdParts[2];
                        var text = $(elem).attr(attrName);
                        if (text != null && text != '') {
                            var key: string = context + '.' + text;
                            applier = new HtmlElementPartAttributeApplier(elem, what, attrName);
                        }
                        break;

                    case 'text':
                        if ($(elem).children().length == 0) {
                            var context = cmdParts[1];
                            var text = $(elem).text();
                            if (text != null && text != '') {
                                var key: string = context + '.' + text;
                                applier = new HtmlElementTextPartApplier(elem, what);
                            }
                        }
                        break;

                    case 'children':
                        $(elem).children().each((index, child) => this.translateElem(<HTMLElement>child));
                        break;
                }

                if (applier != null) {
                    if (this.translations[key] != undefined) {
                        this.translationStatus[key] = true;
                        this.applyTranslation(applier, this.translations[key], true);
                    } else {
                        if (this.translationStatus[key] == undefined) {
                            if (this.translationService != undefined && this.translationService != null && this.translationService != '') {
                                var url = this.translationService + '?lang=' + encodeURI(this.language) + '&key=' + encodeURI(key);
                                jQuery.getJSON(url, data =>
                                {
                                    if (data.translatedText != undefined) {
                                        var response: ITranslationResponse = <ITranslationResponse>data;
                                        this.translationStatus[key] = response.isTranslated;
                                        if (response.isTranslated) {
                                            this.translations[key] = response.translatedText;
                                        }
                                        this.applyTranslation(applier, response.translatedText, response.isTranslated);
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    applyTranslation(applier: HtmlElementPartApplier, translatedText: string, isTranslated: boolean): void
    {
        applier.apply(translatedText);

        if (isTranslated) {
            var ontranslate: string = $(applier.elem).data('ontranslate');
            if (ontranslate != null) {
                var sepIndex = ontranslate.indexOf(':');
                var what = ontranslate.substr(0, sepIndex);
                var detail = ontranslate.substr(sepIndex + 1);

                switch (what) {
                    case 'show':
                        var selector = detail;
                        $(selector).removeClass('n3q-dim');//$(selector).toggle(true);
                        break;

                    case 'eval':
                        var code = detail;
                        eval(code);
                        break;

                }
            }
        }
    }
}
