const path = require('path');
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
        mode: 'development',
        context: path.resolve(__dirname, 'Client'),
        entry: bundleSinkWebpackPlugin.entry,
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