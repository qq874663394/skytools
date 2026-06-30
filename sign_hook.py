import frida
import sys

PACKAGE = "com.netease.gl:va_core"   # 或直接用 PID 3038
manager = frida.get_device_manager()
device = manager.get_device("127.0.0.1:7555")

try:
    session = device.attach(PACKAGE)
except frida.ProcessNotFoundError:
    session = device.attach(3038)

# JavaScript 注入代码：调用 Tools.getPostMethodSignatures
jscode = """
Java.perform(function () {
    var Tools = Java.use('com.netease.nep.Tools');
    var HashMap = Java.use('java.util.HashMap');
    
    // 构造请求头和参数（示例，请替换为你实际需要的数据）
    var map = HashMap.$new();
    map.put("GL-Uid", "你的UID");
    map.put("GL-Token", "你的Token");
    map.put("GL-DeviceId", "你的设备ID");
    map.put("GL-ClientType", "51");
    map.put("GL-Version", "4.19.2");
    map.put("GL-Source", "URS");
    
    var url = "https://god.gameyw.netease.com/v1/app/gameData/queryRoleWingBuff";
    var body = '{"roleId":"667511942","server":"8000"}';
    
    var result = Tools.getPostMethodSignatures(url, body, map);
    send("SIGNED_URL:" + result);
});
"""

def on_message(msg, data):
    if msg['type'] == 'send':
        print(msg['payload'])

script = session.create_script(jscode)
script.on('message', on_message)
script.load()
sys.stdin.read()