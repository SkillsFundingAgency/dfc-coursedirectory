/* eslint-disable no-console */
"use strict";

// requires

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    sass = require("gulp-sass"),
    eslint = require("gulp-eslint"),
    merge = require("merge-stream");

// paths

var paths = {
    webroot: "wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.scss = paths.webroot + "scss/**/*.scss";
paths.vendorScssDest = paths.webroot + "vendor/scss/";
paths.css = paths.webroot + "css/**/*.css";
paths.cssDest = paths.webroot + "css/";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.dist = paths.webroot + "dist/";
paths.concatJsDest = paths.dist + "site.js";
paths.concatMinJsDest = paths.dist + "site.min.js";
paths.concatMinCssDest = paths.dist + "site.min.css";
paths.vendorJsDest = paths.webroot + "vendor/";
paths.vendorJsGovUkFrontend = paths.vendorJsDest + "govuk-frontend/all.js";
paths.vendorJsGovUkFrontendDest = paths.vendorJsDest + "govuk-frontend/all.min.js";

// dependencies

var jsVendorDeps = {
    "jquery": {
        "dist/**.min.js": ""
    },
    "jquery-validation": {
        "dist/**.min.js": ""
    },
    "jquery-validation-unobtrusive": {
        "dist/**/*.min.js": ""
    },
    "govuk-frontend": {
        "all.js": ""
    }
};

// tasks

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatMinJsDest + "", cb);
});

gulp.task("clean:js:vendor", function (cb) {
    rimraf(paths.vendorJsDest + "**/*", cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.css, cb);
    rimraf(paths.concatMinCssDest, cb);
});

gulp.task("sass", function () {
    return gulp.src(paths.scss)
        .pipe(sass({ outputStyle: "expanded" }))
        .pipe(gulp.dest(paths.cssDest));
});

gulp.task("js", function () {
    return gulp.src([paths.js], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(gulp.dest("."));
});

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatMinJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:js:vendor:govuk-frontend", function () {
    return gulp.src([paths.vendorJsGovUkFrontend], { base: "." })
        .pipe(concat(paths.vendorJsGovUkFrontendDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatMinCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("eslint", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(eslint())
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});

gulp.task("js:vendor", function () {
    var streams = [];

    for (var prop in jsVendorDeps) {
        console.log("Getting vendor js for: " + prop);
        for (var itemProp in jsVendorDeps[prop]) {
            console.log("node_modules/" + prop + "/" + itemProp);
            console.log(paths.vendorJsDest + prop + "/" + jsVendorDeps[prop][itemProp]);
            streams.push(gulp.src("node_modules/" + prop + "/" + itemProp)
                .pipe(gulp.dest(paths.vendorJsDest + prop + "/" + jsVendorDeps[prop][itemProp])));
        }
    }

    return merge(streams);
});

// watches

gulp.task("css:watch", function () {
    gulp.watch([paths.css], gulp.series("min:css"));
});

gulp.task("sass:watch", function () {
    gulp.watch(paths.scss, gulp.series("sass"));
});

gulp.task("eslint:watch", function () {
    gulp.watch([paths.js], gulp.series("eslint"));
});

gulp.task("js:watch", function () {
    gulp.watch([paths.js], gulp.series("min:js"));
});

// commands

gulp.task("clean", gulp.parallel("clean:js", "clean:js:vendor", "clean:css"));
gulp.task("min", gulp.parallel("min:js", "min:js:vendor:govuk-frontend", "min:css"));

gulp.task("dev",
    gulp.series(
        "clean",
        "sass",
        "js:vendor",
        "js",
        gulp.parallel(
            "css:watch",
            "sass:watch",
            "js:watch",
            "eslint:watch"))
);

gulp.task("prod",
    gulp.series(
        "clean",
        "sass",
        "js:vendor",
        "eslint",
        "min")
);