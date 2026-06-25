const http = require('http');
const https = require('https');
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const PORT = 8765;
const DIR = __dirname;

const MIME = {
    '.html': 'text/html; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.js': 'application/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
};

// ========== 这组参数有效，不要改 ==========
const ts = '1782378941';
const UF = '004ba46c-27e6-4ba5-9bf6-32afbd2c1373';
const AB = 'dff60cb47bd013a953b1669caf31760095';
const EF = '4783885dcdd100b5cfcb348e406a5acaf3';

function serveFile(filePath, res) {
    const ext = path.extname(filePath);
    fs.readFile(filePath, (err, data) => {
        if (err) {
            res.writeHead(404);
            res.end('Not Found');
            return;
        }
        res.writeHead(200, {
            'Content-Type': MIME[ext] || 'text/plain'
        });
        res.end(data);
    });
}

function fetchWingBuff(roleId, callback) {


    const body = JSON.stringify({
        roleId: roleId,
        server: '8000'
    });

    // const headers = {
    //     'Host': 'god.gameyw.netease.com',
    //     'Content-Type': 'application/json',
    //     'Content-Length': Buffer.byteLength(body),
    //     'GL-Uid': '0801d86b58df4b1cae61ccdab53d518e',
    //     'Accept': 'application/json, text/plain, */*',
    //     'GL-Version': '4.19.2',
    //     'GL-Source': 'URS',
    //     'Origin': 'https://act.ds.163.com',
    //     'User-Agent': 'Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 Godlike/4.19.2 UEPay/com.netease.godlike/iOS_7.13.5',
    //     'Referer': 'https://act.ds.163.com/',
    //     'GL-ClientType': '51',
    //     'GL-Nonce': nonce,
    //     'GL-Token': 'b0d5655c419f45a4a3e048332fa668a1',
    //     'GL-DeviceId': '1BFA9166-F683-44EE-8533-A91C697C5D87',
    //     'Accept-Language': 'zh-CN,zh-Hans;q=0.9',
    //     'Accept-Encoding': 'gzip, deflate, br, zstd',
    //     'Connection': 'keep-alive'
    // };
    const headers = {
        'Host': 'god.gameyw.netease.com',
        'Content-Type': 'application/json',
        'GL-Uid': '0801d86b58df4b1cae61ccdab53d518e',
        'Accept': 'application/json, text/plain, */*',
        'GL-Version': '4.19.2',
        'GL-Source': 'URS',
        'Origin': 'https://act.ds.163.com',
        'Referer': 'https://act.ds.163.com/',
        'GL-ClientType': '51',
        'GL-Nonce': '1782378942197_F229FBF8-DB0E-4C5C-A435-570E2994B32E',
        'GL-Token': 'b0d5655c419f45a4a3e048332fa668a1',
        'GL-DeviceId': '1BFA9166-F683-44EE-8533-A91C697C5D87',
        'Accept-Language': 'zh-CN,zh-Hans;q : 0.9',
        'Accept-Encoding': 'gzip, deflate',
        'User-Agent': 'Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 Godlike/4.19.2 UEPay/com.netease.godlike/iOS_7.13.5'
    };

    const options = {
        hostname: 'god.gameyw.netease.com',
        path: `/v1/app/gameData/queryRoleWingBuff?ts=${ts}&uf=${UF}&ab=${AB}&ef=${EF}`,
        method: 'POST',
        headers: headers
    };

    const req = https.request(options, res => {
        const chunks = [];
        res.on('data', chunk => chunks.push(chunk));
        res.on('end', () => {
            const buffer = Buffer.concat(chunks);
            const encoding = res.headers['content-encoding'];
            let data = buffer;

            if (encoding === 'gzip' || encoding === 'deflate') {
                data = zlib.gunzipSync(buffer);
            } else if (encoding === 'br') {
                data = zlib.brotliDecompressSync(buffer);
            }

            try {
                let parsed = JSON.parse(data.toString());
                // 如果返回的是 { "result": "{\"result\":\"ok\"...}" }，再解析一层
                if (typeof parsed.result === 'string') {
                    parsed = JSON.parse(parsed.result);
                }
                callback(null, parsed);
            } catch (e) {
                callback(e);
            }
        });
    });

    req.on('error', e => callback(e));
    req.write(body);
    req.end();
}

const server = http.createServer((req, res) => {
    const reqUrl = new URL(req.url, `http://localhost:${PORT}`);
    const pathname = reqUrl.pathname;

    if (pathname === '/query' && req.method === 'GET') {
        const roleId = reqUrl.searchParams.get('roleId');
        if (!roleId) {
            res.writeHead(400, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify({
                error: '缺少 roleId'
            }));
            return;
        }

        fetchWingBuff(roleId, (err, data) => {
            if (err) {
                res.writeHead(502, {
                    'Content-Type': 'application/json'
                });
                res.end(JSON.stringify({
                    error: err.message
                }));
                return;
            }
            res.writeHead(200, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify(data));
        });
        return;
    }

    let filePath = pathname === '/' ? '/index.html' : pathname;
    serveFile(path.join(DIR, filePath), res);
});

server.listen(PORT, () => {
    console.log(`http://localhost:${PORT}`);
});