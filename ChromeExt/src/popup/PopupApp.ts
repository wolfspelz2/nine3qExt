import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { AvatarGallery } from '../lib/AvatarGallery';

import imgPopupIcon from '../assets/PopupIcon.png';

export class PopupApp
{
    private display: HTMLElement;

    constructor(private appendToMe: HTMLElement)
    {
    }

    async controls()
    {
        let start = $('<button style="display:inline">Start</button>')[0];
        $(start).bind('click', async ev =>
        {
            await this.start();
        });
        let stop = $('<button style="display:inline">Stop</button>')[0];
        $(stop).bind('click', async ev =>
        {
            this.stop();
        });
        this.appendToMe.append(start);
        this.appendToMe.append(stop);
        this.appendToMe.style.minWidth = '25em';
    }

    async start()
    {
        this.display = $('<div id="n3q-id-popup" class="n3q-base" />')[0];
        this.appendToMe.append(this.display);

        {
            let group = $('<div class="n3q-base n3q-popup-header" />')[0];

            let icon = <HTMLImageElement>$('<img class="n3q-base n3q-popup-icon" />')[0];
            icon.src = imgPopupIcon;
            group.append(icon);

            let title = $('<div class="n3q-base n3q-popup-title">Configure your avatar</div>')[0];
            group.append(title);

            let description = $('<div class="n3q-base n3q-popup-description">Change name and avatar, press [save], and then reload the page.</div>')[0];
            group.append(description);

            this.display.append(group);
        }

        {
            let group = $('<div class="n3q-base n3q-popup-nickname" />')[0];

            let label = $('<div class="n3q-base n3q-popup-label">Name</div>')[0];
            group.append(label);

            let nickname = as.String(await Config.getLocal('nickname', 'Your name'));
            let input = $('<input type="text" id="n3q-id-popup-nickname" class="n3q-base" />')[0];
            $(input).val(nickname);
            group.append(input);

            let button = $('<button class="n3q-base n3q-popup-random" >Random</button>')[0];
            $(button).bind('click', async ev =>
            {
                $('#n3q-id-popup-nickname').val(Utils.randomNickname());
            });
            group.append(button);

            this.display.append(group);
        }

        {
            let button = $('<button class="n3q-base n3q-popup-save" >Save</button>')[0];
            $(button).bind('click', async ev =>
            {
                await Config.setLocal('nickname', $('#n3q-id-popup-nickname').val())
            });
            this.display.append(button);
        }
    }

    stop()
    {
        $(this.display).remove();
        this.display = null;
    }
}
