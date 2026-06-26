const { request } = require('../lib/requestClient');
async function queryWingBuff(roleId, options = {}) {
    return request({ roleId, apiPath: '/v1/app/gameData/queryRoleWingBuff', params: { uf: options.uf, ab: options.ab, ef: options.ef }, headers: options.headers });
}
module.exports = { queryWingBuff };