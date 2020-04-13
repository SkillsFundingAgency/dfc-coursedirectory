(function ($) {
    $('.pttcd-apprenticeships__classroom-location-form').each(function () {
        var $national = $('#National');

        var updateRadiusField = function () {
            var national = $national.is(':checked');

            var $radius = $('#Radius');
            if (national) {
                $radius.attr('disabled', 'disabled');
            } else {
                $radius.removeAttr('disabled');
            }
        }

        $national.change(updateRadiusField);

        updateRadiusField();
    });
})(window.jQuery);