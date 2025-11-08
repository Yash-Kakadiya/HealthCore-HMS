// This script runs on every page after the DOM is fully loaded.
document.addEventListener('DOMContentLoaded', function () {

    // Configure the theme and default animations for all toasts
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3500,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
            const popup = Swal.getPopup();
            if (popup) {
                // These styles are assumed to be in your main CSS file
                popup.style.background = 'var(--glass-bg)';
                popup.style.backdropFilter = 'blur(10px)';
                popup.style.border = '1px solid var(--glass-border)';
                popup.style.color = 'var(--text-color-primary)';
            }
        },
        showClass: { popup: 'animate__animated animate__fadeInRight' },
        hideClass: { popup: 'animate__animated animate__fadeOutRight' }
    });

    // Check sessionStorage for messages, display them, and then clear them
    const successMsg = sessionStorage.getItem('hms_successMessage');
    const successType = sessionStorage.getItem('hms_successType');

    if (successMsg) {
        let toastOptions = {};
        switch (successType) {
            case 'add':
                toastOptions.html = `<div class="d-flex align-items-center"><i class="bi bi-person-plus-fill fs-4 text-success me-2"></i> <div>${successMsg}</div></div>`;
                break;
            case 'update':
                toastOptions.html = `<div class="d-flex align-items-center"><i class="bi bi-pencil-square fs-4 text-primary me-2"></i> <div>${successMsg}</div></div>`;
                break;
            case 'delete':
                toastOptions.html = `<div class="d-flex align-items-center"><i class="bi bi-trash3-fill fs-4 text-danger me-2"></i> <div>${successMsg}</div></div>`;
                toastOptions.showClass = { popup: 'animate__animated animate__shakeX' };
                break;
            default: // Fallback for generic success messages
                toastOptions.icon = 'success';
                toastOptions.title = successMsg;
                break;
        }
        Toast.fire(toastOptions);
        // CRUCIAL: Clear the items from storage after showing them
        sessionStorage.removeItem('hms_successMessage');
        sessionStorage.removeItem('hms_successType');
    }

    const errorMsg = sessionStorage.getItem('hms_errorMessage');
    if (errorMsg) {
        Swal.fire({
            icon: 'error',
            title: 'An Error Occurred',
            text: errorMsg,
        });
        // CRUCIAL: Clear the item from storage after showing it
        sessionStorage.removeItem('hms_errorMessage');
    }
});