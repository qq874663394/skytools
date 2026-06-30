import frida
import sys

js_code = """
var modules = Process.enumerateModules();
var result = [];
modules.forEach(function(m) {
    var name = m.name.toLowerCase();
    if (name.indexOf('nep') !== -1 || name.indexOf('nb') !== -1) {
        result.push(m.name + " | size: " + m.size + " | path: " + m.path);
    }
});
send(result.join('\\n'));
"""

def on_message(message, data):
    print(message['payload'])

device = frida.get_device_manager().add_remote_device("127.0.0.1:27042")
processes = device.enumerate_processes()
target_pid = None
for p in processes:
    if "网易" in p.name:
        target_pid = p.pid
        break
if not target_pid:
    print("Process not found")
    sys.exit(1)

session = device.attach(target_pid)
script = session.create_script(js_code)
script.on('message', on_message)
script.load()
sys.stdin.read()