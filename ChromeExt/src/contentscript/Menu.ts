import * as $ from 'jquery';
import 'jqueryui';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';

interface MenuClickHandler { (ev: JQuery.Event): void }

export class MenuItem
{
    constructor(public id: string, public text: string, public hasIcon: boolean, public onClickCloseMenu: boolean, public onClick: MenuClickHandler)
    {

    }
}

export class MenuColumn
{
    constructor(public id: string, public items: Array<MenuItem>)
    {

    }
}

export class Menu
{
    constructor(private app: ContentApp, public id: string, public columns: Array<MenuColumn>)
    {

    }

    render(): HTMLElement
    {
        let menu = <HTMLElement>$('<div class="n3q-base n3q-menu n3q-menu-avatar" data-translate="children">').get(0);
        let checkbox = <HTMLElement>$('<input type="checkbox" href="#" class="n3q-base n3q-menu-open" name="n3q-id-menu-open-avatarmenu-' + this.id + '" id="n3q-id-menu-open-avatarmenu-' + this.id + '" />').get(0);
        $(menu).append(checkbox);
        let label = <HTMLElement>$('<label for="n3q-id-menu-open-avatarmenu-' + this.id + '" class="n3q-base n3q-menu-open-button"></label>').get(0);
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
                if (!item.hasIcon) {
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
                    if (item.onClickCloseMenu) {
                        $(checkbox).prop('checked', false);
                    }
                });
                $(menu).append(itemElem);
            });
        });

        this.app.translateElem(menu);

        return menu;
    }
}
/*
<div class="n3q-base n3q-menu-item n3q-menu-column-main n3q-item-chat n3q-shadow" data-translate="text:Menu" style="
    text-align: left;
">
    <div class="n3q-base" style="width:14px;height:14px;background: url('https://api.iconify.design/ic:baseline-chat-bubble-outline.svg') no-repeat center center / contain;display: inline-block;vertical-align:middle;"></div>
    <div class="n3q-base n3q-text" style="display: inline-block;vertical-align:middle;">Chat</div>
</div>*/