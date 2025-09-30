// Set up event delegation on document load
document.addEventListener('DOMContentLoaded', function () {
    // Use event delegation to catch form submissions
    document.addEventListener('submit', function (event) {
        debugger;
        // Check if the submitted form is an Umbraco form
        const form = event.target;
        if (form.closest('[class*="umbraco-forms"]')) {
            // Prevent default submission initially
            event.preventDefault();

            // Your custom validation logic here
            customValidation(form).then(isValid => {
                if (!isValid) {
                    return false; // Stop submission if validation fails
                }

                // If validation passes, submit the form normally (let Umbraco handle it)
                submitFormNormally(form);
            });
        }
    });

    async function customValidation(form) {
        // Clear any previous error messages
        clearValidationErrors(form);

        // Add your custom validation logic here
        const requiredFields = form.querySelectorAll('[required]');
        for (let field of requiredFields) {
            if (!field.value.trim()) {
                showValidationError(field, 'Please fill in all required fields');
                field.focus();
                return false;
            }
        }

        // License validation - find the license field using the class that contains "licensenumber"
        const licenseField = form.querySelector('.form-group.licensenumber input[type="text"]');

        console.log('License field found:', licenseField);

        if (licenseField && licenseField.value.trim()) {
            const licenseValidation = await validateLicense(licenseField.value);
            if (!licenseValidation.isValid) {
                showValidationError(licenseField, licenseValidation.message || 'License is expired. Please renew your license to continue.');
                licenseField.focus();
                return false;
            }
        }

        return true;
    }

    async function validateLicense(licenseValue) {
        try {
            const response = await fetch('/umbraco/api/license/validate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ licenseNumber: licenseValue })
            });

            if (!response.ok) {
                console.error('License validation API error:', response.statusText);
                return { isValid: true }; // Allow submission if API fails
            }

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('License validation error:', error);
            return { isValid: true };
        }
    }

    function showValidationError(field, message) {
        // Remove any existing error for this field
        const existingError = field.parentNode.querySelector('.validation-error');
        if (existingError) {
            existingError.remove();
        }

        // Add error class to field
        field.classList.add('validation-error-field');

        // Create error message element
        const errorElement = document.createElement('div');
        errorElement.className = 'validation-error';
        errorElement.textContent = message;
        errorElement.style.color = 'red';
        errorElement.style.fontSize = '0.875rem';
        errorElement.style.marginTop = '4px';

        // Insert error message after the field
        field.parentNode.insertBefore(errorElement, field.nextSibling);
    }

    function clearValidationErrors(form) {
        // Remove error classes and messages
        form.querySelectorAll('.validation-error-field').forEach(field => {
            field.classList.remove('validation-error-field');
        });
        form.querySelectorAll('.validation-error').forEach(error => {
            error.remove();
        });
    }

    // Submit form normally through Umbraco's flow
    function submitFormNormally(form) {
        // Remove our event listener temporarily to avoid infinite loop
        document.removeEventListener('submit', arguments.callee);

        // Create a new submit event and dispatch it
        const submitEvent = new Event('submit', {
            bubbles: true,
            cancelable: true
        });

        // Re-add our event listener after a short delay
        setTimeout(() => {
            document.addEventListener('submit', arguments.callee);
        }, 100);

        // Submit the form normally
        form.submit();
    }
});