console.log("Script started");

var dlsym = Module.findExportByName(null, "dlsym");
console.log("dlsym: " + dlsym);
if (dlsym) {
    Interceptor.attach(dlsym, {
        onEnter: function(args) {
            console.log("dlsym called");
        },
        onLeave: function(ret) {}
    });
    console.log("dlsym hooked");
} else {
    console.log("dlsym not found");
}

var RegisterNatives = null;
var syms = [
    '_ZN3art3JNI15RegisterNativesEP7_JNIEnvP7_jclassPK15JNINativeMethodi',
    'RegisterNatives'
];
for (var i = 0; i < syms.length; i++) {
    var addr = Module.findExportByName("libart.so", syms[i]);
    if (addr) {
        RegisterNatives = addr;
        break;
    }
}
if (!RegisterNatives) {
    RegisterNatives = Module.findExportByName(null, "RegisterNatives");
}
console.log("RegisterNatives: " + RegisterNatives);
if (RegisterNatives) {
    Interceptor.attach(RegisterNatives, {
        onEnter: function(args) {
            var methods = args[2];
            var count = args[3].toInt32();
            console.log("[RegisterNatives] count=" + count);
            for (var i = 0; i < count; i++) {
                var name = methods.add(i * 3 * Process.pointerSize).readPointer().readCString();
                var sig = methods.add(i * 3 * Process.pointerSize + Process.pointerSize).readPointer().readCString();
                var fn = methods.add(i * 3 * Process.pointerSize + 2 * Process.pointerSize).readPointer();
                console.log("  " + name + " " + sig + " -> " + fn);
                var mod = Process.findModuleByAddress(fn);
                if (mod) console.log("    in " + mod.name + " base " + mod.base);
            }
        }
    });
    console.log("RegisterNatives hooked");
} else {
    console.log("RegisterNatives not found");
}