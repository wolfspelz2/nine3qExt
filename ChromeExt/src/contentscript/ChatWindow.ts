import * as $ from 'jquery';
import 'webpack-jquery-ui';
import 'webpack-jquery-ui/css';
import 'webpack-jquery-ui/dialog';
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';

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
    private dialogElem: HTMLElement;
    private windowElem: HTMLElement;
    private chatoutElem: HTMLElement;
    private chatinInputElem: HTMLElement;
    private lines: Record<string, ChatLine> = {};

    constructor(private app: ContentApp, private display: HTMLElement, private participant: Participant, private isSelf: boolean)
    {
        if (Environment.isDevelopment()) {
            this.addLine('1', 'Nickname', 'Lorem');
            this.addLine('2', 'ThisIsALongerNickname', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.');
            this.addLine('3', 'Long name with intmediate spaces', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum');
        }
    }

    addLine(id: string, nick: string, text: string)
    {
        let translated = this.app.translateText(text, 'Chatwindow.' + text);

        let line = new ChatLine(nick, translated);
        if (this.lines[id] == undefined) {
            this.lines[id] = line;
            if (this.chatoutElem != null) {
                this.showLine(this.chatoutElem, nick, text);
            }
        }
    }

    private showLine(chatout: HTMLElement, nick: string, text: string)
    {
        let lineElem = <HTMLElement>$(
            `<div class="n3q-base n3q-chatwindow-line">
                        <span class="n3q-base n3q-nick">`+ nick + `</span>
                        <span class="n3q-base n3q-text">`+ text + `</span>
                    <div>`
        ).get(0);
        $(chatout).append(lineElem).scrollTop($(chatout).get(0).scrollHeight);
    }

    show()
    {
        if (this.windowElem == null) {
            {
                this.dialogElem = <HTMLElement>$('<div class="n3q-base n3q-chatwindow" title="Chat History" data-translate="attr:title:Chatwindow" />').get(0);
                this.chatoutElem = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-out" />').get(0);
                $(this.dialogElem).append(this.chatoutElem);
            }

            {
                let chatinWrapper = <HTMLElement>$('<div class="n3q-base n3q-chatwindow-in" data-translate="children" />').get(0);

                this.chatinInputElem = <HTMLElement>$('<input type="text" class="n3q-base n3q-input n3q-text" placeholder="Enter chat here..." data-translate="attr:placeholder:Chatin" />').get(0);
                $(this.chatinInputElem).on('keydown', ev => { this.onChatinKeydown(ev); });
                $(chatinWrapper).append(this.chatinInputElem);

                let sendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-sendchat" title="SendChat" data-translate="attr:title:Chatin" />').get(0);
                $(sendElem).click(ev =>
                {
                    this.sendChat();
                });
                $(chatinWrapper).append(sendElem);

                $(this.dialogElem).append(chatinWrapper);
            }

            this.app.translateElem(this.dialogElem);

            $(this.dialogElem).dialog({
                width: Config.get('chatWindowWidth', 400),
                height: Config.get('chatWindowHeight', 250),
                // maxHeight: Config.get('chatWindowMaxHeight', 800),
                // appendTo: '#n3q-id-page', // makes the dialog undraggable
            }).on('dialogclose', ev => { this.onClose(ev) });

            this.windowElem = $(this.dialogElem).parentsUntil(this.display).get(0);
            $(this.windowElem).addClass('n3q-ui-dialog');
            $(this.windowElem).addClass('n3q-shadow');

            for (let id in this.lines) {
                let line = this.lines[id];
                if (this.chatoutElem != null) {
                    this.showLine(this.chatoutElem, line.nick, line.text);
                }
            }
        }

        this.pushToTop();
    }

    private sendChat()
    {
        var text: string = as.String($(this.chatinInputElem).val(), '');
        if (text != '') {
            this.participant?.sendGroupChat(text);
            $(this.chatinInputElem).val('').focus();
        }
    }

    private close()
    {
        $(this.dialogElem).dialog('close');
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

    private onClose(ev: JQuery.Event)
    {
        this.dialogElem = null;
        this.windowElem = null;
        this.chatoutElem = null;
        this.chatinInputElem = null;
    }

    private pushToTop()
    {
    }
}
