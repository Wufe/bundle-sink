const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const WebpackAssetsManifest = require('webpack-assets-manifest');

module.exports = {
    mode: 'development',
    context: path.resolve(__dirname, 'Client'),
    entry: {
        'page-a': './page-a/index.ts',
        'page-b': './page-b/index.ts'
    },
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
        new CleanWebpackPlugin(),
        new WebpackAssetsManifest({
            output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
            entrypoints: true
        })
    ]
}