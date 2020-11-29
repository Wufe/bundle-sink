const modules: string[] = [];

export const registeredModules =
    window.registeredModules =
    window.registeredModules || {
        list: () => modules,
        add: name => modules.push(name)
    };