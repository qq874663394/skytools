defineHandler({
    onEnter: function(log, args, state) {
        var symbol = args[1].readCString();
        log("dlsym: " + symbol);
        this.symbol = symbol;
    },
    onLeave: function(log, ret, state) {
        if (this.symbol && ret && !ret.isNull()) {
            var mod = Process.findModuleByAddress(ret);
            if (mod) {
                log("  -> " + ret + " in " + mod.name);
            } else {
                log("  -> " + ret + " in unknown module");
            }
        }
    }
});