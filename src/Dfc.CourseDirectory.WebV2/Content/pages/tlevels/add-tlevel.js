(function ($) {
    $('.pttcd-tlevel__venues-selector').each(function (i, el) {
        const $checkboxItems = $(el).find('.govuk-checkboxes__item');
        const $checkboxes = $checkboxItems.find('input[type = "checkbox"]');
        const totalCheckboxCount = $checkboxItems.length;

        if (totalCheckboxCount < 10) {
            return;
        }

        const selectAllCheckboxItemInputId = $checkboxItems[0].id + '-selectall';

        const $checkbox = $('<input />')
            .attr('type', 'checkbox')
            .attr('id', selectAllCheckboxItemInputId)
            .addClass('govuk-checkboxes__input')
            .change(function (e) {
                $checkboxes.prop('checked', $(e.target).is(':checked'));
                refresh();
            });

        const $selectAllCheckboxItem = $('<div />')
            .addClass('govuk-checkboxes__item')
            .append($checkbox)
            .append(
                $('<label />')
                    .attr('for', selectAllCheckboxItemInputId)
                    .addClass('govuk-label')
                    .addClass('govuk-checkboxes__label')
                    .css('font-weight', 'bold')
                    .text('Select all'));

        $selectAllCheckboxItem.insertBefore($checkboxItems[0]);

        const refresh = function () {
            const checkedCount = $checkboxes.filter(':checked').length;

            if (checkedCount === totalCheckboxCount) {
                $checkbox.prop('checked', true);
            }
            else if (checkedCount > 0) {
                $checkbox.prop('checked', false);
            }
        };

        $checkboxes.change(refresh);

        refresh();
    });
})(window.jQuery);
