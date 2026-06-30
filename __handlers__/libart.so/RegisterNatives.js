{
    onEnter: function(log, args, state) {
        var methods = args[2];
        var count = args[3].toInt32();
        log("RegisterNatives: " + count + " methods");
        for (var i = 0; i < count; i++) {
            var name = methods.add(i * 3 * Process.pointerSize).readPointer().readCString();
            var sig = methods.add(i * 3 * Process.pointerSize + Process.pointerSize).readPointer().readCString();
            var fn = methods.add(i * 3 * Process.pointerSize + 2 * Process.pointerSize).readPointer();
            log("  " + name + " " + sig + " -> " + fn);
            var mod = Process.findModuleByAddress(fn);
            if (mod) {
                log("    in " + mod.name + " base " + mod.base);
            }
        }
    }
}