import './contentscript.scss';

const isThisContentscript: boolean = true;
console.log('isThisContentscript', isThisContentscript);

var page = document.createElement('div');
page.id = 'n3qPage';

var display = document.createElement('div');
display.id = 'n3qDisplay';
page.appendChild(display);

document.body.appendChild(page);

var hello = document.createElement('div');
hello.className = 'n3qContent n3qHello';
hello.innerHTML = 'Hello World';
display.appendChild(hello);
