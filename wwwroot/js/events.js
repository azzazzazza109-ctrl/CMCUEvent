// Gửi GET đến backend để cập nhật số chỗ, không lưu dữ liệu trong JS.
const refresh = document.getElementById('refresh-availability');
if (refresh) {
    refresh.addEventListener('click', async function () {
        const message = document.getElementById('availability-message');
        refresh.disabled = true;
        try {
            const response = await fetch(refresh.dataset.url, { cache: 'no-store' });
            if (!response.ok) throw new Error('Không lấy được dữ liệu');
            const data = await response.json();
            document.getElementById('remaining').textContent = data.remaining;
            document.getElementById('event-status').textContent = data.status;
            message.textContent = 'Đã cập nhật. Tải lại trang nếu muốn cập nhật các nút đăng ký.';
        } catch (error) {
            message.textContent = 'Không thể cập nhật số chỗ. Vui lòng thử lại.';
        } finally { refresh.disabled = false; }
    });
}
