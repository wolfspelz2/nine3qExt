interface IChange extends Array<string> { }
interface IChanges extends Array<IChange> { }
interface IRelease extends Array<string | string | IChanges> { 0: string; 1: string; 2: IChanges }
interface IHistory extends Array<IRelease> { }

export class _Changes
{
    static data: IHistory = [
        ['1.0.X', 'x', [
            ['Add', 'Support for animationsUrl in presence-x-vp:props'],
            ['Change', 'Prefer presence-x-vp:props attributes over identity'],
            ['Fix', 'Window position on small screens'],
        ]],
        ['1.0.3', 'XmppWindow SettingsDialog', [
            ['Add', 'Change history'],
            ['Add', 'Xmpp console window'],
            ['Add', 'Chat console'],
            ['Add', 'In-screen settings dialog + menu entry'],
            ['Add', 'Support for imageUrl in presence-x-vp:props'],
            ['Add', 'Computed identity digest to presence-x-firebat:...'],
            ['Fix', 'Duplicate presence-x-history'],
            ['Fix', 'Avatar position in presence may be float value instead of int'],
        ]],
        ['1.0.2', 'StoreFix', [
            ['Change', 'VPI query http request to https'],
            ['Add', 'VPI resolver'],
        ]],
        ['1.0.1', 'BackgoundDispatcher', [
            ['Change', 'Backgound room/tab dispatcher'],
        ]],
        ['1.0.0', 'MVP', [
            ['Add', 'Basic function'],
        ]],
    ];
}
