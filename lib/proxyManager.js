const axios = require('axios');
const { ProxyVerifier } = require('proxy-verifier');
const genericPool = require('generic-pool');
const config = require('./config');

let pool = null;

async function fetchRawProxies() {
    const all = [];
    for (const apiUrl of config.PROXY_API_URLS) {
        try {
            const resp = await axios.get(apiUrl, { timeout: 10000 });
            let list = [];
            if (apiUrl.includes('scdn')) {
                if (resp.data?.code === 200 && Array.isArray(resp.data?.data?.proxies)) list = resp.data.data.proxies;
            } else if (typeof resp.data === 'string') {
                list = resp.data.split('\n').filter(line => line.includes('.'));
            } else if (Array.isArray(resp.data)) {
                list = resp.data;
            }
            all.push(...list.map(ipPort => `http://${ipPort.trim()}`));
        } catch (e) { console.warn(`[代理源] ${apiUrl} 失败: ${e.message}`); }
    }
    return [...new Set(all)];
}

function verifyProxy(proxyUrl) {
    return new Promise(resolve => {
        ProxyVerifier.testAll(proxyUrl, { testUrl: 'https://www.baidu.com', timeout: 5000 }, (err, result) => {
            resolve(!err && result && result.ok);
        });
    });
}

async function createPool() {
    const raw = await fetchRawProxies();
    if (raw.length === 0) { pool = null; return; }
    const valid = [];
    await Promise.all(raw.map(async p => { if (await verifyProxy(p)) valid.push(p); }));
    if (valid.length === 0) { pool = null; return; }
    pool = genericPool.createPool({
        create: () => valid[Math.floor(Math.random() * valid.length)],
        destroy: () => {}
    }, { max: valid.length, min: 1, idleTimeoutMillis: 30000, acquireTimeoutMillis: 5000 });
    console.log(`[代理池] 可用代理: ${valid.length}`);
}

async function getProxy() {
    if (!pool) return null;
    try { const p = await pool.acquire(); pool.release(p); return p; }
    catch { return null; }
}

createPool();
setInterval(createPool, config.PROXY_REFRESH_INTERVAL);
module.exports = { getProxy };