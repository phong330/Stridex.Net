# STRIDEX Final C# Project

Website bán hàng thể thao STRIDEX dùng **C# ASP.NET Core MVC + SQL Server**.

## Chức năng đã làm

- Trang chủ hiển thị sản phẩm nổi bật.
- Danh sách sản phẩm: tìm kiếm, lọc loại, sắp xếp giá/tên.
- Chi tiết sản phẩm.
- Đăng ký, đăng nhập, đăng xuất bằng Session.
- Đăng nhập bằng Google OAuth 2.0.
- Tự động tạo tài khoản người dùng khi đăng nhập Google lần đầu.
- Cho phép chọn tài khoản Google khác khi đăng nhập lại.
- Giỏ hàng: thêm, cập nhật số lượng, xoá sản phẩm.
- Thanh toán và lưu đơn hàng vào SQL Server.
- Lịch sử đơn hàng và chi tiết đơn hàng.
- Trang Admin:
  - Thống kê số sản phẩm, đơn hàng, người dùng.
  - Thêm, sửa, xoá sản phẩm.
  - Xem và cập nhật trạng thái đơn hàng.
  - Xem danh sách người dùng.

## Database

File SQL nằm ở:

`Database/StridexDataBase.sql`

Cách import:

1. Mở SQL Server Management Studio.
2. Tạo database tên `StridexDB` nếu chưa có.
3. Mở file `Database/StridexDataBase.sql`.
4. Chạy script.

Tài khoản admin có sẵn trong file SQL:

- Email: `admin@gmail.com`
- Mật khẩu: `123456`

## Chỉnh chuỗi kết nối

Mở `appsettings.json` và sửa dòng:

```json
"DefaultConnection": "Server=.;Database=StridexDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Một số ví dụ:

```json
"Server=localhost;Database=StridexDB;Trusted_Connection=True;TrustServerCertificate=True"
```

hoặc nếu dùng SQL Server Express:

```json
"Server=.\\SQLEXPRESS;Database=StridexDB;Trusted_Connection=True;TrustServerCertificate=True"
```

## Cấu hình Google Login

Tạo OAuth Client trên Google Cloud Console.

Sau đó mở `appsettings.json` và thêm:

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

Redirect URI khi chạy local:

```text
https://localhost:xxxx/signin-google
```

Thay `xxxx` bằng port của project.

## Cách chạy

1. Mở thư mục project bằng Visual Studio.
2. Restore NuGet Package nếu Visual Studio hỏi.
3. Kiểm tra chuỗi kết nối trong `appsettings.json`.
4. Kiểm tra Google ClientId và ClientSecret nếu dùng đăng nhập Google.
5. Bấm Run.

Hoặc dùng terminal:

```bash
dotnet restore
dotnet run
```

## Công nghệ

- C#
- ASP.NET Core MVC
- Razor View
- SQL Server
- ADO.NET với `Microsoft.Data.SqlClient`
- Session
- Cookie Authentication
- Google OAuth 2.0 Authentication
- Bootstrap 5
- HTML/CSS/JavaScript cơ bản

## Lưu ý

Project này bám theo mức bài thực hành C#/.NET cơ bản: không dùng microservices, Docker, Kubernetes hay các kỹ thuật quá nâng cao. 

Mục tiêu là chạy được, có CRUD, có giỏ hàng, có đặt hàng, đăng nhập Google OAuth và có quản trị.