var fs = require('fs'),
    atob = require("atob"),
    request = require('request'),
    limit = require("simple-rate-limiter"),
    cheerio = require('cheerio');

var limitedRequest = limit(request).to(3).per(2500);

var scrape1 = function (res) {
    var baseUrl = "{0}/dreamdictionary/{1}_all.htm".format(atob("aHR0cDovL3d3dy5kcmVhbW1vb2RzLmNvbQ=="));

    var requests = [];

    "abcdefghijklmnopqrstuvwxyz".split("").forEach(function (letter, index) {
        requests.push(function (res) {
            var url = baseUrl.format("", letter);
            limitedRequest(url, _processScrape1.bind(this, baseUrl, letter, res));
        });
    });

    requests[0](res);

    //initiate all requests
    // requests.forEach(function (req, index) {
    //     req();
    //     console.log("Launching req id: " + (index + 1));
    // });

    // how to scrape scrape1 into array of terms and explanations

};

var _processScrape1 = function (baseUrl, letter, res, err, resp, html) {
    if (!err) {
        var fileName = "tmp/dl/SITE-{0}-LETTER-{1}.html".format(baseUrl.split("//")[1].split("/")[0], letter);

        //_saveToFileSystem(fileName, html);
        _saveToJson(cheerio.load(html), res);
    } else {
        console.error("Scrape1 ERR: ", err);
    }
};

var _saveToFileSystem = function (fileName, html) {
    fs.writeFile(fileName, html, function (fserr) {
        if (fserr)
            console.error("FS ERROR: ", fserr);
        else
            console.log("FS WRITTEN: ", fileName);
    });
};

var _saveToJson = function ($, res) {
    var terms = [], data = [];
    var currentTerm = null;

    data = $("table[width=950] td[width=750]:first-child > p").map(function (index, el) {
        if ($(el).html().indexOf("<b>") > -1 
         || $(el).html().indexOf("<strong>") > -1)
            return "TERM - " + $(el).text();

        return $(el).text().replaceAll("\r\n", " ").replaceAll("\r", " ").replaceAll("\n", " ").trim().replaceAll("  ", " ").replaceAll("   ", " ").replace(" TOP", "").trimRight();
    });

    data.toArray().forEach(function (e, i) {
        if (e.indexOf("TERM - ") > -1) {
            var termName = e.split("TERM - ")[1];
            currentTerm = {
                name: termName,
                explanations: []
            };

            terms.push(currentTerm);
        } else {
            currentTerm && e.length > 0 && currentTerm.explanations.push(e);
        }
    });

    // refactor to promises with sth like $.all().then()...
    res.setHeader('Content-Type', 'application/json');
    res.send(JSON.stringify(terms));
};

exports = module.exports = scrape1;