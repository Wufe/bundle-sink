const path = require('path');
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

const mode = process.env.NODE_ENV === 'production' ?
    'production' : 'development';

module.exports = env => {

    let entry = {
        'page-a': './Client/page-a/index.ts',
        'page-b': './Client/page-b/index.ts',
        'page-c': './Client/page-c/index.ts',
        'page-a-style': './Styles/page-a-style.scss',
    };

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin({
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        entry,
        env,
        merge: false
    });
    
    return {
        mode,
        context: path.resolve(__dirname),
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
                },
                {
                    test: /\.s[ac]ss$/i,
                    use: [
                        MiniCssExtractPlugin.loader,

                        // Creates `style` nodes from JS strings
                        // "style-loader",

                        // Translates CSS into CommonJS
                        "css-loader",

                        // Compiles Sass to CSS
                        "sass-loader",
                    ],
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
            ...bundleSinkWebpackPlugin.plugins,
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: "[name].css",
                chunkFilename: "[id].css",
            }),
        ]
    }
}