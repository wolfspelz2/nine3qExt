import * as $ from 'jquery';
import 'jqueryui';
import { ContentApp } from './ContentApp';
import { Entity } from './Entity';

export interface IMenuEvents
{
    [menuId: string]: (ev: JQuery.Event) => void;
}

export class Menu
{
    private isClosed: boolean = false;

    constructor(private app: ContentApp, private entity: Entity, private elem: HTMLElement, private events: IMenuEvents, ev: JQuery.Event, private closedHandler: () => void)
    {
        var blurTimer;
        this.entity.getElem().appendChild(elem);
        $(elem)
            // .position({ my: 'left-20 top', of: ev })
            .menu({
                create: (ev, ui) => { },
                blur: (ev, ui) =>
                {
                    blurTimer = setTimeout(() =>
                    {
                        this.close();
                    }, 50);
                },
                focus: (ev, ui) => { clearTimeout(blurTimer); },
                // select: (ev: JQuery.Event, ui) =>
                // {
                //     var selected = ui.item.data('menuid');
                //     if (typeof this.events[selected] !== typeof undefined) {
                //         this.close();
                //         this.events[selected](ev);
                //     }
                // },
            });
        this.app.translateElem(elem);
    }

    private close(): void
    {
        if (!this.isClosed) {
            this.isClosed = true;

            if (typeof this.entity.getElem() != typeof undefined) {
                // Just in case the entity disappears before the menu closes
                this.entity.getElem().removeChild(this.elem);
            }

            if (this.closedHandler != null) {
                this.closedHandler();
            }
        }
    }
}
