import * as $ from 'jquery';
import 'webpack-jquery-ui';
import { xml, jid } from '@xmpp/client';
import log = require('loglevel');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { ContentApp } from './ContentApp';
import { Window } from './Window';
import { _Changes } from './_Changes';

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
        options.titleText = this.app.translateText('Console.Console', 'Console');
        options.resizable = true;

        super.show(options);

        let bottom = as.Int(options.bottom, 400);
        let width = as.Int(options.width, 600);
        let height = as.Int(options.height, 600);
        let onClose = options.onClose;

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-xmppwindow');
            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px' });

            let outElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-out" data-translate="children" />').get(0);
            let inElem = <HTMLElement>$('<div class="n3q-base n3q-xmppwindow-in" data-translate="children" />').get(0);
            let inInputElem = <HTMLElement>$('<textarea class="n3q-base n3q-xmppwindow-in-input n3q-input n3q-text" />').get(0);
            let inSendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-xmppwindow-in-send" title="Send">Send</div>').get(0);
            let inSaveElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-xmppwindow-in-save" title="Save">Save</div>').get(0);
            let outClearElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-xmppwindow-out-clear" title="Clear">Clear</div>').get(0);

            $(outElem).append(outClearElem);

            $(inElem).append(inInputElem);
            $(inElem).append(inSendElem);
            $(inElem).append(inSaveElem);

            $(contentElem).append(outElem);
            $(contentElem).append(inElem);

            this.app.translateElem(windowElem);

            this.chatinInputElem = inInputElem;
            this.chatoutElem = outElem;

            {
                let left = 50;
                let top = this.display.offsetHeight - height - bottom;
                $(windowElem).css({ left: left + 'px', top: top + 'px' });
            }

            this.fixChatInTextWidth(inInputElem, 38, inElem);

            this.onResize = (ev: JQueryEventObject) =>
            {
                this.fixChatInTextWidth(inInputElem, 38, inElem);
                // $(chatinText).focus();
            };

            $(inSendElem).click(ev =>
            {
                this.sendText();
            });

            $(inSaveElem).click(ev =>
            {
                this.saveText();
            });

            $(outClearElem).click(ev =>
            {
                $('#n3q .n3q-xmppwindow-out .n3q-xmppwindow-line').remove();
            });

            this.onClose = async () =>
            {
                await this.saveText();

                this.chatoutElem = null;
                this.chatinInputElem = null;

                if (onClose) { onClose(); }
            };

            this.onDragStop = (ev: JQueryEventObject) =>
            {
                // $(chatinText).focus();
            };

            this.setText(await this.getStoredText());

            this.showHistory();

            $(inInputElem).focus();
        }
    }

    showHistory()
    {
        _Changes.getLines().reverse().forEach(line => { this.showLine('', line); });
    }

    fixChatInTextWidth(chatinText: HTMLElement, delta: number, chatin: HTMLElement)
    {
        let parentWidth = chatin.offsetWidth;
        let width = parentWidth - delta;
        $(chatinText).css({ 'width': width });
    }

    private sendText(): void
    {
        this.saveText();

        let text = this.getSelectedText();
        if (text == '') {
            text = this.getText();
        }
        if (text != '') {
            try {
                let stanza = this.test2Stanza(text);
                this.app.sendStanza(stanza);
            } catch (error) {
                this.showError(error.message);
            }
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

    async saveText()
    {
        await this.storeText(this.getText());
    }

    async storeText(text: string)
    {
        await Config.setSync('dev.scratchPad', text);
    }

    async getStoredText(): Promise<string>
    {
        return await Config.getSync('dev.scratchPad', '');
    }

    test2Stanza(text: string): xml
    {
        let json = $.parseJSON(text);
        let stanza = Utils.jsObject2xmlObject(json);
        return stanza;
    }

    private label_errpr = 'error';
    public showLine(label: string, text: string)
    {
        let lineElem = <HTMLElement>$(
            `<div class="n3q-base n3q-xmppwindow-line` + (label == this.label_errpr ? ' n3q-xmppwindow-line-error' : '') + `">
                <span class="n3q-base n3q-text n3q-xmppwindow-label">` + as.Html(label) + `</span>
                <span class="n3q-base n3q-text n3q-xmppwindow-text">`+ as.Html(text) + `</span>
            <div>`
        ).get(0);

        if (this.chatoutElem) {
            $(this.chatoutElem).append(lineElem).scrollTop($(this.chatoutElem).get(0).scrollHeight);
        }
    }

    public showError(text: string)
    {
        this.showLine(this.label_errpr, text);
    }
}
