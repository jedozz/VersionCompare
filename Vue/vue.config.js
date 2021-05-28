const path = require('path')

function resolve(dir) {
  return path.join(__dirname, dir);
}

const appconfig = require(resolve("appconfig.json"));

module.exports = {
  "outputDir": "../VersionCompare/wwwroot",
  "pages": {
    "index": {
      "entry": "./src/app.js",
      "template": "public/index.html",
      "title": "index",
      "chunks": [
        "chunk-vendors",
        "chunk-common",
        "index"
      ]
    }
  },
  "transpileDependencies": [
    "vuetify"
  ],
  chainWebpack: config => {
    config.resolve.alias
      .set('@', resolve(''))
      .set('@src', resolve('src'))
      .set('@assets', resolve('src/assets'))
      .set('@components', resolve('src/components'))
      .set('@views', resolve('src/views'))
      .set('@public', resolve('public'))
  },
  configureWebpack: config => {
    if (process.env.NODE_ENV == 'production') {
      config.devtool = '';
    } else {
      config.devtool = 'cheap-module-eval-source-map';
    }
  },
  devServer: {
    proxy: appconfig.urlBase.test
  }
}