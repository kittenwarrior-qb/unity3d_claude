# 🧩 Lắp ráp scene Milestone 1–2 (làm tay, ~10 phút)

> Cần bật trước: **Edit → Project Settings → Player → Active Input Handling = `Both`**.
> Cài **TextMeshPro** khi Unity hỏi (Window → TextMeshPro → Import TMP Essentials) — dùng cho đồng hồ.

## 1. Mặt đất & nhà placeholder
- `GameObject → 3D Object → Plane`, scale (5,1,5). Đây là mặt đất.
- Thêm vài `Cube` rải rác làm nhà (sửa scale tuỳ ý). Đặt chung dưới object rỗng `Town`.

## 2. Nhân vật (Player)
1. `GameObject → 3D Object → Capsule`, đổi tên **Player**, đặt y ≈ 1.
2. Xoá component **Capsule Collider** (CharacterController sẽ lo va chạm).
3. **Add Component → Character Controller**.
4. **Add Component → PlayerController** (script trong `Player/`).
   - Để trống `cameraTransform` → tự lấy Camera.main.

## 3. Camera
1. Chọn **Main Camera**.
2. **Add Component → ThirdPersonCamera** (trong `Camera/`).
3. Kéo **Player** vào ô `target`.
4. Bấm Play → WASD chạy, Shift chạy nhanh, Space nhảy, chuột xoay camera. ✅

## 4. Ánh sáng cozy (mint & sáng)
- Chọn **Directional Light**: màu hơi ấm `#FFF3DD`, Intensity ~1.1, xoay ~50° để tạo bóng dài cozy.
- **Window → Rendering → Lighting → Environment**: Ambient màu mint nhạt `#DFF5EC` để tổng thể sáng, mát.

## 5. GameManager
- Object rỗng **_Game** → **Add Component → GameManager**. (đặt target FPS 60, Esc thả chuột)

## 6. Đồng hồ in-game (HUD)
1. Object rỗng **_Game** (hoặc riêng) → **Add Component → GameClock**.
   - `startHour = 17`, `minutesPerSecond = 1` (chỉnh nhanh/chậm tuỳ ý).
2. `GameObject → UI → Canvas` (tự tạo EventSystem).
3. Trong Canvas: tạo **Panel** nhỏ góc trên-trái:
   - Anchor top-left, màu mint `#A8E6CF`, bo góc (sprite 9-slice hoặc để vuông tạm).
   - Bên trong thêm **UI → Text - TextMeshPro**, font tròn (Nunito/Quicksand), màu chữ `#4A5759`.
4. Gắn **ClockHUD** vào Panel: kéo `GameClock` vào `clock`, kéo Text TMP vào `label`.
5. Play → thấy giờ chạy. ✅

## 7. (Tuỳ chọn) Tiếng bước chân
- Trên **Player**: Add **Audio Source** + **FootstepAudio**.
- Kéo vài clip bước chân (từ Kenney/Freesound) vào `footstepClips`.

---

### Mẹo cozy
- URP: bật **Bloom** nhẹ trong Volume + **Color Adjustments** (Saturation +10, Temperature ấm/mát nhẹ) cho cảm giác mơ màng.
- Skybox sáng (pastel) thay cho skybox mặc định.

Khi nối được **Unity MCP**, mình sẽ tự làm hết các bước kéo-thả trên giúp bạn.
