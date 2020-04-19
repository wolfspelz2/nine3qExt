import './contentscript.scss';
const $ = require('jquery');
import { Log } from './Log';
import { App } from './App';
import { Platform } from './Platform';

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElement: HTMLElement = $('<div id="n3q-id-page" />')[0];

// var logElement: HTMLElement = $('<div class="n3q-log" />')[0];
// pageElement.append(logElement);

$('body').append(pageElement);

new App(pageElement).start();


