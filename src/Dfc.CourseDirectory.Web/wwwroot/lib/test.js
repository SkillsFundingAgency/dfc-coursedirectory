﻿tinymce.PluginManager.add('test', function (editor) {
    var self = this;

    function update() {
        editor.theme.panel.find('#charactercount').text(['wibble: {0}', self.getCount()]);
    }

    editor.on('init', function () {
        var statusbar = editor.theme.panel && editor.theme.panel.find('#statusbar')[0];

        if (statusbar) {
            window.setTimeout(function () {
                statusbar.insert({
                    type: 'label',
                    name: 'charactercount',
                    text: ['Characters: {0}', self.getCount()],
                    classes: 'charactercount',
                    disabled: editor.settings.readonly
                }, 0);

                editor.on('setcontent beforeaddundo', update);

                editor.on('keyup', function (e) {
                    alert("heelo");
                    update();
                });
            }, 0);
        }
    });

    self.getCount = function () {
        var tx = editor.getContent({ format: 'raw' });
        var decoded = decodeHtml(tx);
        var decodedStripped = decoded.replace(/(<([^>]+)>)/ig, "").trim();
        var tc = decodedStripped.length;
        return tc;
    };

    function decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    }
});