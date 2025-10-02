# 🚀 ASTEROID BLASTER (2D Unity)
*Game bắn thiên thạch với 2 chế độ điều khiển (Chuột / Bàn phím), có **Máu** (HP), **Mana** (MP), tính điểm theo **thời gian sống** và **số thiên thạch bị bắn vỡ**. Độ khó tăng dần theo **tốc độ rơi** và **góc rơi** của thiên thạch. Trong lúc chơi rơi nhiều **item** hỗ trợ: máu, điểm, mana, đạn, khiên. Hỗ trợ **Cheat Mode** cho mục đích test.*

---

## ✨ TÍNH NĂNG CHÍNH
- **2 chế độ điều khiển**: *Chuột* và *Bàn phím*, người chơi chọn ngay trong game (Settings) hoặc ở màn hình Intro.
- **HP & Mana**: người chơi sống dựa trên thanh máu; mana dùng cho các hiệu ứng/kỹ năng (tùy bạn mở rộng).
- **Chấm điểm**:
  - + Điểm theo **thời gian sống**.
  - + Điểm khi **bắn vỡ thiên thạch**.
- **Độ khó động**: thiên thạch rơi **mau dần** và **góc rơi biến đổi liên tục** theo thời gian.
- **Item rơi trong trận**: Hồi máu, tăng điểm, hồi mana, thêm đạn, cấp **khiên tạm thời**.
- **Cheat Mode (dev/test)**:
  - **Shift + Y** → **Tăng tốc độ bắn** (fire rate).
  - **Shift + C** → **Tăng máu** (HP).

---

## 🕹️ ĐIỀU KHIỂN

### Chế độ **Chuột**
- **Di chuyển**: rê chuột (tàu bám theo/tịnh tiến theo trục phù hợp với game của bạn).
- **Bắn**: **Chuột Trái** *(tự động hoặc theo nhấn, tùy config)*.
- **Tạm dừng**: `Esc`.

### Chế độ **Bàn phím**
- **Di chuyển**: `W/A/S/D` hoặc phím mũi tên.
- **Bắn**: `Space` *(hoặc phím khác bạn cấu hình)*.
- **Tạm dừng**: `Esc`.

> Trong **Settings → Input**, người chơi có thể chọn **Chuột** hoặc **Bàn phím** làm chế độ điều khiển chính.  
> **Cheat Mode** luôn sẵn: **Shift+Y**, **Shift+C**.

---

## ❤️ HP & 🔷 Mana
- **HP (Máu)**: giảm khi va chạm thiên thạch hoặc trúng mảnh vỡ; về 0 → *Game Over*.  
- **Mana (MP)**: dùng cho cơ chế/kỹ năng tiêu hao (ví dụ: bắn cường hóa, kích khiên chủ động nếu bạn triển khai).  
- **Hồi phục**: thông qua **item** rơi trong trận.

---

## 🧮 CÁCH TÍNH ĐIỂM
- **Điểm thời gian**: mỗi giây sống được cộng `SCORE_PER_SECOND`.  
- **Điểm bắn vỡ**: mỗi thiên thạch bị phá hủy cộng `SCORE_PER_ASTEROID` *(có thể nhân hệ số theo kích thước/độ hiếm)*.
