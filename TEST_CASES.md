# Kế hoạch kiểm thử – chưa thực thi

Tất cả dòng dưới đây chưa chạy. Sau khi chạy trên máy thật, ghi PASS/FAIL, ngày giờ, dữ liệu đầu vào thực tế và ảnh/log minh chứng. Không sửa kết quả dự kiến thành kết quả thực tế khi chưa kiểm tra.

| ID | Các bước | Kết quả mong đợi | Thực tế |
|---|---|---|---|
| TC01 | Đăng ký email mới, mật khẩu hợp lệ | Tạo User, không phải Admin | Chưa chạy |
| TC02 | Đăng ký lại email đã có | Không tạo tài khoản trùng | Chưa chạy |
| TC03 | Đăng nhập User đúng | Hiện menu Đã đăng ký | Chưa chạy |
| TC04 | Đăng nhập sai mật khẩu | Báo lỗi; nhiều lần sai có khóa tạm | Chưa chạy |
| TC05 | Đăng xuất rồi vào lịch sử | Yêu cầu đăng nhập | Chưa chạy |
| TC06 | User vào /AdminEvents và /Categories/Create | 403, không sửa dữ liệu | Chưa chạy |
| TC07 | Khách mở sự kiện đang mở | Có link đăng nhập; POST bị bảo vệ | Chưa chạy |
| TC08 | Admin thêm, xem, sửa, xóa danh mục chưa dùng | Thao tác thành công | Chưa chạy |
| TC09 | Xóa danh mục đang chứa sự kiện | Bị từ chối | Chưa chạy |
| TC10 | Admin thêm, xem, sửa và xóa sự kiện chưa có đăng ký | CRUD đúng | Chưa chạy |
| TC11 | Ngày kết thúc trước giờ bắt đầu (thử cả tắt JS) | Không lưu, báo lỗi | Chưa chạy |
| TC12 | Sức chứa = 0 hoặc giảm dưới số đã đăng ký | Không lưu | Chưa chạy |
| TC13 | Mở danh sách 8 sự kiện, đổi trang; tìm/lọc rồi chuyển trang nếu có đủ dữ liệu | 6 mục/trang, giữ bộ lọc | Chưa chạy |
| TC14 | User đăng ký một sự kiện còn chỗ | Có bản ghi Registered, số chỗ giảm 1 | Chưa chạy |
| TC15 | Gửi lại POST đăng ký cùng sự kiện bằng phiên User | Không có bản ghi trùng | Chưa chạy |
| TC16 | Thử đăng ký khi đầy, đóng, quá giờ | Từ chối cả ba tình huống | Chưa chạy |
| TC17 | Hủy đăng ký trước giờ bắt đầu, đăng ký lại | Giải phóng chỗ, rồi giảm lại; không tạo trùng | Chưa chạy |
| TC18 | User B gửi POST Cancel với id của User A, có token hợp lệ của B | 404, A không đổi | Chưa chạy |
| TC19 | Tạo sự kiện sức chứa 1, tạo hai User, dùng hai phiên trình duyệt riêng bấm đăng ký gần đồng thời | Tối đa 1 Registered; kiểm tra trực tiếp SQL | Chưa chạy |
| TC20 | F12 device toolbar kiểm tra trang danh sách/form/admin ở 375px | Nội dung đọc được; bảng có cuộn ngang | Chưa chạy |
| TC21 | Upload JPG/PNG hợp lệ rồi sửa ảnh; thử file .exe hoặc >2MB | Ảnh hợp lệ hiện đúng, file sai bị từ chối | Chưa chạy |
| TC22 | Admin xóa sự kiện từng có đăng ký (kể cả đã hủy) | Bị từ chối | Chưa chạy |
| TC23 | Admin xem Participants; User mở URL này | Admin thấy danh sách, User bị từ chối | Chưa chạy |
| TC24 | Mở chi tiết ID không tồn tại | 404 | Chưa chạy |
| TC25 | Admin đóng/mở sự kiện tương lai; User thử đăng ký | Đóng thì chặn, mở thì cho nếu đủ điều kiện | Chưa chạy |
| TC26 | Gửi POST thay đổi dữ liệu thiếu antiforgery token | 400, không thay đổi dữ liệu | Chưa chạy |

SQL xác minh TC19 (thay 123 bằng Id thật):

```sql
SELECT EventId, COUNT(*) AS RegisteredCount
FROM Registrations
WHERE EventId = 123 AND Status = 'Registered'
GROUP BY EventId;
```

Kết quả chỉ được là 1 nếu có một đăng ký thành công; nếu cả hai bị từ chối do tranh chấp, ghi FAIL và thử lại. Có thể dùng DevTools Network để quan sát request/response; không cần công cụ test ngoài giáo trình.
