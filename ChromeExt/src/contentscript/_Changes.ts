interface IChange extends Array<string> { }
interface IChanges extends Array<IChange> { }
interface IRelease extends Array<string | string | IChanges> { 0: string; 1: string; 2: IChanges }
interface IHistory extends Array<IRelease> { }

export class _Changes
{
    static data: IHistory = [
        ['1.0.X', 'TestWindow', [
            ['Add', 'Test window'],
        ]],
        ['1.0.2', 'StoreFix', [
            ['Change', 'Remove http request'],
            ['Add', 'VPI resolver'],
        ]],
        ['1.0.1', 'Dispatcher', [
            ['Change', 'Backgound room/tab dispatcher'],
        ]],
        ['1.0.0', 'First release', [
            ['Add', 'Basic function'],
        ]],
    ];

    static getLines(): Array<string>
    {
        let lines = [];

        this.data.forEach(release =>
        {
            lines.push(release[0] + ' ' + release[1]);
            release[2].forEach(change =>
            {
                { lines.push(change[0] + ' ' + change[1]); }
            });
            lines.push('');
        });

        return lines;
    }
}
