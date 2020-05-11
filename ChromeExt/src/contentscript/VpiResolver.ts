const $ = require('jquery');
import log = require('loglevel');
import * as crypto from 'crypto';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { BackgroundMessage, FetchUrlResponse } from '../lib/BackgroundMessage';

export enum VpiResolverEvaluateResultType
{
    Error, Delegate, Location
}

export class VpiResolverEvaluateResult
{
    constructor(
        public status: VpiResolverEvaluateResultType,
        public error: any,
        public delegate: string,
        public location: string
    ) { }
}

export interface VpiResolverUrlFetcher
{
    fetchUrl(url: string, version: string): Promise<FetchUrlResponse>;
}

export interface VpiResolverConfigProvider
{
    get(key: string, defaultValue: any): any
}

export class VpiResolverConfigInstance implements VpiResolverConfigProvider
{
    get(key: string, defaultValue: any): any
    {
        return Config.get(key, defaultValue);
    }
}

export class VpiResolver
{
    constructor(private urlFetcher: VpiResolverUrlFetcher, private config: VpiResolverConfigProvider = new VpiResolverConfigInstance())
    {
    }

    async map(documentUrl: string): Promise<string>
    {
        let locationUrl = '';
        let vpiUrl = this.config.get('vp.vpiRoot', 'https://lms.virtual-presence.org/v7/root.xml');
        let iterationCounter = this.config.get('vp.vpiMaxIterations', 10);

        do {
            iterationCounter--;
            let response = await this.urlFetcher.fetchUrl(vpiUrl, '');
            if (response.ok) {
                let result = this.evaluate(documentUrl, vpiUrl, response.data);
                switch (result.status) {

                    case VpiResolverEvaluateResultType.Error: {
                        log.debug('VpiResolver', result.error);
                    } break;

                    case VpiResolverEvaluateResultType.Delegate: {
                        log.debug('VpiResolver', result.status, result.delegate);
                        vpiUrl = result.delegate;
                    } break;

                    case VpiResolverEvaluateResultType.Location: {
                        log.debug('VpiResolver', result.status, result.location);
                        locationUrl = result.location;
                    } break;

                }
            }
        } while (locationUrl == '' && iterationCounter > 0);

        return locationUrl;
    }

    evaluate(documentUrl: string, dataSrc: string, data: string): VpiResolverEvaluateResult
    {
        try {
            let resultType = VpiResolverEvaluateResultType.Error;
            let delegate = '';
            let location = '';
            let xml = $.parseXML(data);
            for (let vpiChildIndex = 0; vpiChildIndex < xml.documentElement.children.length; vpiChildIndex++) {
                let vpiChild = xml.documentElement.children[vpiChildIndex];

                let matchExpr = '.*';
                let matchAttr = vpiChild.attributes['match'];
                if (matchAttr) {
                    matchExpr = matchAttr.value;
                }
                let regex = new RegExp(matchExpr);
                let matchResult = regex.exec(documentUrl);
                if (matchResult) {
                    if (vpiChild.tagName == 'delegate') {
                        let nextVpi = vpiChild.firstElementChild.textContent;
                        let nextVpiUrl = new URL(nextVpi, dataSrc);
                        delegate = nextVpiUrl.toString();
                        resultType = VpiResolverEvaluateResultType.Delegate;
                    } else if (vpiChild.tagName == 'location') {
                        let protocol = 'xmpp';
                        let server = '';
                        let room = '';
                        for (let locationChildIndex = 0; locationChildIndex < vpiChild.children.length; locationChildIndex++) {
                            let locationChild = vpiChild.children[locationChildIndex];
                            switch (locationChild.tagName) {
                                case 'service': // <service>jabber:muc4.virtual-presence.org</service>
                                    {
                                        let locationServiceText: string = locationChild.textContent;
                                        let colonIndex = locationServiceText.indexOf(':');
                                        server = locationServiceText.substr(colonIndex + 1);
                                    } break;
                                case 'name': // All the same: <name hash="true">\5</name> | <name hash="SHA1">\5</name> | <name>\5</name>
                                    {
                                        let hash = locationChild.attributes.hash ? as.String(locationChild.attributes.hash.value, 'SHA1') : 'SHA1';
                                        if (hash == 'true') { hash = 'SHA1'; }

                                        let prefix = locationChild.attributes.prefix ? as.String(locationChild.attributes.prefix.value, '') : '';

                                        let nameExpr = locationChild.textContent;
                                        let name = '';
                                        for (let nameExprCharIndex = 0; nameExprCharIndex < nameExpr.length; nameExprCharIndex++) {
                                            if (nameExpr[nameExprCharIndex] == '\\') {
                                                nameExprCharIndex++;
                                                let numberStr = '';
                                                while (nameExpr[nameExprCharIndex] >= '0' && nameExpr[nameExprCharIndex] <= 9 && nameExprCharIndex < nameExpr.length) {
                                                    numberStr += nameExpr[nameExprCharIndex];
                                                    nameExprCharIndex++;
                                                }
                                                let number = as.Int(numberStr, 1);
                                                if (number < matchResult.length) {
                                                    name += matchResult[number];
                                                }
                                            }
                                        }

                                        let hasher = crypto.createHash(hash);
                                        hasher.update(name);
                                        room = prefix + hasher.digest('hex');
                                    } break;
                            }
                        }
                        location = protocol + ':' + room + '@' + server;
                        resultType = VpiResolverEvaluateResultType.Location;
                    }
                    break;
                }
            }
            return new VpiResolverEvaluateResult(resultType, '', delegate, location);
        } catch (error) {
            return new VpiResolverEvaluateResult(VpiResolverEvaluateResultType.Error, error, '', '');
        }
    }
}
