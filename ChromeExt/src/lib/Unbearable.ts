import log = require('loglevel');

interface UnbearableProblemCallback { (): void }

export class Unbearable
{
    private static problemCallback: UnbearableProblemCallback;
    private static inProblemCallback: boolean = false;

    static onProblem(callback: UnbearableProblemCallback): void
    {
        Unbearable.problemCallback = callback;
    }

    static problem(): void
    {
        if (!Unbearable.inProblemCallback) {
            Unbearable.inProblemCallback = true;
            if (Unbearable.problemCallback != undefined) {
                log.info('Unbearable.problem');
                Unbearable.problemCallback();
            }
        }
    }
}