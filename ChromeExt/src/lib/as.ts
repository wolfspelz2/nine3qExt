export class as
{
    private static readonly escapeHtml_entityMap = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '\'': '&quot;',
        '"': '&#39;',
    }

    private static readonly typeBoolean: string = typeof true;
    private static readonly typeString: string = typeof '';
    private static readonly typeNumber: string = typeof 1;

    static Bool(val: any, alt?: boolean): boolean
    {
        var res = alt;
        if (typeof val === this.typeBoolean) {
            res = val;
        } else {
            if (typeof val === this.typeString) {
                if (val == 'true' || val == 'True' || val == 'TRUE' || val == '1' || val == 'yes') {
                    res = true;
                } else { res = false; }
            } else {
                if (typeof val === this.typeNumber) {
                    if (val >= 1) {
                        res = true;
                    } else { res = false; }
                }
            }
        }
        return res;
    }

    static String(val: any, alt?: string): string
    {
        var res = alt;
        if (typeof val === this.typeString) {
            res = val;
        } else {
            if (typeof val === this.typeNumber) {
                res = '' + val;
            } else {
                if (typeof val === this.typeBoolean) {
                    res = val ? 'true' : 'false';
                }
            }
        }
        return res;
    }

    static Int(val: any, alt?: number): number
    {
        var res = alt;
        if (typeof val === this.typeNumber) {
            res = Math.round(val);
        } else {
            if (typeof val === this.typeString) {
                res = parseInt(val);
                if (isNaN(res)) {
                    res = alt;
                }
            }
        }
        return res;
    }

    static Float(val: any, alt?: number): number
    {
        var res = alt;
        if (typeof val === this.typeNumber) {
            res = val;
        } else {
            if (typeof val === this.typeString) {
                res = parseFloat(val);
                if (isNaN(res)) {
                    res = alt;
                }
            }
        }
        return res;
    }

    static makeLinksClickable(text): string
    {
        var urlRegex = /(https?:\/\/[^\s]+)/g;
        return text.replace(urlRegex, url => 
        {
            return '<a href="' + url + '">' + url + '</a>';
        });
    }

    static Html(val: any, alt?: string): string
    {
        let res = as.String(val, alt);
        let htmlEncoded = String(res).replace(/[&<>'"]/g, (s) => this.escapeHtml_entityMap[s]);
        let clickableEncoded = as.makeLinksClickable(htmlEncoded);
        return clickableEncoded;
    }

    static HtmlLink(val: any, text?: string, urlFilter?: (s: string) => string, alt?: string): string
    {
        var res = as.String(val, alt);
        if (urlFilter == null) {
            urlFilter = (s => s.substr(0, 4) == 'http' ? s : '');
        }
        var url = urlFilter(res);
        if (as.String(url) != '') {
            if (text == '') {
                text = url;
            }
            res = '<a href="' + as.Html(url) + '">' + as.Html(text) + '</a>'
        }
        return res;
    }

    static Object(val: any, alt?: string): any
    {
        var res = as.String(val, alt);
        var obj = null;
        try {
            obj = JSON.parse(res);
        } catch (exception) {
            obj = JSON.parse(alt);
        }
        return obj;
    }
}
