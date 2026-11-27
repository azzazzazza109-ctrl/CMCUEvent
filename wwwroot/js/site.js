// Mở hoặc đóng menu khi màn hình nhỏ.
const menuButton = document.getElementById('menu-button');
const nav = document.getElementById('main-nav');
if (menuButton && nav) {
    menuButton.addEventListener('click', function () {
        const open = nav.classList.toggle('open');
        menuButton.setAttribute('aria-expanded', String(open));
    });
}
// Xác nhận trước khi thực hiện thao tác cần lưu ý.
for (const form of document.querySelectorAll('form[data-confirm]')) {
    form.addEventListener('submit', function (event) {
        if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
}
