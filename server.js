const wingApp = require('./wing/wingServer');
const config = require('./lib/config');

wingApp.listen(config.WING_PORT, () => console.log(`🕊️  光翼服务: http://localhost:${config.WING_PORT}`));