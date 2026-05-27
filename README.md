# Escape The HUMG

Đây là project game Unity của **Escape The HUMG**. Repo này đã bao gồm mã nguồn, scene, script, hình ảnh, video nhỏ, prefab, package và toàn bộ file `.meta` cần thiết để tải về mở lại trong Unity.

![Tải project từ GitHub](docs/images/huong-dan-tai-github.svg)

## 1. Cần cài những gì?

Trước khi mở project, máy cần có:

- **Windows 10 hoặc Windows 11**
- **Unity Hub**
- **Unity Editor 6000.3.9f1**
- Internet trong lần mở đầu tiên để Unity tải package theo file `Packages/manifest.json`

Project này được tạo bằng đúng phiên bản:

```text
Unity 6000.3.9f1
```

Nên dùng đúng bản này để tránh lỗi package, lỗi scene, lỗi render hoặc Unity tự nâng cấp project.

## 2. Cài Unity đúng phiên bản

![Cài Unity bằng Unity Hub](docs/images/cai-unity-hub.svg)

Làm theo các bước sau:

1. Mở **Unity Hub**.
2. Vào tab **Installs**.
3. Bấm **Install Editor**.
4. Tìm và cài **Unity 6000.3.9f1**.
5. Nếu Unity Hub không hiện đúng bản này, hãy vào **Unity Download Archive**, tìm `6000.3.9f1`, rồi bấm **Install with Unity Hub**.

Khi cài module, nếu chỉ mở project và bấm Play trong Editor thì không cần cài thêm gì đặc biệt. Nếu muốn build game cho người khác chơi, nên cài thêm:

- **Windows Build Support (IL2CPP)** nếu muốn build file `.exe`
- **WebGL Build Support** nếu muốn build bản chơi trên trình duyệt

## 3. Tải project từ GitHub

Link repo:

```text
https://github.com/phongnk123nk/escape-the-humg
```

Cách tải:

1. Mở link repo trên GitHub.
2. Bấm nút **Code** màu xanh.
3. Chọn **Download ZIP**.
4. Giải nén file ZIP ra một thư mục dễ tìm, ví dụ:

```text
D:\UnityProjects\escape-the-humg
```

Không mở project trực tiếp trong file ZIP. Bắt buộc phải giải nén trước.

## 4. Mở project bằng Unity Hub

![Mở project bằng Unity Hub](docs/images/mo-project-unity-hub.svg)

Làm như sau:

1. Mở **Unity Hub**.
2. Bấm **Add** hoặc **Add project from disk**.
3. Chọn thư mục project đã giải nén.
4. Chọn đúng thư mục có các folder sau:

```text
Assets
Packages
ProjectSettings
```

5. Bấm **Open**.
6. Chờ Unity import toàn bộ asset.

Lần đầu mở project sẽ hơi lâu vì Unity phải tự tạo lại thư mục `Library`.

## 5. Chạy game trong Unity

![Chạy game trong Unity](docs/images/chay-game-unity.svg)

Sau khi Unity import xong:

1. Trong cửa sổ **Project**, mở thư mục:

```text
Assets/Scenes
```

2. Mở scene:

```text
main menu.unity
```

3. Bấm nút **Play** ở phía trên Unity.

Scene đầu tiên của game là:

```text
Assets/Scenes/main menu.unity
```

## 6. Danh sách scene trong Build Settings

Project hiện có các scene chính sau:

```text
Assets/Scenes/main menu.unity
Assets/Scenes/room1.unity
Assets/Scenes/GOODENDING.unity
Assets/Scenes/BangXepHinh.unity
Assets/Scenes/hanh lang 1.unity
Assets/Scenes/PhongThiNghiem.unity
Assets/Scenes/hanh lang 2.unity
Assets/Scenes/PhongTinHoc.unity
Assets/Scenes/hanh lang 3.unity
Assets/Scenes/ending 1.unity
```

Nếu build game, hãy đảm bảo `main menu.unity` đứng đầu danh sách scene.

## 7. Các thư mục không có trong GitHub

Một số thư mục Unity tự sinh ra nên không cần đưa lên GitHub:

```text
Library
Temp
Logs
UserSettings
.plastic
```

Khi tải project về và mở bằng Unity, Unity sẽ tự tạo lại các thư mục này.

## 8. Lỗi thường gặp

### Unity báo sai phiên bản

Hãy cài đúng bản:

```text
Unity 6000.3.9f1
```

Nếu dùng bản Unity khác, Unity có thể tự nâng cấp project và làm thay đổi file setting.

### Mở project bị mất hình, mất sprite, mất prefab

Nguyên nhân thường là thiếu file `.meta`.

Khi tải project về, không được xóa các file `.meta` trong thư mục `Assets`.

### Unity import rất lâu

Đây là bình thường trong lần mở đầu tiên. Unity đang tạo lại thư mục `Library`.

### Project báo lỗi package

Thử làm theo thứ tự:

1. Đóng Unity.
2. Mở lại project bằng Unity Hub.
3. Kiểm tra máy có Internet.
4. Nếu vẫn lỗi, xóa thư mục `Library`, sau đó mở lại project.

## 9. Build game ra file cho người khác chơi

Nếu chỉ gửi source code thì người nhận cần cài Unity. Nếu muốn người khác chỉ tải về và chơi luôn, hãy build ra bản Windows.

Cách build:

1. Mở Unity.
2. Vào **File > Build Profiles** hoặc **File > Build Settings**.
3. Chọn platform **Windows**.
4. Kiểm tra scene đầu tiên là:

```text
Assets/Scenes/main menu.unity
```

5. Bấm **Build**.
6. Chọn thư mục output, ví dụ:

```text
Builds/Windows
```

7. Sau khi build xong, nén cả thư mục build thành file `.zip`.
8. Gửi file `.zip` đó cho người khác.

Người chơi chỉ cần giải nén và chạy file `.exe`, không cần cài Unity.

## 10. Clone bằng Git

Nếu không tải ZIP mà dùng Git, chạy lệnh:

```bash
git clone https://github.com/phongnk123nk/escape-the-humg.git
```

Sau đó mở thư mục vừa clone bằng Unity Hub.

## 11. Ghi chú

- Không xóa file `.meta`.
- Không cần tải hoặc copy thư mục `Library`.
- Nên mở bằng Unity `6000.3.9f1`.
- Lần đầu mở project có thể mất vài phút để Unity import lại toàn bộ dữ liệu.
