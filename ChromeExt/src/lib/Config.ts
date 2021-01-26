import log = require('loglevel');
import { Utils } from './Utils';

interface ConfigGetCallback { (value: any): void }
interface ConfigSetCallback { (): void }

export class Config
{
    public static devConfigName = 'dev';
    private static devConfig: any = {};

    public static onlineConfigName = 'online';
    private static onlineConfig: any = {};

    public static staticConfigName = 'static';
    public static staticConfig: any = {
        me: {
            nickname: '',//'新しいアバター',//'new-avatar',
            avatar: '',
            active: '',
        },
        client: {
            name: 'weblin.io',
            variant: '',
        },
        design: {
            name: 'basic',
            version: ''
        },
        vp: {
            deferPageEnterSec: 0.3,
            vpiRoot: 'https://lms.virtual-presence.org/v7/root.xml',
            vpiMaxIterations: 15,
            ignoredDomainSuffixes: ['vulcan.weblin.com'],
        },
        config: {
            serviceUrl: 'https://webex.vulcan.weblin.com/Config',
            updateIntervalSec: Utils.randomInt(86400-2000, 86400+1000),
            checkUpdateIntervalSec: Utils.randomInt(120-10, 120+10),
        },
        httpCache: {
            maxAgeSec: 3600,
            maintenanceIntervalSec: 60,
        },
        test: {
            itemServiceRpcUrl: 'http://localhost:5000/rpc',
        },
        room: {
            fadeInSec: 0.3,
            checkPageUrlSec: 3.0,
            defaultAvatarSpeedPixelPerSec: 100,
            randomEnterPosXMin: 300,
            randomEnterPosXMax: 600,
            showNicknameTooltip: true,
            avatarDoubleClickDelaySec: 0.1,
            chatBuubleFadeStartSec: 60.0,
            chatBuubleFadeDurationSec: 60.0,
            maxChatAgeSec: 60,
            chatWindowWidth: 400,
            chatWindowHeight: 250,
            chatWindowMaxHeight: 800,
            keepAliveSec: 120,
            nicknameOnHover: true,
            defaultStillimageSize: 80,
            defaultAnimationSize: 100,
            vCardAvatarFallback: false,
            vCardAvatarFallbackOnHover: true,
            vidconfUrl: 'https://jitsi.vulcan.weblin.com/{room}#userInfo.displayName="{name}"',
            vidconfBottom: 200,
            vidconfWidth: 600,
            vidconfHeight: 400,
            pokeToastDurationSec: 10,
            privateChatToastDurationSec: 60,
            errorToastDurationSec: 8,
            applyItemErrorToastDurationSec: 5,
        },
        xmpp: {
            service: 'wss://xmpp.vulcan.weblin.com/xmpp-websocket',
            domain: 'xmpp.vulcan.weblin.com',
            maxMucEnterRetries: 4,
            pingBackgroundToKeepConnectionAliveSec: 12,
        },
        avatars: {
            animationsUrlTemplate: 'https://webex.vulcan.weblin.com/avatars/gif/{id}/config.xml',
            animationsProxyUrlTemplate: 'https://webex.vulcan.weblin.com/Avatar/DataUrl?url={url}',
            dataUrlProxyUrlTemplate: 'https://webex.vulcan.weblin.com/Avatar/DataUrl?url={url}',
            list: ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
            randomList: ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
        },
        identity: {
            url: '',
            digest: '',
            identificatorUrlTemplate: 'https://webex.vulcan.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}',
        },
        inventory: {
            enabled: false,
            itemSize: 64,
            borderPadding: 4,
            dropZoneHeight: 100,
        },
        backpack: {
            enabled: false,
            itemSize: 64,
            borderPadding: 4,
            dropZoneHeight: 100,
            itemPropertiesTooltip: false,
        },
        projector: {
            enabled: false,
        },
        itemProviders: {
            'nine3q':
            {
                name: 'weblin.io Items',
                description: 'Things on web pages',
                configUrl: 'https://webit.vulcan.weblin.com/Config?id={id}&client={client}',
            }
        },
        i18n: {
            defaultLanguage: 'en-US',
            languageMapping: {
                'de': 'de-DE',
            },
            translations: {
                'en-US': {
                    'Common.Close': 'Close',
                    'Common.Undock': 'Open in separate window',

                    'Chatin.Enter chat here...': 'Enter chat here...',
                    'Chatin.SendChat': 'Send chat',

                    'Popup.title': 'Your weblin',
                    'Popup.description': 'Change name and avatar, then press [save].',
                    'Popup.Name': 'Name',
                    'Popup.Random': 'Random',
                    'Popup.Avatar': 'Avatar',
                    'Popup.Save': 'Save',
                    'Popup.Saving': 'Saving',
                    'Popup.Saved': 'Saved',
                    'Popup.Show avatar': 'Show avatar on pages',
                    'Popup.Uncheck to hide': 'Uncheck to hide avatar on pages',

                    'Menu.Settings': 'Settings',
                    'Menu.Stay Here': 'Stay on tab change',
                    'Menu.Inventory': 'Your Stuff',
                    'Menu.Backpack': 'Stuff',
                    'Menu.Chat Window': 'History',
                    'Menu.Video Conference': 'Video Conference',
                    'Menu.Chat': 'Chat',
                    'Menu.Actions:': 'Actions:',
                    'Menu.wave': 'Wave',
                    'Menu.dance': 'Dance',
                    'Menu.cheer': 'Cheer',
                    'Menu.kiss': 'Kiss',
                    'Menu.clap': 'Clap',
                    'Menu.laugh': 'Laugh',
                    'Menu.angry': 'Angry',
                    'Menu.deny': 'Deny',
                    'Menu.yawn': 'Yawn',
                    'Menu.Greet': 'Greet',
                    'Menu.Private Chat': 'Private Chat',

                    'Chatwindow.Chat History': 'Chat',
                    'Chatwindow.entered the room': '**entered the room**',
                    'Chatwindow.was already there': '**was already there**',
                    'Chatwindow.left the room': '**left the room**',
                    'Chatwindow.appeared': '*appeared*',
                    'Chatwindow.is present': '*is present*',
                    'Chatwindow.disappeared': '*disappeared*',

                    'PrivateChat.Private Chat with': 'Private Chat with',

                    'Vidconfwindow.Video Conference': 'Video Conference',
                    'Settingswindow.Settings': 'Settings',
                    'InventoryWindow.Inventory': 'Your Stuff',
                    'BackpackWindow.Inventory': 'Your Stuff',

                    'Inventory.Not yet loaded': 'Not yet loaded',

                    'Toast.Do not show this message again': 'Do not show this message again',
                    'Toast.greets': '...greeted you',
                    'Toast.tousles': '...tousled you',
                    'Toast.nudges': '...nudged you',

                    // ['ErrorFact.' + ItemException.Fact[ItemException.Fact.Error]]: 'Error',
                    'ErrorFact.Error': 'Error',

                    'ErrorFact.NotRezzed': 'Failed to drop item',
                    'ErrorFact.NotDerezzed': 'Failed to pick up item',
                    'ErrorFact.NotAdded': 'Failed to add item',
                    'ErrorFact.NotChanged': 'Failed to update item',
                    'ErrorFact.NoItemsReceived': 'No items recevied',
                    'ErrorFact.NotExecuted': 'Not executed',
                    'ErrorFact.NotCreated': 'No item created',
                    'ErrorFact.NotApplied': 'Item not applied',

                    'ErrorReason.UnknownReason': 'Unknown reason :-(',
                    'ErrorReason.ItemAlreadyRezzed': 'Item already on page.',
                    'ErrorReason.ItemNotRezzedHere': 'Item is not on this page',
                    'ErrorReason.ItemsNotAvailable': 'Items not available. The feature may be disabled.',
                    'ErrorReason.ItemDoesNotExist': 'This is an not a known item.',
                    'ErrorReason.NoUserToken': 'No user id. Maybe not logged in as item user.',
                    'ErrorReason.SeeDetail': '',
                    'ErrorReason.InvalidChecksum': 'Invalid checksum. Not a valid item.',
                    'ErrorReason.StillInCooldown': 'Still in cooldown period.',
                    'ErrorReason.InvalidPropertyValue': 'Property invalid.',
                    'ErrorReason.NotYourItem': 'This is not your item.',

                    'ErrorDetail.Applier.Apply': 'Applying an item to another',
                    'ErrorDetail.Pid.Id': 'Id',
                },
                'de-DE': {
                    'Common.Close': 'Schließen',
                    'Common.Undock': 'Im eigenen Fenster öffnen',

                    'Chatin.Enter chat here...': 'Chat Text hier...',
                    'Chatin.SendChat': 'Chat abschicken',

                    'Popup.title': 'Dein weblin',
                    'Popup.description': 'Wähle Name und Avatar, dann drücke [Speichern].',
                    'Popup.Name': 'Name',
                    'Popup.Random': 'Zufallsname',
                    'Popup.Avatar': 'Avatar',
                    'Popup.Save': 'Speichern',
                    'Popup.Saving': 'Speichern',
                    'Popup.Saved': 'Gespeichert',
                    'Popup.Show avatar': 'Avatar auf Seiten anzeigen',
                    'Popup.Uncheck to hide': 'Abschalten, um das Avatar auf Webseiten nicht anzuzeigen',

                    'Menu.Settings': 'Einstellungen',
                    'Menu.Stay Here': 'Bleiben bei Tabwechsel',
                    'Menu.Inventory': 'Deine Gegenstände',
                    'Menu.Backpack': 'Gegenstände',
                    'Menu.Chat Window': 'Chatverlauf',
                    'Menu.Video Conference': 'Videokonferenz',
                    'Menu.Chat': 'Sprechblase',
                    'Menu.Actions:': 'Aktionen:',
                    'Menu.wave': 'Winken',
                    'Menu.dance': 'Tanzen',
                    'Menu.cheer': 'Jubeln',
                    'Menu.kiss': 'Küssen',
                    'Menu.clap': 'Klatschen',
                    'Menu.laugh': 'Lachen',
                    'Menu.angry': 'Ärgern',
                    'Menu.deny': 'Ablehnen',
                    'Menu.yawn': 'Gähnen',
                    'Menu.Greet': 'Grüßen',
                    'Menu.Private Chat': 'Privater Chat',

                    'Chatwindow.Chat History': 'Chat',
                    'Chatwindow.entered the room': '**hat den Raum betreten**',
                    'Chatwindow.was already there': '**war schon da**',
                    'Chatwindow.left the room': '**hat den Raum verlassen**',
                    'Chatwindow.appeared': '*erschienen*',
                    'Chatwindow.is present': '*ist da*',
                    'Chatwindow.disappeared': '*verschwunden*',

                    'PrivateChat.Private Chat with': 'Privater Chat mit',

                    'Vidconfwindow.Video Conference': 'Videokonferenz',
                    'Settingswindow.Settings': 'Einstellungen',
                    'InventoryWindow.Inventory': 'Deine Gegenstände',
                    'BackpackWindow.Inventory': 'Deine Gegenstände',

                    'Inventory.Not yet loaded': 'Noch nicht geladen',

                    'Toast.Do not show this message again': 'Diese Nachricht nicht mehr anzeigen',
                    'Toast.greets': '...hat dich gegrüßt',
                    'Toast.tousles': '...hat dich gewuschelt',
                    'Toast.nudges': '...hat dich angestupst',

                    'ErrorFact.Error': 'Fehler',
                    'ErrorFact.NotRezzed': 'Ablegen fehlgeschlagen',
                    'ErrorFact.NotDerezzed': 'Von der Seite nehmen fehlgeschlagen',
                    'ErrorFact.NotAdded': 'Gegenstand nicht hinzugefügt',
                    'ErrorFact.NotChanged': 'Gegenstand nicht geändert',
                    'ErrorFact.NoItemsReceived': 'Keine Gegenstände bekommen',
                    'ErrorFact.NotExecuted': 'Nicht ausgeführt',
                    'ErrorFact.NotCreated': 'Kein Gegenstand erstellt',
                    'ErrorFact.NotApplied': 'Gegenstand nicht angewendet',

                    'ErrorReason.UnknownReason': 'Grund unbekannt :-(',
                    'ErrorReason.ItemAlreadyRezzed': 'Gegenstand ist schon auf einer Seite.',
                    'ErrorReason.ItemNotRezzedHere': 'Gegenstand ist nicht auf dieser Seite',
                    'ErrorReason.ItemsNotAvailable': 'Keine Gegenstände verfügbar. Die Funktion ist vielleicht nicht eingeschaltet.',
                    'ErrorReason.ItemDoesNotExist': 'Dieser Gegenstand ist nicht bekannt.',
                    'ErrorReason.NoUserToken': 'Keine Benutzerkennung. Möglicherweise nicht als Benutzer von Gegenständen angemeldet.',
                    'ErrorReason.SeeDetail': '',
                    'ErrorReason.InvalidChecksum': 'Falsche Checksumme. Kein zulässiger Gegenstand.',
                    'ErrorReason.StillInCooldown': 'Braucht noch Zeit, um sich zu erholen.',
                    'ErrorReason.InvalidPropertyValue': 'Falsche Eigenschaft.',
                    'ErrorReason.NotYourItem': 'Das ist nicht dein Gegenstand.',

                    'ErrorDetail.Applier.Apply': 'Beim Anwenden eines Gegenstands auf einen anderen.',
                    'ErrorDetail.Pid.Id': 'Id',
                },
            },
            'serviceUrl': '',
        },

        _last: 0
    }

    static get(key: string, defaultValue: any): any
    {
        let result = null;
        if (result == undefined || result == null) {
            result = Config.getDev(key);
        }
        if (result == undefined || result == null) {
            result = Config.getOnline(key);
        }
        if (result == undefined || result == null) {
            result = Config.getStatic(key);
        }
        if (result == undefined || result == null) {
            result = defaultValue;
        }
        return result;
    }

    static getDev(key: string): any { return Config.getFromTree(this.devConfig, key); }
    static getOnline(key: string): any { return Config.getFromTree(this.onlineConfig, key); }
    static getStatic(key: string): any { return Config.getFromTree(this.staticConfig, key); }

    private static getFromTree(tree: any, key: string): any
    {
        let parts = key.split('.');
        let current = tree;
        parts.forEach(part =>
        {
            if (current != undefined && current != null && current[part] != undefined) {
                current = current[part];
            } else {
                current = null;
            }
        });
        return current;
    }

    private static setInTree(tree: any, key: string, value: any)
    {
        let parts = key.split('.');
        if (parts.length == 0) { return; }
        let lastPart = parts[parts.length - 1];
        parts.splice(parts.length - 1, 1);
        let current = tree;
        parts.forEach(part =>
        {
            if (current != undefined && current != null && current[part] != undefined) {
                current = current[part];
            } else {
                current = null;
            }
        });
        if (current) {
            current[lastPart] = value;
        }
    }

    static getDevTree(): any { return this.devConfig; }
    static getOnlineTree(): any { return this.onlineConfig; }
    static getStaticTree(): any { return this.staticConfig; }

    static setOnline(key: string, value: any)
    {
        log.debug('Config.setOnline', key);
        return Config.setInTree(this.onlineConfig, key, value);
    }

    static setDevTree(tree: any)
    {
        log.debug('Config.setDevTree');
        this.devConfig = tree;
    }

    static setOnlineTree(tree: any): void
    {
        log.debug('Config.setOnlineTree');
        this.onlineConfig = tree;
    }

    static setStaticTree(tree: any): void
    {
        log.debug('Config.setStaticTree');
        this.staticConfig = tree;
    }

}
