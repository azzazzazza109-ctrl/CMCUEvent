// Kiểm tra nhanh trên trình duyệt; server vẫn kiểm tra lại.
const registerForm = document.getElementById('register-form');
if (registerForm) {
    registerForm.addEventListener('submit', function (event) {
        const password = document.getElementById('Password').value;
        const confirmPassword = document.getElementById('ConfirmPassword').value;
        const message = document.getElementById('password-error');
        message.textContent = '';
        if (password !== confirmPassword) {
            event.preventDefault();
            message.textContent = 'Mật khẩu xác nhận không khớp.';
        }
    });
}
const eventForm = document.getElementById('event-form');
if (eventForm) {
    const startInput = document.getElementById('StartTime');
    const endInput = document.getElementById('EndTime');
    const message = document.getElementById('time-error');

    // Cập nhật lỗi ngay khi người dùng sửa thời gian.
    function validateTime() {
        const start = startInput.value;
        const end = endInput.value;
        const invalid = start && end && new Date(end) <= new Date(start);
        message.textContent = invalid ? 'Ngày kết thúc phải sau ngày bắt đầu.' : '';
        return !invalid;
    }

    startInput.addEventListener('input', validateTime);
    endInput.addEventListener('input', validateTime);
    startInput.addEventListener('change', validateTime);
    endInput.addEventListener('change', validateTime);
    eventForm.addEventListener('submit', function (event) {
        if (!validateTime()) event.preventDefault();
    });
}
