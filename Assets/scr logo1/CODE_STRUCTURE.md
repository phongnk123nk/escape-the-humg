# 📐 Cấu Trúc Code Puzzle 3x3

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────┐
│         Scene - Unity Hierarchy             │
├─────────────────────────────────────────────┤
│                                             │
│  ┌─ PuzzleManager (QuanLyXepHinh)          │
│  │  ├─ Input: logoMaMiSprite (Sprite)      │
│  │  ├─ Input: logoBinhThuongSprite (Sprite)│
│  │  ├─ Quản lý: Flow scene                 │
│  │  ├─ Quản lý: Tạo puzzle                 │
│  │  ├─ Quản lý: Di chuyển                  │
│  │  └─ Output: Hiển thị logo               │
│  │                                         │
│  ├─ LogoMo (BamLogoBang)                   │
│  │  └─ OnMouseDown → BamVaoLogoMo()        │
│  │                                         │
│  ├─ LogoLon                                │
│  │                                         │
│  └─ PuzzleContainer                        │
│     └─ (8 Tiles được tạo tự động)          │
│        ├─ ManhGhepPuzzle_0                 │
│        ├─ ManhGhepPuzzle_1                 │
│        ├─ ...                              │
│        └─ ManhGhepPuzzle_7                 │
│                                             │
└─────────────────────────────────────────────┘
```

## 🔗 Data Flow

### 1. **Khởi Động Scene**
```
Start() in QuanLyXepHinh
  ↓ (chờ thoiGianLogoMoXuatHien = 2 giây)
  ↓
HienLogoMoSauVaiGiay()
  ↓
LogoMo.SetActive(true)
```

### 2. **Bấm Logo**
```
LogoMo.OnMouseDown() [BamLogoBang]
  ↓
quanLyXepHinh.BamVaoLogoMo()
  ↓
ChayLogoLonRoiTaoPuzzle()
  ├─ LogoMo.SetActive(false)
  ├─ LogoLon.SetActive(true)
  ├─ Chờ thoiGianLogoLonHien = 1 giây
  ├─ LogoLon.SetActive(false)
  └─ TaoPuzzleTuDong()
```

### 3. **Tạo Puzzle**
```
TaoPuzzleTuDong()
  ├─ Cắt sprite thành 9 phần bằng Sprite.Create()
  │  └─ Mỗi phần = 1/9 của sprite
  │
  ├─ Tạo 8 GameObject (0-7), bỏ slot 8
  │  └─ Mỗi GameObject:
  │     ├─ SpriteRenderer (sprite)
  │     ├─ BoxCollider2D (isTrigger=true)
  │     ├─ ManhGhepPuzzle (script)
  │     └─ VienPhatSang (GameObject con - optional)
  │
  ├─ Xáo trộn puzzle (XaoTronPuzzle)
  │  └─ 50 bước di chuyển ngẫu nhiên
  │
  └─ puzzleDangHoatDong = true
```

### 4. **Bấm Mảnh Puzzle**
```
ManhGhep.OnMouseDown() [ManhGhepPuzzle]
  ↓
quanLy.ThuDiChuyen(this)
  ↓
if (CoKe(currentIndex, emptyIndex)) ✓
  ├─ DiChuyenManh()
  │  ├─ Cập nhật currentIndex mảnh
  │  ├─ Cập nhật emptyIndex
  │  └─ Di chuyển position
  │
  └─ KiemTraHoanThanh()
     ├─ Kiểm tra mọi mảnh: currentIndex == correctIndex?
     ├─ Kiểm tra emptyIndex == 8?
     └─ Nếu true → OnPuzzleSolved()
```

### 5. **Hoàn Thành Puzzle**
```
OnPuzzleSolved()
  ├─ Ẩn tất cả 8 mảnh puzzle
  │  └─ manh.gameObject.SetActive(false)
  │
  └─ HienLogoBinhThuong()
     ├─ Tạo GameObject mới "LogoBinhThuong"
     ├─ Add SpriteRenderer + logoBinhThuongSprite
     ├─ Tính scale = totalSize / spriteSize
     └─ logoObject.localScale = scale
```

## 📋 Script Mapping

### QuanLyXepHinh.cs (500+ lines)
```
Public Methods:
├─ BamVaoLogoMo()              ← Called từ BamLogoBang
├─ ThuDiChuyen(ManhGhepPuzzle) ← Called từ ManhGhepPuzzle
└─ DevSolvePuzzle()            ← Called từ Input.GetKeyDown

Private Methods:
├─ Start()
├─ Update()
├─ HienLogoMoSauVaiGiay()      [Coroutine]
├─ ChayLogoLonRoiTaoPuzzle()   [Coroutine]
├─ TaoPuzzleTuDong()
│  ├─ XoaPuzzleCu()
│  ├─ Sprite.Create() × 8
│  ├─ GameObject.Instantiate() × 8
│  ├─ TaoVienPhatSang()        [Optional glow]
│  └─ XaoTronPuzzle()
├─ XaoTronPuzzle(int steps)
│  ├─ Random.Range(slots)
│  └─ DiChuyenManh() × N
├─ TimSlotKe(int slot)         [Find neighbors]
├─ ThuDiChuyen(ManhGhepPuzzle) [Input handler]
├─ CoKe(int, int)              [Check Manhattan]
├─ DiChuyenManh(ManhGhepPuzzle)
│  ├─ Update dictionary
│  ├─ Update positions
│  └─ Update empty slot
├─ KiemTraHoanThanh()
│  └─ OnPuzzleSolved() [if solved]
├─ OnPuzzleSolved()
│  ├─ Hide all tiles
│  └─ HienLogoBinhThuong()
├─ HienLogoBinhThuong()
│  ├─ Create GameObject
│  ├─ Add SpriteRenderer
│  └─ Calculate & apply scale
└─ DevSolvePuzzle()
   ├─ Reset tất cả tiles
   ├─ OnPuzzleSolved()
   └─ Debug.Log()

Private Variables:
├─ danhSachManh                [List<ManhGhepPuzzle>]
├─ emptyIndex                  [int: 0-8]
├─ gridPositions               [Vector3[9]]
├─ currentIndexToTile          [Dictionary<int, Tile>]
├─ daTaoPuzzle                 [bool]
└─ puzzleDangHoatDong          [bool]
```

### ManhGhepPuzzle.cs (20 lines)
```
Public Methods:
└─ OnMouseDown()              ← Called by Unity

Private Methods:
└─ (None)

Public Variables:
├─ quanLy                      [QuanLyXepHinh]
├─ correctIndex                [int: 0-7]
└─ currentIndex                [int: 0-8]
```

### BamLogoBang.cs (20 lines)
```
Public Methods:
├─ OnMouseDown()              ← Called by Unity
└─ (None)

Private Methods:
└─ (None)

Public Variables:
└─ quanLyXepHinh              [QuanLyXepHinh]
```

## 🔄 Communication Diagram

```
Update Loop (Per Frame):
│
├─ Input.GetKeyDown(K) [Dev]
│  └─ DevSolvePuzzle()
│     └─ OnPuzzleSolved()
│
└─ Coroutine (Time-based):
   ├─ HienLogoMoSauVaiGiay()
   └─ ChayLogoLonRoiTaoPuzzle()

OnMouseDown (Physics):
│
├─ LogoMo.OnMouseDown() [BamLogoBang]
│  └─ BamVaoLogoMo()
│
└─ ManhGhep.OnMouseDown() [ManhGhepPuzzle]
   └─ ThuDiChuyen()
      └─ KiemTraHoanThanh()
```

## 🎯 Key Variables Tracking

### Per Tile Information
```csharp
public class ManhGhepPuzzle : MonoBehaviour
{
    public int correctIndex;   // Vị trí gốc (không đổi)
                               // 0 1 2
                               // 3 4 5
                               // 6 7 X (8 là empty)
    
    public int currentIndex;   // Vị trí hiện tại (thay đổi)
                               // Mỗi lần di chuyển:
                               // currentIndex = emptyIndex
}
```

### Grid Index Mapping
```
Index 0-8 → X,Y Coordinate:
Index  X  Y
  0    0  0  (top-left)
  1    1  0
  2    2  0  (top-right)
  3    0  1
  4    1  1  (center)
  5    2  1
  6    0  2
  7    1  2
  8    2  2  (bottom-right - empty)

Formula:
X = index % gridSize (0-2)
Y = index / gridSize (0-2)
```

### Manhattan Distance
```csharp
// Kiểm tra 2 vị trí có kề nhau không (4 hướng)
distance = |x1 - x2| + |y1 - y2|

if (distance == 1) → Kề nhau → Có thể trượt
if (distance == 0) → Cùng vị trí
if (distance > 1) → Không kề → Không trượt
```

## 🎨 Rendering Layer

```
Sorting Order:
  25 - Logo bình thường (khi hoàn thành)
  20 - Mảnh puzzle
  19 - Viền phát sáng (glow) của mảnh

Z Position:
  0 - Tất cả (2D → Z không quan trọng)
```

## 🔐 State Management

```
Scene States:
1. BEFORE_PUZZLE:
   - logoMo không hiện
   - logoLon không hiện
   - khungPuzzle không hiện

2. LOGO_MO_SHOWING:
   - logoMo hiện
   - Đợi click

3. LOGO_LON_SHOWING:
   - logoMo ẩn
   - logoLon hiện
   - Đợi hết timeout

4. PUZZLE_PLAYING:
   - logoLon ẩn
   - khungPuzzle + 8 mảnh hiện
   - puzzleDangHoatDong = true
   - Chờ click mảnh

5. PUZZLE_SOLVED:
   - 8 mảnh ẩn
   - Logo bình thường hiện
   - puzzleDangHoatDong = false
```

## 📊 Memory Usage

```
Per Puzzle Creation:
- 8 × ManhGhepPuzzle (MonoBehaviour)
- 8 × Sprite (Create runtime)
- 8 × GameObject
- 1 × Dictionary<int, Tile>
- 1 × List<Tile>
- 1 × Logo GameObject (on solve)

Cleanup:
- destroyOnComplete? No (logos stay)
- Tiles: Hidden (SetActive false)
- Dictionary: Cleared nếu reset
```

---

**Complexity:**
- Time: O(1) per move (constant operations)
- Space: O(1) (fixed 9 tiles max)
- Shuffle: O(50) = constant
- Check Solved: O(8) = constant

**Dependencies:**
- Unity 2D (SpriteRenderer, BoxCollider2D)
- Physics2D (OnMouseDown)
- Coroutines (Time-based events)
