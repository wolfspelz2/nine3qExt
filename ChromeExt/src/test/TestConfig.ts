import { expect } from 'chai';
import { Config } from '../lib/Config';


export class TestConfig
{
    get_from_static_config()
    {
        Config.setAllStaticConfig({
            'xmpp': {
                'service': 'wss://xmpp.weblin.sui.li/xmpp-websocket',
                'domain': 'xmpp.weblin.sui.li',
                'user': 'at4peaaa1o74iuv2us4h4v8u4k',
                'pass': 'e8149223956afdd79a4345fcdf884e9c502c1bea'
            },
            'avatars': {
                'baseUrl': 'https://avatar.weblin.sui.li/avatar/?url=http://avatar.zweitgeist.com/gif/',
                'list': [
                    '002/sportive03_m',
                    '002/business03_m',
                    '002/child02_m',
                ]
            }
        });
        expect(Config.get('xmpp.user', 'no-value')).to.equal('at4peaaa1o74iuv2us4h4v8u4k');
        expect(Config.get('avatars.baseUrl', 'no-value')).to.equal('https://avatar.weblin.sui.li/avatar/?url=http://avatar.zweitgeist.com/gif/');
        expect(Config.get('avatars.list', 'no-value')[0]).to.equal('002/sportive03_m');
    }

    get_from_online_config_with_static_fallback()
    {
        Config.setAllStaticConfig({
            'notInOnlineConfig1': {
                'notInOnlineConfig2': 'notInOnlineConfig3',
            },
            'xmpp': {
                'service': 'no-service',
            },
        });
        Config.setAllOnlineConfig({
            'xmpp': {
                'service': 'wss://xmpp.weblin.sui.li/xmpp-websocket',
                'domain': 'xmpp.weblin.sui.li',
                'user': 'at4peaaa1o74iuv2us4h4v8u4k',
                'pass': 'e8149223956afdd79a4345fcdf884e9c502c1bea'
            },
            'avatars': {
                'baseUrl': 'https://avatar.weblin.sui.li/avatar/?url=http://avatar.zweitgeist.com/gif/',
                'list': [
                    '002/sportive03_m',
                    '002/business03_m',
                    '002/child02_m',
                ]
            }
        });
        expect(Config.getStaticConfig('notInOnlineConfig1.notInOnlineConfig2')).to.equal('notInOnlineConfig3');
        expect(Config.getStaticConfig('xmpp.service')).to.equal('no-service');
        expect(Config.getOnlineConfig('xmpp.service')).to.equal('wss://xmpp.weblin.sui.li/xmpp-websocket');
        expect(Config.get('xmpp.service','no-value')).to.equal('wss://xmpp.weblin.sui.li/xmpp-websocket');
    }
}