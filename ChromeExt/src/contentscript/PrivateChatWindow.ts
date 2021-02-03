import * as $ from 'jquery';
import 'webpack-jquery-ui';
// import markdown = require('markdown');
import log = require('loglevel');
import { as } from '../lib/as';
import { Utils } from '../lib/Utils';
import { Config } from '../lib/Config';
import { Environment } from '../lib/Environment';
import { ContentApp } from './ContentApp';
import { ChatWindow } from './ChatWindow';
import { Participant } from './Participant';

export class PrivateChatWindow extends ChatWindow
{
    constructor(app: ContentApp, private participant: Participant)
    {
        super(app, participant.getRoom());
    }

    async show(options: any)
    {
        if (options.titleText == null) { options.titleText = this.app.translateText('PrivateChat.Private Chat with', 'Private Chat with') + ' ' + this.participant.getDisplayName(); }

        await super.show(options);
    }

    protected sendChat(): void
    {
        var text: string = as.String($(this.chatinInputElem).val(), '');
        if (text != '') {

            let nick = this.participant.getRoomNick();

            let name = this.room.getParticipant(this.room.getMyNick()).getDisplayName();

            this.room?.sendPrivateChat(text, nick);

            this.addLine(nick + Date.now(), name, text);

            $(this.chatinInputElem)
                .val('')
                .focus()
                ;
        }
    }
}
