$(function () {
    // If the focused element on page load is within a govuk-form-group ensure the top of govuk-form-group is visible
    const formGroup = $(':focus').closest('.govuk-form-group')[0];
    if (formGroup) {
        formGroup.scrollIntoView();
    }
})
