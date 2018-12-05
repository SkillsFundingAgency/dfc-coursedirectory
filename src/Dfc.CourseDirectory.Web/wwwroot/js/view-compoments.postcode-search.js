/* eslint-disable no-console */

"use strict";

(function ($) {

    $('#postCodeSearchForm').addTriggersToJqueryValidate().triggerElementValidationsOnFormValidation();


    $('#PostCode').elementValidation(function (element, result) {
        //alert("FIRED");
        var errorClass = 'error';
        var formGroup = $(element).closest('.form-group');
        //alert(formGroup);
        if (formGroup) {
            if (!formGroup.hasClass(errorClass) && result === false) {
                //alert("addclass");
                formGroup.addClass(errorClass);
            } else if (formGroup.hasClass(errorClass) && result === true) {
                //alert("removeclass");
                formGroup.removeClass(errorClass);
            }
        }
    });

   // $('#PostCode').on('blur', function () {
       // alert("error");
        //    var errorClass = 'error';
        //    var formGroup = $(this).closest('.form-group');
        //    var validationMessage = $('span[data-valmsg-for="PostCode"]');
        //    var isEmpty = $(this).val().length === 0;

        //    if (isEmpty && formGroup) {
        //        if (formGroup.hasClass(errorClass)) {
        //            formGroup.removeClass(errorClass);
        //            validationMessage.removeClass('field-validation-error').html('');
        //        }
        //    }
    //});
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

    //var replaceAll = function (search, find, replace) {
    //    return search.split(find).join(replace);
    //};

    var makeRequestWithPayload = function (payload, success) {
        console.log(payload);
        var qs = $.param(payload);
        //qs = replaceAll(qs, "%5B%5D", "");
        $.get("/PostCodeSearch?" + qs, success);
    };

    //var makeRequestWithUrl = function (url, success) {
    //    console.log(url);
    //    $.get(url, success);
    //};

    var removeSearchResults = function () {
        var $postCodeSearchResultContainer = $("#PostCodeSearchContainer");
        $postCodeSearchResultContainer.html("");
    };

    var replaceSearchResult = function (searchResults) {
        var $postCodeSearchResultContainer = $("#PostCodeSearchContainer");
        $postCodeSearchResultContainer.html("");
        $postCodeSearchResultContainer.html(searchResults);
    };

    var $Postcode = $("#PostCode");
    var $PostcodeSearch = $("#postcode-search");
    var $ChangeLink = $("#postcode-change");

    var doSearch = function () {
        if (isNullOrWhitespace($Postcode.val())) {
            //removeSearchResults();
        } else {
            //var $allCheckedNotionalNvqLevelV2FilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked");
            //var $allCheckedAwardOrgCodeFilterCheckboxes = $("input[name='AwardOrgCodeFilter']:checkbox:checked");
            //var $allSectorSubjectAreaTier1FilterCheckboxes = $("input[name='SectorSubjectAreaTier1Filter']:checkbox:checked");
            //var $allSectorSubjectAreaTier2FilterCheckboxes = $("input[name='SectorSubjectAreaTier2Filter']:checkbox:checked");

            makeRequestWithPayload({
                PostCode: $Postcode.val(),
                //NotionalNVQLevelv2Filter: $allCheckedNotionalNvqLevelV2FilterCheckboxes.map(function () {
                //    return $(this).val();
                //}).get(),
                //AwardOrgCodeFilter: $allCheckedAwardOrgCodeFilterCheckboxes.map(function () {
                //    return $(this).val();
                //}).get(),
                //SectorSubjectAreaTier1Filter: $allSectorSubjectAreaTier1FilterCheckboxes.map(function () {
                //    return $(this).val();
                //}).get(),
                //SectorSubjectAreaTier2Filter: $allSectorSubjectAreaTier2FilterCheckboxes.map(function () {
                //    return $(this).val();
                //}).get()
            }, onSucess);
        }
    };

    //var assignEventsToAllCheckboxes = function () {
    //    var $notionalNvqLevelV2FilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox");
    //    var $awardOrgCodeFilterCheckboxes = $("input[name='AwardOrgCodeFilter']:checkbox");
    //    var $sectorSubjectAreaTier1FilterCheckboxes = $("input[name='SectorSubjectAreaTier1Filter']:checkbox");
    //    var $sectorSubjectAreaTier2FilterCheckboxes = $("input[name='SectorSubjectAreaTier2Filter']:checkbox");

    //    $notionalNvqLevelV2FilterCheckboxes.on("click", doSearch);
    //    $awardOrgCodeFilterCheckboxes.on("click", doSearch);
    //    $sectorSubjectAreaTier1FilterCheckboxes.on("click", doSearch);
    //    $sectorSubjectAreaTier2FilterCheckboxes.on("click", doSearch);
    //};

    //var assignEventToClearAllFiltersLink = function () {
    //    var $clearAllFiltersLink = $("#ClearAllFilters");

    //    $clearAllFiltersLink.on("click", function (e) {
    //        e.preventDefault();
    //        var $allCheckedFilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked, input[name='AwardOrgCodeFilter']:checkbox:checked, input[name='SectorSubjectAreaTier1Filter']:checkbox, input[name='SectorSubjectAreaTier2Filter']:checkbox");
    //        var allCheckedFilterCheckboxesLength = $allCheckedFilterCheckboxes.length;

    //        for (var i = 0; i < allCheckedFilterCheckboxesLength; i++) {
    //            if (i === (allCheckedFilterCheckboxesLength - 1)) {
    //                $($allCheckedFilterCheckboxes[i]).trigger("click");
    //            } else {
    //                $($allCheckedFilterCheckboxes[i]).prop('checked', false);
    //            }
    //        }
    //    });
    //};

    //var assignEventsToLarsSearchPagination = function () {
    //    var $larsSearchResultPaginationItems = $("#LarsSearchResultContainer .pagination .pagination__item");
    //    $larsSearchResultPaginationItems.on("click", function (e) {
    //        e.preventDefault();
    //        var url = $(e.target).attr("href");
    //        makeRequestWithUrl(url, onSucess);
    //    });
    //};



    var change = function () {
        removeSearchResults();
        $.get("/Venue/PostCodeSearch");

        
    };

    var onSucess = function (data) {
        replaceSearchResult(data);
        //assignEventsToAllCheckboxes();
        //assignEventToClearAllFiltersLink();
        //assignEventsToLarsSearchPagination();
    };

    $PostcodeSearch.on("click", debounce(doSearch, 400));

    $ChangeLink.on("click", debounce(change, 400));
})(jQuery);