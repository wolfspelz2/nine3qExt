import * as $ from 'jquery';
import { as } from '../lib/as';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';

export class Chatin
{
    elem: HTMLElement;
    textElem: HTMLElement;
    sendElem: HTMLElement;
    closeElem: HTMLElement;

    constructor(private app: ContentApp, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLElement>$('<div class="n3q-base n3q-chatin n3q-shadow" data-translate="children" />').get(0);
        this.setVisibility(false);

        this.textElem = <HTMLElement>$('<input type="text" class="n3q-base n3q-input n3q-text" style="position: relative; line-height: 14px; padding: 4px; outline: none; font-family: Arial, sans-serif; font-size: 11px; font-weight: normal; font-style: normal; border: none; box-shadow: unset;" placeholder="Enter chat here..." data-translate="attr:placeholder:Chatin" />').get(0);
        $(this.textElem).bind('keydown', ev =>
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
        });
        this.elem.appendChild(this.textElem);

        this.sendElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-sendchat" title="SendChat" data-translate="attr:title:Chatin" />').get(0);
        $(this.sendElem).click(ev =>
        {
            this.sendChat();
        });
        this.elem.appendChild(this.sendElem);

        this.closeElem = <HTMLElement>$('<div class="n3q-base n3q-button n3q-button-overlay n3q-button-close-small" title="Close" data-translate="attr:title:Common" />').get(0);
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

    sendChat(): void
    {
        var text: string = as.String($(this.textElem).val(), '');
        if (text != '') {
            this.participant.sendGroupChat(text);
            $(this.textElem).val('').focus();
        }
    }

    // Visibility

    setVisibility(visible: boolean): void
    {
        this.isVisible = visible;
        if (visible) {
            $(this.elem).removeClass('n3q-hidden');
            $(this.textElem).focus();
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
