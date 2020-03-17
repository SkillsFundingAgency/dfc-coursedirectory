$('*[data-pttcd-module="rich-text-editor"]').each(function (i, el) {
    var textarea = $(el).find('.govuk-textarea')[0];

    tinymce.init({
        target: textarea,
        plugins: 'advlist lists autoresize paste',
        menubar: false,
        statusbar: false,
        resize: false,
        toolbar: 'formatselect | bold | numlist bullist | removeformat',
        paste_as_text: true,
        content_css: '/v2/pttcd.css',
        setup: function (editor) {
            editor.on('change', function () {
                textarea.innerHTML = editor.getContent();
            });

            editor.on('focus', function () {
                $(el).find('.tox-tinymce').addClass('tox-tinymce--focused');
            });

            editor.on('blur', function () {
                $(el).find('.tox-tinymce').removeClass('tox-tinymce--focused');
            });
        }
    });
});