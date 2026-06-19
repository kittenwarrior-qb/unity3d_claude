# 🗺️ BUILD PLAN — Cozy Stroll (góc nhìn senior solo dev)

Triết lý: **làm cái chạy được trước, đẹp sau, tính năng sau cùng**. Mỗi milestone là một build chơi được, không vỡ.

## Nguyên tắc làm việc
- **Vertical slice trước**: 1 nhân vật chạy quanh 1 map nhỏ + camera + UI tối thiểu → cảm nhận vibe sớm.
- **Code tách module**: `Player / Camera / UI / Core / Environment`, không nhồi 1 script khổng lồ.
- **Commit nhỏ, thường xuyên**. Mỗi feature = 1 commit.
- **Asset đến sau code**: dùng primitive (capsule/cube) làm placeholder, thay art sau.
- **Giữ scope**: thấy tính năng "hay ho" ngoài MVP → ghi vào Backlog, không làm ngay.

---

## Milestone 0 — Setup (bạn làm thủ công)
- [x] Repo + docs + UI guide.
- [ ] Tạo Unity project URP `CozyStroll` trong repo này.
- [ ] Cài Unity MCP, nối Claude Code (xem [SETUP.md](SETUP.md)).
- [ ] **Project Settings → Player → Active Input Handling = `Both`** (để script dùng được Input cũ).

## Milestone 1 — Vertical slice "chạy vòng vòng" ⭐ MVP lõi
> Mục tiêu: capsule chạy quanh mặt phẳng, camera theo sau. Chơi được.
- [ ] Mặt đất (Plane) + vài Cube làm nhà placeholder.
- [ ] `PlayerController.cs` — đi/chạy/nhảy bằng CharacterController, di chuyển theo hướng camera.
- [ ] `ThirdPersonCamera.cs` — follow mượt + xoay bằng chuột.
- [ ] Ánh sáng URP ấm/mát + skybox sáng.
- **Done khi:** WASD chạy mượt, Shift chạy nhanh, Space nhảy, camera bám mượt.

## Milestone 2 — Lớp cozy & UI
- [ ] `GameClock.cs` — đồng hồ in-game chạy, hiển thị HH:MM.
- [ ] HUD: đồng hồ góc trên trái (mint, bo tròn) theo [UI_STYLE.md](UI_STYLE.md).
- [ ] `Minimap` — camera nhìn từ trên, RenderTexture tròn góc phải.
- [ ] Gợi ý điều khiển hiện lúc đầu rồi mờ dần.
- [ ] Main Menu cozy (Bắt đầu / Cài đặt / Thoát).

## Milestone 3 — Sống động (juice)
- [ ] `FootstepAudio.cs` — tiếng bước chân theo tốc độ.
- [ ] Nhạc nền lo-fi loop.
- [ ] Particle nhẹ (lá rơi / đom đóm).
- [ ] Thay capsule bằng nhân vật Mixamo + animation (idle/walk/run/jump).
- [ ] Thay cube bằng asset Kenney low-poly.

## Milestone 4 — Tương tác cozy
- [ ] `Interactable.cs` — nhấn E để nhặt hoa / ngồi ghế / vỗ mèo.
- [ ] Ngày-đêm nhẹ (xoay directional light theo GameClock).
- [ ] NPC đi dạo đơn giản (waypoint).

## Backlog (chưa làm, để dành)
- Ngày/đêm đầy đủ, thời tiết.
- Chụp ảnh / photo mode.
- Cá nhân hoá nhân vật.

---

## Cấu trúc thư mục code
```
Assets/
  Scripts/
    Core/        GameManager.cs
    Player/      PlayerController.cs
    Camera/      ThirdPersonCamera.cs
    UI/          GameClock.cs, ClockHUD.cs
    Audio/       FootstepAudio.cs
  UI/            (sprite, font cozy)
  Art/           (model, material)
  Audio/         (nhạc, sfx)
```

> Các script ở Milestone 1–2 đã được viết sẵn trong `Assets/Scripts/` — chỉ việc gắn vào GameObject.
