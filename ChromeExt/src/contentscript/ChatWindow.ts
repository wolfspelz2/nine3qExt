import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp } from './ContentApp';
import { Room } from './Room';
import { Window } from './Window';
import { ChatConsole } from './ChatConsole';

class ChatLine
{
    constructor(public nick: string, public text: string)
    {
    }
}

export class ChatWindow extends Window
{
    private chatoutElem: HTMLElement;
    private chatinInputElem: HTMLElement;
    private lines: Record<string, ChatLine> = {};

    constructor(app: ContentApp, private room: Room)
    {
        super(app);

        if (Environment.isDevelopment()) {
            this.addLine('1', 'Nickname', 'Lorem');
            this.addLine('2', 'ThisIsALongerNickname', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.');
            this.addLine('3', 'Long name with intmediate spaces', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum');
            this.addLine('4', 'Long text no spaces', 'mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm');
        }
    }

    show(options: any)
    {
        options.titleText = this.app.translateText('Chatwindow.Chat History', 'Chat');
        options.resizable = true;

        super.show(options);

        let aboveElem: HTMLElement = options.above;
        let bottom = as.Int(options.bottom, 200);
        let width = as.Int(options.width, 400);
        let height = as.Int(options.height, 300);

        if (this.windowElem) {
            let windowElem = this.windowElem;
            let contentElem = this.contentElem;
            $(windowElem).addClass('n3q-chatwindow');

            let left = 50;
            if (aboveElem) {
                left = aboveElem.offsetLeft - 180;
                if (left < 0) { left = 0; }
            }
            let top = this.app.getDisplay().offsetHeight - height - bottom;
            {
                let minTop = 10;
                if (top < minTop) {
                    height -= minTop - top;
                    top = minTop;
                }
            }

            let chatoutElem = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-chatout" data-translate="children" />').get(0);
            let chatinElem = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-chatin" data-translate="children" />').get(0);
            let chatinTextElem = <HTMLElement>$('<input type="text" class="n3q-base n3q-chatwindow-chatin-input n3q-input n3q-text" rows="1" placeholder="Enter chat here..." data-translate="attr:placeholder:Chatin" />').get(0);
            let chatinSendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-inline" title="SendChat" data-translate="attr:title:Chatin"><div class="n3q-base n3q-button-symbol n3q-button-sendchat" />').get(0);

            $(chatinElem).append(chatinTextElem);
            $(chatinElem).append(chatinSendElem);

            $(contentElem).append(chatoutElem);
            $(contentElem).append(chatinElem);

            this.app.translateElem(windowElem);

            this.chatinInputElem = chatinTextElem;
            this.chatoutElem = chatoutElem;

            $(windowElem).css({ 'width': width + 'px', 'height': height + 'px', 'left': left + 'px', 'top': top + 'px' });

            this.fixChatInTextWidth(chatinTextElem, chatinElem);

            this.onResize = (ev: JQueryEventObject) =>
            {
                this.fixChatInTextWidth(chatinTextElem, chatinElem);
                // $(chatinText).focus();
            };

            $(chatinTextElem).on('keydown', ev =>
            {
                return this.onChatinKeydown(ev);
            });

            $(chatinSendElem).click(ev =>
            {
                this.sendChat();
                ev.stopPropagation();
            });

            this.onClose = () =>
            {
                this.chatoutElem = null;
                this.chatinInputElem = null;
            };

            this.onDragStop = (ev: JQueryEventObject) =>
            {
                // $(chatinText).focus();
            };

            for (let id in this.lines) {
                let line = this.lines[id];
                this.showLine(line.nick, line.text);
            }

            $(chatinTextElem).focus();
        }
    }

    isOpen(): boolean
    {
        return this.windowElem != null;
    }

    fixChatInTextWidth(chatinText: HTMLElement, chatin: HTMLElement)
    {
        let delta = 14;
        let parentWidth = chatin.offsetWidth;
        let width = parentWidth - delta;
        $(chatinText).css({ 'width': width });
    }

    addLine(id: string, nick: string, text: string)
    {
        let translated = this.app.translateText('Chatwindow.' + text, text);

        // // Beware: without markdown in showLine: as.Html(text)
        // let markdowned = markdown.markdown.toHTML(translated);
        // let line = new ChatLine(nick, markdowned);

        let line = new ChatLine(nick, translated);
        if (this.lines[id] == undefined) {
            this.lines[id] = line;
            this.showLine(line.nick, line.text);
        }
    }

    private showLine(nick: string, text: string)
    {
        let lineElem = <HTMLElement>$(
            `<div class="n3q-base n3q-chatwindow-line">
                <span class="n3q-base n3q-text n3q-nick">`+ as.Html(nick) + `</span>
                <span class="n3q-base n3q-text n3q-chat">`+ as.Html(text) + `</span>
            <div>`
        ).get(0);

        if (this.chatoutElem) {
            $(this.chatoutElem).append(lineElem).scrollTop($(this.chatoutElem).get(0).scrollHeight);
        }
    }

    private onChatinKeydown(ev: JQuery.Event): boolean
    {
        var keycode = (ev.keyCode ? ev.keyCode : (ev.which ? ev.which : ev.charCode));
        switch (keycode) {
            case 13:
                this.sendChat();
                return false;
            case 27:
                this.close();
                ev.stopPropagation();
                return false;
            default:
                return true;
        }
    }

    private sendChat(): void
    {
        var text: string = as.String($(this.chatinInputElem).val(), '');
        if (text != '') {

            let handledByChatCommand = false;
            try {
                handledByChatCommand = ChatConsole.isChatCommand(text, {
                    app: this.app,
                    room: this.room,
                    out: (data) =>
                    {
                        if (typeof data == typeof '') {
                            this.showLine('', data[0]);
                        } else if (Array.isArray(data)) {
                            if (Array.isArray(data[0])) {
                                let text = '';
                                data.forEach(line =>
                                {
                                    this.showLine(line[0], line[1]);
                                });
                            } else {
                                this.showLine(data[0], data[1]);
                            }
                        }
                    }
                });
            } catch (error) {
                //
            }

            if (handledByChatCommand) {
                $(this.chatinInputElem).val('');
                return;
            }

            this.room?.sendGroupChat(text);

            $(this.chatinInputElem)
                .val('')
                .focus()
                ;
        }
    }
}
