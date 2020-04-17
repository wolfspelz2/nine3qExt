import './contentscript.scss';
import { Log } from './Log';
import { App } from './App';
const $ = require('jquery');

const isContentscript: boolean = true;
console.log('Contentscript', isContentscript);

var pageElement: HTMLElement = $('<div id="n3q-id-page" />')[0];

// var logElement: HTMLElement = $('<div class="n3q-log" />')[0];
// pageElement.append(logElement);

$('body').append(pageElement);

// {
//   let entityElement = <HTMLDivElement>$('<div class="n3q-entity"></div>')[0];
//   let e = <HTMLElement>$('<div class="n3q-centertable"><div class="n3q-centercell"></div></div>')[0];
//   let centerElement = $(e).find('div.n3q-centercell')[0];
//   entityElement.appendChild(e);
//   displayElement.appendChild(entityElement);
//   entityElement.style.left = '100px';
// }

// {
//   let entityElement = <HTMLDivElement>$('<div class="n3q-entity"></div>')[0];
//   let avatarElement = <HTMLDivElement>$('<div class="n3q-avatar" />')[0];
//   entityElement.appendChild(avatarElement);
//   displayElement.appendChild(entityElement);
//   entityElement.style.left = '100px';
// }

new App(pageElement).start();
