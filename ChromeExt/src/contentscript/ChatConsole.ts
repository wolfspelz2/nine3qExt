import { ContentApp } from './ContentApp';
import { Room } from './Room';

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
                    ['help', '/roominfo']
                ]);
                isHandled = true;
                break;
            case '/xmpp':
                context.app?.showXmppWindow();
                isHandled = true;
                break;
            case '/roominfo':
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
