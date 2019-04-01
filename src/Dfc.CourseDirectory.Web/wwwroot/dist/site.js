(function ($) {

    // Create the defaults once
    var pluginName = "govUkFormGroup",
        defaults = {
            errorCssClass: "govuk-form-group--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    Plugin.prototype.init = function () {
        if ($(this.element).find(".govuk-error-message").length) {
            this.invalidState();
        }
    };

    Plugin.prototype.validState = function () {
        $(this.element).removeClass(this.options.errorCssClass);
    };

    Plugin.prototype.invalidState = function () {
        if (!$(this.element).hasClass(this.options.errorCssClass)) {
            $(this.element).addClass(this.options.errorCssClass);
        }
    };

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkErrorMessage",
        defaults = {
            errorCssClass: "govuk-error-message"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    Plugin.prototype.init = function () {
        if (!$(this.element).hasClass(this.options.errorCssClass)) {
            $(this.element).addClass(this.options.errorCssClass);
        }
        $(this.element).hide();
    };

    Plugin.prototype.show = function (message) {
        $(this.element).show();
        $(this.element).text(message);
    };

    Plugin.prototype.hide = function () {
        $(this.element).hide();
    };

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkInput",
        defaults = {
            errorCssClass: "govuk-input--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        $(this.element).removeClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();

        var $errorElement="";

        if ($(this.element).prevAll(".govuk-hint").length) {
            $errorElement = $(this.element).prevAll(".govuk-hint");
        }
        else {
            if ($(this.element).prevAll(".govuk-label").length) {
                $errorElement = $(this.element).prevAll(".govuk-label");
            }
            else {
                $errorElement = $(this.element)
            }
        }
       

        $(this.element).addClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore($errorElement);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkSelect",
        defaults = {
            errorCssClass: "govuk-select--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        $(this.element).removeClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        $(this.element).addClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkRadios",
        defaults = {};

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkCheckboxes",
        defaults = {};

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkTextarea",
        defaults = {
            errorCssClass: "govuk-input--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        $(this.element).removeClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        $(this.element).addClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        var characterCountErrorMessages = $(this._formGroup).find(".govuk-character-count__message.govuk-error-message");
        if (characterCountErrorMessages.length) {
            characterCountErrorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var outerIndex = 0;
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                outerIndex = outerIndex + index;
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + outerIndex;
                list.push($anchor);
            });
        }
        var characterCountErrorMessages = $(this._formGroup).find(".govuk-character-count__message.govuk-error-message");
        if (characterCountErrorMessages.length) {
            outerIndex = outerIndex + 1;
            characterCountErrorMessages.each(function (index, element) {
                outerIndex = outerIndex + index;
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + outerIndex;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkDateInput",
        defaults = {
            errorCssClass: "govuk-input--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._dayInput = undefined;
        this._monthInput = undefined;
        this._yearInput = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
        var inputs = $(this.element).find("input.govuk-date-input__input");
        if (inputs.length > 0) {
            this._dayInput = inputs[0];
        }
        if (inputs.length >= 1) {
            this._monthInput = inputs[1];
        }
        if (inputs.length >= 2) {
            this._yearInput = inputs[2];
        }
    };

    Plugin.prototype.validState = function () {
        $(this._dayInput).removeClass(this.options.errorCssClass);
        $(this._monthInput).removeClass(this.options.errorCssClass);
        $(this._yearInput).removeClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        $(this._dayInput).addClass(this.options.errorCssClass);
        $(this._monthInput).addClass(this.options.errorCssClass);
        $(this._yearInput).addClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    Plugin.prototype.getISODateString = function () {
        var parsedDay = "";
        if (this._dayInput) {
            parsedDay = parseInt($(this._dayInput).val());
            if (!isNaN(parsedDay)) {
                if (parsedDay > 0 && parsedDay < 10) {
                    parsedDay = "0" + parsedDay;
                }
            } else {
                parsedDay = NaN;
            }
        }
        var parsedMonth = "";
        if (this._monthInput) {
            parsedMonth = parseInt($(this._monthInput).val());
            if (!isNaN(parsedMonth)) {
                if (parsedMonth > 0 && parsedMonth < 10) {
                    parsedMonth = "0" + parsedMonth;
                }
            } else {
                parsedMonth = NaN;
            }
        }
        var parsedYear = "";
        if (this._yearInput) {
            parsedYear = parseInt($(this._yearInput).val());
            if (!isNaN(parsedYear)) {
                if (parsedYear > 0 && parsedYear < 10) {
                    parsedYear = "000" + parsedYear;
                } else if (parsedYear > 10 && parsedYear < 100) {
                    parsedYear = "00" + parsedYear;
                } else if (parsedYear > 100 && parsedYear < 1000) {
                    parsedYear = "0" + parsedYear;
                }
            } else {
                parsedYear = NaN;
            }
        }
        if (isNaN(parsedYear) && isNaN(parsedMonth) && isNaN(parsedDay)) return "";
        if (isNaN(parsedYear) || isNaN(parsedMonth) || isNaN(parsedDay) || parsedYear < 1 || parsedMonth < 1 || parsedDay < 1) return "Invalid Date";
        var parsedDate = new Date(parsedYear + "-" + parsedMonth + "-" + parsedDay);
        if (parsedDate.toString() === "Invalid Date") return parsedDate;
        if (parseInt(parsedDate.toISOString().substr(5, 2)) !== parseInt($(this._monthInput).val())) return "Invalid Date";
        var isoDateString = parsedDate.toString() === "Invalid Date" ? "Invalid Date" : parsedDate.toISOString().substr(0, 10);
        return isoDateString;
    };

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkFileUpload",
        defaults = {
            errorCssClass: "govuk-input--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
    };

    Plugin.prototype.validState = function () {
        $(this.element).removeClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        $(this.element).addClass(this.options.errorCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkDurationInput",
        defaults = {
            errorInputCssClass: "govuk-input--error",
            errorSelectCssClass: "govuk-select--error"
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this._formGroup = undefined;
        this._lengthInput = undefined;
        this._timePeriodSelect = undefined;
        this._errorMessages = [];
        this.init();
    }

    Plugin.prototype.init = function () {
        this._formGroup = $(this.element).closest(".govuk-form-group");
        this._formGroup.govUkFormGroup();
        var lengthInput = $(this.element).find("input");
        var timePeriodSelect = $(this.element).find("select");
        if (lengthInput.length > 0) {
            this._lengthInput = lengthInput[0];
        }
        if (timePeriodSelect.length >= 1) {
            this._timePeriodSelect = timePeriodSelect[0];
        }
    };

    Plugin.prototype.validState = function () {
        $(this._lengthInput).removeClass(this.options.errorInputCssClass);
        $(this._timePeriodSelect).removeClass(this.options.errorSelectCssClass);
        this._formGroup.govUkFormGroup("validState");
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("destroy");
            this._errorMessages.remove();
            this._errorMessages = [];
        }
    };

    Plugin.prototype.invalidState = function (messages) {
        this.validState();
        $(this._lengthInput).addClass(this.options.errorInputCssClass);
        $(this._timePeriodSelect).addClass(this.options.errorSelectCssClass);
        this._formGroup.govUkFormGroup("invalidState");
        if (typeof messages === "string") {
            messages = [messages];
        }
        if (Array.isArray(messages)) {
            messages.forEach(function (message) {
                var span = document.createElement("span");
                span.classList.add("govuk-error-message");
                span.innerHTML = message;
                $(span).insertBefore(this.element);
            }, this);
        }
        this._errorMessages = $(this.element).prevAll(".govuk-error-message");
        if (this._errorMessages.length) {
            this._errorMessages.govUkErrorMessage();
            this._errorMessages.govUkErrorMessage("show");
        }
    };

    Plugin.prototype.getErrorMessages = function () {
        var list = [];
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                list.push(element.innerHTML);
            });
        }
        return list;
    }

    Plugin.prototype.getErrorHashLinks = function () {
        var list = [];
        var elementId = this.element.id;
        if (this._errorMessages.length) {
            this._errorMessages.each(function (index, element) {
                var $anchor = document.createElement("a");
                $anchor.href = "#govuk-label-" + elementId;
                $anchor.innerHTML = element.innerHTML;
                $anchor.id = "error-hash-link-" + elementId + "-" + index;
                list.push($anchor);
            });
        }
        return list;
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

(function ($) {

    // Create the defaults once
    var pluginName = "govUkErrorSummary",
        defaults = {
            errorCssClass: ""
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;
        this._summaryList = undefined;
        this.options = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    Plugin.prototype.init = function () {
        if (!$(this.element).hasClass(this.options.errorCssClass)) {
            $(this.element).addClass(this.options.errorCssClass);
        }
        this._summaryList = $(this.element).find(".govuk-error-summary__list")[0];
    };

    Plugin.prototype.show = function () {
        $(this.element).show();
    };

    Plugin.prototype.hide = function () {
        $(this.element).hide();
    };

    Plugin.prototype.hideIfEmpty = function () {
        if ($(this._summaryList).find("li").length === 0) {
            this.hide();
        }
    }

    Plugin.prototype.add = function (errors) {
        if (!Array.isArray(errors)) {
            errors = [errors];
        }
        if (errors && Array.isArray(errors)) {
            errors.forEach(function (error) {
                var li = document.createElement("li");
                if (error instanceof HTMLElement) {
                    $("#" + error.id).remove();
                    li.append(error);
                } else {
                    li.innerHTML = error;
                }
                this._summaryList.append(li);
            }, this);
        }
    }

    Plugin.prototype.remove = function (selector) {
        if (typeof selector === "string") {
            $(this._summaryList).find(selector).remove();
            $(this._summaryList).find("li:empty").remove();
        }
    }

    Plugin.prototype.removeErrorHashLinksFor = function (id) {
        if (typeof id === "string") {
            var selector = "[id^=error-hash-link-" + id + "-]";
            this.remove(selector);
        }
    }

    Plugin.prototype.removeAll = function () {
        $(this._summaryList).empty();
    }

    $.fn[pluginName] = function (options) {
        var args = arguments;
        if (options === undefined || typeof options === "object") {
            return this.each(function () {
                if (!$.data(this, "plugin_" + pluginName)) {
                    $.data(this, "plugin_" + pluginName, new Plugin(this, options));
                }
            });
        } else if (typeof options === "string" && options[0] !== "_" && options !== "init") {
            var returns;
            this.each(function () {
                var instance = $.data(this, "plugin_" + pluginName);
                if (instance instanceof Plugin && typeof instance[options] === "function") {
                    returns = instance[options].apply(instance, Array.prototype.slice.call(args, 1));
                }
                if (options === "destroy") {
                    $.data(this, "plugin_" + pluginName, null);
                }
            });
            return returns !== undefined ? returns : this;
        }
    };

}(jQuery, window, document));

// validation
// ==========

(function ($) {

    function Plugin() {
        this.init();
    }

    Plugin.prototype.init = function () { };

    Plugin.prototype.validate = function (input) {
        return !input ? false : true;
    };

    $.extend({
        requiredValidate: function (input) {
            var plugin = new Plugin();
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.max = parseInt(this.options.max);
    };

    Plugin.prototype.validate = function (input) {
        var parsed = !input ? 0 : input.length;
        if (parsed === 0) return true;
        if (isNaN(parsed) || isNaN(this.options.max)) {
            return false;
        } else {
            return this.options.max >= parsed;
        }
    };

    $.extend({
        maxLengthValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.min = parseInt(this.options.min);
    };

    Plugin.prototype.validate = function (input) {
        var parsed = !input ? 0 : input.length;
        if (parsed === 0) return true;
        if (isNaN(parsed) || isNaN(this.options.min)) {
            return false;
        } else {
            return this.options.min <= parsed;
        }
    };

    $.extend({
        minLengthValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.min = parseFloat(this.options.min);
        this.options.max = parseFloat(this.options.max);
    };

    Plugin.prototype.validate = function (input) {
        var parsed = parseFloat(input);
        if (isNaN(parsed) && input.length === 0) {
            return true;
        }
        if (isNaN(parsed) || isNaN(this.options.min) || isNaN(this.options.max)) {
            return false;
        } else {
            return parsed >= this.options.min && parsed <= this.options.max;
        }
    };

    $.extend({
        rangeValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.regex = new RegExp(new String(this.options.regex));
    };

    Plugin.prototype.validate = function (input) {
        if (input === "") {
            return true;
        } else {
            return this.options.regex.test(input);
        }
    };

    $.extend({
        regexValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin() {
        this.init();
    }

    Plugin.prototype.init = function () { };

    Plugin.prototype.validate = function (input) {
        if (!input) {
            return true;
        } else {
            var date = new Date(input);
            if (date.toString() === "Invalid Date") return false;
            if (!/(\d{4})-(\d{2})-(\d{2})/.test(input)) return false;
            return parseInt(date.toISOString().substr(7, 2)) === parseInt(input.substr(7, 2));
        }
    };

    $.extend({
        dateValidate: function (input) {
            var plugin = new Plugin();
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.ref = new Date(new String(this.options.ref));
    };

    Plugin.prototype.validate = function (input) {
        if (!input || Object.prototype.toString.call(this.options.ref) !== "[object Date]") {
            return true;
        } else {
            var date = new Date(input);
            if (date.toString() === "Invalid Date") return false;
            if (!/(\d{4})-(\d{2})-(\d{2})/.test(input)) return false;
            if (parseInt(date.toISOString().substr(7, 2)) !== parseInt(input.substr(7, 2))) return false;
            return (this.options.ref.getTime() - date.getTime()) > 0;
        }
    };

    $.extend({
        pastDateValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.ref = new Date(new String(this.options.ref));
    };

    Plugin.prototype.validate = function (input) {
        if (!input || Object.prototype.toString.call(this.options.ref) !== "[object Date]") {
            return true;
        } else {
            var date = new Date(input);
            if (date.toString() === "Invalid Date") return false;
            if (!/(\d{4})-(\d{2})-(\d{2})/.test(input)) return false;
            if (parseInt(date.toISOString().substr(7, 2)) !== parseInt(input.substr(7, 2))) return false;
            return (this.options.ref.getTime() - date.getTime()) < 0;
        }
    };

    $.extend({
        futureDateValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.max = parseInt(this.options.max);
    };

    Plugin.prototype.validate = function (input) {
        if (!input) {
            return true;
        } else {
            return input.length > this.options.max ? false : true;
        }
    };

    $.extend({
        characterCountValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.max = parseInt(this.options.max);
    };

    Plugin.prototype.validate = function (input) {
        if (isNaN(this.options.max)) return false;
        if (input instanceof File) {
            return this.options.max >= input.size;
        } else {
            return false;
        }
    };

    $.extend({
        fileSizeMaxValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.min = parseInt(this.options.min);
    };

    Plugin.prototype.validate = function (input) {
        if (isNaN(this.options.max)) return false;
        if (input instanceof File) {
            return this.options.min <= input.size;
        } else {
            return false;
        }
    };

    $.extend({
        fileSizeMinValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(options) {
        this.options = $.extend({}, options);
        this.init();
    }

    Plugin.prototype.init = function () {
        this.options.extensions = this.options.extensions.split(",").map(function (item) {
            return item.trim();
        });
    };

    Plugin.prototype.validate = function (input) {
        if (!$.isArray(this.options.extensions)) return false;
        if (input instanceof File) {
            var ext = input.name.slice((input.name.lastIndexOf(".") - 1 >>> 0) + 2);
            return $.inArray(ext, this.options.extensions) > -1;
        } else {
            return false;
        }
    };

    $.extend({
        fileExtensionsValidate: function (options, input) {
            var plugin = new Plugin(options);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));

(function ($) {

    function Plugin(fn) {
        this.fn = fn;
        this.init();
    }

    Plugin.prototype.init = function () { };

    Plugin.prototype.validate = function (input) {
        return this.fn(input);
    };

    $.extend({
        customValidate: function (fn, input) {
            var plugin = new Plugin(fn);
            return plugin.validate(input);
        }
    });

}(jQuery, window, document));
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
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


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