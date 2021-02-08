declare global
{
    namespace NodeJS
    {
        interface ProcessEnv
        {
            GITHUB_AUTH_TOKEN: string;
            NODE_ENV: 'development' | 'production';
            PORT?: string;
            PWD: string;
        }
    }
}

export class Environment
{
    static NODE_ENV_development = 'development';

    static get_NODE_ENV(): string
    {
        return process.env.NODE_ENV;
    }

    static isDevelopment(): boolean
    {
        return Environment.get_NODE_ENV() == Environment.NODE_ENV_development;
    }

    static isEmbedded(): boolean
    {
        return !Environment.isExtension(); //typeof chrome === 'undefined' && typeof chrome.storage === 'undefined';
    }

    static isExtension(): boolean
    {
        if (typeof chrome !== 'undefined') {
            if (typeof chrome.runtime !== 'undefined') {
                return true;
            }
        }
        return false;
    }
}
