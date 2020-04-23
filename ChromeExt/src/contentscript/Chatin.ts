import * as $ from 'jquery';
import { as } from './as';
import { App } from './App';
import { Participant } from './Participant';

export class Chatin
{
    elem: HTMLElement;
    textElem: HTMLElement;
    sendElem: HTMLElement;
    closeElem: HTMLElement;

    constructor(private app: App, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-chatin n3q-shadow" data-translate="children" />')[0];
        this.elem.style.display = 'none';

        this.textElem = <HTMLInputElement>$('<input type="text" class="n3q-text" placeholder="Enter chat here..." data-translate="attr:placeholder:Client" />')[0];
        $(this.textElem).bind('keydown', ev =>
        {
            var keycode = (ev.keyCode ? ev.keyCode : (ev.which ? ev.which : ev.charCode));
            switch (keycode) {
                case 13:
                    this.sendChat();
                    return false;
                case 27:
                    this.setVisible(false);
                    ev.stopPropagation();
                    return false;
                default:
                    return true;
            }
        });
        this.elem.appendChild(this.textElem);

        this.sendElem = <HTMLSpanElement>$('<span class="n3q-button n3q-button-medium n3q-button-sendchat glyphicon glyphicon-forward" title="SendChat" data-translate="attr:title:Client" />')[0];
        $(this.sendElem).click(ev =>
        {
            this.sendChat();
        });
        this.elem.appendChild(this.sendElem);

        this.closeElem = <HTMLSpanElement>$('<span class="n3q-button n3q-button-overlay n3q-button-small n3q-button-close glyphicon glyphicon-remove" title="Close" data-translate="attr:title:Client" />')[0];
        $(this.closeElem).click(ev =>
        {
            $(this.elem).stop(true);
            this.setVisible(false);
            ev.stopPropagation();
        });
        this.elem.appendChild(this.closeElem);

        display.appendChild(this.elem);
        //hw notyet this.app.translateElem(this.elem);
    }

    sendChat(): void
    {
        var text: string = as.String($(this.textElem).val(), '');
        if (text != '') {
            this.participant.sendGroupChat(text);
            $(this.textElem).val('').focus();
        }
    }

    setVisible(visible: boolean): void
    {
        this.elem.style.display = visible ? 'block' : 'none';
        if (visible) {
            $(this.textElem).focus();
        }
    }

    toggle(): void
    {
        var visible = this.elem.style.display == 'block';
        this.setVisible(!visible);
    }
}
