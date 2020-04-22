export class Log
{
    constructor(private container: HTMLElement) { }

    private static _log(className: string, args: any[]): void
    {
        // let text: string = '';
        // for (let arg of args) { text += arg + ' '; }
        // this.container.append($('<div class="' + className + '"></div>').text(text));
    }

    static info(...args: any[]): void
    {
        console.log(...args);
        // this._log('n3q-logline n3q-logline-info', args);
    }

    static verbose(...args: any[]): void
    {
        console.log(...args);
        // this._log('n3q-logline n3q-logline-verbose', args);
    }

    static error(...args: any[]): void
    {
        console.error(...args);
        // this._log('n3q-logline n3q-logline-error', args);
    }
}
