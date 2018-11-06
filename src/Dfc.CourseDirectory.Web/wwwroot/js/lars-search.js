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

    var $larsSearchTerm = $("#LarsSearchTerm");
    var $larsSearchResultContainer = $("#LarsSearchResultContainer");

    $larsSearchTerm.on("keyup", debounce(function () {
        console.log($larsSearchTerm.val());

        $.get("/LarsSearch", {
            SearchTerm: $larsSearchTerm.val()
        }, function (data) {
            $larsSearchResultContainer.html("");
            $larsSearchResultContainer.html(data);
        });
    }, 400));



})(jQuery);
