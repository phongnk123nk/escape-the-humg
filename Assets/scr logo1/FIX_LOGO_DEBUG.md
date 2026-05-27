# 🔧 Sửa Chữa Puzzle - Logo Sai

## ✅ Những Gì Đã Sửa

### 1. **Cập nhật `currentIndexToTile` sau xáo trộn**
   - **Vấn đề:** Ban đầu dictionary chỉ ánh xạ `correctIndex` → `tile`, nhưng sau xáo trộn, nó phải ánh xạ `currentIndex` (vị trí hiện tại)
   - **Fix:** Thêm bước clear + rebuild dictionary sau xáo trộn
   ```csharp
   currentIndexToTile.Clear();
   foreach (ManhGhepPuzzle manh in danhSachManh)
   {
       currentIndexToTile[manh.currentIndex] = manh;
   }
   ```

### 2. **Thêm Debug Logs để Theo Dõi**
   - **Xáo trộn:** Log tất cả bước di chuyển + vị trí từng mảnh
   - **Hoàn thành:** Log kiểm tra tất cả mảnh có đúng vị trí không
   - **Logo:** Log thông tin kích thước + scale logo bình thường

### 3. **Kiểm tra Shuffle Thành Công**
   - Thêm check xem puzzle có bị xáo trộn chưa (có mảnh không ở vị trí gốc?)
   - Nếu tất cả mảnh vẫn ở vị trí gốc → In cảnh báo

### 4. **Tăng Sorting Order Logo**
   - Từ 25 → 30 (chắc chắn cao hơn mảnh puzzle 20)

### 5. **Thêm Validation trong OnPuzzleSolved**
   - Kiểm tra lại trước khi ẩn mảnh + hiển thị logo
   - Log chi tiết từng mảnh

## 🔍 Cách Debug

### Bước 1: Chạy Game
1. Play scene
2. Chờ 2 giây
3. Bấm logo nhỏ
4. **Kiểm tra Console**

### Bước 2: Xem Console Logs

#### Nếu thấy: "✓ Puzzle đã được xáo trộn thành công!"
- ✅ Xáo trộn hoạt động đúng
- Tìm nguyên nhân ở nơi khác

#### Nếu thấy: "⚠ Cảnh báo: Puzzle chưa bị xáo trộn!"
- ❌ Xáo trộn không hoạt động
- Xem "Troubleshooting" bên dưới

#### Xem chi tiết từng mảnh:
```
Mảnh 0: currentIndex=3, correctIndex=0, Match=False ✓
Mảnh 1: currentIndex=1, correctIndex=1, Match=True ✗
...
```

### Bước 3: Giải Puzzle
1. Bấm các mảnh để di chuyển
2. Sau khi đúng, kiểm tra Console:
   - Có log "Puzzle solved!"?
   - Log show tất cả match?

### Bước 4: Kiểm tra Logo
1. Sau khi xong, có logo bình thường xuất hiện?
2. Console show log về scale + kích thước?

## 🐛 Troubleshooting

### Problem 1: "Puzzle chưa bị xáo trộn"
```
Nguyên nhân: XaoTronPuzzle() không di chuyển mảnh
Kiểm tra:
- Xem log "Xáo trộn hoàn thành: X bước"
- Nếu X = 0 → emptyIndex sai
- Nếu X > 0 nhưng mảnh vẫn đúng → Logic di chuyển sai
```

**Fix:** Kiểm tra `emptyIndex` ban đầu có = 8 không
```csharp
private int emptyIndex = 8;  // Phải là 8 (bottom-right)
```

### Problem 2: "Mảnh không trượt"
```
Log shows: mảnh ở đúng vị trí ngay sau xáo trộn
→ Xáo trộn không hoạt động
→ Kiểm tra TimSlotKe() có return neighbors không
```

### Problem 3: "Logo bình thường không hiện"
```
Kiểm tra Console:
- Có log "Logo bình thường đã hiển thị!" không?
- Có lỗi gì không?
- logoBinhThuongSprite có được kéo vào không?
```

### Problem 4: "Logo bình thường ở vị trí/kích thước sai"
```
Xem log:
- "Total puzzle size: X" 
- "Sprite world dimensions: W x H"
- "Calculated scale: S"

Tính toán:
- Logo kích thước = gridSize × tileSize + gap
- Scale = logo kích thước / sprite kích thước
```

## 📝 Cách Xóa Debug Logs (Khi Production)

### Xóa trong `XaoTronPuzzle()`:
```csharp
// Comment out:
// Debug.Log($"Xáo trộn hoàn thành: {moveHistory.Count} bước...");
// foreach (ManhGhepPuzzle manh in danhSachManh) { ... }
```

### Xóa trong `TaoPuzzleTuDong()`:
```csharp
// Comment out:
// if (isPuzzleShuffled) { Debug.Log(...); }
```

### Xóa trong `OnPuzzleSolved()`:
```csharp
// Comment out tất cả Debug.Log() khác Log("Puzzle solved!")
```

### Xóa trong `HienLogoBinhThuong()`:
```csharp
// Comment out tất cả Debug.Log()
```

## ✨ Improvements Đã Áp Dụng

1. ✅ **More Robust Shuffling**
   - Đảm bảo dictionary được cập nhật sau shuffle
   - Kiểm tra puzzle thực sự bị shuffle

2. ✅ **Better Debugging**
   - Chi tiết mỗi bước xáo trộn
   - Verify từng mảnh trước/sau puzzle solve

3. ✅ **Higher Visibility**
   - Sorting order logo 30 (từ 25)
   - Chắc chắn không bị các mảnh che phủ

4. ✅ **Better Error Handling**
   - Kiểm tra tất cả mảnh đúng vị trí trước ẩn
   - Log chi tiết nếu có vấn đề

## 🎮 Test Checklist

- [ ] Play → Wait 2 sec → Logo nhỏ hiện
- [ ] Bấm logo nhỏ → Logo lớn hiện 1 sec
- [ ] Console: "✓ Puzzle đã được xáo trộn thành công!"
- [ ] 8 mảnh hiện ở vị trí xáo trộn (không ở vị trí gốc)
- [ ] Bấm mảnh kề ô trống → Trượt
- [ ] Xếp đúng → Console: "All tiles correct? True"
- [ ] 8 mảnh ẩn đi
- [ ] Logo bình thường hiện ra (full size)
- [ ] Phím K → Auto solve + logo hiện

## 📊 Expected Console Output

```
✓ Puzzle đã được xáo trộn thành công!
Xáo trộn hoàn thành: 50 bước. Empty Index: 3
Mảnh 0: currentIndex=5, position=(x, y, z)
Mảnh 1: currentIndex=2, position=(x, y, z)
...
Mảnh 7: currentIndex=1, position=(x, y, z)

[When solved]
Puzzle solved! Checking all tiles:
Mảnh 0: currentIndex=0, correctIndex=0, Match=True ✓
Mảnh 1: currentIndex=1, correctIndex=1, Match=True ✓
...
All tiles correct? True
EmptyIndex: 8 (nên là 8) ✓

Logo bình thường created at: (0, 0, -1), scale: (2.5, 2.5, 1)
Logo bình thường đã hiển thị!
```

---

**Status: ✅ Sửa xong - Hãy test và check console để xác định vấn đề**
