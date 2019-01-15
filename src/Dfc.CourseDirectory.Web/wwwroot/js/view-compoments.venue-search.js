/* eslint-disable no-console */

"use strict";

(function ($) {

    $(function () {



    //    var formGroupErrorClass = "govuk-form-group--error";
    //    var elementErrorClass = "govuk-input--error";
    //    var $searchForm = $("form.site-search");
    //    var $elementSearchTerm = $("#@nameof(Model.SearchTerm)");
    //    var $venueSearchResultContainer = $("#VenueSearchResultContainer");
    //    var $validationMessageSearchTerm = $elementSearchTerm.prev("[data-valmsg-for='@nameof(Model.SearchTerm)']");
    //    $validationMessageSearchTerm.css("margin-bottom", "0");
    //    var $formGroupSearchTerm = $elementSearchTerm.closest('.govuk-form-group');
    //    var $form = $elementSearchTerm.closest("form");
    //    $form.addTriggersToJqueryValidate().triggerElementValidationsOnFormValidation();
    //    var validator = $form.validate();
    //    $elementSearchTerm.on("blur", function () {
    //        var result = validator.element($elementSearchTerm);
    //        if (result) {
    //            HideValidationErrorMessage();
    //        } else {
    //            ShowValidationErrorMessage();
    //        }
    //    });
    //    $elementSearchTerm.elementValidation(function (element, result) {
    //        if (!$formGroupSearchTerm.hasClass(formGroupErrorClass)) {
    //            $validationMessageSearchTerm.hide();
    //        }
    //        if (result) {
    //            HideValidationErrorMessage();
    //        }
    //        else {
    //            ShowValidationErrorMessage()
    //        }
    //    });

     
      

    //    $("#searchProvider").on("click", function (e) {
    //        e.preventDefault();
    //        ValidateAndSearchForProvider();
    //    });

    //    $(document).keypress(function (e) {
    //        if (e.which == 13) {
    //            e.preventDefault();
    //            ValidateAndSearchForProvider();
    //        }
    //    });

    //    function ValidateAndSearchForProvider() {
    //        $elementSearchTerm.trigger("blur");
    //        var result = validator.element($elementSearchTerm);
    //        if (result) {
    //            makeRequestWithPayload({
    //                SearchTerm: $elementSearchTerm.val()
    //            }, onSucess);

    //            HideValidationErrorMessage();
    //        }
    //        else {
    //            $venueSearchResultContainer.html("");
    //            ShowValidationErrorMessage();
    //        }
    //    }

    //    function ShowValidationErrorMessage() {
    //        $elementSearchTerm.addClass(elementErrorClass);
    //        $formGroupSearchTerm.addClass(formGroupErrorClass);
    //        $searchForm.addClass("govuk-form-group, govuk-form-group--error");
    //        $validationMessageSearchTerm.show();
    //        $validationMessageSearchTerm.css("margin-bottom", "15px");
    //    }
    //    function HideValidationErrorMessage() {
    //        $elementSearchTerm.removeClass(elementErrorClass);
    //        $formGroupSearchTerm.removeClass(formGroupErrorClass);
    //        $searchForm.removeClass("govuk-form-group, govuk-form-group--error");
    //        $validationMessageSearchTerm.hide();
    //        $validationMessageSearchTerm.css("margin-bottom", "0");
    //    }




    //    var debounce = function (cb, delay) {
    //        var inDebounce;
    //        return function () {
    //            var context = this;
    //            var args = arguments;
    //            clearTimeout(inDebounce);
    //            inDebounce = setTimeout(function () {
    //                cb.apply(context, args);
    //            }, delay);
    //        };
    //    };

    //    var isNullOrWhitespace = function (input) {
    //        if (typeof input === 'undefined' || input == null) return true;
    //        return input.replace(/\s/g, '').length < 1;
    //    }

    //    var replaceAll = function (search, find, replace) {
    //        return search.split(find).join(replace);
    //    };

    //    var makeRequestWithPayload = function (payload, success) {
    //        console.log(payload);
    //        var qs = $.param(payload);
    //        qs = replaceAll(qs, "%5B%5D", "");
    //        $.get("/VenueSearch?" + qs, success);
    //    };

    //    var removeSearchResults = function () {
    //        var $venueSearchResultContainer = $("#VenueSearchResultContainer");
    //        $venueSearchResultContainer.html("");
    //    };

    //    var replaceSearchResult = function (searchResults) {
    //        var $venueSearchResultContainer = $("#VenueSearchResultContainer");
    //        $venueSearchResultContainer.html("");
    //        $venueSearchResultContainer.html(searchResults);
    //    };

    //    var $UKPrn = $("#UKPrn");
    //    var $SearchButton = $("#search");

    //    var doSearch = function () {
    //        if (isNullOrWhitespace($UKPrn.val())) {
    //            removeSearchResults();

    //        } else {
    //            makeRequestWithPayload({
    //                SearchTerm: $UKPrn.val()
    //            }, onSucess);
    //        }
    //    };

    //    var onSucess = function (data) {
    //        replaceSearchResult(data);

    //        //TODO MUST!!! RUN THIS AFTER CHANGING THE DOM
    //        window.GOVUKFrontend.initAll();
    //    };

    //    $(document).keypress(function (e) {
    //        if (e.which == 13) {
    //            e.preventDefault();
    //            doSearch();
    //        }
    //    });

    //    $SearchButton.on("click", debounce(doSearch, 400));
    });

})(jQuery);