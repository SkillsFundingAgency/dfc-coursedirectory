(function ($) {
    var regionIndex = 0;

    $('.pttcd-new-apprenticeship-provider__apprenticeship-employer-locations-regions__region').each(function (i, el) {
        var $checkboxes = $(el).find('input[type="checkbox"]');

        var selectAllCheckboxId = 'region-' + regionIndex + '-select-all';

        var updateCheckboxes = function () {
            var checked = $('#' + selectAllCheckboxId).is(':checked');
            $checkboxes.prop('checked', checked);
        };

        var createSelectAllCheckbox = function () {
            var $selectAllCheckboxWrapper = $('<div />')
                .addClass('govuk-checkboxes__item')
                .addClass('govuk-!-margin-bottom-3')
                .css('width', '100%');

            var $checkbox = $('<input />')
                .attr('type', 'checkbox')
                .attr('id', selectAllCheckboxId)
                .addClass('govuk-checkboxes__input')
                .change(updateCheckboxes);
            $selectAllCheckboxWrapper.append($checkbox);

            var $label = $('<label />')
                .attr('for', selectAllCheckboxId)
                .addClass('govuk-checkboxes__label')
                .addClass('govuk-label')
                .html('All');
            $selectAllCheckboxWrapper.append($label);

            $(el).find('.govuk-checkboxes').prepend($selectAllCheckboxWrapper);
        };

        var refreshSelectAllCheckbox = function () {
            var checkedCount = $checkboxes.filter(':checked').length;
            var uncheckedCount = $checkboxes.not(':checked').length;
            var totalCount = $checkboxes.length;

            if (checkedCount === totalCount) {
                $('#' + selectAllCheckboxId).prop('checked', true);
            }
            else if (uncheckedCount > 0) {
                $('#' + selectAllCheckboxId).prop('checked', false);
            };
        };

        $checkboxes.change(refreshSelectAllCheckbox);

        createSelectAllCheckbox();
        refreshSelectAllCheckbox();

        regionIndex++;
    });
})(window.jQuery);