import log = require('loglevel');

interface PanicNowCallback { (): void }

export class Panic
{
    private static callback: PanicNowCallback;
    private static inCallback: boolean = false;

    static onNow(callback: PanicNowCallback): void
    {
        Panic.callback = callback;
    }

    static now(): void
    {
        if (!Panic.inCallback) {
            Panic.inCallback = true;
            if (Panic.callback != undefined) {
                log.info('I am not feeling well');
                Panic.callback();
            }
        }
    }
}