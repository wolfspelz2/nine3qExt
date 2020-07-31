plugin_paths = { "/etc/prosody/plugins/mod_auth_sha1_salt" }

cross_domain_websocket = { "*" };
s2s_require_encryption = false

component_ports = { 5347  }
component_interface = "*"

-- k8s
Component "itemsinv.k8s.sui.li"
         component_secret = "28756a7ff5dce"

Component "itemsxmpp.k8s.sui.li"
         component_secret = "28756a7ff5dce"

Component "stage01-itemsxmpp.k8s.sui.li"
         component_secret = "28756a7ff5dce"

Component "stage01-itemsinv.k8s.sui.li"
         component_secret = "28756a7ff5dce"

-- weblin.com
Component "itemsinv.vulcan.weblin.com"
         component_secret = "5440fa19cf838"

Component "itemsxmpp.vulcan.weblin.com"
         component_secret = "5440fa19cf838"

Component "stage01-itemsxmpp.vulcan.weblin.com"
         component_secret = "5440fa19cf838"

Component "stage01-itemsinv.vulcan.weblin.com"
         component_secret = "5440fa19cf838"


VirtualHost "xmpp.k8s.sui.li"

    ssl = {
        certificate = "/etc/prosody/certs/localhost.crt";
        key = "/etc/prosody/certs/localhost.key";
    }

    authentication = "sha1_salt"

    modules_enabled = {
        "bosh";
        "websocket";
        "pubsub";
        "ping"; -- Enable mod_ping
        "auth_sha1_salt";
    }
    c2s_require_encryption = false
    allow_unencrypted_plain_auth = true
    consider_bosh_secure = true


VirtualHost "xmpp.vulcan.weblin.com"

    ssl = {
        certificate = "/etc/prosody/certs/localhost.crt";
        key = "/etc/prosody/certs/localhost.key";
    }

    authentication = "sha1_salt"

    modules_enabled = {
        "bosh";
        "websocket";
        "pubsub";
        "ping"; -- Enable mod_ping
        "auth_sha1_salt";
    }
    c2s_require_encryption = false
    allow_unencrypted_plain_auth = true
    consider_bosh_secure = true
