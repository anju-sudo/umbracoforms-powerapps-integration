// Set up event delegation on document load
document.addEventListener('DOMContentLoaded', function () {
    console.log('License validation script loaded');

    let isValidating = false;
    let validationComplete = false;

    // Use event delegation to catch form submissions
    document.addEventListener('submit', function (event) {
        // Check if the submitted form is an Umbraco form
        const form = event.target;

        // Look for Umbraco forms
        if (form.closest('[class*="umbraco-forms"]') ||
            form.querySelector('[class*="umbraco-forms"]') ||
            form.action.includes('SubmitForm')) {

            console.log('Umbraco form submission detected');

            // Check if this submission was triggered by a navigation button (Previous only)
            const activeElement = document.activeElement;
            const isPreviousSubmission = activeElement &&
                (activeElement.name === '_prev' ||
                    activeElement.value === 'Previous' ||
                    activeElement.classList.contains('btn-prev'));

            if (isPreviousSubmission) {
                console.log('Previous button submission detected, allowing without validation');
                return true; // Allow Previous button submissions only
            }

            // If validation is complete and passed, allow submission
            if (validationComplete) {
                console.log('Validation already completed, allowing submission');
                validationComplete = false; // Reset for next time
                return true; // Allow normal submission
            }

            // If we're currently validating, prevent submission
            if (isValidating) {
                console.log('Validation in progress, preventing submission');
                event.preventDefault();
                return false;
            }

            // Start validation process
            console.log('Starting validation process');
            event.preventDefault();
            event.stopImmediatePropagation();

            handleFormValidation(form);
        }
    }, true);

    async function handleFormValidation(form) {
        console.log('Handling form validation...');
        isValidating = true;

        try {
            // Clear any previous error messages
            clearValidationErrors(form);

            // Find license field
            let licenseField = findLicenseField(form);
            console.log('License field found:', licenseField);

            if (licenseField && licenseField.value.trim()) {
                console.log('Validating license:', licenseField.value.trim());

                // Validate license via your controller
                const licenseValidation = await validateLicense(licenseField.value.trim());
                console.log('License validation result:', licenseValidation);

                if (!licenseValidation.isValid) {
                    // Show validation error and stop submission
                    showValidationError(licenseField, licenseValidation.message || 'License validation failed');
                    licenseField.focus();
                    isValidating = false;
                    return;
                }
            }

            // Validation passed
            console.log('Validation passed, triggering form submission');
            isValidating = false;
            validationComplete = true;

            // Trigger form submission after a small delay to ensure flags are set
            setTimeout(() => {
                form.submit();
            }, 10);

        } catch (error) {
            console.error('Error in form validation:', error);
            // On error, allow normal submission
            isValidating = false;
            validationComplete = true;
            setTimeout(() => {
                form.submit();
            }, 10);
        }
    }

    function findLicenseField(form) {
        // Try multiple selectors to find the license field
        const selectors = [
            '.form-group.licensenumber input[type="text"]',
            'input[name*="license"]',
            'input[name*="licence"]',
            'input[id*="license"]',
            'input[id*="licence"]',
            '.umbraco-forms-field.licencenumber input',
            '.form-group input[name*="33e67d74"]',
            'input[name*="33e67d74"]'
        ];

        for (let selector of selectors) {
            const field = form.querySelector(selector);
            if (field) {
                console.log(`License field found with selector: ${selector}`);
                return field;
            }
        }

        console.log('No license field found');
        return null;
    }

    async function validateLicense(licenseValue) {
        try {
            console.log('Calling license validation API...');
            const response = await fetch('/umbraco/api/license/validate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ licenseNumber: licenseValue })
            });

            if (!response.ok) {
                console.error('License validation API error:', response.status, response.statusText);
                return { isValid: true }; // Allow submission if API fails
            }

            const result = await response.json();
            console.log('API response:', result);
            return result;
        } catch (error) {
            console.error('License validation error:', error);
            return { isValid: true }; // Allow submission if API fails
        }
    }

    function showValidationError(field, message) {
        console.log('Showing validation error:', message);

        // Remove any existing errors
        clearValidationErrors(field.closest('form'));

        // Add error class to field
        field.classList.add('validation-error-field');
        field.style.borderColor = '#dc3545';
        field.style.borderWidth = '2px';

        // Create error message element
        const errorElement = document.createElement('div');
        errorElement.className = 'validation-error license-validation-error';
        errorElement.textContent = message;
        errorElement.style.cssText = `
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 4px;
            font-weight: 500;
            display: block;
        `;

        // Insert error message after the field's wrapper
        const fieldWrapper = field.closest('.col-sm-10') || field.parentNode;
        fieldWrapper.appendChild(errorElement);

        // Scroll to the error field
        field.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function clearValidationErrors(form) {
        // Remove error classes and messages
        form.querySelectorAll('.validation-error-field').forEach(field => {
            field.classList.remove('validation-error-field');
            field.style.borderColor = '';
            field.style.borderWidth = '';
        });
        form.querySelectorAll('.validation-error').forEach(error => {
            error.remove();
        });
    }

    // Clear validation errors when user starts typing in license field
    document.addEventListener('input', function (event) {
        const field = event.target;
        if (field.matches('input[name*="license"], input[name*="licence"], input[id*="license"], input[id*="licence"]') ||
            field.closest('.form-group.licensenumber') ||
            field.name.includes('33e67d74')) {

            if (field.classList.contains('validation-error-field')) {
                clearValidationErrors(field.closest('form'));
            }
        }
    });

    // Handle submit button clicks as backup
    document.addEventListener('click', function (event) {
        if (event.target.type === 'submit' || event.target.matches('input[type="submit"], button[type="submit"]')) {
            const form = event.target.closest('form');
            if (form && (form.closest('[class*="umbraco-forms"]') || form.querySelector('[class*="umbraco-forms"]'))) {

                // Check if this is a Previous button - exclude from validation
                const isPreviousButton = event.target.name === '_prev' ||
                    event.target.value === 'Previous' ||
                    event.target.classList.contains('btn-prev');

                if (isPreviousButton) {
                    console.log('Previous button clicked, allowing without validation');
                    return true; // Allow Previous button to work normally
                }

                console.log('Submit button clicked');

                // If validation is complete, allow click
                if (validationComplete) {
                    console.log('Validation complete, allowing button click');
                    return true;
                }

                // If validation in progress, prevent
                if (isValidating) {
                    console.log('Validation in progress, preventing button click');
                    event.preventDefault();
                    return false;
                }

                // Start validation
                console.log('Starting validation from button click');
                event.preventDefault();
                event.stopImmediatePropagation();
                handleFormValidation(form);
            }
        }
    }, true);
});