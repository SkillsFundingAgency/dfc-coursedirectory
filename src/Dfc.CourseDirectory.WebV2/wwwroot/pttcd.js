$('*[data-pttcd-module="rich-text-editor"]').each(function (i, el) {
    var textarea = $(el).find('.govuk-textarea, .govuk-js-character-count')[0];

    tinymce.init({
        target: textarea,
        plugins: 'advlist lists paste',
        menubar: false,
        statusbar: false,
        toolbar: 'formatselect | bold | numlist bullist | removeformat',
        paste_as_text: true,
        content_css: '/v2/pttcd.css',
        setup: function (editor) {
            var triggerCharacterCountUpdate = function () {
                var e = document.createEvent('HTMLEvents');
                e.initEvent('keyup', false, true);
                textarea.dispatchEvent(e);
            };

            // Add a new invisible backing field that holds the HTML that will be POSTed to back-end.
            // If character-count is in use we want the GDS library to count the text length after HTML is stripped
            var name = textarea.getAttribute('name');
            textarea.removeAttribute('name');

            var initialHtml = $('<div />').html(textarea.innerHTML).text();

            var $backingField = $('<input>')
                .attr('type', 'hidden')
                .attr('name', name)
                .val(initialHtml);
            $(textarea).before($backingField);

            var updateBackingFields = function (html) {
                $backingField.val(html);

                var stripped = $('<div />').html(html).text();
                textarea.innerHTML = stripped;

                triggerCharacterCountUpdate();
            };

            var updateBackingFieldsFromEditor = function () {
                var html = editor.getContent();
                updateBackingFields(html);
            };

            editor.on('keyup', updateBackingFieldsFromEditor);
            editor.on('change', updateBackingFieldsFromEditor);

            editor.on('focus', function () {
                $(el).find('.tox-tinymce').addClass('tox-tinymce--focused');
            });

            editor.on('blur', function () {
                $(el).find('.tox-tinymce').removeClass('tox-tinymce--focused');
            });

            textarea.removeAttribute('id');

            updateBackingFields(initialHtml);
            triggerCharacterCountUpdate();
        }
    });
});