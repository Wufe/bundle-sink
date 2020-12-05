const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const WebpackAssetsManifest = require('webpack-assets-manifest');

const onlyParamName = 'only';

module.exports = function BundleSink(options = {}) {

    let {
        clean = false,
        output = "manifest.json",
        partial = false,
        entry = {},
        env = {},
        assetsManifestOptions = {}
    } = options;

    const plugins = [];
    
    let entryName;
    if ((entryName = env[onlyParamName])) {
        let entryValue;
        if ((entryValue = entry[entryName])) {
            partial = true;
            entry = { [entryName]: entryValue };
        } else {
            throw new Error(`Syntax: --env bundle-only=<entry name>`);
        }
    }

    if (clean && !partial) {
        plugins.push(new CleanWebpackPlugin());
    }

    const webpackAssetsManifest = new WebpackAssetsManifest({
        output,
        merge: true,
        entrypoints: true,
        ...assetsManifestOptions
    });

    plugins.push(webpackAssetsManifest);

    return {
        plugins,
        entry
    };
};