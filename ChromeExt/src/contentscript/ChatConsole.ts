import { Client } from '../lib/Client';
import { ContentApp } from './ContentApp';
import { Room } from './Room';
import { TestWindow } from './TestWindow';

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
                ChatConsole.out(context, [
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
                context.app?.getRoom().showChatWindow(context.app?.getRoom().getParticipant(context.app?.getRoom().getMyNick()).getElem());
                isHandled = true;
                break;
            case '/items':
            case '/backpack':
            case '/stuff':
            case '/things':
                context.app?.showBackpackWindow(context.app?.getRoom().getParticipant(context.app?.getRoom().getMyNick()).getElem());
                isHandled = true;
                break;
            case '/video':
            case '/vid':
            case '/vidconf':
            case '/conf':
            case '/jitsi':
                context.app?.getRoom().showVideoConference(context.app?.getRoom().getParticipant(context.app?.getRoom().getMyNick()).getElem(), context.app?.getRoom().getParticipant(context.app?.getRoom().getMyNick()).getDisplayName());
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
        }

        return isHandled;
    }

    private static out(context: ChatConsoleContext, data: any): void
    {
        if (context.out) { context.out(data); }
    }
}
