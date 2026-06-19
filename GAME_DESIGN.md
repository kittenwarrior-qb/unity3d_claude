# 🎮 GAME DESIGN — Cozy Stroll

## Ý tưởng cốt lõi
Người chơi điều khiển một nhân vật dễ thương **chạy/đi vòng vòng** quanh một thị trấn nhỏ sáng sủa, cozy. Mục tiêu là **thư giãn** — ngắm cảnh, nghe nhạc, tương tác lặt vặt. Không có thắng/thua.

## Pillar (3 trụ cột giữ vibe)
1. **Sáng & ấm** — bảng màu pastel, ánh sáng golden hour, không bóng tối nặng.
2. **Tự do, không áp lực** — không timer, không HP, không enemy.
3. **Phản hồi dễ chịu** — animation mượt, âm thanh bước chân, particle nhẹ.

## Gameplay loop (MVP)
1. Spawn ở quảng trường trung tâm.
2. Đi/chạy bằng WASD, camera theo sau (third-person).
3. Khám phá: công viên, hồ nước, vài căn nhà, cây cối.
4. Tương tác nhỏ: nhặt hoa, vỗ vào con mèo, ngồi ghế băng (giai đoạn sau).

## Điều khiển
| Phím | Hành động |
|---|---|
| WASD / Left stick | Di chuyển |
| Shift | Chạy nhanh |
| Space | Nhảy |
| Chuột / Right stick | Xoay camera |
| E | Tương tác (giai đoạn sau) |

## Phạm vi (Scope)
### MVP (làm trước)
- [ ] Map nhỏ: mặt đất + đường + vài khối nhà low-poly.
- [ ] Nhân vật third-person di chuyển + chạy + nhảy.
- [ ] Camera follow mượt.
- [ ] Ánh sáng URP ấm + skybox sáng.
- [ ] Nhạc nền lo-fi + âm thanh bước chân.

### Giai đoạn 2 (sau)
- [ ] Vật thể tương tác (E để nhặt/ngồi).
- [ ] Ngày/đêm nhẹ.
- [ ] NPC đi dạo.
- [ ] Particle (lá rơi, đom đóm).

## Không làm (để giữ scope nhỏ)
- ❌ Combat, kẻ địch, máu.
- ❌ Inventory phức tạp, quest.
- ❌ Multiplayer.
