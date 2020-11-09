const $ = require('jquery');
import log = require('loglevel');
import * as crypto from 'crypto';
import { as } from '../lib/as';
import { Config } from '../lib/Config';
import { BackgroundMessage, FetchUrlResponse } from '../lib/BackgroundMessage';
import { Utils } from '../lib/Utils';

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
    language: string = '';

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
                let result = this.evaluate(documentUrl, vpiUrl, response.data, this.language);
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

    evaluate(documentUrl: string, dataSrc: string, data: string, language: string = ''): VpiResolverEvaluateResult
    {
        try {
            let resultType = VpiResolverEvaluateResultType.Error;
            let delegate = '';
            let location = '';
            let xml = $.parseXML(data);
            let logData = {};
            for (let vpiChildIndex = 0; vpiChildIndex < xml.documentElement.children.length; vpiChildIndex++) {
                let vpiChild = xml.documentElement.children[vpiChildIndex];

                let matchExpr = '.*';
                let matchAttr = vpiChild.attributes['match'];
                if (matchAttr) {
                    matchExpr = matchAttr.value;
                }

                let regex = new RegExp(matchExpr);
                let unsafeMatchResult = regex.exec(documentUrl);

                let matchResult: Array<string> = null;
                if (unsafeMatchResult) {
                    matchResult = new Array<string>();
                    for (let iMatch = 0; iMatch < unsafeMatchResult.length; iMatch++) {
                        matchResult[iMatch] = unsafeMatchResult[iMatch] ? unsafeMatchResult[iMatch] : '';
                    }
                }

                if (matchResult) {
                    if (vpiChild.tagName == 'delegate') {

                        let nextVpiExpr = as.String(vpiChild.firstElementChild?.textContent, '');
                        let nextVpi = this.replaceMatch(nextVpiExpr, matchResult);

                        let nextVpiUrl = new URL(nextVpi, dataSrc);
                        delegate = nextVpiUrl.toString();
                        resultType = VpiResolverEvaluateResultType.Delegate;
                        break;

                    } else if (vpiChild.tagName == 'location') {

                        logData['regex'] = matchExpr;

                        let protocol = 'xmpp';
                        let server = '';
                        let room = '';
                        let suffix = '';
                        for (let locationChildIndex = 0; locationChildIndex < vpiChild.children.length; locationChildIndex++) {
                            let locationChild = vpiChild.children[locationChildIndex];
                            switch (locationChild.tagName) {
                                case 'service': // <service>jabber:muc4.virtual-presence.org</service>
                                    {
                                        let locationServiceText: string = locationChild.textContent;
                                        let colonIndex = locationServiceText.indexOf(':');
                                        server = locationServiceText.substr(colonIndex + 1);
                                    } break;

                                case 'name': // The same: '<name hash="true">\5</name>' | '<name hash="SHA1">\5</name>' BUT: '<name>\5</name>' does not hash
                                    {
                                        let hash = locationChild.attributes.hash ? as.String(locationChild.attributes.hash.value, 'SHA1') : '';
                                        if (hash == 'true') { hash = 'SHA1'; }

                                        let prefix = locationChild.attributes.prefix ? as.String(locationChild.attributes.prefix.value, '') : '';

                                        let nameExpr = locationChild.textContent;
                                        let name = this.replaceMatch(nameExpr, matchResult);

                                        logData['replace'] = nameExpr;
                                        logData['name'] = name;

                                        if (hash && hash != '') {
                                            let hasher = crypto.createHash(hash.toLowerCase());
                                            hasher.update(name);
                                            name = hasher.digest('hex');
                                        }
                                        room = prefix + name;
                                    } break;

                                case 'select': // see https://lms.virtual-presence.org/v7/name/f/a/facebook.xml
                                    {
                                        let defaultTag = locationChild.attributes.tag ? as.String(locationChild.attributes.tag.value, '') : '';

                                        let options: Array<{ tag: string, suffix: string }> = [];
                                        for (let selectChildIndex = 0; selectChildIndex < locationChild.children.length; selectChildIndex++) {
                                            let selectChild = locationChild.children[selectChildIndex];
                                            switch (selectChild.tagName) {
                                                case 'option':
                                                    {
                                                        let suffix = selectChild.attributes.suffix ? as.String(selectChild.attributes.suffix.value, '') : '';
                                                        let tag = as.String(selectChild.firstElementChild?.textContent, '');
                                                        if (tag != '' && suffix != '') {
                                                            options.push({ 'tag': tag, 'suffix': suffix });
                                                        }
                                                    } break;
                                            }
                                        }

                                        let langTag = 'lang:' + language;
                                        let candidates = options.filter(option => { return option.tag == langTag; });
                                        if (candidates.length == 0) {
                                            candidates = options.filter(option => { return option.tag == defaultTag; });
                                        }
                                        if (candidates.length > 0) {
                                            let rndIndex = Utils.randomInt(0, candidates.length);
                                            suffix = candidates[rndIndex].suffix;
                                        }

                                    } break;

                            }
                        }
                        location = protocol + ':' + room + suffix + '@' + server;
                        log.debug('VpiResolver', logData);
                        resultType = VpiResolverEvaluateResultType.Location;
                        break;
                    }
                }
            }

            return new VpiResolverEvaluateResult(resultType, '', delegate, location);
        } catch (error) {
            return new VpiResolverEvaluateResult(VpiResolverEvaluateResultType.Error, error, '', '');
        }
    }

    replaceMatch(replaceExpression: string, matches: Array<string>): string
    {
        let replaced = '';

        for (let charIndex = 0; charIndex < replaceExpression.length;) {
            if (replaceExpression[charIndex] == '\\') {
                charIndex++;
                let numberStr = '';
                while (replaceExpression[charIndex] >= '0' && replaceExpression[charIndex] <= '9' && charIndex < replaceExpression.length) {
                    numberStr += replaceExpression[charIndex++];
                }
                let number = as.Int(numberStr, 0);
                if (number < matches.length) {
                    replaced += matches[number];
                }
            } else {
                replaced += replaceExpression[charIndex++];
            }
        }

        return replaced;
    }
}
