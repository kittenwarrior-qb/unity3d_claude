# ▶️ Cách chạy Cozy Stroll

Toàn bộ game được **dựng tự động bằng code** khi bấm Play — không cần kéo-thả gì trong Editor.

## Chạy
1. Mở project `My project (1)` bằng **Unity 6 (6000.1.x)**.
2. Khi Unity hỏi, **Import TMP Essentials** (không bắt buộc — HUD dùng font built-in nên vẫn chạy không cần TMP).
3. Mở scene `Assets/Scenes/SampleScene.unity` (hoặc bất kỳ scene nào).
4. Bấm **Play**. Cả thị trấn tự dựng và hiện **Main Menu** trên nền cảnh game.
5. Bấm **Bắt đầu** để cầm điều khiển nhân vật. (Cài đặt: chỉnh âm lượng nhạc & tốc độ thời gian.)

Thế giới gồm: mặt đất, nhà pastel, cây, hồ, ánh sáng ấm ngày/đêm, nhạc nền,
HUD (đồng hồ + minimap + gợi ý), NPC đi dạo, hoa & mèo tương tác, lá rơi & đom đóm.

## Điều khiển
| Phím | Hành động |
|---|---|
| WASD | Di chuyển (theo hướng camera) |
| Shift | Chạy nhanh |
| Space | Nhảy |
| Chuột | Xoay camera |
| E | Tương tác (nhặt hoa / vỗ mèo / ngồi ghế) |
| Esc | Thả chuột (để dừng Play) |

## Cơ chế auto-build
`Core/SceneBootstrapper.cs` có `[RuntimeInitializeOnLoadMethod]` → tự sinh một GameObject `[CozyStroll]` ngay khi vào Play và dựng toàn bộ scene. Muốn chỉnh tham số (seed, số NPC, số hoa, giờ bắt đầu...), kéo `SceneBootstrapper` vào một Empty GameObject trong scene và sửa trong Inspector — bản tự động sẽ nhường cho bản đặt tay.

## Kiến trúc script
```
Scripts/
  Core/
    GameManager.cs        # target FPS, Esc thả chuột, singleton
    SceneBootstrapper.cs   # ⭐ dựng cả thế giới khi Play
  Player/
    PlayerController.cs    # đi/chạy/nhảy theo camera (CharacterController)
  Camera/
    ThirdPersonCamera.cs   # follow mượt + xoay chuột
  Environment/
    PaletteLibrary.cs      # vật liệu pastel URP (cache theo màu)
    TownGenerator.cs        # mặt đất, nhà, cây, hồ, ghế, hàng rào
    DayNightLight.cs        # mặt trời + ngày/đêm ấm theo GameClock
    AmbientParticles.cs     # lá rơi (ngày) + đom đóm (đêm)
  UI/
    GameClock.cs            # đồng hồ in-game HH:MM
    ClockHUD.cs / SimpleClockLabel.cs  # bind giờ vào label (TMP / uGUI)
    HudBuilder.cs           # dựng Canvas: đồng hồ + minimap + gợi ý
    MainMenuBuilder.cs      # dựng Main Menu + Cài đặt (code-built)
    MainMenuController.cs    # logic Bắt đầu / Cài đặt / Thoát
    UiButtonBounce.cs        # hover/press bounce cho nút
    UISfx.cs                 # tiếng "pop" khi bấm nút
    RoundedSprite.cs        # sinh sprite bo góc + tròn + dot mềm runtime
    Minimap.cs              # camera nhìn từ trên → RenderTexture
    ControlHints.cs         # gợi ý điều khiển mờ dần
  Audio/
    FootstepAudio.cs        # tiếng bước chân theo tốc độ
    ProceduralAudio.cs      # tổng hợp bước chân + pad lo-fi (không cần file)
    AmbientMusic.cs         # loop nhạc nền
  Interaction/
    Interactable.cs         # vật tương tác (E)
    PlayerInteractor.cs     # tìm vật gần nhất + hiện prompt + bấm E
  NPC/
    Wanderer.cs             # dân làng đi dạo waypoint
```

## Thay placeholder bằng art thật (sau)
- `PaletteLibrary`/`TownGenerator` dùng primitive — thay bằng prefab Kenney low-poly.
- Player capsule → model Mixamo + Animator.
- `ProceduralAudio` → clip lo-fi/SFX thật từ Freesound/Kenney.
Xem [ASSETS.md](ASSETS.md).
