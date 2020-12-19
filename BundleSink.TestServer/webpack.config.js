const path = require('path');
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const FixStyleOnlyEntriesPlugin = require('webpack-fix-style-only-entries');

const mode = process.env.NODE_ENV === 'production' ?
    'production' : 'development';

module.exports = env => {

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin({
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        entry: {
            'page-a': './Client/page-a/index.ts',
            'page-b': './Client/page-b/index.ts',
            'page-c': './Client/page-c/index.ts',
            'page-a-style': './Styles/page-a-style.scss',
            'page-d': './Client/page-d/index.ts',
            'page-e': './Client/page-e/index.ts'
        },
        env
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
            extensions: ['.js', '.jsx', '.ts', '.tsx', '.css', '.scss']
        },
        output: {
            path: path.resolve(__dirname, 'wwwroot/dist'),
            libraryTarget: 'umd',
            filename: '[name].js',
            chunkFilename: '[name].[contenthash].js',
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
            new FixStyleOnlyEntriesPlugin(),
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: "[name].css",
                chunkFilename: "[id].css",
            }),
        ]
    }
}