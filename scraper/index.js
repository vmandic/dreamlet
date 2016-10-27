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
    var baseUrl = "{0}/dreamdictionary/{1}_all.htm".format(atob("aHR0cDovL3d3dy5kcmVhbW1vb2RzLmNvbQ=="));

    var requests = [];

    "abcdefghijklmnopqrstuvwxyz".split("").forEach(function (letter, index) {
        requests.push(function () {
            var url = baseUrl.format("", letter);
            limitedRequest(url, processScrape1.bind(this, baseUrl, letter));
        });
    });

    requests[0]();

    // initiate all requests
    // requests.forEach(function(req, index){ 
    //     req();
    //     console.log("Launching req id: " + (index+1));
    // });
};

var processScrape1 = function (baseUrl, letter, err, resp, html) {
    if (!err) {
        var $ = cheerio.load(html);
        var fileName = "tmp/dl/SITE-{0}-LETTER-{1}.html".format(baseUrl.split("//")[1].split("/")[0], letter);

        fs.writeFile(fileName, html, function(fserr) {
            if(fserr)
                console.error("FS ERROR: ", fserr);
            else
                console.log("FS WRITTEN: ", fileName);    
        });
    } else {
        console.error("Scrape1 ERR: ", err);
    }
};

app.get('/', function (req, res) {
    scrape1();
});

app.listen('8081');
console.log('Magic happens on port 8081');
exports = module.exports = app;