const HtmlWebpackPlugin = require('html-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { join } = require('path');

module.exports = {
    mode: 'development',
    node: {
        fs: 'empty'
    },
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
        new MiniCssExtractPlugin({
            filename: '[name].css',
            chunkFilename: '[name].css',
        }),
        new HtmlWebpackPlugin({
            filename: "popup.html",
            title: "Configure Avatars and Things on Web Pages",
        })
    ],
    resolve: {
        extensions: ['.ts', '.js']
    },
}