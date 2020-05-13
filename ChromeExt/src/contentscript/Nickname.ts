import * as $ from 'jquery';
import { as } from '../lib/as';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Environment } from '../lib/Environment';
import { Menu, MenuColumn, MenuItem, MenuHasIcon, MenuOnClickClose, MenuHasCheckbox } from './Menu';

export class Nickname implements IObserver
{
    private elem: HTMLDivElement;
    private textElem: HTMLElement;
    private menuElem: HTMLElement;
    private nickname: string;

    constructor(private app: ContentApp, private participant: Participant, private isSelf: boolean, private display: HTMLElement)
    {
        this.elem = <HTMLDivElement>$('<div class="n3q-base n3q-nickname n3q-shadow-small" />').get(0);
        $(this.elem).click(() => { this.participant?.select(); });

        // $(this.elem).on('mouseenter', ev => this.participant?.onMouseEnterAvatar(ev));
        // $(this.elem).on('mouseleave', ev => this.participant?.onMouseLeaveAvatar(ev));

        let menu = new Menu(this.app, Utils.randomString(10));

        {
            let column = new MenuColumn(menu, 'main');
            if (this.isSelf) {
                column.addItem('chat', 'Chat', MenuHasIcon.Yes, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.toggleChatin(); });
                if (Environment.isDevelopment()) { column.addItem('test', 'Test', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.app.test(); }); }
            } else {
                column.addItem('chat', 'Chat', MenuHasIcon.Yes, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.toggleChatout(); });
            }
            if (this.isSelf) {
                column.addItem('settings', 'Settings', MenuHasIcon.Yes, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { if (this.participant) { this.app.showSettings(this.participant.getElem()); } });
                column.addItem('chatwin', 'Chat Window', MenuHasIcon.Yes, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.showChatWindow(); });
                column.addItem(
                    'tabstay',
                    'Stay Here',
                    MenuHasIcon.Yes,
                    app.getStayOnTabChange() ? MenuHasCheckbox.YesChecked : MenuHasCheckbox.YesUnchecked,
                    MenuOnClickClose.Yes,
                    ev =>
                    {
                        this.app.toggleStayOnTabChange();
                        menu.setCheckbox('main', 'tabstay', app.getStayOnTabChange() ? MenuHasCheckbox.YesChecked : MenuHasCheckbox.YesUnchecked);
                    }
                );
            }
            menu.addColumn(column);
        }

        if (this.isSelf) {
            let column = new MenuColumn(menu, 'action');
            column.addItem('Actions', 'Actions:', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.No, null);
            column.addItem('wave', 'wave', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('wave'); });
            column.addItem('dance', 'dance', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('dance') });
            column.addItem('cheer', 'cheer', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('cheer'); });
            column.addItem('kiss', 'kiss', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('kiss'); });
            column.addItem('clap', 'clap', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('clap'); });
            column.addItem('laugh', 'laugh', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('laugh'); });
            column.addItem('angry', 'angry', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('angry'); });
            column.addItem('deny', 'deny', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('deny'); });
            column.addItem('yawn', 'yawn', MenuHasIcon.No, MenuHasCheckbox.No, MenuOnClickClose.Yes, ev => { this.participant?.do('yawn'); });
            menu.addColumn(column);
        }

        this.menuElem = menu.render();
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
