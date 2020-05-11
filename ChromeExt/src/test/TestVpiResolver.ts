import { expect } from 'chai';
import { VpiResolver, VpiResolverEvaluateResultType, VpiResolverConfigProvider, VpiResolverUrlFetcher } from '../contentscript/VpiResolver';
import { FetchUrlResponse } from '../lib/BackgroundMessage';

export class TestVpiResolver
{
    VpiResolver_evaluate_firstMatched_delegate()
    {
        let documentUrl = 'https://www.weblin.com/';
        let vpiUrl = 'https://lms.virtual-presence.org/v7/root.xml';
        let vpiData = `<?xml version="1.0" encoding="UTF-8"?> <vpi xmlns='http://virtual-presence.org/schemas/vpi'> <delegate match='^(http://test[^.]*\\.weblin\\.com)($|/.*$)'> <uri>test.xml</uri> </delegate> <delegate match='^(https?://([^/]*[.])?((zweitgeist|weblin)\\.(com|de)))($|/.*$)'> <uri>weblin.xml</uri> </delegate> <!-- handle IP4 addresses --> <delegate match='^https?://[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+(:[0-9]+)?($|/.*$)'> <uri>ip4/index.xml</uri> </delegate> <!-- pipe everything thru the brand name handler --> <delegate> <uri>name/index.xml</uri> </delegate> </vpi>`;
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).status).to.equal(VpiResolverEvaluateResultType.Delegate);
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).delegate).to.equal('https://lms.virtual-presence.org/v7/weblin.xml');
    }

    VpiResolver_evaluate_default_delegate()
    {
        let documentUrl = 'https://www.galactic-developments.de/';
        let vpiUrl = 'https://lms.virtual-presence.org/v7/root.xml';
        let vpiData = `<?xml version="1.0" encoding="UTF-8"?> <vpi xmlns='http://virtual-presence.org/schemas/vpi'> <delegate match='^(http://test[^.]*\\.weblin\\.com)($|/.*$)'> <uri>test.xml</uri> </delegate> <delegate match='^(https?://([^/]*[.])?((zweitgeist|weblin)\\.(com|de)))($|/.*$)'> <uri>weblin.xml</uri> </delegate> <!-- handle IP4 addresses --> <delegate match='^https?://[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+(:[0-9]+)?($|/.*$)'> <uri>ip4/index.xml</uri> </delegate> <!-- pipe everything thru the brand name handler --> <delegate> <uri>name/index.xml</uri> </delegate> </vpi>`;
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).status).to.equal(VpiResolverEvaluateResultType.Delegate);
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).delegate).to.equal('https://lms.virtual-presence.org/v7/name/index.xml');
    }


    VpiResolver_evaluate_catchall_location()
    {
        let documentUrl = 'https://www.galactic-developments.de/';
        let vpiUrl = 'https://lms.virtual-presence.org/v7/default.xml';
        let vpiData = `<?xml version="1.0" encoding="UTF-8"?> <vpi xmlns='http://virtual-presence.org/schemas/vpi'> <location match='^(https?://((w)+([0-9]*)\\.)?([^/]+\\.([a-zA-Z]+)))($|/.*$)'> <service>jabber:muc4.virtual-presence.org</service> <name hash="true">\\5</name> <destination>\\1</destination> <topology level="domain"/> <displayoptions><usercount multiple='15' crowd='40' massive='80'/></displayoptions> </location> <location match='.*'> <service>jabber:muc4.virtual-presence.org</service> <ignore/> <!--name>nirvana</name--> </location> </vpi>`;
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).status).to.equal(VpiResolverEvaluateResultType.Location);
        expect(new VpiResolver(null).evaluate(documentUrl, vpiUrl, vpiData).location).to.equal('xmpp:d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
    }

    async VpiResolver_map_weblin()
    {
        let resolver = new VpiResolver(new TestDataProvider(), new TestConfigInstance_map_galdev());
        let mapped = await resolver.map('https://www.weblin.com/home.php?room=de2');
        expect(mapped).to.equal('xmpp:zweitgeistde2@muc4.virtual-presence.org');
    }

    async VpiResolver_map_galdev()
    {
        let resolver = new VpiResolver(new TestDataProvider(), new TestConfigInstance_map_galdev());
        let mapped = await resolver.map('https://www.galactic-developments.de/');
        expect(mapped).to.equal('xmpp:d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org');
    }
}

class TestDataProvider implements VpiResolverUrlFetcher
{
    private data = {
        'https://lms.virtual-presence.org/v7/root.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
            <delegate match='^(http://test[^.]*\\.weblin\\.com)($|/.*$)'>
            <uri>test.xml</uri>
            </delegate>
        
            <delegate match='^(https?://([^/]*[.])?((zweitgeist|weblin)\\.(com|de)))($|/.*$)'>
            <uri>weblin.xml</uri>
            </delegate>
        
            <!-- handle IP4 addresses -->
            <delegate match='^https?://[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+(:[0-9]+)?($|/.*$)'>
            <uri>ip4/index.xml</uri>
            </delegate>
        
            <!-- pipe everything thru the brand name handler -->
            <delegate>
            <uri>name/index.xml</uri>
            </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/weblin.xml': `<?xml version="1.0" encoding="utf-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <Info
           Version="1"
           Description="This is the mapping for the weblin domains"
           AdminContact="admin@weblin.com"
           TechContact="admin@weblin.com"
           TimeToLive="3600"
           />
        
          <location match='^(https?://blog\\.weblin\\.(com|de))($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name>zweitgeist-blog</name>
            <destination>http://blog.weblin.com/</destination>
          </location>
          <location match='^(https?://de\\.blog\\.weblin\\.com)($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name>zweitgeist-blog</name>
            <destination>http://de.blog.weblin.com/</destination>
          </location>
        
          <location match='^(https?://(.*[.])?((zweitgeist|weblin)\\.(com|de)))($|/.*room=([a-z0-9]+).*|.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name>zweitgeist\\7</name>
            <destination>http://www.weblin.com/</destination>
            <topology level="selection"/>
          </location>
        
          <delegate>
            <uri>default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/name/index.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <delegate match='^https?://([^./]+\\.)*((g|t)[^./]*)\\.([^./]+)\\.(ee|il|mx|tr|ua|uk)(:[0-9]+)?($|/.*$)'>
            <uri>sld/\\3/index.xml</uri>
          </delegate>
          
          <delegate match='^https?://([^./]+\\.)*((f|g|h|m|n|t|y)[^./]*)\\.([^./]+)(:[0-9]+)?($|/.*$)'>
            <uri>\\3/index.xml</uri>
          </delegate>
        
          <delegate>
            <uri>default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/name/g/index.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <delegate match='^https?://([^/]+\\.)?(g(o)[^.]*)\\.([^.]+)(:[0-9]+)?($|/.*$)'>
            <uri>\\3/index.xml</uri>
          </delegate>
        
          <delegate>
            <uri>../default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/name/default.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <!-- After we tried the brand names -->
        
          <!-- Let certain ccSLD domains handle it hierarchically -->
          <delegate match='^https?://[^/:]*\\.([^.]+)\\.(br|in)(:[0-9]+)?($|/.*$)'>
            <uri>../sld/\\2/index.xml</uri>
          </delegate>
        
          <!-- ... other domains in the ccTLD hierarchy ...-->
          <delegate match='^https?://[^/:]*\\.(com|de|eu|lt|net|org|pl|ws)(:[0-9]+)?($|/.*$)'>
            <uri>../tld/\\1/index.xml</uri>
          </delegate>
        
          <!-- ... or go straight to the exit -->
          <delegate>
            <uri>../default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/tld/de/index.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <!-- Descend using the first letter of the domain -->
          <delegate match='^https?://([^/]+\\.)?((b|d|e|i|p|r|w|y)[^.]*)\\.([^.]+)(:[0-9]+)?($|/.*$)'>
            <uri>\\3/index.xml</uri>
          </delegate>
        
          <delegate>
            <uri>default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/tld/de/default.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <location match='^https?://([^/]+\\.)?ebay\\.de($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name>ebay.de</name>
            <topology level="site"/>
            <!--delay sec="0"/-->
          </location>
        
          <location match='^(https?://(.*\\.)?(icewars\\.de))($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name hash="SHA1">\\3</name>
            <destination>http://www.icewars.de/</destination>
            <topology level="domain"/>
            <!--delay sec="0"/-->
          </location>
        
          <location match='^(https?://(.*\\.)?(spiegel\\.de))($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name hash="SHA1">\\3</name>
            <destination>http://www.spiegel.de/</destination>
            <topology level="domain"/>
        
            <select>
              <option suffix='1-no-lang' title='Politics'/>
              <option suffix='2-no-lang' title='Economy'/>
              <option suffix='3-no-lang' title='Sport'/>
              <option suffix='4-no-lang' title='Networld'/>
              <option suffix='5-no-lang' title='Science'/>
            </select>
        
            <!--delay sec="0"/-->
          </location>
        
          <location match='^(https?://.*\\.(beepworld\\.de/members88/wuff230))($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name hash="SHA1">power-radio-pur2005.beep.de</name>
            <destination>http://www.power-radio-pur2005.beep.de</destination>
            <topology level="domain"/>
            <!--delay sec="0"/-->
          </location>
        
          <location match='^((https?://(.*\\.)?)(beepworld\\.de/members[0-9]*/[^/]*/))($|.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name hash="SHA1">\\4</name>
            <destination>\\2\\4</destination>
            <topology level="channel"/>
            <!--delay sec="0"/-->
          </location>
        
          <delegate>
            <uri>../default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/tld/default.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <delegate>
            <uri>../default.xml</uri>
          </delegate>
        
        </vpi>`,

        'https://lms.virtual-presence.org/v7/default.xml': `<?xml version="1.0" encoding="UTF-8"?>
        <vpi xmlns='http://virtual-presence.org/schemas/vpi'>
        
          <location match='^(https?://((w)+([0-9]*)\\.)?([^/]+\\.([a-zA-Z]+)))($|/.*$)'>
            <service>jabber:muc4.virtual-presence.org</service>
            <name hash="true">\\5</name>
            <destination>\\1</destination>
            <topology level="domain"/>
            <displayoptions><usercount multiple='15' crowd='40' massive='80'/></displayoptions>
          </location>
        
          <location match='.*'>
            <service>jabber:muc4.virtual-presence.org</service>
            <ignore/>
            <!--name>nirvana</name-->
          </location>
        
        </vpi>`,
    };

    fetchUrl(url: string, version: string): Promise<FetchUrlResponse>
    {
        return new Promise((resolve, reject) =>
        {
            if (this.data[url]) {
                resolve(new FetchUrlResponse(true, '', '', this.data[url]));
            } else {
                reject('No data for ' + url);
            }
        });
    }
}

class TestConfigInstance_map_galdev implements VpiResolverConfigProvider
{
    get(key: string, defaultValue: any): any
    {
        switch (key) {
            case 'vp.vpiRoot': return 'https://lms.virtual-presence.org/v7/root.xml'; break;
            case 'vp.vpiMaxIterations': return 10; break;
        }
        return defaultValue;
    }
}
