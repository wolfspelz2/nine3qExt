import * as $ from 'jquery';
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { Platform } from '../lib/Platform';
import { Translator } from '../lib/Translator';
import { AvatarGallery } from '../lib/AvatarGallery';

// @ts-ignore
import imgPopupIcon from '../assets/PopupIcon.png';

export class PopupApp
{
    private display: HTMLElement;
    private babelfish: Translator;
    private defaultDevConfig = `{
    "config": {
        "serviceUrl": "https://config.weblin.sui.li/"
    }
}`;

    constructor(private appendToMe: HTMLElement)
    {
        let language: string = Translator.mapLanguage(navigator.language, lang => { return Config.get('i18n.languageMapping', {})[lang]; }, Config.get('i18n.defaultLanguage', 'en-US'));
        this.babelfish = new Translator(Config.get('i18n.translations', {})[language], language, Config.get('i18n.serviceUrl', ''));
    }

    async dev_start()
    {
        let start = $('<button style="display:inline">Start</button>').get(0);
        $(start).bind('click', async ev =>
        {
            await this.start();
        });
        let stop = $('<button style="display:inline">Stop</button>').get(0);
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
        try {
            let config = await Platform.getConfig();
            Config.setAllOnline(config);
        } catch (error) {
            log.warn(error);
        }

        this.display = $('<div id="n3q-id-popup" class="n3q-base" data-translate="children"/>').get(0);

        let nickname = as.String(await Config.getSync('me.nickname', 'Your name'));
        let avatar = as.String(await Config.getSync('me.avatar', ''));

        {
            let group = $('<div class="n3q-base n3q-popup-header" data-translate="children"/>').get(0);

            let icon = <HTMLImageElement>$('<img class="n3q-base n3q-popup-icon" />').get(0);
            icon.src = imgPopupIcon;
            group.append(icon);

            let title = $('<div class="n3q-base n3q-popup-title" data-translate="text:Popup.title">Configure your avatar</div>').get(0);
            group.append(title);

            let description = $('<div class="n3q-base n3q-popup-description" data-translate="text:Popup.description">Change name and avatar, then reload the page.</div>').get(0);
            group.append(description);

            $(icon).on('click', async ev =>
            {
                if (ev.ctrlKey) {
                    let dev = $('#n3q-popup-dev').get(0);
                    if (dev == null) {
                        dev = $('<div id="n3q-popup-dev" class="n3q-base n3q-popup-hidden" style="" />').get(0);
                        let text = $('<textarea class="n3q-base" style="width: 100%; height: 100px; margin-top: 1em;" />').get(0);
                        let data = await Config.getSync('dev.config', this.defaultDevConfig);
                        $(text).val(data);
                        $(dev).append(text);
                        let apply = $('<button class="n3q-base" style="margin-top: 0.5em;">Save</button>').get(0);
                        $(apply).on('click', async ev =>
                        {
                            let data = $(text).val();
                            await Config.setSync('dev.config', data);
                        });
                        $(dev).append(apply);
                        $(group).append(dev);
                    }
                    if (dev != null) {
                        if ($(dev).hasClass('n3q-popup-hidden')) {
                            $(dev).removeClass('n3q-popup-hidden');
                        } else {
                            $(dev).addClass('n3q-popup-hidden');
                        }
                    }
                }
            });
            // $(dev).removeClass('n3q-popup-hidden');

            this.display.append(group);
        }

        {
            let group = $('<div class="n3q-base n3q-popup-group n3q-popup-group-nickname" data-translate="children"/>').get(0);

            let label = $('<div class="n3q-base n3q-popup-label" data-translate="text:Popup">Name</div>').get(0);
            group.append(label);

            let input = $('<input type="text" id="n3q-id-popup-nickname" class="n3q-base" />').get(0);
            $(input).val(nickname);
            group.append(input);

            let button = $('<button class="n3q-base n3q-popup-random" data-translate="text:Popup">Random</button>').get(0);
            $(button).bind('click', async ev =>
            {
                $('#n3q-id-popup-nickname').val(Utils.randomNickname());
            });
            group.append(button);

            this.display.append(group);
        }

        {
            let list: Array<string> = Config.get('avatars.list', [avatar]);

            let avatarIdx = list.indexOf(avatar);
            if (avatarIdx < 0) {
                avatar = AvatarGallery.getRandomAvatar();
                avatarIdx = list.indexOf(avatar);
                if (avatarIdx < 0) {
                    avatar = '004/pinguin';
                }
                await Config.setSync('me.avatar', avatar);
            }

            let group = $('<div class="n3q-base n3q-popup-group n3q-popup-group-avatar" data-translate="children"/>').get(0);

            let input = $('<input type="hidden" id="n3q-id-popup-avatar" class="n3q-base" />').get(0);
            $(input).val(avatar);
            group.append(input);

            let label = $('<div class="n3q-base n3q-popup-label" data-translate="text:Popup">Avatar</div>').get(0);
            group.append(label);

            let left = <HTMLElement>$('<button class="n3q-base n3q-popup-avatar-arrow n3q-popup-avatar-left">&lt;</button>').get(0);
            group.append(left);

            let icon = <HTMLImageElement>$('<img class="n3q-base n3q-popup-avatar-current" />').get(0);
            group.append(icon);

            let right = <HTMLElement>$('<button class="n3q-base n3q-popup-avatar-arrow n3q-popup-avatar-right">&gt;</button>').get(0);
            group.append(right);

            let name = $('<div class="n3q-base n3q-popup-avatar-name" />').get(0);
            group.append(name);

            this.setCurrentAvatar(avatar, icon, input, name);

            $(left).on('click', () =>
            {
                let idx = list.indexOf(<string>$(input).val());
                idx--;
                if (idx < 0) { idx = list.length - 1; }
                this.setCurrentAvatar(list[idx], icon, input, name);
            });

            $(right).on('click', () =>
            {
                let idx = list.indexOf(<string>$(input).val());
                idx++;
                if (idx >= list.length) { idx = 0; }
                this.setCurrentAvatar(list[idx], icon, input, name);
            });

            this.display.append(group);
        }

        {
            let group = $('<div class="n3q-base n3q-popup-group n3q-popup-group-save" data-translate="children"/>').get(0);

            let saving = $('<div class="n3q-base n3q-popup-save-saving" data-translate="text:Popup">Saving</div>').get(0);
            let saved = $('<div class="n3q-base n3q-popup-save-saved" data-translate="text:Popup">Saved</div>').get(0);

            let save = $('<button class="n3q-base n3q-popup-save" data-translate="text:Popup">Save</button>').get(0);
            $(save).bind('click', async ev =>
            {
                $(saving).fadeTo(200, 1.0);
                await Config.setSync('me.nickname', $('#n3q-id-popup-nickname').val());
                await Config.setSync('me.avatar', $('#n3q-id-popup-avatar').val());
                $(saving).fadeTo(200, 0.0);

                var nickname = await Config.getSync('me.nickname', '');
                var avatar = await Config.getSync('me.avatar', '');
                if (nickname != '' && avatar != '') {
                    $(saved).fadeTo(300, 1.0).fadeTo(2000, 0.0);
                }
            });
            group.append(save);

            group.append(saving);
            group.append(saved);

            let close = $('<button class="n3q-base n3q-popup-close" data-translate="text:Common">Close</button>').get(0);
            $(close).bind('click', async ev =>
            {
                window.close();
            });
            group.append(close);

            this.display.append(group);
        }

        this.babelfish.translateElem(this.display);
        this.appendToMe.append(this.display);
    }

    private setCurrentAvatar(id: string, displayElem: HTMLImageElement, hiddenElem: HTMLElement, nameElem: HTMLElement)
    {
        // $(nameElem).text(id);
        $(hiddenElem).val(id);
        displayElem.src = this.getAvatarDisplayUrlFromAvatarId(id);
    }

    private getAvatarDisplayUrlFromAvatarId(id: string)
    {
        let avatarUrl = as.String(Config.get('avatars.animationsUrlTemplate', 'https://avatar.zweitgeist.com/gif/{id}/config.xml')).replace('{id}', id);
        let idleUrl = new URL('idle.gif', avatarUrl);
        return idleUrl.toString();
    }

    stop()
    {
        $(this.display).remove();
        this.display = null;
    }
}
