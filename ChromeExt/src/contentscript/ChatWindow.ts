import * as $ from 'jquery';
import 'webpack-jquery-ui';
import 'webpack-jquery-ui/css';
import 'webpack-jquery-ui/dialog';
// import markdown = require('markdown');
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp } from './ContentApp';
import { Room } from './Room';

class ChatLine
{
    private contentElem: HTMLElement;
    private windowElem: HTMLElement;

    constructor(public nick: string, public text: string)
    {
    }
}

export class ChatWindow
{
    private windowElem: HTMLElement;
    private chatoutElem: HTMLElement;
    private chatinInputElem: HTMLElement;
    private lines: Record<string, ChatLine> = {};

    constructor(private app: ContentApp, private display: HTMLElement, private room: Room)
    {
        if (Environment.isDevelopment()) {
            this.addLine('1', 'Nickname', 'Lorem');
            this.addLine('2', 'ThisIsALongerNickname', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.');
            this.addLine('3', 'Long name with intmediate spaces', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum');
        }
    }

    show()
    {
        if (!this.windowElem) {
            let window = <HTMLElement>$('<div class="n3q-base n3q-window n3q-chatwindow n3q-shadow-medium" data-translate="children" />').get(0);
            let titleBar = <HTMLElement>$('<div class="n3q-base n3q-window-title-bar" data-translate="children" />').get(0);
            let title = <HTMLElement>$('<div class="n3q-base n3q-window-title" data-translate="text:Chatwindow">Chat History</div>').get(0);
            let close = <HTMLElement>$(
                `<div class="n3q-base n3q-window-button" title="Close" data-translate="attr:title:Common">
                    <div class="n3q-base n3q-button-symbol n3q-button-close" />
                </div>`
            ).get(0);
            let content = <HTMLElement>$('<div class="n3q-base n3q-window-content" data-translate="children" />').get(0);
            let chatout = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-chatout" data-translate="children" />').get(0);
            let chatin = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-chatin" data-translate="children" />').get(0);
            let chatinText = <HTMLElement>$('<textarea class="n3q-base n3q-chatwindow-chatin-input n3q-input n3q-text" rows="1" placeholder="Enter chat here..." data-translate="attr:placeholder:Chatin" />').get(0);
            let chatinSend = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-inline" title="SendChat" data-translate="attr:title:Chatin"><div class="n3q-base n3q-button-symbol n3q-button-sendchat" />').get(0);

            $(titleBar).append(title);
            $(titleBar).append(close);
            $(window).append(titleBar);

            $(chatin).append(chatinText);
            $(chatin).append(chatinSend);

            $(content).append(chatout);
            $(content).append(chatin);

            $(window).append(content);

            this.app.translateElem(window);

            $(chatinText).on('keydown', ev =>
            {
                this.onKeydown(ev);
            });

            $(chatinSend).click(ev =>
            {
                this.sendChat();
            });

            $(close).click(ev =>
            {
                this.close();
            });

            $(window).draggable({
                scroll: false,
                stack: '.n3q-entity',
                // opacity: 0.5,
                distance: 4,
                containment: 'document',
            });

            this.chatinInputElem = chatinText;
            this.chatoutElem = chatout;
            this.windowElem = window;

            for (let id in this.lines) {
                let line = this.lines[id];
                this.showLine(line.nick, line.text);
            }

            $(this.display).append(window);
        }

        this.pushToTop();
    }

    addLine(id: string, nick: string, text: string)
    {
        let translated = this.app.translateText(text, 'Chatwindow.' + text);

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

        if (this.chatoutElem != null) {
            $(this.chatoutElem).append(lineElem).scrollTop($(this.chatoutElem).get(0).scrollHeight);
        }
    }

    private onKeydown(ev: JQuery.Event)
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

    private sendChat()
    {
        var text: string = as.String($(this.chatinInputElem).val(), '');
        if (text != '') {
            this.room?.sendGroupChat(text);
            $(this.chatinInputElem).val('').focus();
        }
    }

    private onChatinKeydown(ev: JQuery.Event)
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

    private pushToTop()
    {
    }

    private close()
    {
        $(this.windowElem).remove();
        this.windowElem = null;
        this.chatoutElem = null;
        this.chatinInputElem = null;
    }
}
