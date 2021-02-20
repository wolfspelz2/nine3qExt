import { BackgroundMessage } from '../lib/BackgroundMessage';
import { Client } from '../lib/Client';
import { Config } from '../lib/Config';
import { Translator } from '../lib/Translator';
import { ContentApp } from './ContentApp';
import { Room } from './Room';
import { TestWindow } from './TestWindow';
import { VpiResolver } from './VpiResolver';

export interface ChatConsoleOut { (data: any): void }

export class ChatConsoleContext
{
    app: ContentApp;
    room: Room;
    out: ChatConsoleOut;
}

export class ChatConsole
{
    static isChatCommand(text: string, context: ChatConsoleContext): boolean
    {
        if (text.substring(0, 1) == '/') {
            return ChatConsole.chatCommand(text, context);
        }
        return false;
    }

    private static chatCommand(text: string, context: ChatConsoleContext): boolean
    {
        let isHandled = false;

        var parts: string[] = text.split(' ');
        if (parts.length < 1) { return; }
        var cmd: string = parts[0];

        switch (cmd) {
            case '/help':
            case '/?':
                this.out(context, ['>', text]);
                this.out(context, [
                    ['help', '/xmpp'],
                    ['help', '/room'],
                    ['help', '/changes'],
                ]);
                isHandled = true;
                break;
            case '/xmpp':
                context.app?.showXmppWindow();
                isHandled = true;
                break;
            case '/chat':
                context.app?.showChatWindow();
                isHandled = true;
                break;
            case '/items':
            case '/backpack':
            case '/stuff':
            case '/things':
                context.app?.showBackpackWindow();
                isHandled = true;
                break;
            case '/video':
            case '/vid':
            case '/vidconf':
            case '/conf':
            case '/jitsi':
                context.app?.showVidconfWindow();
                isHandled = true;
                break;
            case '/test':
                new TestWindow(context.app).show({});
                isHandled = true;
                break;
            case '/changes':
                context.app?.showChangesWindow();
                isHandled = true;
                break;
            case '/info':
                ChatConsole.out(context, [
                    ['info', Client.getDetails()]
                ]);
                isHandled = true;
                break;
            case '/room':
                context.room?.getInfo().forEach(line =>
                {
                    ChatConsole.out(context, [line[0], line[1]]);
                });
                isHandled = true;
                break;
            case '/map':
                this.out(context, ['>', text]);
                let vpi = new VpiResolver(BackgroundMessage, Config);
                let language: string = Translator.mapLanguage(navigator.language, lang => { return Config.get('i18n.languageMapping', {})[lang]; }, Config.get('i18n.defaultLanguage', 'en-US'));
                let translator = new Translator(Config.get('i18n.translations', {})[language], language, Config.get('i18n.serviceUrl', ''));
                vpi.language = Translator.getShortLanguageCode(translator.getLanguage());
                let lines = new Array<[string, string]>();
                let url = parts[1];
                lines.push(['URL', url]);
                vpi.trace = (key, value) => { lines.push([key, value]); };
                vpi.map(url).then(location =>
                {
                    lines.forEach(line =>
                    {
                        ChatConsole.out(context, [line[0], line[1]]);
                    });
                });
                isHandled = true;
                break;
        }

        return isHandled;
    }

    private static out(context: ChatConsoleContext, data: any): void
    {
        if (context.out) { context.out(data); }
    }
}
