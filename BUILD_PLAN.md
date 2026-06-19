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

> ⚡ **Cập nhật:** toàn bộ scene giờ **tự dựng bằng code** khi bấm Play
> (`Core/SceneBootstrapper.cs`). Không cần kéo-thả tay. Xem [PLAY.md](PLAY.md).

## Milestone 1 — Vertical slice "chạy vòng vòng" ⭐ MVP lõi
> Mục tiêu: capsule chạy quanh mặt phẳng, camera theo sau. Chơi được.
- [x] Mặt đất (Plane) + nhà low-poly placeholder (`TownGenerator`).
- [x] `PlayerController.cs` — đi/chạy/nhảy bằng CharacterController, di chuyển theo hướng camera.
- [x] `ThirdPersonCamera.cs` — follow mượt + xoay bằng chuột.
- [x] Ánh sáng URP ấm/mát + bầu trời pastel + fog mềm.
- **Done khi:** WASD chạy mượt, Shift chạy nhanh, Space nhảy, camera bám mượt. ✅

## Milestone 2 — Lớp cozy & UI
- [x] `GameClock.cs` — đồng hồ in-game chạy, hiển thị HH:MM.
- [x] HUD: đồng hồ góc trên trái (mint, bo tròn) theo [UI_STYLE.md](UI_STYLE.md) (`HudBuilder`).
- [x] `Minimap` — camera nhìn từ trên, RenderTexture tròn góc phải.
- [x] Gợi ý điều khiển hiện lúc đầu rồi mờ dần (`ControlHints`).
- [x] Main Menu cozy (Bắt đầu / Cài đặt / Thoát) + settings + hover bounce + pop sound (`MainMenuBuilder`).

## Milestone 3 — Sống động (juice)
- [x] `FootstepAudio.cs` — tiếng bước chân theo tốc độ.
- [x] Nhạc nền lo-fi loop (`AmbientMusic` + `ProceduralAudio` tổng hợp runtime).
- [x] Particle nhẹ — lá rơi (ban ngày) + đom đóm (ban đêm theo GameClock) (`AmbientParticles`).
- [ ] Thay capsule bằng nhân vật Mixamo + animation (idle/walk/run/jump). *(cần asset)*
- [ ] Thay cube bằng asset Kenney low-poly. *(cần asset)*

## Milestone 4 — Tương tác cozy
- [x] `Interactable.cs` + `PlayerInteractor.cs` — nhấn E để nhặt hoa / ngồi ghế / vỗ mèo.
- [x] Ngày-đêm nhẹ (`DayNightLight` xoay sun + đổi màu theo GameClock).
- [x] NPC đi dạo đơn giản (`Wanderer`, waypoint ngẫu nhiên).

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
