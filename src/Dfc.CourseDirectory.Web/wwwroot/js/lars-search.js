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

    var doSearch = function (payload, success) {
        $.get("/LarsSearch", payload, success);
    };

    var assignEventsToNotionalNvqLevelV2Checkboxes = function () {
        var $notionalNvqLevelV2Checkboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox");

        $notionalNvqLevelV2Checkboxes.on("click", function () {
            var $allCheckedNotionalNvqLevelV2Checkboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked");
            console.log($allCheckedNotionalNvqLevelV2Checkboxes);

            doSearch({
                SearchTerm: $larsSearchTerm.val(),
                NotionalNVQLevelv2Filter: $allCheckedNotionalNvqLevelV2Checkboxes.map(function () {
                    return $(this).val();
                }).get()
            }, onSucess);
        });
    };

    var $larsSearchTerm = $("#LarsSearchTerm");
    var $larsSearchResultContainer = $("#LarsSearchResultContainer");

    var onSucess = function (data) {
        $larsSearchResultContainer.html("");
        $larsSearchResultContainer.html(data);
        assignEventsToNotionalNvqLevelV2Checkboxes();
    };

    $larsSearchTerm.on("keyup", debounce(function () {
        console.log($larsSearchTerm.val());
        doSearch({
            SearchTerm: $larsSearchTerm.val()
        }, onSucess);
    }, 400));

})(jQuery);
