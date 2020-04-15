import "./contentscript.scss";

const isThisContentscript: boolean = true;
console.log("isThisContentscript", isThisContentscript);

var div = document.createElement("div");
div.style.width = "100%";
div.style.height = "100px";
div.style.position = "absolute";
div.style.left = "0px";
div.style.bottom = "0px";
div.style.backgroundColor = "transparent";
div.style.border = "2px solid black";
div.style.zIndex = "1000000000";
div.innerHTML = "Hello World";
document.body.appendChild(div); 