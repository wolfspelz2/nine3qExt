### payload format
    {
        payload: {
            api: https://n3q-api.com/v1
            provider: 'suat-theatre',
            user: '765fgvhuz7t6ft6ftijt6fthbiit6ftbh',
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
->  POST n3q-api.com/
    {
        payload: {
            method: getProperties
            token: token,
            pids: [ "DocumentText", "DocumentMaxLength" ]
        },
        hash: sha256(developerSecret + payload),
    }
<-  response
    {
        status: "ok",
        result: {
            DocumentText: "This is a text", 
            DocumentMaxLength: 2000
        }
    }

->  POST n3q-api.com/
    {
        payload: {
            method: changeProperties
            token: token,
            set: {
                DocumentText: "This is a text", 
            }
            delete: [ "TestInt" ]
        },
        hash: sha256(developerSecret + properties),
    }
<-  response
    {
        status: "ok"
    }

### ItemAction später:
        payload: {
            user: '765fgvhuz7t6ft6ftijt6fthbiit6ftbh',
            item: 'pirml6rhf5tp2go3mulhh3o',
            // aspect: 'Extractor',
            action: 'Extract',
            from: 'pirml6rhf5tp2go3mulhh3o'
            entropy: random
            expires: unxitime
        },
        hash: sha256(systemSecret + payload)
    }
