# ⚙️ SETUP — Nối Claude Code với Unity qua MCP

Mục tiêu: để Claude điều khiển được Unity Editor (tạo scene, sửa script, kéo thả object) bằng ngôn ngữ tự nhiên.

> Máy bạn đã có sẵn: Unity 6 (6000.1.10f1), `uv` 0.10, Node 22. Đủ điều kiện.

---

## Bước 1 — Tạo Unity project
1. Mở **Unity Hub → New project**.
2. Template: **Universal 3D (URP)**.
3. Tên: `CozyStroll`, Location: `d:\Desktop\game3d`.
4. Create → đợi Unity mở xong.

## Bước 2 — Cài MCP package vào Unity
1. Trong Unity: **Window → Package Manager**.
2. Bấm nút **`+`** (góc trên trái) → **Add package from git URL…**
3. Dán URL này rồi Add:
   ```
   https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
   ```
4. Đợi Unity import xong (có thể mất 1–2 phút).

## Bước 3 — Tự động cấu hình Claude Code
1. Trong Unity: **Window → MCP for Unity**.
2. Bấm **`Configure All Detected Clients`** (hoặc chọn riêng *Claude Code*).
   - Nút này tự ghi cấu hình MCP server vào Claude Code — **không cần gõ lệnh thủ công**.
3. Nếu nó không tìm thấy Claude, bấm **`Choose Claude Install Location`** và trỏ tới file `claude`.

## Bước 4 — Kiểm tra kết nối
1. Cửa sổ **MCP for Unity** phải báo trạng thái **Connected** (chấm xanh).
2. Trong Claude Code, chạy:
   ```
   /mcp
   ```
   → phải thấy server `unity` / `mcp-for-unity` ở trạng thái connected.
3. Test: bảo Claude *"liệt kê các GameObject trong scene hiện tại"*.

---

## Lưu ý quan trọng
- **Unity Editor phải đang mở** thì Claude mới điều khiển được (kết nối qua localhost).
- Đổi transport (http ↔ stdio) thì phải **restart Claude Code**.
- Beta channel: thay `#main` bằng `#beta` ở URL git.

## Khắc phục sự cố
| Lỗi | Cách xử lý |
|---|---|
| `/mcp` không thấy server | Bấm lại *Configure All Detected Clients*, restart Claude Code |
| Báo Python version | MCP dùng `uv` tự quản Python 3.10+, không cần Python hệ thống |
| Not connected | Đảm bảo Unity đang mở & cửa sổ MCP for Unity báo xanh |
