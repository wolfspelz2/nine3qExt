import * as $ from 'jquery';
import 'webpack-jquery-ui';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Window } from './Window';

export class XmppWindow extends Window
{
    private chatoutElem: HTMLElement;
    private chatinInputElem: HTMLElement;

    constructor(app: ContentApp, display: HTMLElement)
    {
        super(app, display);
    }

    async show(options: any)
    {
        options.titleText = this.app.translateText('XmppConsole.Xmpp Console', 'Xmpp Console');
        options.resizable = true;

        super.show(options);

        let bottom = as.Int(options.bottom, 300);
        let width = as.Int(options.width, 600);
        let height = as.Int(options.height, 600);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-xmppwindow');
            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px' });

            let chatoutElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-chatout" data-translate="children" />').get(0);
            let chatinElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-chatin" data-translate="children" />').get(0);
            let chatinInputElem = <HTMLElement>$('<textarea class="n3q-base n3q-xmppwindow-chatin-input n3q-input n3q-text" />').get(0);
            let chatinSendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-inline" title="SendXml"><div class="n3q-base n3q-button-symbol n3q-button-sendchat" />').get(0);

            $(chatinElem).append(chatinInputElem);
            $(chatinElem).append(chatinSendElem);

            $(contentElem).append(chatoutElem);
            $(contentElem).append(chatinElem);

            this.app.translateElem(windowElem);

            this.chatinInputElem = chatinInputElem;
            this.chatoutElem = chatoutElem;

            {
                let left = 50;
                let top = this.display.offsetHeight - height - bottom;
                $(windowElem).css({ left: left + 'px', top: top + 'px' });
            }

            this.fixChatInTextWidth(chatinInputElem, 38, chatinElem);

            this.onResize = (ev: JQueryEventObject) =>
            {
                this.fixChatInTextWidth(chatinInputElem, 38, chatinElem);
                // $(chatinText).focus();
            };

            $(chatinSendElem).click(ev =>
            {
                this.sendText();
            });

            this.onClose = async () =>
            {
                await this.saveText(this.getText());

                this.chatoutElem = null;
                this.chatinInputElem = null;
            };

            this.onDragStop = (ev: JQueryEventObject) =>
            {
                // $(chatinText).focus();
            };

            this.setText(await this.getSavedText());

            $(chatinInputElem).focus();
        }
    }

    fixChatInTextWidth(chatinText: HTMLElement, delta: number, chatin: HTMLElement)
    {
        let parentWidth = chatin.offsetWidth;
        let width = parentWidth - delta;
        $(chatinText).css({ 'width': width });
    }

    private showLine(text: string)
    {
        let lineElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-line">' + as.Html(text) + '<div>').get(0);
        if (this.chatoutElem) {
            $(this.chatoutElem).append(lineElem).scrollTop($(this.chatoutElem).get(0).scrollHeight);
        }
    }

    private showError(text: string)
    {
        let lineElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-line n3q-xmppwindow-line-error">' + as.Html(text) + '<div>').get(0);
        if (this.chatoutElem) {
            $(this.chatoutElem).append(lineElem).scrollTop($(this.chatoutElem).get(0).scrollHeight);
        }
    }

    private setText(text: string): void
    {
        $(this.chatinInputElem).val(text);
    }

    getText(): string
    {
        return as.String($(this.chatinInputElem).val(), '');
    }

    getSelectedText(): string
    {
        var txtarea = <HTMLTextAreaElement>this.chatinInputElem;
        var start = txtarea.selectionStart;
        var finish = txtarea.selectionEnd;
        var selectedText = this.getText().substring(start, finish);
        return selectedText;
    }

    private sendText(): void
    {
        let text = this.getSelectedText();
        if (text == '') {
            text = this.getText();
        }
        if (text != '') {
            this.saveText(text);

            try {
                let stanza = this.test2Stanza(text);
                this.app.sendStanza(stanza);
            } catch (error) {
                this.showError(error.message);
            }
        }
    }

    async saveText(text: string)
    {
        await Config.setSync('dev.xmppWindow', text);
    }

    async getSavedText(): Promise<string>
    {
        return await Config.getSync('dev.xmppWindow', '');
    }

    test2Stanza(text: string): xml
    {
        let json = $.parseJSON(text);
        let stanza = Utils.jsObject2xmlObject(json);
        return stanza;
    }
}
