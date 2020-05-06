import * as $ from 'jquery';
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';

export class Chatin
{
    private elem: HTMLElement;
    private chatinInputElem: HTMLElement;
    private sendElem: HTMLElement;
    private closeElem: HTMLElement;

    constructor(private app: ContentApp, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLElement>$('<div class="n3q-base n3q-chatin n3q-shadow" data-translate="children" />').get(0);
        this.setVisibility(false);

        this.chatinInputElem = <HTMLElement>$('<input type="text" class="n3q-base n3q-input n3q-text" placeholder="Enter chat here..." data-translate="attr:placeholder:Chatin" />').get(0);
        $(this.chatinInputElem).on('keydown', ev => { this.onKeydown(ev); });
        this.elem.appendChild(this.chatinInputElem);

        this.sendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-sendchat" title="SendChat" data-translate="attr:title:Chatin" />').get(0);
        $(this.sendElem).click(ev =>
        {
            this.sendChat();
        });
        this.elem.appendChild(this.sendElem);

        this.closeElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-overlay n3q-shadow" title="Close" data-translate="attr:title:Common"><div class="n3q-base n3q-button-close-small" />').get(0);
        $(this.closeElem).click(ev =>
        {
            $(this.elem).stop(true);
            this.setVisibility(false);
            ev.stopPropagation();
        });
        this.elem.appendChild(this.closeElem);

        this.app.translateElem(this.elem);
        display.appendChild(this.elem);
    }

    onKeydown(ev: JQuery.Event)
    {
        var keycode = (ev.keyCode ? ev.keyCode : (ev.which ? ev.which : ev.charCode));
        switch (keycode) {
            case 13:
                this.sendChat();
                return false;
            case 27:
                this.setVisibility(false);
                ev.stopPropagation();
                return false;
            default:
                return true;
        }
    }

    sendChat(): void
    {
        var text: string = as.String($(this.chatinInputElem).val(), '');
        if (text != '') {
            this.participant.sendGroupChat(text);
            $(this.chatinInputElem).val('').focus();
        }
    }

    // Visibility

    setVisibility(visible: boolean): void
    {
        this.isVisible = visible;
        if (visible) {
            $(this.elem).removeClass('n3q-hidden');
            $(this.chatinInputElem).focus();
        } else {
            $(this.elem).addClass('n3q-hidden');
        }
    }

    private isVisible = true;
    toggleVisibility(): void
    {
        this.setVisibility(!this.isVisible);
    }
}
