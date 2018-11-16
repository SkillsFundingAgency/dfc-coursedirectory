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
        $.get("/LarsSearch?" + qs, success);
    };

    var makeRequestWithUrl = function (url, success) {
        console.log(url);
        $.get(url, success);
    };

    var removeSearchResults = function () {
        var $larsSearchResultContainer = $("#LarsSearchResultContainer");
        $larsSearchResultContainer.html("");
    };

    var replaceSearchResult = function (searchResults) {
        var $larsSearchResultContainer = $("#LarsSearchResultContainer");
        $larsSearchResultContainer.html("");
        $larsSearchResultContainer.html(searchResults);
    };

    var $larsSearchTerm = $("#LarsSearchTerm");

    var doSearch = function () {
        if (isNullOrWhitespace($larsSearchTerm.val())) {
            removeSearchResults();
        } else {
            var $allCheckedNotionalNvqLevelV2FilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked");
            var $allCheckedAwardOrgCodeFilterCheckboxes = $("input[name='AwardOrgCodeFilter']:checkbox:checked");

            makeRequestWithPayload({
                SearchTerm: $larsSearchTerm.val(),
                NotionalNVQLevelv2Filter: $allCheckedNotionalNvqLevelV2FilterCheckboxes.map(function () {
                    return $(this).val();
                }).get(),
                AwardOrgCodeFilter: $allCheckedAwardOrgCodeFilterCheckboxes.map(function () {
                    return $(this).val();
                }).get()
            }, onSucess);
        }
    };

    var assignEventsToAllCheckboxes = function () {
        var $notionalNvqLevelV2FilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox");
        var $awardOrgCodeFilterCheckboxes = $("input[name='AwardOrgCodeFilter']:checkbox");

        $notionalNvqLevelV2FilterCheckboxes.on("click", doSearch);
        $awardOrgCodeFilterCheckboxes.on("click", doSearch);
    };

    var assignEventToClearAllFiltersLink = function () {
        var $clearAllFiltersLink = $("#ClearAllFilters");

        $clearAllFiltersLink.on("click", function (e) {
            e.preventDefault();
            var $allCheckedFilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked, input[name='AwardOrgCodeFilter']:checkbox:checked");
            var allCheckedFilterCheckboxesLength = $allCheckedFilterCheckboxes.length;

            for (var i = 0; i < allCheckedFilterCheckboxesLength; i++) {
                if (i === (allCheckedFilterCheckboxesLength - 1)) {
                    $($allCheckedFilterCheckboxes[i]).trigger("click");
                } else {
                    $($allCheckedFilterCheckboxes[i]).prop('checked', false);
                }
            }
        });
    };

    var assignEventsToLarsSearchPagination = function () {
        var $larsSearchResultPaginationItems = $("#LarsSearchResultContainer .pagination .pagination__item");
        $larsSearchResultPaginationItems.on("click", function (e) {
            e.preventDefault();
            var url = $(e.target).attr("href");
            makeRequestWithUrl(url, onSucess);
        });
    };

    var onSucess = function (data) {
        replaceSearchResult(data);
        assignEventsToAllCheckboxes();
        assignEventToClearAllFiltersLink();
        assignEventsToLarsSearchPagination();
    };

    $larsSearchTerm.on("keyup", debounce(doSearch, 400));
})(jQuery);