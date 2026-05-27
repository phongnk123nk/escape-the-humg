# ✅ Setup Checklist - Puzzle 3x3

## 📋 Pre-Setup Checklist

### Sprites & Assets
- [ ] Chuẩn bị sprite "Logo Ma Mi" (ảnh cắt puzzle)
  - [ ] Format: PNG/JPG
  - [ ] Kích thước: 300x300 px hoặc bội số 3 (chia hết cho 3)
  - [ ] Có alpha channel (nếu cần)
  - [ ] **IMPORTANT:** Trong Inspector → Texture Type: Sprite → Apply
  - [ ] **IMPORTANT:** Read/Write Enabled: ON → Apply

- [ ] Chuẩn bị sprite "Logo Bình Thường" (ảnh hiển thị khi xong)
  - [ ] Format: PNG/JPG
  - [ ] Kích thước: ~300x300 px
  - [ ] Trong Inspector → Texture Type: Sprite → Apply
  - [ ] Read/Write Enabled: ON → Apply

## 🏗️ Scene Setup

### Bước 1: Tạo Hierarchy
- [ ] Tạo GameObject tên "PuzzleManager"
  - [ ] Position: (0, 0, 0)
  - [ ] Scale: (1, 1, 1)

- [ ] Tạo GameObject tên "LogoMo"
  - [ ] Thêm SpriteRenderer (hiển thị logo nhỏ)
  - [ ] Position: (-5, 3, 0) hoặc vị trí muốn
  - [ ] Scale: phù hợp

- [ ] Tạo GameObject tên "LogoLon"
  - [ ] Thêm SpriteRenderer (hiển thị logo lớn ở giữa)
  - [ ] Position: (0, 0, 0)
  - [ ] Scale: phù hợp

- [ ] Tạo GameObject tên "PuzzleContainer"
  - [ ] Position: (0, 0, 0)
  - [ ] Đây sẽ là parent của 8 mảnh puzzle
  - [ ] **Để trống, mảnh sẽ được tạo tự động**

### Bước 2: Gắn Scripts

#### PuzzleManager
1. Chọn "PuzzleManager"
2. Inspector → Add Component → QuanLyXepHinh
3. Kéo thả các field:

```
✓ Logo Ma Mi Sprite: [Sprite "Logo Ma Mi"]
✓ Logo Binh Thuong Sprite: [Sprite "Logo Bình Thường"]
✓ Logo Mo: [GameObject "LogoMo"]
✓ Logo Lon: [GameObject "LogoLon"]
✓ Khung Puzzle: [Transform của "PuzzleContainer"]
```

4. Kiểm tra các cài đặt mặc định:
```
✓ Grid Size: 3
✓ Tile Size: 1.4
✓ Gap Pixels: 5
✓ Thoi Gian Logo Mo Xuat Hien: 2
✓ Thoi Gian Logo Lon Hien: 1
✓ Bat Vien Phat Sang: true (hoặc false nếu không cần)
✓ Bat Dev Skip: true (hoặc false để disable phím K)
✓ Phim Skip: K
```

#### LogoMo
1. Chọn "LogoMo"
2. Inspector → Add Component → BamLogoBang
3. Kéo thả:

```
✓ Quan Ly Xep Hinh: [PuzzleManager - QuanLyXepHinh component]
```

4. **Thêm BoxCollider2D** để nhận click:
   - Add Component → Physics 2D → Box Collider 2D
   - Size: fit sprite của LogoMo
   - Is Trigger: true

## 🎨 Sprite Configuration

### Logo Ma Mi Sprite
1. Chọn sprite "Logo Ma Mi"
2. Inspector:
   - [ ] Texture Type: **Sprite (2D and UI)**
   - [ ] Sprite Mode: **Single**
   - [ ] Format: **Compressed** hoặc **Truecolor**
   - [ ] **Read/Write Enabled: ON** ← QUAN TRỌNG!
   - [ ] Filter Mode: Point hoặc Bilinear (tuỳ chọn)
   - [ ] Click Apply

### Logo Bình Thường Sprite
1. Chọn sprite "Logo Bình Thường"
2. Inspector:
   - [ ] Texture Type: **Sprite (2D and UI)**
   - [ ] Sprite Mode: **Single**
   - [ ] **Read/Write Enabled: ON** ← QUAN TRỌNG!
   - [ ] Click Apply

## 🎮 Testing Checklist

### Khởi Động
- [ ] Play Scene
- [ ] Chờ 2 giây
- [ ] **LogoMo xuất hiện** ✓

### Bấm Logo
- [ ] Bấm vào LogoMo
- [ ] **LogoLon hiện ở giữa** ✓
- [ ] Chờ 1 giây
- [ ] **LogoLon biến mất** ✓
- [ ] **PuzzleContainer kích hoạt** ✓
- [ ] **8 mảnh puzzle xuất hiện** ✓
- [ ] **Mảnh ở vị trí xáo trộn** ✓
- [ ] **1 ô trống ở góc dưới bên phải** ✓

### Chơi Puzzle
- [ ] Bấm mảnh kề ô trống
- [ ] **Mảnh trượt vào ô trống** ✓
- [ ] Bấm mảnh **không kề** ô trống
- [ ] **Mảnh không di chuyển** ✓
- [ ] Kiểm tra mỗi bước có kiểm tra hoàn thành không
- [ ] Console không có lỗi

### Hoàn Thành Puzzle
- [ ] Xếp đủ 8 mảnh về vị trí gốc
- [ ] **8 mảnh ẩn đi** ✓
- [ ] **Logo bình thường xuất hiện** ✓
- [ ] **Logo bình thường fit khung puzzle 3x3** ✓
- [ ] **Logo ở tâm puzzle** ✓

### Dev Skip (Phím K)
- [ ] Play → Bấm LogoMo → Hiện puzzle
- [ ] Bấm phím K
- [ ] **Tất cả mảnh về vị trí gốc** ✓
- [ ] **Logo bình thường xuất hiện** ✓
- [ ] Console: "DEV SOLVE: ..." ✓

## 🔧 Troubleshooting Checklist

### Puzzle Không Hiện
```
Kiểm tra:
□ Logo Ma Mi Sprite có được kéo vào không?
□ PuzzleContainer có được kéo vào không?
□ Sprite có Read/Write Enabled không?
□ Console có error gì không?
→ Kiểm tra Debug.LogError("Chưa kéo Logo...")
```

### Mảnh Không Trượt
```
Kiểm tra:
□ Mỗi mảnh có BoxCollider2D không?
□ Collider có isTrigger = true không?
□ BamLogoBang có được gắn vào LogoMo không?
□ QuanLyXepHinh có được kéo vào BamLogoBang không?
□ Bấm LogoMo có console log gì không?
```

### Logo Không Hiện Khi Xong
```
Kiểm tra:
□ Logo Binh Thuong Sprite có được kéo vào không?
□ Sprite có Read/Write Enabled không?
□ Console có warning không?
□ puzzleDangHoatDong = false đúng không?
```

### Phím K Không Hoạt Động
```
Kiểm tra:
□ Bat Dev Skip = true không?
□ Phim Skip = K không?
□ Puzzle đã hiện (8 mảnh có không)?
□ Focus vào game window rồi không?
□ Console có warning không?
```

### Mảnh Để Lại Viền Phát Sáng Khi Di Chuyển
```
Bình thường, viền là GameObject con
Kiểm tra:
□ VienPhatSang ở đúng vị trí không?
□ Sorting Order: 19 (nhỏ hơn mảnh 20)?
```

## 🚀 Performance Checklist

- [ ] Tile Size không quá nhỏ (<0.5) → Performance OK
- [ ] Gap Pixels không quá lớn (>50) → Nhìn rõ ràng
- [ ] Batch Count OK (8 tiles = nhỏ)
- [ ] Memory Usage OK (mỗi tile ~10KB)
- [ ] FPS >= 60 (hoặc >= 30 mobile)

## 📋 Final Verification

### Code Files
- [ ] QuanLyXepHinh.cs (trong Assets/scr logo1/)
- [ ] ManhGhepPuzzle.cs (trong Assets/scr logo1/)
- [ ] BamLogoBang.cs (trong Assets/scr logo1/)

### Documentation
- [ ] HUONG_DAN_SU_DUNG_PUZZLE.md (đã đọc)
- [ ] README_PUZZLE_3x3.md (đã đọc)
- [ ] CODE_STRUCTURE.md (đã đọc)

### Before Delivery
- [ ] Không có compile errors
- [ ] Không có runtime errors
- [ ] Tất cả features hoạt động
- [ ] Console clean (không có unexpected logs)
- [ ] Scene saved

## 🎯 Expected Results

| Tính Năng | Expected | Status |
|-----------|----------|--------|
| Flow 2s → LogoMo | ✅ Xuất hiện | □ |
| Bấm LogoMo | ✅ LogoLon hiện 1s | □ |
| Sau LogoLon | ✅ Puzzle hiện 8 ô | □ |
| Bấm ô kề | ✅ Trượt vào empty | □ |
| Bấm ô không kề | ✅ Không di chuyển | □ |
| Puzzle xong | ✅ Logo bình thường | □ |
| Phím K | ✅ Auto solve + logo | □ |

---

## 💬 Support

Nếu gặp vấn đề:
1. Kiểm tra Console → Xem error message
2. Kiểm tra Inspector → Xem tất cả field đã kéo
3. Kiểm tra Sprite → Read/Write Enabled ON
4. Kiểm tra Hierarchy → Tất cả GameObject có không
5. Kiểm tra Scripts → Tất cả gắn đúng không

**Debug Print:**
- Add: `Debug.Log("Checkpoint 1");` tại các điểm chính
- Chạy game → Check Console
- Sẽ giúp xác định chỗ bị lỗi

---

**Status: ✅ Sẵn sàng sử dụng**
