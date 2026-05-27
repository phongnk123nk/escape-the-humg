# Hướng Dẫn Sử Dụng Puzzle Trượt 3x3

## 📋 Tổng Quan
Đây là một hệ thống puzzle trượt 3x3 hoàn chỉnh với các tính năng:
- Tự động cắt sprite thành 9 phần
- Cơ chế sliding puzzle đúng nghĩa (trượt ô vào ô trống)
- Kiểm tra tự động khi puzzle hoàn thành
- Hiển thị logo bình thường khi giải xong
- Phím K để skip puzzle (dev mode)

## 🎬 Flow Scene
1. **Khởi động (0-2 giây):** Chỉ hiện nền bảng
2. **Hiện Logo Nhỏ (2 giây):** LogoMo xuất hiện
3. **Bấm Logo Nhỏ:** Trigger để bắt đầu
4. **Logo Lớn (1 giây):** LogoLon hiện ra ở giữa
5. **Puzzle Xuất Hiện (1 giây sau):** 
   - 8 mảnh puzzle xuất hiện (ô góc dưới phải trống)
   - 8 mảnh bị xáo trộn
6. **Chơi Puzzle:** Bấm vào mảnh để trượt vào ô trống
7. **Hoàn Thành:** Khi tất cả mảnh đúng vị trí:
   - 8 mảnh ẩn đi
   - Logo bình thường hiện ra (full size)

## ⚙️ Setup Trong Unity

### Bước 1: Tạo Hierarchy
```
Scene
├── Canvas (hoặc UI root)
├── PuzzleContainer (GameObject để chứa puzzle)
│   └── (puzzle tiles sẽ được tạo tự động)
├── LogoMo (GameObject hiển thị logo nhỏ)
└── LogoLon (GameObject hiển thị logo lớn)
```

### Bước 2: Gắn Script QuanLyXepHinh
1. Tạo GameObject tên "PuzzleManager"
2. Gắn script `QuanLyXepHinh.cs` vào GameObject này

### Bước 3: Cấu Hình Inspector
Kéo thả các tham số vào Inspector của `QuanLyXepHinh`:

#### **[Header("Ảnh logo dùng để cắt")]**
- **Logo Ma Mi Sprite:** Ảnh logo có hiệu ứng (dùng để cắt puzzle)

#### **[Header("Ảnh logo hiển thị khi giải xong")]**
- **Logo Bình Thường Sprite:** Ảnh logo bình thường hiển thị sau khi giải xong

#### **[Header("Object trong scene")]**
- **Logo Mo:** Kéo GameObject LogoMo từ scene vào
- **Logo Lon:** Kéo GameObject LogoLon từ scene vào  
- **Khung Puzzle:** Kéo GameObject PuzzleContainer vào (nơi chứa puzzle)

#### **[Header("Cài đặt puzzle")]**
- **Grid Size:** 3 (không đổi cho 3x3)
- **Tile Size:** 1.4 (kích thước mỗi ô)
- **Thoi Gian Logo Lon Hien:** 1 (thời gian hiện LogoLon - giây)
- **Thoi Gian Logo Mo Xuat Hien:** 2 (thời gian chờ rồi hiện LogoMo - giây)

#### **[Header("Khoảng cách giữa các ô")]**
- **Gap Pixels:** 5 (khoảng cách giữa các ô - pixels)

#### **[Header("Viền phát sáng")]**
- **Bat Vien Phat Sang:** true/false (bật/tắt viền phát sáng)
- **Mau Vien Phat Sang:** Color (màu viền - mặc định đỏ)
- **Glow Scale Them:** 0.08 (mức độ phát sáng)
- **Glow Alpha:** 0.55 (độ trong suốt viền)

#### **[Header("Vị trí puzzle")]**
- **Puzzle Center:** Vector3(0, 0, 0) (tâm puzzle trong world)

#### **[Header("Dev Skip")]**
- **Bat Dev Skip:** true (bật/tắt phím skip)
- **Phim Skip:** K (phím để skip puzzle)

### Bước 4: Gắn Script BamLogoBang
1. Chọn GameObject LogoMo
2. Gắn script `BamLogoBang.cs`
3. Kéo PuzzleManager (GameObject có QuanLyXepHinh) vào field `QuanLyXepHinh`

## 🎮 Cách Chơi
- **Bấm vào mảnh puzzle:** Nếu mảnh nằm cạnh ô trống (trái, phải, trên, dưới), nó sẽ trượt vào ô trống
- **Mục tiêu:** Xếp tất cả 8 mảnh về vị trí ban đầu của chúng
- **Phím K (dev):** Tự động xếp puzzle về trạng thái hoàn thành và hiển thị logo bình thường

## 🔧 Script Chi Tiết

### QuanLyXepHinh.cs (Main Manager)
**Chức năng:**
- Quản lý toàn bộ logic puzzle
- Cắt sprite thành 9 phần
- Xử lý di chuyển mảnh
- Kiểm tra hoàn thành
- Hiển thị logo bình thường

**Public Methods:**
- `BamVaoLogoMo()` - Gọi khi bấm LogoMo
- `ThuDiChuyen(ManhGhepPuzzle manh)` - Xử lý khi bấm mảnh
- `DevSolvePuzzle()` - Skip puzzle (phím K)

### ManhGhepPuzzle.cs (Tile Script)
**Chức năng:**
- Lưu thông tin mảnh (vị trí đúng, vị trí hiện tại)
- Xử lý OnMouseDown để nhận click

**Public Variables:**
- `correctIndex` - Vị trí đúng ban đầu
- `currentIndex` - Vị trí hiện tại
- `quanLy` - Reference đến QuanLyXepHinh

### BamLogoBang.cs (Logo Clicker)
**Chức năng:**
- Nhận click trên LogoMo
- Gọi BamVaoLogoMo() từ QuanLyXepHinh

## 🐛 Troubleshooting

### Puzzle không hiện
- ✅ Kiểm tra LogoMaMiSprite có được kéo vào không
- ✅ Kiểm tra KhungPuzzle có được kéo vào không
- ✅ Kiểm tra texture của sprite có bị compress không (phải là readable)

### Mảnh không trượt được
- ✅ Kiểm tra mỗi mảnh có BoxCollider2D không
- ✅ Kiểm tra Collider có isTrigger = true không
- ✅ Kiểm tra BamLogoBang được gắn vào LogoMo không

### Logo bình thường không hiện
- ✅ Kiểm tra LogoBinhThuongSprite có được kéo vào không
- ✅ Kiểm tra console có lỗi gì không

### Phím K không work
- ✅ Kiểm tra BatDevSkip = true trong Inspector
- ✅ Kiểm tra puzzle đã được tạo (phải hiện 8 mảnh trước)

## 📝 Lưu Ý Kỹ Thuật

### Cách Xáo Trộn
Puzzle sử dụng phương pháp **shuffle an toàn** - giả lập 50 bước trượt ngẫu nhiên từ trạng thái hoàn thành. Điều này đảm bảo puzzle **luôn có thể giải được**.

### Kiểm Tra Hoàn Thành
Puzzle được coi là hoàn thành khi:
1. Tất cả 8 mảnh đang ở đúng vị trí gốc của chúng (correctIndex == currentIndex)
2. Ô trống (emptyIndex) ở vị trí 8 (góc dưới bên phải)

### Kích Thước Logo
Logo bình thường được scale sao cho:
- **Chiều rộng + cao = tổng kích thước grid**
- **Công thức:** totalSize = gridSize × tileSize + (gridSize - 1) × gap

### Sorting Order
- Mảnh puzzle: sortingOrder = 20
- Logo bình thường: sortingOrder = 25
- Viền phát sáng: sortingOrder = 19

## 🎨 Tùy Chỉnh

### Thay Đổi Kích Thước Ô
Sửa `Tile Size` trong Inspector (mặc định 1.4)

### Thay Đổi Khoảng Cách
Sửa `Gap Pixels` trong Inspector (mặc định 5)

### Bật/Tắt Viền Phát Sáng
Sửa `Bat Vien Phat Sang` trong Inspector

### Thay Đổi Màu Viền
Sửa `Mau Vien Phat Sang` trong Inspector

### Thay Đổi Thời Gian
- **Thoi Gian Logo Mo Xuat Hien:** Thời gian chờ rồi hiện LogoMo
- **Thoi Gian Logo Lon Hien:** Thời gian LogoLon hiện trước puzzle
