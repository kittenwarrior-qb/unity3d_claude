# 🎨 UI STYLE GUIDE — Cozy Stroll

Phong cách: **cozy, sáng sủa, mint & pastel mát**. Mọi UI bám theo guide này.

## Bảng màu (Mint & pastel mát)
| Vai trò | Màu | Hex |
|---|---|---|
| Nền chính | Trắng ngà | `#FBFCF8` |
| Panel / khung | Mint nhạt | `#A8E6CF` |
| Điểm nhấn / nút | Mint đậm | `#7FD8B6` |
| Phụ / hover | Hồng phấn | `#FFD3E0` |
| Chữ chính | Xám ấm | `#4A5759` |
| Chữ phụ | Xám nhạt | `#8FA3A0` |

## Typography
- Font: **Quicksand** (heading) + **Nunito** (body) — tròn, mềm.
- Heading bo đậm (SemiBold/Bold), body Regular.
- Tránh chữ nhọn, cứng.

## Hình khối & hiệu ứng
- **Bo góc tròn lớn** (radius 16–24px), không viền sắc.
- **Soft shadow** mờ, lan nhẹ (không đổ bóng đen gắt).
- Hover: phình nhẹ ~1.05x + bounce mượt (ease-out-back).
- Click: âm thanh "pop" dễ thương.
- Không gradient gắt; nếu có, chỉ pastel rất nhẹ.

## HUD trong game (nhẹ + đồng hồ/minimap)
```
┌────────────────────────┐
│ 🕐 17:30      ◔ minimap │  ← góc trên: giờ in-game + minimap tròn
│                        │
│           🧍           │  ← nhân vật giữa màn (third-person)
│                        │
│                        │  ← phần lớn màn để trống, ngắm cảnh
└────────────────────────┘
```
- **Đồng hồ** góc trên trái: giờ trong game, nền mint bo tròn.
- **Minimap tròn** góc trên phải: viền mint, chấm vị trí người chơi.
- Gợi ý điều khiển hiện lúc đầu rồi **mờ dần** sau vài giây.
- Không thanh máu, không số liệu — giữ thư giãn.

## Màn hình chính (Main Menu)
- Tiêu đề lớn "Cozy Stroll" font Quicksand.
- Nút: **Bắt đầu / Cài đặt / Thoát**, bo tròn, nền mint.
- Background: cảnh game mờ + ánh sáng mint/sáng.

## Unity implementation note
- Dùng **UI Toolkit** hoặc **uGUI (Canvas)** — sẽ chốt khi dựng.
- Sprite bo góc dùng 9-slice để co giãn mượt.
- Đặt asset UI trong `Assets/UI/`.
