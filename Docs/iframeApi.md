### contextToken format
base64(
    {
        api: https://n3q-api.com/v1
        payload: {
            user: '765fgvhuz7t6ft6ftijt6fthbiit6ftbh'
            item: 'pirml6rhf5tp2go3mulhh3o'
            room: '9ca05afb1a49f26fb59642305c481661f8b370bd@muc4.virtual-presence.org'
            entropy: random
            expires: unixtime
        },
        hash: sha256(systemSecret + base64(payload))
    }
)

### partnerToken format
base64(
    {
        api: https://n3q-api.com/v1
        payload: {
            partner: "suat-theatre-tf5768gihu89z7t6ftugzuhji97t6fituljnjz6t"
            entropy: random
            expires: unixtime
        },
        hash: sha256(systemSecret + base64(payload))
    }
)

### get payload hash
->  POST https://n3q-api.com/v1
    urlencode(
        {
            payload: base64(payload)
        }
    )
<-  response
    {
        status: "ok"
        result: sha256(systemSecret + base64(payload))
    }

### open iframe
https://theatre.weblin.sui.li/iframe.html?context=<contextToken>

### make backend api call
->  POST https://n3q-api.com/v1
    {
        partner: partnerToken
        context: contextToken
        method: "getProperties"
        pids: [ "DocumentText", "DocumentMaxLength", "Container" ]
    }
<-  response
    {
        status: "ok",
        result: {
            DocumentText: "This is a text", 
            DocumentMaxLength: 2000
        }
    }

->  POST https://n3q-api.com/v1
    {
        partner: partnerToken
        context: contextToken
        method: "changeProperties"
        set: {
            DocumentText: "This is a text"
        }
        delete: [ "TestInt" ]
    }
<-  response
    {
        status: "ok"
    }

### ItemAction:
->  POST https://n3q-api.com/v1
    {
        partner: partnerToken
        context: contextToken
        method: "itemAction"
        aspect: "Document"
        action: "SetText"
        args: {
            text: "This is a text"
        }
    }
