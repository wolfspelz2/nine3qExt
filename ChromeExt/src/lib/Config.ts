import log = require('loglevel');
import { Environment } from './Environment';
import { Pid } from './ItemProperties';
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
        environment: {
            // NODE_ENV: 'production',
        },
        extension: {
            id: 'cgfkfhdinajjhfeghebnljbanpcjdlkm',
        },
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
            strippedUrlPrefixes: ['https://cdn.weblin.io/?', 'https://cdn.weblin.io/'],
            notStrippedUrlPrefixes: ['https://cdn.weblin.io/v1/', 'https://cdn.weblin.io/sso/'],
        },
        config: {
            serviceUrl: 'https://webex.vulcan.weblin.com/Config',
            updateIntervalSec: 83567,
            checkUpdateIntervalSec: 123,
        },
        httpCache: {
            maxAgeSec: 3600,
            maintenanceIntervalSec: 60,
        },
        points: {
            enabled: false,
            passiveEnabled: true,
            submissionIntervalSec: 300,
            fullLevels: 2,
            fractionalLevels: 1,
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
            pointsOnHover: true,
            defaultStillimageSize: 80,
            defaultAnimationSize: 100,
            vCardAvatarFallback: false,
            vCardAvatarFallbackOnHover: true,
            vidconfUrl: 'https://jitsi.vulcan.weblin.com/{room}#userInfo.displayName="{name}"',
            vidconfBottom: 200,
            vidconfWidth: 600,
            vidconfHeight: 400,
            pokeToastDurationSec: 10,
            privateVidconfToastDurationSec: 60,
            privateChatToastDurationSec: 60,
            errorToastDurationSec: 8,
            applyItemErrorToastDurationSec: 5,
            claimToastDurationSec: 15,
            itemStatsTooltip: true,
            itemStatsTooltipDelay: 500,
            itemStatsTooltipOffset: { x: 3, y: 3 },
        },
        xmpp: {
            service: 'wss://xmpp.vulcan.weblin.com/xmpp-websocket',
            domain: 'xmpp.vulcan.weblin.com',
            maxMucEnterRetries: 4,
            pingBackgroundToKeepConnectionAliveSec: 12,
        },
        avatars: {
            animationsProxyUrlTemplate: 'https://webex.vulcan.weblin.com/Avatar/InlineData?url={url}',
            dataUrlProxyUrlTemplate: 'https://webex.vulcan.weblin.com/Avatar/DataUrl?url={url}',

            animationsUrlTemplate: 'https://webex.vulcan.weblin.com/avatars/{id}/config.xml',
            // animationsUrlTemplate: 'https://webex.vulcan.weblin.com/avatars/gif/{id}/config.xml',

            // list: ['gif/002/sportive03_m', 'gif/002/business03_m', 'gif/002/child02_m', 'gif/002/sportive01_m', 'gif/002/business06_m', 'gif/002/casual04_f', 'gif/002/business01_f', 'gif/002/casual30_m', 'gif/002/sportive03_f', 'gif/002/casual16_m', 'gif/002/casual10_f', 'gif/002/business03_f', 'gif/002/casual03_m', 'gif/002/sportive07_m', 'gif/002/casual13_f', 'gif/002/casual09_m', 'gif/002/casual16_f', 'gif/002/child02_f', 'gif/002/sportive08_m', 'gif/002/casual15_m', 'gif/002/casual15_f', 'gif/002/casual01_f', 'gif/002/casual11_f', 'gif/002/sportive09_m', 'gif/002/casual20_f', 'gif/002/sportive02_f', 'gif/002/business05_m', 'gif/002/casual06_m', 'gif/002/casual10_m', 'gif/002/casual02_f',],
            // randomList: ['gif/002/sportive03_m', 'gif/002/business03_m', 'gif/002/child02_m', 'gif/002/sportive01_m', 'gif/002/business06_m', 'gif/002/casual04_f', 'gif/002/business01_f', 'gif/002/casual30_m', 'gif/002/sportive03_f', 'gif/002/casual16_m', 'gif/002/casual10_f', 'gif/002/business03_f', 'gif/002/casual03_m', 'gif/002/sportive07_m', 'gif/002/casual13_f', 'gif/002/casual09_m', 'gif/002/casual16_f', 'gif/002/child02_f', 'gif/002/sportive08_m', 'gif/002/casual15_m', 'gif/002/casual15_f', 'gif/002/casual01_f', 'gif/002/casual11_f', 'gif/002/sportive09_m', 'gif/002/casual20_f', 'gif/002/sportive02_f', 'gif/002/business05_m', 'gif/002/casual06_m', 'gif/002/casual10_m', 'gif/002/casual02_f',],
            list: ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
            randomList: ['002/sportive03_m', '002/business03_m', '002/child02_m', '002/sportive01_m', '002/business06_m', '002/casual04_f', '002/business01_f', '002/casual30_m', '002/sportive03_f', '002/casual16_m', '002/casual10_f', '002/business03_f', '002/casual03_m', '002/sportive07_m', '002/casual13_f', '002/casual09_m', '002/casual16_f', '002/child02_f', '002/sportive08_m', '002/casual15_m', '002/casual15_f', '002/casual01_f', '002/casual11_f', '002/sportive09_m', '002/casual20_f', '002/sportive02_f', '002/business05_m', '002/casual06_m', '002/casual10_m', '002/casual02_f',],
        },
        identity: {
            url: '',
            digest: '',
            identificatorUrlTemplate: 'https://webex.vulcan.weblin.com/Identity/Generated?avatarUrl={avatarUrl}&nickname={nickname}&digest={digest}&imageUrl={imageUrl}&points={points}',
        },
        roomItem: {
            statsPopupOffset: 10,
            messageMagic: 'uzv65b76t_weblin2screen',
            frameUndockedLeft: 100,
            frameUndockedTop: 100,
        },
        backpack: {
            enabled: true,
            itemSize: 64,
            borderPadding: 4,
            dropZoneHeight: 100,
            itemInfoOffset: { x: 2, y: 2 },
            itemInfoExtended: false,
            deleteToastDurationSec: 100,
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
                config: {
                    apiUrl: 'https://webit.vulcan.weblin.com/rpc',
                    backpackApiUrl: 'https://webit.vulcan.weblin.com/backpack',
                    itemPropertyUrlFilter: {
                        '{image.item.nine3q}': 'https://webit.vulcan.weblin.com/images/Items/',
                        '{iframe.item.nine3q}': 'https://webit.vulcan.weblin.com/ItemFrame/',
                    },
                },
            }
        },
        i18n: {
            defaultLanguage: 'en-US',
            languageMapping: {
                'de': 'de-DE',
            },
            translations: {
                'en-US': {
                    'Extension.Disable': 'Disable weblin.io',
                    'Extension.Enable': 'Enable weblin.io',

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

                    'Menu.Menu': 'Menu',
                    'Menu.Settings': 'Settings',
                    'Menu.Stay Here': 'Stay on tab change',
                    'Menu.Backpack': 'Stuff',
                    'Menu.Chat Window': 'Chat History',
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

                    'PrivateVidconf.Private Videoconference with': 'PrivateVidconf.Private Videoconference with',

                    'Vidconfwindow.Video Conference': 'Video Conference',
                    'Settingswindow.Settings': 'Settings',
                    'BackpackWindow.Inventory': 'Your Stuff',

                    'Backpack.Shredder': 'Shredder',
                    'Backpack.Go to item': 'Go there',
                    'Backpack.Derez item': 'Pick up',

                    'Toast.Do not show this message again': 'Do not show this message again',
                    'Toast.greets': '...greeted you',
                    'Toast.tousles': '...tousled you',
                    'Toast.nudges': '...nudged you',
                    'Toast.Your claim has been removed': 'Your claim has been removed',
                    'Toast.A stronger A stronger item just appeared': 'A stronger item just appeared.',
                    'Toast.greet back': 'Greet back',
                    'Toast.tousle back': 'Tousle back',
                    'Toast.nudge back': 'Nudge back',
                    'Toast.Really delete?': 'Really delete?',
                    'Toast.Yes, delete item': 'Yes, delete item',
                    'Toast.No, keep it': 'No, keep it',
                    'Toast.Wants to start a private videoconference': 'Invites you to a private videoconference',
                    'Toast.Refuses to join the private videoconference': 'Refuses to join the videoconference',
                    'Toast.Accept': 'Accept',
                    'Toast.Decline': 'Decline',

                    // ['ErrorFact.' + ItemException.Fact[ItemException.Fact.Error]]: 'Error',
                    'ErrorFact.Error': 'Error',

                    'ErrorFact.NotRezzed': 'Item not dropped',
                    'ErrorFact.NotDerezzed': 'Failed to pick up item',
                    'ErrorFact.NotAdded': 'Item not added',
                    'ErrorFact.NotChanged': 'Item not changed',
                    'ErrorFact.NoItemsReceived': 'No items recevied',
                    'ErrorFact.NotExecuted': 'Not executed',
                    'ErrorFact.NotCreated': 'No item created',
                    'ErrorFact.NotApplied': 'Item not applied',
                    'ErrorFact.ClaimFailed': 'Failed to claim the page',
                    'ErrorFact.NotTransferred': 'Item not transferred',

                    'ErrorReason.UnknownReason': 'Unknown reason :-(',
                    'ErrorReason.ItemAlreadyRezzed': 'Item already on page.',
                    'ErrorReason.ItemNotRezzedHere': 'Item is not on this page',
                    'ErrorReason.ItemsNotAvailable': 'Items not available. The feature may be disabled.',
                    'ErrorReason.ItemDoesNotExist': 'This is an not a known item.',
                    'ErrorReason.NoUserId': 'No user id. Maybe not logged in as item user.',
                    'ErrorReason.SeeDetail': '',
                    'ErrorReason.InvalidChecksum': 'Invalid checksum. Not a valid item.',
                    'ErrorReason.StillInCooldown': 'Still in cooldown period.',
                    'ErrorReason.InvalidPropertyValue': 'Property invalid.',
                    'ErrorReason.NotYourItem': 'This is not your item.',
                    'ErrorReason.ItemMustBeStronger': 'Your item is not stronger than the other.',
                    'ErrorReason.ItemIsNotTransferable': 'Item not transferable.',
                    'ErrorReason.NoMatch': 'Item do not match.',

                    'ErrorDetail.Applier.Apply': 'Applying an item to another',
                    'ErrorDetail.Pid.Id': 'Id',
                    'ErrorDetail.Pid.Actions': 'Actions',

                    'ItemPid.Label': 'Label',
                    'ItemPid.Comment': 'Comment',
                    'ItemPid.ClaimStrength': 'Strength',
                    'ItemPid.ClaimName': 'For',
                    'ItemPid.OwnerName': 'Owner',
                    'ItemPid.DispenserAvailable': 'Remaining',
                    'ItemPid.DispenserCooldownSec': 'Cooldown',
                    'ItemPid.NicknameText': 'Name',
                    'ItemPid.PointsTotal': 'Collected',
                    'ItemPid.PointsCurrent': 'Available',
                    'ItemPid.RezzedDestination': 'Page',
                    'ItemPid.IsRezzed': 'On page',
                    'ItemPid.CoinCurrency': 'Currency',
                    'ItemPid.CoinAmount': 'Amount',
                    'ItemPid.IframeUrl': 'URL',
                    'ItemPid.IframeAuto': 'Autostart',

                    'ItemValue.true': 'Yes',
                    'ItemValue.false': 'No',

                    'ItemLabel.Dot1': '1-Point',
                },
                'de-DE': {
                    'Extension.Disable': 'weblin.io ausschalten',
                    'Extension.Enable': 'weblin.io einschalten',

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

                    'Menu.Menu': 'Menü',
                    'Menu.Settings': 'Einstellungen',
                    'Menu.Stay Here': 'Bleiben bei Tabwechsel',
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

                    'PrivateVidconf.Private Videoconference with': 'PrivateVidconf.Private Videokonferenz mit',

                    'Vidconfwindow.Video Conference': 'Videokonferenz',
                    'Settingswindow.Settings': 'Einstellungen',
                    'BackpackWindow.Inventory': 'Deine Gegenstände',

                    'Backpack.Shredder': 'Schredder',
                    'Backpack.Go to item': 'Dort hingehen',
                    'Backpack.Derez item': 'Einsammeln',

                    'Toast.Do not show this message again': 'Diese Nachricht nicht mehr anzeigen',
                    'Toast.greets': '...hat dich gegrüßt',
                    'Toast.tousles': '...hat dich gewuschelt',
                    'Toast.nudges': '...hat dich angestupst',
                    'Toast.Your claim has been removed': 'Der Anspruch wurde zurückgenommen',
                    'Toast.A stronger item just appeared': 'Ein stärkerer Gegenstand wurde gerade installiert.',
                    'Toast.greet back': 'Zurück grüßen',
                    'Toast.tousle back': 'Zurück wuscheln',
                    'Toast.nudge back': 'Zurück stupsen',
                    'Toast.Really delete?': 'Wirklich löschen?',
                    'Toast.Yes, delete item': 'Ja, Gegenstand löschen',
                    'Toast.No, keep it': 'Nein, behalten',
                    'Toast.Wants to start a private videoconference': 'Lädt zu einer privaten Videokonferenz ein',
                    'Toast.Refuses to join the private videoconference': 'Lehnt die Videokonferenz ab',
                    'Toast.Accept': 'Annehmen',
                    'Toast.Decline': 'Ablehnen',

                    'ErrorFact.Error': 'Fehler',
                    'ErrorFact.NotRezzed': 'Ablegen fehlgeschlagen',
                    'ErrorFact.NotDerezzed': 'Von der Seite nehmen fehlgeschlagen',
                    'ErrorFact.NotAdded': 'Gegenstand nicht hinzugefügt',
                    'ErrorFact.NotChanged': 'Gegenstand nicht geändert',
                    'ErrorFact.NoItemsReceived': 'Keine Gegenstände bekommen',
                    'ErrorFact.NotExecuted': 'Nicht ausgeführt',
                    'ErrorFact.NotCreated': 'Kein Gegenstand erstellt',
                    'ErrorFact.NotApplied': 'Gegenstand nicht angewendet',
                    'ErrorFact.ClaimFailed': 'Anspruch nicht durchgesetzt',
                    'ErrorFact.NotTransferred': 'Gegenstand nicht übertragen',

                    'ErrorReason.UnknownReason': 'Grund unbekannt :-(',
                    'ErrorReason.ItemAlreadyRezzed': 'Gegenstand ist schon auf einer Seite.',
                    'ErrorReason.ItemNotRezzedHere': 'Gegenstand ist nicht auf dieser Seite',
                    'ErrorReason.ItemsNotAvailable': 'Keine Gegenstände verfügbar. Die Funktion ist vielleicht nicht eingeschaltet.',
                    'ErrorReason.ItemDoesNotExist': 'Dieser Gegenstand ist nicht bekannt.',
                    'ErrorReason.NoUserId': 'Keine Benutzerkennung. Möglicherweise nicht als Benutzer von Gegenständen angemeldet.',
                    'ErrorReason.SeeDetail': '',
                    'ErrorReason.InvalidChecksum': 'Falsche Checksumme. Kein zulässiger Gegenstand.',
                    'ErrorReason.StillInCooldown': 'Braucht noch Zeit, um sich zu erholen.',
                    'ErrorReason.InvalidPropertyValue': 'Falsche Eigenschaft.',
                    'ErrorReason.NotYourItem': 'Das ist nicht dein Gegenstand.',
                    'ErrorReason.ItemMustBeStronger': 'Der Gegenstand ist nicht stärker als der andere.',
                    'ErrorReason.ItemIsNotTransferable': 'Der Gegenstand ist nicht übertragbar.',
                    'ErrorReason.NoMatch': 'Gegenstände passen nicht.',

                    'ErrorDetail.Applier.Apply': 'Beim Anwenden eines Gegenstands auf einen anderen.',
                    'ErrorDetail.Pid.Id': 'Id',
                    'ErrorDetail.Pid.Actions': 'Aktionen',

                    'ItemPid.Label': 'Bezeichnung',
                    'ItemPid.Comment': 'Kommentar',
                    'ItemPid.ClaimStrength': 'Stärke',
                    'ItemPid.ClaimName': 'Für',
                    'ItemPid.OwnerName': 'Besitzer',
                    'ItemPid.DispenserAvailable': 'Übrig',
                    'ItemPid.DispenserCooldownSec': 'Wartezeit',
                    'ItemPid.NicknameText': 'Name',
                    'ItemPid.PointsTotal': 'Gesammelt',
                    'ItemPid.PointsCurrent': 'Verfügbar',
                    'ItemPid.RezzedDestination': 'Webseite',
                    'ItemPid.IsRezzed': 'Auf Webseite',
                    'ItemPid.CoinCurrency': 'Währung',
                    'ItemPid.CoinAmount': 'Betrag',
                    'ItemPid.IframeUrl': 'URL',
                    'ItemPid.IframeAuto': 'Automatisch',

                    'ItemValue.true': 'Ja',
                    'ItemValue.false': 'Nein',

                    'ItemLabel.Points': 'Punkte',
                    'ItemLabel.Dot1': '1-Punkt',
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
