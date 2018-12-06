/* eslint-disable no-console */

"use strict";

(function ($) {
    var debounce = function (cb, delay) {
        var inDebounce;
        return function () {
            var context = this;
            var args = arguments;
            clearTimeout(inDebounce);
            inDebounce = setTimeout(function () {
                cb.apply(context, args);
            }, delay);
        };
    };

    var isNullOrWhitespace = function (input) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    }

    var replaceAll = function (search, find, replace) {
        return search.split(find).join(replace);
    };

    var makeRequestWithPayload = function (payload, success) {
        console.log(payload);
        var qs = $.param(payload);
        qs = replaceAll(qs, "%5B%5D", "");
        $.get("/ProviderSearch?" + qs, success);
    };

    var removeSearchResults = function () {
        var $venueSearchResultContainer = $("#ProviderSearchResultContainer");
        $venueSearchResultContainer.html("");
    };

    var replaceSearchResult = function (searchResults) {
        var $venueSearchResultContainer = $("#ProviderSearchResultContainer");
        $venueSearchResultContainer.html("");
        $venueSearchResultContainer.html(searchResults);
    };

    var $UKPrn = $("#UKPrn");
    var $SearchButton = $("#searchProvider");

    var doSearch = function () {
        if (isNullOrWhitespace($UKPrn.val())) {
            removeSearchResults();
        
        } else {
            makeRequestWithPayload({
                SearchTerm: $UKPrn.val()
            }, onSucess);
        }
    };

    var onSucess = function (data) {
        replaceSearchResult(data);

        //TODO MUST!!! RUN THIS AFTER CHANGING THE DOM
        window.GOVUKFrontend.initAll();
    };

    $SearchButton.on("click", debounce(doSearch, 400));
})(jQuery);