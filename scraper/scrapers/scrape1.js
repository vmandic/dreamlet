var fs = require('fs'),
    atob = require("atob"),
    request = require('request'),
    limit = require("simple-rate-limiter"),
    async = require("async"),
    cheerio = require('cheerio'),
    limitedRequest = limit(request).to(3).per(2500);

var scrape1 = function (res) {
    var baseUrl = "{0}/dreamdictionary/{1}_all.htm".format(atob("aHR0cDovL3d3dy5kcmVhbW1vb2RzLmNvbQ==")),
        requests = [];

    // prepare request objects
    "abcdefghijklmnopqrstuvwxyz".split("").forEach(function (letter, index) {
        requests.push(function (callback) {
            var url = baseUrl.format("", letter);
            // remove "_all" from these letters as these sites do not have the aggregated list
            url = url.replace(/([jkquvxyz])(_all).htm/, "$1.htm");
            limitedRequest(url, _processScrape1.bind(this, baseUrl, letter, callback));
        });
    });

    // initiate all requests
    async.parallel(requests, function (error, results) {
        res.setHeader('Content-Type', 'application/json');
        res.json(results);
    });
};

var _processScrape1 = function (baseUrl, letter, callback, err, resp, html) {
    if (!err) {
        //_saveToFileSystem(baseUrl, letter, html);
        _saveToJson(cheerio.load(html), callback);
    } else {
        console.error("Scrape1 ERR: ", err);
    }
};

var _saveToFileSystem = function (baseUrl, letter, html) {
    var fileName = "tmp/dl/SITE-{0}-LETTER-{1}.html".format(baseUrl.split("//")[1].split("/")[0], letter);

    fs.writeFile(fileName, html, function (fserr) {
        if (fserr)
            console.error("FS ERROR: ", fserr);
        else
            console.log("FS WRITTEN: ", fileName);
    });
};

var _saveToJson = function ($, callback) {
    var terms = [],
        data = [],
        currentTerm = null;

    // filter out data for postprocessing
   data = $("table[width=950] td[width=750]:first-child > *").map(function (index, el) {
        if ($(el).html().indexOf("<script") === -1) {
            if ($(el).text().split("/")[0].split(" ").length < 4 && $(el).text().indexOf("*Please") === -1)
                return "TERM - " + _sanitize($(el).text());

            return _sanitize($(el).text());
        }

        // Skip if contains a script tag
        return "";
    });

    // build term objects
    data.toArray().forEach(function (e, i) {
        if (e.length !== 0) {
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
        }
    });

    callback(null, terms.filter(function (t) {
        return t.explanations.some(function (_t) {
            return _t.indexOf("function") === -1;
        });
    }));
};

var _sanitize = function (str) {
    return str.replaceAll("\r\n", " ").replaceAll("\r", " ").replaceAll("\n", " ").trim().replaceAll("  ", " ").replaceAll("   ", " ").replace(" TOP", "").trimRight();
};

exports = module.exports = scrape1;