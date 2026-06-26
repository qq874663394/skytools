const axios = require('axios');
const HttpsProxyAgent = require('https-proxy-agent');
const pLimit = require('p-limit');
const crypto = require('crypto');
const config = require('./config');
const { getRandomUA } = require('./uaManager');
const { getProxy } = require('./proxyManager');
const cache = require('./cacheManager');
const limit = pLimit(config.MAX_GLOBAL_CONCURRENT);

async function request({ roleId, apiPath, params = {}, body = {}, headers = {}, maxRetries = 3 }) {
    const cacheKey = `${roleId}:${apiPath}`;
    const cached = cache.get(cacheKey);
    if (cached) return cached;

    return limit(async () => {
        const ts = Math.floor(Date.now() / 1000);
        const nonce = Date.now() + '_' + crypto.randomUUID().toUpperCase();
        const reqParams = { ts, uf: params.uf || config.DEFAULT_UF, ab: params.ab || config.DEFAULT_AB, ef: params.ef || config.DEFAULT_EF, ...params };
        const reqHeaders = { ...config.DEFAULT_BASE_HEADERS, ...headers, 'User-Agent': getRandomUA(), 'GL-Nonce': nonce };
        const requestBody = { ...body, roleId, server: '8000' };

        let lastError;
        for (let i = 0; i < maxRetries; i++) {
            const proxy = await getProxy();
            const instance = axios.create({
                timeout: config.REQUEST_TIMEOUT,
                headers: reqHeaders,
                httpsAgent: proxy ? new HttpsProxyAgent(proxy) : undefined,
            });
            try {
                const resp = await instance.post(`https://god.gameyw.netease.com${apiPath}`, requestBody, { params: reqParams });
                let data = resp.data;
                if (typeof data.result === 'string') data = JSON.parse(data.result);
                if (data.result !== 'ok') throw new Error(`API 异常: ${JSON.stringify(data)}`);
                cache.set(cacheKey, data);
                return data;
            } catch (e) { lastError = e; }
        }
        throw lastError || new Error('所有请求均失败');
    });
}

module.exports = { request };