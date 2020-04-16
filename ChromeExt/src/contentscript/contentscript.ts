import './contentscript.scss';
import { Log } from './Log';
import { Connection } from './Connection';
const $ = require('jquery');

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElement: HTMLElement = $('<div id="n3q-id-page" />')[0];
var displayElement: HTMLElement = $('<div class="n3q-display" />')[0];
var logElement: HTMLElement = $('<div class="n3q-log" />')[0];
pageElement.append(displayElement);
// pageElement.append(logElement);
$('body').append(pageElement);

setTimeout(() => {
  var helloElement: HTMLElement = $('<div class="n3q-content n3q-hello">Hello World</div>')[0];
  displayElement.append(helloElement);
}, 500);

var connection = new Connection();
connection.start();
