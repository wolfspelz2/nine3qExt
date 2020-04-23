import * as $ from 'jquery';
import { as } from './as';
import { App } from './App';
import { IObserver, IObservable } from './ObservableProperty';
import { Participant } from './Participant';

export class Nickname implements IObserver
{
    private elem: HTMLDivElement;
    private textElem: HTMLSpanElement;

    constructor(private app: App, private participant: Participant, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-nickname n3q-shadow" />')[0];
        $(this.elem).click(() => { this.participant.select(); });

        $(this.elem).on('mouseenter', ev => this.participant.onMouseEnterAvatar(ev));
        $(this.elem).on('mouseleave', ev => this.participant.onMouseLeaveAvatar(ev));

        this.textElem = <HTMLSpanElement>$('<span class="n3q-text" />')[0];
        this.elem.appendChild(this.textElem);

        display.appendChild(this.elem);
    }

    update(name: string, value: any): void
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
