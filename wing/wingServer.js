const express = require('express');
const path = require('path');
const config = require('../lib/config');
const { queryWingBuff } = require('./wingService');

const app = express();
// 静态文件从 wing 目录提供
app.use(express.static(path.join(__dirname)));

app.get('/query/wing', async (req, res) => {
    const roleId = req.query.roleId;
    if (!roleId) return res.status(400).json({ error: '缺少 roleId' });
    try {
        const data = await queryWingBuff(roleId, {
            uf: req.headers['x-uf'] || config.DEFAULT_UF,
            ab: req.headers['x-ab'] || config.DEFAULT_AB,
            ef: req.headers['x-ef'] || config.DEFAULT_EF,
        });
        res.json(data);
    } catch (e) { res.status(502).json({ error: e.message }); }
});

if (require.main === module) {
    app.listen(config.WING_PORT, () => console.log(`🕊️ 光翼服务: http://localhost:${config.WING_PORT}`));
}
module.exports = app;