import * as $ from 'jquery';
import { as } from '../lib/as';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';

export class Nickname implements IObserver
{
    private elem: HTMLDivElement;
    private textElem: HTMLElement;

    constructor(private app: ContentApp, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-nickname n3q-shadow" />')[0];
        $(this.elem).click(() => { this.participant.select(); });

        $(this.elem).on('mouseenter', ev => this.participant.onMouseEnterAvatar(ev));
        $(this.elem).on('mouseleave', ev => this.participant.onMouseLeaveAvatar(ev));

        this.textElem = <HTMLElement>$('<div class="n3q-base n3q-text" />')[0];
        this.elem.appendChild(this.textElem);

        display.appendChild(this.elem);
    }

    updateObservableProperty(name: string, value: any): void
    {
        this.setNickname(value);
    }

    setNickname(nickname: string): void
    {
        $(this.textElem).html(as.Html(nickname));
    }

    onCloseButtonPressed(): void
    {
        this.elem.style.display = 'none';
        //hw later this.app.sendSetUserAttributesMessage({ 'HideNickname': true });
    }

    setVisible(visible: boolean): void
    {
        this.elem.style.display = visible ? 'block' : 'none';
    }
}
