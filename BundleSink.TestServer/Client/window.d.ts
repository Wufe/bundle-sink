export {}
declare global {
    interface Window {
        registeredModules: {
            list: () => string[];
            add: (name: string) => void;
        };
    }
}