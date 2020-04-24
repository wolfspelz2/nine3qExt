import * as $ from 'jquery';
import { as } from './as';
import { App } from './App';
import { Participant } from './Participant';

export class Chatout
{
    private elem: HTMLElement;
    private textElem: HTMLElement;
    private closeElem: HTMLElement;

    constructor(private app: App, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-chatout" />')[0];
        this.elem.style.display = 'none';
        $(this.elem).click(() =>
        {
            $(this.elem).stop(true).fadeTo('fast', 1);
            this.participant.select();
        });

        var speechBubble = <HTMLDivElement>$('<div class="n3q-speech n3q-shadow" />')[0];

        this.textElem = <HTMLParagraphElement>$('<p class="n3q-text" />')[0];
        speechBubble.appendChild(this.textElem);
        this.elem.appendChild(speechBubble);

        this.closeElem = <HTMLElement>$('<div class="n3q-button n3q-button-overlay n3q-button-close-small" title="Close" data-translate="attr:title:Client" />')[0];
        $(this.closeElem).click(ev =>
        {
            $(this.elem).stop(true);
            this.setVisible(false);
            ev.stopPropagation();
        });
        this.elem.appendChild(this.closeElem);
        //hw later this.app.translateElem(this.closeElem);

        display.appendChild(this.elem);
    }

    setText(text: string): void
    {
        if (text == null || text == '') {
            return;
        }

        $(this.elem).stop(true).fadeTo('fast', 1);

        $(this.textElem).html(as.Html(text));

        this.elem.style.display = 'block';
        //$(this.elem).delay(10000).fadeOut(10000);
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
