interface IChange extends Array<string> { }
interface IChanges extends Array<IChange> { }
interface IRelease extends Array<string | string | IChanges> { 0: string; 1: string; 2: IChanges }
interface IHistory extends Array<IRelease> { }

export class _Changes
{
    static data: IHistory = [
        ['1.0.7', 'unsafe-eval', [
            ['Add', 'Manage page claims'],
            ['Add', 'Detecatable for embedded'],
            ['Change', 'Skip call to item config'],
            ['Fix', 'Remove unneccesary unsafe-eval from content_security_policy (for MS Edge Addons Store)'],
        ]],
        ['1.0.6', 'PrivateChat Greet RecvDependentItems', [
            ['Add', 'Allow vidconf fullscreen'],
            ['Add', 'Private chat'],
            ['Add', 'Greet'],
            ['Add', 'Show user dependent items'],
            ['Add', 'Persist stay-on-tab-change flag'],
            ['Add', 'Persist open backpack'],
            ['Add', 'Undock vidconf window'],
            ['Add', 'vpi ignore (e.g. all google)'],
            ['Change', 'RallySpeaker URL variables, iframe allows 4 vidconf'],
            ['Change', 'Much longer chat bubble duration, 2 min. total instead of 20 sec.'],
            ['Change', 'Update to item inventory grain and chat room based inventory view (internal).'],
            ['Fix', 'Chat window focused input style'],
            ['Fix', 'Message replication by tab change'],
        ]],
        ['1.0.5', 'SPA', [
            ['Add', 'Stay in the room if vidconf|inventory|chat are open'],
            ['Change', 'Use new prod cluster'],
            ['Change', 'Preload only idle, move animations'],
            ['Fix', 'No avatar in sleep state without sleep animation'],
            ['Fix', 'Navigate on single page applications (check URL continuously)'],
        ]],
        ['1.0.4', 'Vidconf', [
            ['Add', 'Videoconf demo'],
            ['Add', 'Support for animationsUrl in presence-x-vp:props'],
            ['Add', 'Clickable chat links'],
            ['Add', 'XMPP vCard on hover'],
            ['Add', 'Avatar and item stacking order'],
            ['Change', 'Prefer presence-x-vp:props attributes over identity'],
            ['Change', 'Settings title to brand name (lowercase)'],
            ['Change', 'Variable avatar size'],
            ['Fix', 'Window position on small screens'],
            ['Fix', 'Url mapping (JS undefined)'],
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
