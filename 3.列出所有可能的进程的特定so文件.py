import frida
import sys
import time

def on_message(message, data):
    if message['type'] == 'send':
        if data is None:
            print("[-] 未找到模块")
        else:
            print("[+] 收到数据，大小：{} 字节".format(len(data)))
            with open("libnep_dump.so", "wb") as f:
                f.write(data)
            print("[+] 保存成功")
            sys.exit(0)
    else:
        print(message)

device = frida.get_device_manager().add_remote_device("127.0.0.1:27042")
print("[+] 连接成功")

# 获取所有进程
processes = device.enumerate_processes()
target_procs = []
for proc in processes:
    if "netease" in proc.name.lower() or "网易" in proc.name:
        target_procs.append((proc.pid, proc.name))
        print("[*] 找到候选进程: {} (PID: {})".format(proc.name, proc.pid))

if not target_procs:
    print("[-] 未找到任何网易相关进程")
    sys.exit(1)

# 对每个候选进程，枚举模块
for pid, name in target_procs:
    print("\n[*] 检查进程: {} (PID: {})".format(name, pid))
    try:
        session = device.attach(pid)
        print("    [+] 附加成功")
    except Exception as e:
        print("    [-] 附加失败: {}".format(e))
        continue

    # 创建脚本：枚举模块并查找包含 "nep" 的，同时列出所有 .so
    js_code = """
    var modules = Process.enumerateModules();
    var found = null;
    var soList = [];
    for (var i = 0; i < modules.length; i++) {
        var mod = modules[i];
        if (mod.name.endsWith(".so")) {
            soList.push(mod.name);
        }
        if (mod.name.toLowerCase().indexOf("nep") !== -1) {
            found = mod;
        }
    }
    if (found) {
        console.log("[+] Found module: " + found.name + " at " + found.base + ", size: " + found.size);
        var buffer = ptr(found.base).readByteArray(found.size);
        send(buffer);
    } else {
        console.log("[-] No module with 'nep' found.");
        console.log("[*] All .so modules: " + soList.join(", "));
        send(null);
    }
    """
    script = session.create_script(js_code)
    script.on('message', on_message)
    script.load()
    time.sleep(1)  # 等待结果
    session.detach()

print("\n[*] 检查完毕。如果未找到，请根据列出的 .so 文件名确定真实的 SO 名称。")