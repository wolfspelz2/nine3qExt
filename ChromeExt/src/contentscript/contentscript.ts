import './contentscript.scss';
import { Log } from './Log';
import { Connection } from './Connection';
const $ = require('jquery');

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElement: HTMLElement = $('<div id="n3q-id-page" />')[0];

var displayElement: HTMLElement = $('<div class="n3q-display" />')[0];
pageElement.append(displayElement);

// var logElement: HTMLElement = $('<div class="n3q-log" />')[0];
// pageElement.append(logElement);

var roomElement: HTMLElement = $('<div class="n3q-content n3q-hello">Hello World</div>')[0];
displayElement.append(roomElement);

var enterButton: HTMLButtonElement = $('<button id="n3q-id-enter">enter</button>')[0];
roomElement.append(enterButton);

$('body').append(pageElement);

var connection = new Connection();
connection.start();

$('#n3q-id-enter').click(() =>
{
  connection.enterRoomByJid('2883fcb56d5ac9d5e7adad03a38bce8a362dbdc2@muc4.virtual-presence.org');
});
