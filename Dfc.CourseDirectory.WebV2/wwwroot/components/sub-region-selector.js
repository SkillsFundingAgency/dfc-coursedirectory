$(function () {
    $('*[data-pttcd-module="sub-region-selector"]').each(function (_, c) {
        $(c).find('.pttcd-sub-regions-selector__region').each(function (i, el) {
            const $checkboxes = $(el).find('input[type="checkbox"]');
            const totalCheckboxCount = $checkboxes.length;

            const $selectAllCheckboxWrapper = $('<div />')
                .addClass('govuk-checkboxes__item')
                .addClass('govuk-!-margin-bottom-3')
                .css('width', '100%');

            const $checkbox = $('<input />')
                .attr('type', 'checkbox')
                .attr('id', $(el).closest('.govuk-accordion').attr('id') + '-region-' + i + '-select-all')
                .addClass('govuk-checkboxes__input')
                .change(function (e) {
                    $checkboxes.prop('checked', $(e.target).is(':checked'));
                    refresh();
                });

            const $label = $('<label />')
                .attr('for', $checkbox.attr('id'))
                .addClass('govuk-checkboxes__label')
                .addClass('govuk-label')
                .html('Select all');

            const $summary = $('<div />')
                .addClass('govuk-accordion__section-summary')
                .addClass('govuk-body')
                .hide();

            $selectAllCheckboxWrapper.append($checkbox);
            $selectAllCheckboxWrapper.append($label);
            $(el).find('.govuk-checkboxes').prepend($selectAllCheckboxWrapper);
            $(el).find('.govuk-accordion__section-header').append($summary);

            const refresh = function () {
                const checkedCount = $checkboxes.filter(':checked').length;

                if (checkedCount === totalCheckboxCount) {
                    $checkbox.prop('checked', true);
                    $summary.text('All areas selected').show();
                }
                else if (checkedCount > 0) {
                    $checkbox.prop('checked', false);
                    $summary.text(checkedCount + ' area' + (checkedCount > 1 ? 's' : '') + ' selected').show();
                }
                else {
                    $checkbox.prop('checked', false);
                    $summary.text('').hide();
                }
            };

            $checkboxes.change(refresh);

            refresh();
        });
    });
});
