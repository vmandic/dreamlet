require("./common.js");

var express = require('express'),
    fs = require('fs'),
    atob = require("atob"),
    request = require('request'),
    limit = require("simple-rate-limiter"),
    cheerio = require('cheerio'),
    app = express();

var limitedRequest = limit(request).to(2).per(5000);

var scrape1 = function () {
    var baseUrl = "{0}/dreamdictionary/{1}_all.html".format(atob("aHR0cDovL3d3dy5kcmVhbW1vb2RzLmNvbQ=="));

    var requests = [];

    ["abcdefghijklmnopqrstuvwxyz"].forEach(function (letter, index) {
        requests.push(function () {
            var url = baseUrl.format("", letter);
            limitedRequest(url, processScrape1);
        });
    });

    // TEST: explicitly trigger the first one, letter a
    requests[0]();
};

var processScrape1 = function (err, resp, html) {
    if (!err) {
        var $ = cheerio.load(html);

    } else {
        console.error("Scrape1 ERR: ", err);
    }
};

app.get('/scrape', function (req, res) {
    scrape1();
});

app.listen('8081');
console.log('Magic happens on port 8081');
exports = module.exports = app;