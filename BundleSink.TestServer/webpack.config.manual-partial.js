const path = require('path');
const BundleSinkWebpackPlugin = require('../BundleSink.WebpackPlugin/index');

module.exports = env => {

    let entry = {
        'page-a': './page-a/index.ts',
        'page-b': './page-b/index.ts'
    }

    const bundleSinkOptions = {
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        partial: false
    };
    
    if (env.only) {
        const selectedEntry = env.only;
        if (!entry[selectedEntry]) {
            throw new Error(`Syntax: --env only=<entry name>`);
        }
        entry = { [selectedEntry]: entry[selectedEntry] };
        bundleSinkOptions.partial = true;
    }

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin(bundleSinkOptions);
    
    return {
        mode: 'development',
        context: path.resolve(__dirname, 'Client'),
        entry,
        devtool: 'source-map',
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    loader: 'awesome-typescript-loader',
                    options: {
                        transpileOnly: true
                    },
                    exclude: /node_modules/,
                }
            ]
        },
        resolve: {
            extensions: ['.js', '.jsx', '.ts', '.tsx']
        },
        output: {
            path: path.resolve(__dirname, 'wwwroot/dist'),
            publicPath: '/ist',
            libraryTarget: 'umd',
            filename: '[name].[contenthash].js',
        },
        optimization: {
            splitChunks: {
                chunks: 'all'
            }
        },
        externals: {
            vue: 'Vue'
        },
        plugins: [
            ...bundleSinkWebpackPlugin.plugins
        ]
    }
}