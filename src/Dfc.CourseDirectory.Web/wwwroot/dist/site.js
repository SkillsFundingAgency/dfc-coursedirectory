/* eslint-disable no-console */
// jQuery Validate Hooks

// see: https://gist.github.com/beccasaurus/957732#file-jquery-validate-hooks-js
// source: https://gist.githubusercontent.com/beccasaurus/957732/raw/e09b422c12c7d8098fa9ae5bb44b50d4e049baaf/jquery.validate.hooks.js

'use strict';

(function ($) {
    $.fn.addTriggersToJqueryValidate = function () {
        // Loop thru the elements that we jQuery validate is attached to
        // and return the loop, so jQuery function chaining will work.
        return this.each(function () {
            var form = $(this);

            // Grab this element's validator object (if it has one)
            var validator = form.data('validator');

            // Only run this code if there's a validator associated with this element
            if (!validator)
                return;

            // Only add these triggers to each element once
            if (form.data('jQueryValidateTriggersAdded'))
                return;
            else
                form.data('jQueryValidateTriggersAdded', true);

            // Override the function that validates the whole form to trigger a
            // formValidation event and either formValidationSuccess or formValidationError
            var oldForm = validator.form;
            validator.form = function () {
                var result = oldForm.apply(this, arguments);
                var form = this.currentForm;
                $(form).trigger((result == true) ? 'formValidationSuccess' : 'formValidationError', form);
                $(form).trigger('formValidation', [form, result]);
                return result;
            };

            // Override the function that validates the whole element to trigger a
            // elementValidation event and either elementValidationSuccess or elementValidationError
            var oldElement = validator.element;
            validator.element = function (element) {
                var result = oldElement.apply(this, arguments);
                $(element).trigger((result == true) ? 'elementValidationSuccess' : 'elementValidationError', element);
                $(element).trigger('elementValidation', [element, result]);
                return result;
            };
        });
    };

    /* Below here are helper methods for calling .bind() for you */

    $.fn.extend({
        // Wouldn't it be nice if, when the full form's validation runs, it triggers the
        // element* validation events?  Well, that's what this does!
        //
        // NOTE: This is VERY coupled with jquery.validation.unobtrusive and uses its
        //       element attributes to figure out which fields use validation and
        //       whether or not they're currently valid.
        //
        triggerElementValidationsOnFormValidation: function () {
            return this.each(function () {
                $(this).bind('formValidation', function (e, form) {
                    $(form).find('*[data-val=true]').each(function (i, field) {
                        if ($(field).hasClass('input-validation-error')) {
                            $(field).trigger('elementValidationError', field);
                            $(field).trigger('elementValidation', [field, false]);
                        } else {
                            $(field).trigger('elementValidationSuccess', field);
                            $(field).trigger('elementValidation', [field, true]);
                        }
                    });
                });
            });
        },

        formValidation: function (fn) {
            return this.each(function () {
                $(this).bind('formValidation', function (e, element, result) { fn(element, result); });
            });
        },

        formValidationSuccess: function (fn) {
            return this.each(function () {
                $(this).bind('formValidationSuccess', function (e, element) { fn(element); });
            });
        },

        formValidationError: function (fn) {
            return this.each(function () {
                $(this).bind('formValidationError', function (e, element) { fn(element); });
            });
        },

        formValidAndInvalid: function (valid, invalid) {
            return this.each(function () {
                $(this).bind('formValidationSuccess', function (e, element) { valid(element); });
                $(this).bind('formValidationError', function (e, element) { invalid(element); });
            });
        },

        elementValidation: function (fn) {
            return this.each(function () {
                $(this).bind('elementValidation', function (e, element, result) { fn(element, result); });
            });
        },

        elementValidationSuccess: function (fn) {
            return this.each(function () {
                $(this).bind('elementValidationSuccess', function (e, element) { fn(element); });
            });
        },

        elementValidationError: function (fn) {
            return this.each(function () {
                $(this).bind('elementValidationError', function (e, element) { fn(element); });
            });
        },

        elementValidAndInvalid: function (valid, invalid) {
            return this.each(function () {
                $(this).bind('elementValidationSuccess', function (e, element) { valid(element); });
                $(this).bind('elementValidationError', function (e, element) { invalid(element); });
            });
        }
    });
})(jQuery);
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
            var $allSectorSubjectAreaTier1FilterCheckboxes = $("input[name='SectorSubjectAreaTier1Filter']:checkbox:checked");
            var $allSectorSubjectAreaTier2FilterCheckboxes = $("input[name='SectorSubjectAreaTier2Filter']:checkbox:checked");

            makeRequestWithPayload({
                SearchTerm: $larsSearchTerm.val(),
                NotionalNVQLevelv2Filter: $allCheckedNotionalNvqLevelV2FilterCheckboxes.map(function () {
                    return $(this).val();
                }).get(),
                AwardOrgCodeFilter: $allCheckedAwardOrgCodeFilterCheckboxes.map(function () {
                    return $(this).val();
                }).get(),
                SectorSubjectAreaTier1Filter: $allSectorSubjectAreaTier1FilterCheckboxes.map(function () {
                    return $(this).val();
                }).get(),
                SectorSubjectAreaTier2Filter: $allSectorSubjectAreaTier2FilterCheckboxes.map(function () {
                    return $(this).val();
                }).get()
            }, onSucess);
        }
    };

    var assignEventsToAllCheckboxes = function () {
        var $notionalNvqLevelV2FilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox");
        var $awardOrgCodeFilterCheckboxes = $("input[name='AwardOrgCodeFilter']:checkbox");
        var $sectorSubjectAreaTier1FilterCheckboxes = $("input[name='SectorSubjectAreaTier1Filter']:checkbox");
        var $sectorSubjectAreaTier2FilterCheckboxes = $("input[name='SectorSubjectAreaTier2Filter']:checkbox");

        $notionalNvqLevelV2FilterCheckboxes.on("click", doSearch);
        $awardOrgCodeFilterCheckboxes.on("click", doSearch);
        $sectorSubjectAreaTier1FilterCheckboxes.on("click", doSearch);
        $sectorSubjectAreaTier2FilterCheckboxes.on("click", doSearch);
    };

    var assignEventToClearAllFiltersLink = function () {
        var $clearAllFiltersLink = $("#ClearAllFilters");

        $clearAllFiltersLink.on("click", function (e) {
            e.preventDefault();
            var $allCheckedFilterCheckboxes = $("input[name='NotionalNVQLevelv2Filter']:checkbox:checked, input[name='AwardOrgCodeFilter']:checkbox:checked, input[name='SectorSubjectAreaTier1Filter']:checkbox, input[name='SectorSubjectAreaTier2Filter']:checkbox");
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