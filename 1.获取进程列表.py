import frida

manager = frida.get_device_manager()
device = manager.get_device("127.0.0.1:7555")

print("[*] 正在枚举进程...")
try:
    for app in device.enumerate_processes():
        if app.pid > 0:
            print(f"{app.pid} : {app.name}")
except Exception as e:
    print(f"错误: {e}")