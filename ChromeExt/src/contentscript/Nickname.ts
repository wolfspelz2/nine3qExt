import * as $ from 'jquery';
import { as } from '../lib/as';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Environment } from '../lib/Environment';
import { Menu, MenuColumn, MenuItem, MenuHasIcon, MenuOnClickClose } from './Menu';

export class Nickname implements IObserver
{
    private elem: HTMLDivElement;
    private textElem: HTMLElement;
    private menuElem: HTMLElement;
    private nickname: string;

    constructor(private app: ContentApp, private participant: Participant, private isSelf: boolean, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-nickname n3q-shadow" />').get(0);
        $(this.elem).click(() => { this.participant.select(); });

        // $(this.elem).on('mouseenter', ev => this.participant.onMouseEnterAvatar(ev));
        // $(this.elem).on('mouseleave', ev => this.participant.onMouseLeaveAvatar(ev));

        let columns = new Array<MenuColumn>();
        {
            let items = new Array<MenuItem>();
            if (this.isSelf) {
                items.push(new MenuItem('chat', 'Chat', MenuHasIcon.Yes, MenuOnClickClose.Yes, ev => { this.participant.toggleChatin(); }));
                if (Environment.isDevelopment()) { items.push(new MenuItem('test', 'Test', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.app.test(); })); }
            } else {
                items.push(new MenuItem('chat', 'Chat', MenuHasIcon.Yes, MenuOnClickClose.Yes, ev => { this.participant.toggleChatout(); }));
            }
            if (this.isSelf) {
                items.push(new MenuItem('chatwin', 'Chat Window', MenuHasIcon.Yes, MenuOnClickClose.Yes, ev => { this.participant.showChatWindow(); }));
            }
            columns.push(new MenuColumn('main', items));
        }
        if (this.isSelf) {
            let items = new Array<MenuItem>();
            items.push(new MenuItem('Actions', 'Actions:', MenuHasIcon.No, MenuOnClickClose.No, null));
            items.push(new MenuItem('wave', 'wave', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('wave'); }));
            items.push(new MenuItem('dance', 'dance', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('dance') }));
            items.push(new MenuItem('cheer', 'cheer', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('wave'); }));

            items.push(new MenuItem('kiss', 'kiss', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('kiss'); }));
            items.push(new MenuItem('clap', 'clap', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('clap'); }));
            items.push(new MenuItem('laugh', 'laugh', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('laugh'); }));
            items.push(new MenuItem('angry', 'angry', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('angry'); }));
            items.push(new MenuItem('deny', 'deny', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('deny'); }));
            items.push(new MenuItem('yawn', 'yawn', MenuHasIcon.No, MenuOnClickClose.Yes, ev => { this.participant.do('yawn'); }));

            columns.push(new MenuColumn('action', items));
        }
        this.menuElem = new Menu(this.app, Utils.randomString(10), columns).render();
        $(this.elem).append(this.menuElem);

        this.textElem = <HTMLElement>$('<div class="n3q-base n3q-text" />').get(0);
        this.elem.appendChild(this.textElem);

        display.appendChild(this.elem);
    }

    stop()
    {
        // Nothing to do
    }

    updateObservableProperty(name: string, value: any): void
    {
        this.setNickname(value);
    }

    setNickname(nickname: string): void
    {
        this.nickname = nickname;
        $(this.textElem).html(as.Html(nickname));
        if (Config.get('room.showNicknameTooltip', true)) {
            $(this.textElem).prop('title', nickname);;
        }
    }

    getNickname(): string
    {
        return this.nickname;
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
