const HtmlWebpackPlugin = require('html-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { join } = require('path');

module.exports = {
    mode: 'development',
    node: {
        fs: 'empty'
    },
    entry: join(__dirname, 'src/test/test.ts'),
    output: {
        path: __dirname + '/dist',
        filename: 'test.js'
    },
    module: {
        rules: [
            {
                exclude: /node_modules/,
                test: /\.ts$/,
                use: 'awesome-typescript-loader?{configFileName: "tsconfig.json"}',
            },
            {
                test: /\.css$/i,
                use: [MiniCssExtractPlugin.loader, 'css-loader'],
            },
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: '[name].css',
            chunkFilename: '[id].css',
        }),
        new HtmlWebpackPlugin({
            filename: "test.html",
            title: "Browser Tests",
        })
    ],
    resolve: {
        extensions: ['.ts', '.js']
    },
}