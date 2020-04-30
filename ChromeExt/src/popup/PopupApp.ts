import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { AvatarGallery } from '../lib/AvatarGallery';

export class PopupApp
{
    private display: HTMLElement;

    constructor(private appendToMe: HTMLElement)
    {
    }

    public async start()
    {
        this.display = $('<div id="n3q-id-popup" class="n3q-base" />')[0];
        this.appendToMe.append(this.display);

        {
            let nickname = as.String(await Config.getLocal('nickname', 'Your name'));
            let group = $('<div class="n3q-base n3q-popup-nickname" />')[0];
            let label = $('<div class="n3q-base n3q-popup-label" >Name<div>')[0];
            group.append(label);
            let input = $('<input type="text" id="n3q-id-popup-nickname" class="n3q-base" />')[0];
            $(input).text(nickname);
            group.append(input);
            this.display.append(group);
        }

        {
            let button = $('<button class="n3q-base n3q-popup-save" >Save</button>')[0];
            $(button).bind('click', ev =>
            {
                console.log($('#n3q-id-popup-nickname').val());
            });
            this.display.append(button);
        }


    }

    public stop()
    {
        $('#n3q-id-page').remove();
        this.display = null;
    }
}
