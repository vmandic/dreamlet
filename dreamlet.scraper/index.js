require("./common.js");

var express = require('express'),
    app = express(),
    scrape1 = require("./scrapers/scrape1.js");

app.get('/', function (req, res) {
    var data = scrape1(res);
});

app.listen('8081');

console.log('Magic happens on port 8081');
exports = module.exports = app;