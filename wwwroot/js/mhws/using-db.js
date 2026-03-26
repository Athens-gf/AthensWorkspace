// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// noinspection JSUnresolvedReference

//region DB
const db = new Dexie("MHWsDB");

db.version(1).stores({
    amulet: "key",
});

db.open();

function getSetting(setting, key, def) {
    return setting.get(key)
        .then((setting) => {
            if (setting) {
                return setting.val
            }
            return def;
        })
        .catch((error) => console.error(error));
}

function makeDbFunction(settings, base) {
    const setter = (key, val) => {
        settings.put({key: `${base}_${key}`, val: val}).catch((error) => console.error(error));
    };
    const getter = (key, def) => getSetting(settings, `${base}_${key}`, def);
    return [getter, setter]
}

const [getAmulets, setAmulets] = makeDbFunction(db.amulet, '');
