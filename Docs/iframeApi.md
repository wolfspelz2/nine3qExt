### payload format
    {
        api: https://n3q-api.com/v1
        payload: {
            user: '9ca05afb1a49f26fb59642305c481661f8b370bd',
            item: 'pirml6rhf5tp2go3mulhh3o',
            room: '9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org',
            entropy: random
            expires: unxitime
        },
        hash: sha256(systemSecret + payload)
    }

### get payload hash
https://n3qweb.k8s.sui.li/PayloadHash?id=ext-3jo6rap97qnklec9wdjkcbbtrakqstqi2kel3at3&payload=<base64(payload)>

### open iframe
https://theatre.weblin.sui.li/iframe.html?token=<base64(token)>


### make backend api call
    POST n3q-api.com/
    {
        action: updateProperties
        token: token,
        properties: {}
        hash: sha256(developerSecret + token + properties),
    }


### ItemAction später:

        payload: {
            user: '9ca05afb1a49f26fb59642305c481661f8b370bd',
            item: 'pirml6rhf5tp2go3mulhh3o',
            // aspect: 'Extractor',
            action: 'Extract',
            from: 'pirml6rhf5tp2go3mulhh3o'
            entropy: random
            expires: unxitime
        },
        hash: sha256(systemSecret + payload)
    }
