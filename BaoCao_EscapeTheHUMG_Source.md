# BỘ GIÁO DỤC VÀ ĐÀO TẠO
# TRƯỜNG ĐẠI HỌC MỎ - ĐỊA CHẤT

# BÁO CÁO ĐỒ ÁN TỐT NGHIỆP

# XÂY DỰNG VÀ PHÁT TRIỂN GAME 2D “ESCAPE THE HUMG” BẰNG UNITY

Sinh viên thực hiện: Nguyễn Hồng Minh
Mã sinh viên: 212121051315
Chuyên ngành: Công nghệ thông tin
Công cụ thực hiện: Unity 6, C#, Visual Studio Code/Visual Studio

Hà Nội, 2026

\page

# MỤC LỤC

\toc

\page

# THÔNG TIN ĐỒ ÁN

## 1. Thông tin chung

| Nội dung | Thông tin |
| --- | --- |
| Tên đề tài | Xây dựng và phát triển game 2D “Escape The HUMG” bằng Unity |
| Thể loại | Game 2D point-and-click, giải đố, kinh dị học đường |
| Nền tảng | Windows PC |
| Công nghệ | Unity 6, ngôn ngữ C#, SpriteRenderer, BoxCollider2D, Canvas UI, VideoPlayer |
| Mục tiêu sản phẩm | Xây dựng một trò chơi giải đố theo tuyến cảnh, có hệ thống điều hướng, mini-game, khóa mật mã, video, pause menu và nhiều ending. |

## 2. Mục tiêu

- Thiết kế một game 2D có không khí kinh dị, lấy bối cảnh trường học và các phòng chức năng như lớp học, hành lang, phòng thí nghiệm và phòng tin học.
- Xây dựng hệ thống điều hướng bằng mũi tên và vùng click để người chơi chuyển qua các góc nhìn khác nhau.
- Tích hợp các câu đố tương tác gồm xếp hình, cân bằng phương trình hóa học, mini-game quân mã, mini-game giao hàng và ổ khóa mật mã.
- Tạo hệ thống menu chính, pause bằng phím ESC, chuyển scene và các đoạn video/cutscene phục vụ mạch chơi.
- Tổ chức project Unity rõ ràng, dễ chỉnh sửa trực tiếp trong Scene View và Inspector.

## 3. Phạm vi nghiên cứu và phương pháp thực hiện

Đề tài tập trung vào quá trình xây dựng một trò chơi 2D dạng point-and-click escape room. Phạm vi triển khai bao gồm thiết kế scene, tổ chức tài nguyên hình ảnh, xây dựng hệ thống tương tác, lập trình các câu đố, quản lý luồng chuyển scene và kiểm thử chức năng cơ bản. Phương pháp thực hiện là phân tích yêu cầu, chia nhỏ hệ thống thành các module, triển khai từng chức năng trong Unity bằng C#, sau đó kiểm thử trực tiếp trong Play Mode.

## 4. Kết quả chính đạt được

- Hoàn thiện nhiều scene chính: main menu, room1, BangXepHinh, hanh lang 1, hanh lang 2, hanh lang 3, PhongThiNghiem, PhongTinHoc, ending 1 và GOODENDING.
- Tạo được hệ thống điều hướng theo frame cho các hành lang và phòng tin học.
- Tạo được các mini-game có điều kiện thắng, phần thưởng dạng con số và khả năng liên kết với ổ khóa/câu đố cuối.
- Tạo được pause menu toàn cục bằng phím ESC, có thể chỉnh UI trong scene và không hoạt động tại main menu.
- Tạo được các hiệu ứng chuyển cảnh, video mở đầu, video chuyển cảnh và hiệu ứng jumpscare.

\page

# MỞ ĐẦU

## 1. Lý do chọn đề tài

Game 2D dạng point-and-click là một hướng phát triển phù hợp để vận dụng kiến thức lập trình, thiết kế giao diện và tổ chức hệ thống tương tác trong Unity. Thể loại này không yêu cầu điều khiển nhân vật phức tạp như các game hành động thời gian thực, nhưng lại đòi hỏi người phát triển phải tổ chức tốt bối cảnh, vùng tương tác, câu đố, luồng sự kiện và phản hồi cho người chơi.

Với đề tài “Escape The HUMG”, trò chơi được xây dựng theo phong cách kinh dị học đường, trong đó người chơi khám phá các khu vực trong trường, giải câu đố và tìm cách thoát khỏi không gian bí ẩn. Việc lựa chọn Unity giúp quá trình triển khai thuận lợi vì Unity hỗ trợ tốt game 2D, hệ thống scene, SpriteRenderer, Collider2D, Canvas UI, VideoPlayer và lập trình C#.

Thông qua đồ án, sinh viên có thể rèn luyện khả năng phân tích yêu cầu, thiết kế gameplay, tổ chức mã nguồn, xử lý tương tác người dùng và kiểm thử sản phẩm game.

## 2. Bố cục đồ án

- Chương 1: Tổng quan lý thuyết và lĩnh vực nghiên cứu.
- Chương 2: Phân tích thiết kế hệ thống game Escape The HUMG.
- Chương 3: Kết quả xây dựng và triển khai game trong Unity.
- Kết luận và hướng phát triển.

\page

# CHƯƠNG 1. TỔNG QUAN LÝ THUYẾT VÀ LĨNH VỰC NGHIÊN CỨU

## 1.1 Cơ sở lý thuyết

### 1.1.1 Game point-and-click 2D

Game point-and-click là thể loại trò chơi trong đó người chơi tương tác chủ yếu bằng chuột hoặc thao tác chạm. Người chơi quan sát khung cảnh, tìm các vùng có thể tương tác, nhấn vào đồ vật, mũi tên hoặc điểm nghi vấn để chuyển góc nhìn, thu thập thông tin và giải câu đố. Trong game Escape The HUMG, cơ chế point-and-click được sử dụng ở hầu hết các scene như phòng tin học, phòng thí nghiệm và các hành lang.

### 1.1.2 Game giải đố và escape room

Escape room là dạng trò chơi tập trung vào việc giải chuỗi câu đố để mở khóa khu vực mới hoặc tiến tới kết thúc. Các câu đố thường được thiết kế dưới dạng mật mã, tương tác vật phẩm, mini-game hoặc tìm manh mối trong môi trường. Sản phẩm trong đồ án áp dụng mô hình này thông qua ổ khóa hai chữ số, câu đố hóa học, puzzle xếp hình và các mini-game lấy số làm manh mối.

### 1.1.3 Unity Engine

Unity Engine là công cụ phát triển game hỗ trợ mạnh cho cả game 2D và 3D. Đối với dự án này, Unity được sử dụng để quản lý scene, hiển thị hình ảnh nền bằng SpriteRenderer, tạo vùng click bằng BoxCollider2D, xây dựng UI bằng Canvas, điều khiển video bằng VideoPlayer và lập trình gameplay bằng C#.

### 1.1.4 Ngôn ngữ C# trong Unity

C# là ngôn ngữ chính dùng để điều khiển logic trong Unity. Các script trong dự án được dùng để xử lý chuyển scene, điều hướng frame, nhập mật mã, điều khiển mini-game, quản lý pause menu, xử lý animation mũi tên và điều khiển luồng chơi. Việc tách chức năng thành nhiều script giúp project dễ bảo trì và dễ chỉnh sửa trong Inspector.

### 1.1.5 Một số thành phần Unity được sử dụng

- GameObject: đối tượng cơ bản chứa Transform và các component trong scene.
- SpriteRenderer: hiển thị background, icon, mũi tên và các vật thể 2D.
- BoxCollider2D: tạo vùng click và vùng va chạm cho hotspot, ổ khóa, xe giao hàng và các đối tượng tương tác.
- Canvas UI: xây dựng main menu, pause menu, popup nhập mật mã và các panel hỗ trợ.
- VideoPlayer: phát video mở đầu và video chuyển cảnh.
- SceneManager: chuyển giữa các scene trong luồng chơi.

## 1.2 Các công cụ hỗ trợ

| Công cụ | Vai trò |
| --- | --- |
| Unity 6 | Xây dựng scene, quản lý asset, Play Mode và build game. |
| C# | Lập trình logic gameplay và UI. |
| Visual Studio Code/Visual Studio | Chỉnh sửa mã nguồn, kiểm tra lỗi biên dịch. |
| Unity Asset/ảnh tự chuẩn bị | Cung cấp background, icon, mũi tên, ổ khóa, nhân vật và video. |
| TextMeshPro/Unity UI | Hiển thị chữ, nút, popup và thông báo trong game. |

## 1.3 Kết chương

Chương 1 đã trình bày cơ sở lý thuyết và các công nghệ được dùng trong quá trình phát triển game Escape The HUMG. Những nội dung này là nền tảng để phân tích thiết kế hệ thống và triển khai sản phẩm ở các chương tiếp theo.

\page

# CHƯƠNG 2. PHÂN TÍCH THIẾT KẾ HỆ THỐNG

## 2.1 Mở đầu chương

Chương này trình bày quá trình phân tích chức năng và thiết kế hệ thống cho game Escape The HUMG. Game được xây dựng theo cấu trúc nhiều scene, mỗi scene đảm nhiệm một phần trong mạch trải nghiệm: menu, phòng học, bảng xếp hình, hành lang, phòng thí nghiệm, phòng tin học và ending.

## 2.2 Khảo sát hệ thống

Người chơi bắt đầu từ main menu, vào room1, xem video mở đầu, sau đó lần lượt khám phá các scene, giải câu đố và mở khóa đường đi. Các scene được thiết kế theo dạng ảnh nền 2D với các vùng click thật trong Hierarchy để có thể chỉnh sửa trực tiếp. Mỗi nút mũi tên hoặc hotspot đều có Collider2D để nhận thao tác nhấn chuột.

## 2.3 Tổng quan chức năng

| Mã chức năng | Tên chức năng | Mô tả |
| --- | --- | --- |
| F01 | Menu chính | Hiển thị tên game, nút Chơi ngay, Ủng hộ và Thoát. |
| F02 | Điều hướng scene/frame | Chuyển giữa các ảnh nền hoặc các góc nhìn bằng mũi tên và hotspot. |
| F03 | Pause menu | Nhấn ESC để tạm dừng game, tiếp tục hoặc quay về menu chính. |
| F04 | Puzzle xếp hình | Người chơi hoàn thành câu đố xếp hình để mở đường tiếp theo. |
| F05 | Phòng thí nghiệm | Khám phá nhiều góc nhìn, mở tủ, tương tác máy và giải câu đố phương trình hóa học. |
| F06 | Phòng tin học | Điều hướng nhiều frame, chơi hai mini-game trên màn hình máy tính và mở khóa cửa. |
| F07 | Mini-game quân mã | Di chuyển quân mã trên bàn cờ 3x3, ăn đủ 3 bụi cỏ để nhận số. |
| F08 | Mini-game giao hàng | Điều khiển xe lấy 3 đơn hàng và giao tới 3 vị trí ngẫu nhiên trên đường. |
| F09 | Video và jumpscare | Phát video/cutscene, làm tối màn hình và hiển thị jumpscare theo thời gian tùy chỉnh. |
| F10 | Ending | Chuyển tới ending tương ứng sau khi hoàn thành chuỗi nhiệm vụ. |

### 2.3.1 Tác nhân trong hệ thống

Tác nhân chính của hệ thống là người chơi. Người chơi thao tác bằng chuột để nhấn mũi tên, vùng click, icon mini-game, nút UI và nhập mật mã. Hệ thống phản hồi bằng cách đổi background, bật/tắt object theo frame, phát video, hiển thị popup hoặc chuyển scene.

### 2.3.2 Use case tổng quát

- Người chơi chọn Chơi ngay tại main menu để vào room1.
- Người chơi xem video mở đầu, sau đó bắt đầu khám phá môi trường.
- Người chơi nhấn mũi tên để chuyển qua các hành lang và phòng chức năng.
- Người chơi giải các câu đố để nhận manh mối hoặc mở khóa đường đi.
- Người chơi có thể nhấn ESC trong quá trình chơi để tạm dừng.
- Khi hoàn thành điều kiện cuối, game chuyển tới scene ending.

## 2.4 Đặc tả chức năng chính

### 2.4.1 Chức năng điều hướng phòng tin học

Scene PhongTinHoc được quản lý bởi ComputerRoomNavigator. Hệ thống có các frame chính gồm Frame44_MainDoorView, Frame45_MainComputerView, Frame46_BackToDoorView, Frame47_ComputerDeskView, Frame49_ComputerScreenView và Frame50_DoorCloseView. Mỗi frame có background riêng, các hotspot tương ứng và có thể chỉnh sửa trực tiếp trong Scene View. Khi đổi frame, script bật nhóm object thuộc frame hiện tại và tắt các nhóm khác.

### 2.4.2 Chức năng ổ khóa cửa

Tại Frame50_DoorCloseView có ổ khóa hai chữ số. Khi người chơi bấm vào ổ khóa, popup nhập mã hiện ra. Người phát triển có thể chỉnh mã đúng trong Inspector thông qua các biến doorLockFirstDigit và doorLockSecondDigit. Nếu nhập đúng, hệ thống chuyển tới scene tiếp theo đã cấu hình.

### 2.4.3 Chức năng mini-game quân mã

Mini-game quân mã được tích hợp trong Frame49_ComputerScreenView. Người chơi bấm icon quân mã để mở bàn cờ 3x3. Quân mã di chuyển theo luật hình chữ L, mục tiêu là ăn đủ 3 bụi cỏ. Vị trí bụi cỏ xuất hiện ngẫu nhiên, không trùng với các ô đã từng xuất hiện. Sau khi hoàn thành, bàn cờ ẩn đi và icon quân mã chuyển thành một con số làm manh mối.

### 2.4.4 Chức năng mini-game giao hàng

Mini-game giao hàng được tích hợp vào icon đồ ăn. Người chơi điều khiển xe trong vùng giới hạn, lấy package và giao tới location tương ứng. Package và location xuất hiện ngẫu nhiên trên đường, đồng thời được bố trí xa vị trí xuất hiện trước đó. Khi giao đủ 3 đơn hàng, hệ thống phát chuỗi hoàn thành gồm màn hình tối, jumpscare, ẩn mini-game và biến icon thành số.

### 2.4.5 Chức năng phòng thí nghiệm

Scene PhongThiNghiem có hệ thống nhiều góc nhìn như MainLab, BackView, CabinetView, CabinetOpenView, MachineView và TableView. Người chơi di chuyển qua các góc nhìn bằng mũi tên, tương tác với tủ, bàn và máy. Câu đố chính là cân bằng phương trình hóa học; khi giải đúng, người chơi nhận chìa khóa hoặc mở vùng thoát sang scene tiếp theo.

### 2.4.6 Chức năng hành lang

Các scene hanh lang 1, hanh lang 2 và hanh lang 3 sử dụng HallwayImageNavigator. Mỗi frame là một GameObject thật trong Hierarchy, có ảnh nền và mũi tên riêng. Người phát triển có thể tách các frame trong Scene View để chỉnh vị trí, kích thước và collider. Hệ thống hỗ trợ hiệu ứng fade khi chuyển frame, phát video tại một frame nhất định và chuyển scene khi bấm vùng click cuối.

### 2.4.7 Chức năng pause menu

GlobalEscPauseMenu tạo menu tạm dừng dùng chung cho toàn game. Khi người chơi nhấn ESC ở các scene gameplay, panel TẠM DỪNG hiện ra với hai lựa chọn TIẾP TỤC và THOÁT. Riêng scene main menu không nhận ESC để tránh chồng menu pause lên giao diện chính.

## 2.5 Thiết kế dữ liệu và scene

| Scene | Vai trò |
| --- | --- |
| main menu | Màn hình chính, nút chơi ngay, ủng hộ và thoát. |
| room1 | Phòng học mở đầu, video giới thiệu và các tương tác ban đầu. |
| BangXepHinh | Scene puzzle xếp hình và chuyển tới hành lang. |
| hanh lang 1 | Chuỗi 5 frame hành lang, có video chuyển cảnh và vùng chuyển scene. |
| PhongThiNghiem | Phòng thí nghiệm, hệ thống góc nhìn và câu đố hóa học. |
| hanh lang 2 | Chuỗi 9 frame hành lang tiếp theo, frame cuối chuyển scene. |
| PhongTinHoc | Phòng tin học, hai mini-game và ổ khóa cửa. |
| hanh lang 3 | Chuỗi 6 frame hành lang và frame phụ liên kết room1. |
| ending 1 | Ending dạng chữ/cutscene. |
| GOODENDING | Kết thúc tốt. |

## 2.6 Thiết kế các script chính

| Script | Chức năng |
| --- | --- |
| MainMenuButtonActions | Gán chức năng Chơi ngay và Thoát tại main menu. |
| GlobalEscPauseMenu | Quản lý pause menu toàn cục bằng phím ESC. |
| RoomIntroVideoPlayer | Phát video mở đầu trong room1 và hiệu ứng chuyển cảnh sau video. |
| HallwayImageNavigator | Điều hướng frame trong các scene hành lang, hỗ trợ video và fade. |
| HallwayArrowHotspot | Nhận click mũi tên trong hành lang. |
| LabSceneNavigator | Điều hướng nhiều góc nhìn trong phòng thí nghiệm. |
| LabEquationPuzzleManager | Quản lý câu đố cân bằng phương trình hóa học. |
| ComputerRoomNavigator | Điều hướng frame phòng tin học, ổ khóa, puzzle và chuyển scene. |
| ComputerRoomMiniGameIcon | Mở mini-game từ icon và đổi icon thành số sau khi hoàn thành. |
| GameManager / BoardManager / Knight | Quản lý mini-game quân mã. |
| DeliveryOrderMiniGameManager | Quản lý mini-game giao hàng, package, location và jumpscare. |
| DeliveryCarPlayAreaLimiter | Giới hạn xe trong vùng chơi. |

## 2.7 Yêu cầu phi chức năng

- Giao diện phải dễ nhìn, các nút tương tác rõ ràng và có phản hồi khi click.
- Các vùng click phải tồn tại thật trong scene để người phát triển chỉnh sửa được trước khi Play.
- Game cần chạy ổn định trong Play Mode, tránh tự xóa object người dùng đã đặt trong scene.
- Các scene phải có khả năng mở rộng bằng cách thêm frame, hotspot hoặc mini-game mới.
- Text trong game sử dụng tiếng Việt có dấu để phù hợp với người chơi Việt Nam.

\page

# CHƯƠNG 3. KẾT QUẢ XÂY DỰNG VÀ TRIỂN KHAI GAME ESCAPE THE HUMG

## 3.1 Giới thiệu chương

Chương này trình bày kết quả xây dựng sản phẩm trong Unity, bao gồm giao diện chính, hệ thống scene, các câu đố, mini-game và các chức năng hỗ trợ như pause menu, video, hiệu ứng chuyển cảnh.

## 3.2 Kết quả đạt được

### 3.2.1 Giao diện main menu

Main menu hiển thị tên game Escape The HUMG cùng các nút CHƠI NGAY, ỦNG HỘ và THOÁT. Nút CHƠI NGAY được gán để chuyển vào scene room1, nút THOÁT dùng để thoát game, còn nút ỦNG HỘ được giữ lại cho chức năng phát triển sau.

### 3.2.2 Hệ thống phòng học và video mở đầu

Scene room1 chứa bối cảnh phòng học. Trước khi người chơi tương tác, video mở đầu “video da sua” được phát. Sau khi video kết thúc, màn hình chuyển cảnh tối sáng dần rồi mới bắt đầu gameplay. Cách làm này giúp người chơi tiếp nhận bối cảnh trước khi bước vào phần giải đố.

### 3.2.3 Puzzle xếp hình

Scene BangXepHinh chứa hệ thống xếp hình bằng các mảnh ghép. Khi người chơi hoàn thành puzzle, hệ thống cho phép đi tiếp tới scene hành lang. Mũi tên chuyển scene được gắn collider và script chuyển scene.

### 3.2.4 Hệ thống hành lang

Các scene hành lang được xây dựng bằng nhiều frame ảnh nền theo thứ tự. Mỗi frame có PreviewBackground và NextArrow riêng, có thể chỉnh sửa trực tiếp trong Scene View. Ở hanh lang 1, frame 4 có mũi tên phát video vap trước khi tự chuyển sang frame 5; frame 5 sử dụng vùng BoxCollider lớn để chuyển scene. Hanh lang 2 và hanh lang 3 được thiết lập tương tự.

### 3.2.5 Phòng thí nghiệm

Phòng thí nghiệm có nhiều góc nhìn và các vùng tương tác như tủ, bàn, máy và cửa. Câu đố cân bằng phương trình hóa học được xây dựng bằng UI, các nút NỘP, ĐẶT LẠI, GỢI Ý và BỎ QUA. Khi hoàn thành, người chơi nhận phần thưởng hoặc mở đường đi tiếp.

### 3.2.6 Phòng tin học

Phòng tin học là scene có nhiều hệ thống nhất. Người chơi di chuyển qua các frame 44, 45, 46, 47, 49 và 50. Tại frame 49, màn hình máy tính chứa hai icon mini-game. Người chơi phải hoàn thành mini-game quân mã và giao hàng để nhận hai con số. Tại frame 50, người chơi nhập mã vào ổ khóa để mở cửa và chuyển tới scene tiếp theo.

### 3.2.7 Mini-game quân mã

Mini-game quân mã có bàn cờ 3x3, quân mã, bãi cỏ mục tiêu và hệ thống điều khiển theo luật di chuyển của quân mã. Sau mỗi lần ăn cỏ, vị trí cỏ mới được chọn ngẫu nhiên và không trùng với vị trí đã dùng trước đó. Khi ăn đủ 3 lần, mini-game kết thúc và icon quân mã đổi thành số đầu tiên.

### 3.2.8 Mini-game giao hàng

Mini-game giao hàng cho phép điều khiển xe trên hệ thống đường. Package và location được đặt ngẫu nhiên trên road, đồng thời bảo đảm vị trí mới đủ xa so với vị trí trước đó. Khi lấy package, màn hình hiển thị “Bạn có 1 đơn hàng”. Khi giao đủ 3 đơn, hệ thống phát hiệu ứng hoàn thành gồm màn hình tối, jumpscare và đổi icon đồ ăn thành số thứ hai.

### 3.2.9 Pause menu và UI tiếng Việt

Pause menu toàn cục sử dụng các chữ TẠM DỪNG, TIẾP TỤC và THOÁT. Main menu không nhận ESC để tránh hiện pause menu sai ngữ cảnh. Các chữ trong game được chỉnh sang tiếng Việt có dấu nhằm tăng tính hoàn thiện của sản phẩm.

## 3.3 Kiểm thử chức năng

| Chức năng | Kịch bản kiểm thử | Kết quả mong đợi |
| --- | --- | --- |
| Main menu | Bấm CHƠI NGAY | Chuyển vào scene room1. |
| Pause menu | Nhấn ESC trong scene gameplay | Hiện menu TẠM DỪNG, có thể tiếp tục hoặc thoát về menu. |
| Hành lang | Bấm mũi tên ở từng frame | Chuyển sang frame tiếp theo, có fade transition. |
| Video hành lang | Bấm mũi tên frame có video | Video phát xong tự chuyển sang frame kế tiếp. |
| Phòng thí nghiệm | Giải đúng câu đố hóa học | Nhận phần thưởng/mở đường đi tiếp. |
| Mini-game quân mã | Ăn đủ 3 bụi cỏ | Mini-game ẩn và icon đổi thành số. |
| Mini-game giao hàng | Lấy và giao đủ 3 đơn | Hiển thị hoàn thành, chạy jumpscare và đổi icon thành số. |
| Ổ khóa phòng tin học | Nhập đúng hai số | Cửa mở và chuyển scene tiếp theo. |

## 3.4 Đánh giá kết quả

Sản phẩm đã triển khai được các chức năng chính của một game 2D point-and-click escape room. Các scene được tổ chức theo luồng chơi rõ ràng, có nhiều dạng câu đố khác nhau và có khả năng chỉnh sửa trực tiếp trong Unity Scene View. Hệ thống script được chia theo từng nhóm chức năng nên thuận tiện cho việc bảo trì và mở rộng.

Một số điểm có thể tiếp tục hoàn thiện gồm bổ sung âm thanh nền và hiệu ứng âm thanh cho từng tương tác, tối ưu lại toàn bộ asset hình ảnh, thêm hệ thống lưu tiến trình, bổ sung hướng dẫn chơi trong game và kiểm thử trên nhiều độ phân giải màn hình.

\page

# KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN

## 1. Kết luận chung

Đồ án đã hoàn thành việc xây dựng một game 2D dạng point-and-click giải đố bằng Unity. Sản phẩm có đầy đủ các thành phần cơ bản của một game hoàn chỉnh gồm main menu, hệ thống scene, điều hướng, câu đố, mini-game, video, pause menu và ending. Quá trình thực hiện giúp củng cố kiến thức về Unity, C#, tổ chức scene, thiết kế UI, xử lý tương tác và quản lý luồng chơi.

## 2. Hướng phát triển

- Bổ sung hệ thống lưu tiến trình để người chơi có thể tiếp tục từ vị trí đã chơi.
- Thêm âm thanh nền, hiệu ứng âm thanh khi click, mở khóa, jumpscare và hoàn thành puzzle.
- Hoàn thiện thêm nhiều câu đố liên kết với cốt truyện.
- Tối ưu UI cho nhiều tỉ lệ màn hình khác nhau.
- Bổ sung hướng dẫn chơi và nhật ký manh mối để người chơi dễ theo dõi.
- Đóng gói bản build Windows hoàn chỉnh để phát hành thử nghiệm.

\page

# TÀI LIỆU THAM KHẢO

[1] Unity Technologies, Unity Manual - 2D game development.

[2] Microsoft, C# documentation.

[3] Unity Technologies, Manual: Scenes, GameObjects and Components.

[4] Unity Technologies, Manual: SpriteRenderer, Collider2D, Canvas UI and VideoPlayer.

[5] Tài liệu, mã nguồn và asset trong project Escape The HUMG.
