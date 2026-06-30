import frida
import sys

# 改成子进程（从你的进程列表里找的）
PACKAGE = "com.netease.gl:va_core"   # 或者直接用 PID 3038
SO_NAME = "libnep.so"
OUTPUT_PATH = "/sdcard/libnep_dumped.so"

manager = frida.get_device_manager()
device = manager.get_device("127.0.0.1:7555")

session = None
try:
    session = device.attach(PACKAGE)
except frida.ProcessNotFoundError:
    # 如果进程名不行，用 PID 3038
    session = device.attach(3038)

def on_message(msg, data):
    if msg['type'] == 'send':
        print(msg['payload'])

jscode = f"""
'use strict';
var mod = Process.findModuleByName("{SO_NAME}");
if (mod) {{
    var size = mod.size;
    var base = mod.base;
    var f = new File("{OUTPUT_PATH}", "wb");
    f.write(Memory.readByteArray(base, size));
    f.close();
    send("✅ dump success -> {OUTPUT_PATH}  size: " + size);
}} else {{
    send("❌ {SO_NAME} not found in this process");
    // 列出前20个模块，便于排查
    Process.enumerateModules().slice(0, 20).forEach(m => {{
        send("   " + m.name);
    }});
}}
"""

script = session.create_script(jscode)
script.on('message', on_message)
script.load()
print("[*] 正在 dump，请稍候...")
sys.stdin.read()