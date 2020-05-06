import * as $ from 'jquery';
import 'jqueryui';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';

interface MenuClickHandler { (ev: JQuery.Event): void }

export enum MenuHasIcon
{
    No,
    Yes,
}

export enum MenuOnClickClose
{
    No,
    Yes,
}

export class MenuItem
{
    constructor(public id: string, public text: string, public hasIcon: MenuHasIcon, public onClickClose: MenuOnClickClose, public onClick: MenuClickHandler) { }
}

export class MenuColumn
{
    constructor(public id: string, public items: Array<MenuItem>) { }
}

export class Menu
{
    private checkboxElem: HTMLElement;

    constructor(private app: ContentApp, public id: string, public columns: Array<MenuColumn>) { }

    render(): HTMLElement
    {
        let menu = <HTMLElement>$('<div class="n3q-base n3q-menu n3q-menu-avatar" data-translate="children">').get(0);
        this.checkboxElem = <HTMLElement>$('<input type="checkbox" href="#" class="n3q-base n3q-menu-open" name="n3q-id-menu-open-avatarmenu-' + this.id + '" id="n3q-id-menu-open-avatarmenu-' + this.id + '" />').get(0);
        $(this.checkboxElem).on('keydown', ev => { this.onKeydown(ev); });

        $(menu).append(this.checkboxElem);
        let label = <HTMLElement>$('<label for="n3q-id-menu-open-avatarmenu-' + this.id + '" class="n3q-base n3q-menu-open-button"></label>').get(0);
        $(label).on('keydown', ev => { this.onKeydown(ev); });
        $(label).on('click', ev => { $(label).focus(); });
        $(menu).append(label);

        this.columns.forEach(column =>
        {
            column.items.forEach(item =>
            {
                let itemElem = <HTMLElement>$('<div class="n3q-base n3q-menu-item n3q-menu-column-' + column.id + ' n3q-item-' + item.id + ' n3q-shadow" data-translate="children" />').get(0);
                if (item.onClick == undefined || item.onClick == null) {
                    $(itemElem).addClass('n3q-menu-item-disabled');
                }
                let icon = <HTMLElement>$('<div class="n3q-base n3q-menu-icon"></div>').get(0);
                if (item.hasIcon == MenuHasIcon.No) {
                    $(icon).addClass('n3q-menu-icon-noicon');
                }
                $(itemElem).append(icon);
                let text = <HTMLElement>$('<div class="n3q-base n3q-text" data-translate="text:Menu.' + item.id + '">' + item.text + '</div>').get(0);
                $(itemElem).append(text);
                $(itemElem).on('click', ev =>
                {
                    if (typeof item.onClick == 'function') {
                        item.onClick(ev);
                    }
                    if (item.onClickClose == MenuOnClickClose.Yes) {
                        this.close();
                    }
                });
                $(menu).append(itemElem);
            });
        });

        this.app.translateElem(menu);

        return menu;
    }

    private onKeydown(ev: JQuery.Event)
    {
        var keycode = (ev.keyCode ? ev.keyCode : (ev.which ? ev.which : ev.charCode));
        switch (keycode) {
            case 13:
                // this.sendChat();
                return false;
            case 27:
                this.close();
                ev.stopPropagation();
                return false;
            default:
                return true;
        }
    }

    close()
    {
        if (this.checkboxElem != null) {
            $(this.checkboxElem).prop('checked', false);
        }
    }
}
