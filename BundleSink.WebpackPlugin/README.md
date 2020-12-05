# Bundle-Sink
## Webpack plugin

Companion webpack plugin for [BundleSink](https://github.com/wufe/bundle-sink).   

***

## Installation

`yarn add --dev bundle-sink-webpack-plugin` or `npm install --save-dev bundle-sink-webpack-plugin`

## Usage

> webpack.config.js :
```js
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');

module.exports = env => {

    let entry = {
        'page-a': './page-a/index.ts',
        'page-b': './page-b/index.ts'
    };

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin({
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        entry,
        env
    });
    
    return {
        entry: bundleSinkWebpackPlugin.entry,
        ...ADDITIONAL WEBPACK PARAMETERS,
        plugins: [
            ...bundleSinkWebpackPlugin.plugins
        ]
    };
};

```

## Options

- clean  
Default: false  
If set to true, adds a CleanWebpackPlugin to the stack of plugins.


- output  
Default: "client-manifest.json"  
The path of the output manifest file.  


- partial  
Default: false  
Used for partial compilations with manual configuration.
If set to true, the Clean variable is set to false.


- entry  
Default: {}  
The entry object used for partial compilations with automatic configuration.


- env  
Default: {}  
The env object passed to the main webpack config function.  
Used for partial compilations with automatic configuration.


- assetsManifestOptions  
Default: {}  
Object containing options to be passed to webpack-assets-manifest plugin.