const { CheckerPlugin } = require('awesome-typescript-loader');
const HtmlWebpackPlugin = require('html-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { optimize } = require('webpack');
const { join } = require('path');
let prodPlugins = [];

if (process.env.NODE_ENV === 'production') {
    prodPlugins.push(
        new optimize.AggressiveMergingPlugin(),
        new optimize.OccurrenceOrderPlugin()
    );
}

module.exports = {
    mode: process.env.NODE_ENV,
    devtool: 'inline-source-map',
    entry: { popup: join(__dirname, 'src/popup/popup.ts') },
    output: {
        path: __dirname + '/dist',
        filename: 'popup.js'
    },
    module: {
        rules: [
            {
                exclude: /node_modules/,
                test: /\.ts$/,
                use: 'awesome-typescript-loader?{configFileName: "tsconfig.json"}',
            },
            {
                test: /\.scss$/,
                use: [MiniCssExtractPlugin.loader, 'css-loader', 'sass-loader'],
            },
            {
                test: /\.(png|jpg)$/,
                use: 'url-loader',
            },
        ]
    },
    plugins: [
        new CheckerPlugin(),
        ...prodPlugins,
        new MiniCssExtractPlugin({
            filename: '[name].css',
            chunkFilename: '[name].css',
        }),
        new HtmlWebpackPlugin({
            filename: "popup.html",
            title: "Configure your avatar",
        })
    ],
    resolve: {
        extensions: ['.ts', '.js']
    },
}