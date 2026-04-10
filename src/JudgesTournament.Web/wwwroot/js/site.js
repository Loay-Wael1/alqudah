// بطولة القضاة 2026 - Site JavaScript

// File upload size validation (client-side)
document.addEventListener('DOMContentLoaded', function () {
    const fileInput = document.querySelector('input[type="file"]');
    if (fileInput) {
        fileInput.addEventListener('change', function () {
            const maxSize = 2 * 1024 * 1024; // 2MB
            if (this.files.length > 0 && this.files[0].size > maxSize) {
                alert('حجم الملف يتجاوز الحد المسموح (2 ميجابايت). يرجى اختيار ملف أصغر.');
                this.value = '';
            }
        });
    }

    // Prevent double submit
    const form = document.getElementById('registration-form');
    if (form) {
        form.addEventListener('submit', function () {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> جاري الإرسال...';
            }
        });
    }
});
