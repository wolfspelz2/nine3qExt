import './contentscript.scss';

const isContentscript: boolean = true;
console.log('contentscript', isContentscript);

var page: HTMLElement = document.createElement('div');
page.id = 'n3qPage';

var display: HTMLElement = document.createElement('div');
display.id = 'n3qDisplay';
page.appendChild(display);

document.body.appendChild(page);

setTimeout(() => {
  var hello: HTMLElement = document.createElement('div');
  hello.className = 'n3qContent n3qHello';
  hello.innerHTML = 'Hello World';
  display.appendChild(hello);
}, 1000);
