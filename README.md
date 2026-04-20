# Ô Ăn Quan (C# WinForms)
Trò chơi Ô Ăn Quan truyền thống Việt Nam được xây dựng bằng **C# WinForms**, hỗ trợ chơi 1 người (đấu với máy) và 2 người chơi với nhau (người với người).

---

# Giới thiệu
Ô Ăn Quan là trò chơi dân gian Việt Nam với bàn cờ gồm 12 ô:
- 10 ô dân (5 ô mỗi bên), mỗi ô ban đầu có 5 dân
- 2 ô quan, mỗi ô có 1 quan (1 quan tương đương 10 điểm).
Người chơi lần lượt bốc hạt từ ô của mình và rải theo chiều trái hoặc phải, thu hạt khi thỏa điều kiện. Người có tổng điểm cao nhất khi ăn hết 2 ô quan thì thắng cuộc.

---

# Yêu cầu hệ thống
- Visual Studio 2022 chọn Windows Forms App 
- Thư viện Guna.UI2 (UI components)
- Thư viện NAudio (xử lý âm thanh)

---

# Cài đặt & Chạy
1. Download file zip BTL_AI
1. Giải nén và mở file `AI.csproj` bằng Visual Studio.
2. Restore NuGet packages (Guna.UI2, NAudio).
3. Build và chạy dự án (`F5`).

---

# Cấu trúc dự án

BTL_AI/
│
├── Program.cs              # Điểm khởi chạy ứng dụng
├── AppState.cs             # Trạng thái toàn cục (mute, độ khó, điều hướng)
├── SoundManager.cs         # Quản lý âm thanh (nhạc nền + hiệu ứng)
│
├── Main.cs                 # Form trang chủ (menu chính)
├── Start.cs                # Form chọn chế độ chơi (1 hay 2 người)
├── OnePeople.cs            # Form nhập tên & chọn độ khó (1 người)
├── TwoPeople.cs            # Form nhập tên 2 người chơi
├── Game.cs                 # Logic chính của trò chơi
├── Pause.cs                # Form tạm dừng
├── Result.cs               # Form kết quả
├── Guide.cs                # Form hướng dẫn chơi
├── About.cs                # Form thông tin về ứng dụng
│
├── Sounds/
│   ├── main.wav            # Nhạc nền
│   ├── click.wav           # Âm thanh click nút
│   ├── an.wav              # Âm thanh ăn quân
│   └── win.wav             # Âm thanh chiến thắng
│
└── AI.csproj               # File cấu hình dự án


---

# Chế độ chơi

# 1 Người (Người vs Máy)
- Nhập tên người chơi.
- Chọn độ khó:
  - Dễ 
  - Trung bình 
  - Khó 

# 2 Người (Người vs Người)
- Nhập tên 2 người chơi, lần lượt chơi trên cùng một máy.

---

# Luật chơi

1. Chọn ô: Người chơi chọn một ô dân ở phía mình
2. Rải hạt: Chọn hướng trái hoặc phải để rải lần lượt vào các ô liền kề.
3. Rải tiếp: Nếu ô cuối cùng vừa rải có hạt, bốc tiếp ô đó và rải tiếp.
4. Ăn hạt: Nếu ô tiếp theo sau ô cuối rỗng, và ô kế tiếp nữa có dân/quan → thu về kho.
5. Dừng lượt: Khi ô tiếp theo là ô quan rỗng, hoặc 2 ô dân liên tiếp cũng trống.
6. Hết dân: Nếu một bên hết dân, lấy dân từ kho để tiếp tục
7. Tính điểm: `Tổng điểm = Số dân + Số quan × 10`.

---

# Tính năng khác

- 45 giây mỗi lượt; hết giờ tự động chọn nước đi ngẫu nhiên.
- Tạm dừng giữa chừng, có thể chơi lại hoặc về trang trước đó.
- Bật/tắt âm thanh bất kỳ lúc nào.
- Chơi lại ngay sau khi kết thúc ván.
