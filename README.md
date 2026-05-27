# Escape The HUMG

Day la project game Unity. File source, scene, script, hinh anh, video nho va cac file `.meta` deu nam trong repo nay.

## Yeu cau cai dat

- Windows 10/11
- Unity Hub
- Unity Editor `6000.3.9f1`
- Ket noi internet trong lan mo dau tien de Unity tai package theo `Packages/manifest.json`

Nen dung dung ban Unity trong project:

```text
Unity 6.3 LTS - 6000.3.9f1
```

Neu khong thay dung ban nay trong Unity Hub, co the cai ban Unity 6 gan nhat, nhung nen uu tien `6000.3.9f1` de tranh loi package hoac setting.

## Cach tai project ve may

1. Mo link GitHub:

   ```text
   https://github.com/phongnk123nk/escape-the-humg
   ```

2. Bam nut **Code** mau xanh.
3. Chon **Download ZIP**.
4. Giai nen file ZIP ra mot thu muc bat ky, vi du:

   ```text
   D:\UnityProjects\escape-the-humg
   ```

Khong mo truc tiep project trong file ZIP. Phai giai nen truoc.

## Cach cai Unity dung ban

1. Mo **Unity Hub**.
2. Vao tab **Installs**.
3. Bam **Install Editor**.
4. Chon Unity `6000.3.9f1`.
5. Neu Unity Hub khong hien dung ban:
   - Vao trang Unity Download Archive.
   - Tim Unity `6000.3.9f1`.
   - Bam **Install with Unity Hub**.
6. Khi cai module, toi thieu can co:
   - **Windows Build Support (IL2CPP)** neu muon build file `.exe`
   - **WebGL Build Support** neu muon build cho trinh duyet

Chi can mo/chay trong Editor thi khong bat buoc cai module build.

## Cach mo project trong Unity

1. Mo **Unity Hub**.
2. Bam **Add** hoac **Add project from disk**.
3. Chon thu muc project vua giai nen.

   Chon thu muc co cac folder nay:

   ```text
   Assets
   Packages
   ProjectSettings
   ```

4. Bam **Open**.
5. Lan dau mo project se lau vi Unity phai tao lai folder `Library`.
6. Cho Unity import xong het asset va package.

Khong can folder `Library`, `Temp`, `Logs`, `UserSettings` trong GitHub. Unity se tu tao lai.

## Cach chay game trong Unity

1. Trong Unity, mo cua so **Project**.
2. Vao:

   ```text
   Assets/Scenes
   ```

3. Mo scene:

   ```text
   main menu.unity
   ```

4. Bam nut **Play** o tren cung Unity.

Scene build hien tai gom:

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

## Neu bi loi khi mo project

### Unity bao sai version

Hay cai dung Unity:

```text
6000.3.9f1
```

Neu dung ban khac, Unity co the nang cap project va lam thay doi file setting.

### Mo project bi mat hinh, mat sprite, mat prefab

Kiem tra khi tai ve co day du file `.meta` khong. Khong duoc xoa file `.meta` trong `Assets`.

### Unity import rat lau

Binh thuong trong lan mo dau. Unity dang tao lai `Library`.

### Bao loi package

Thu cac buoc:

1. Dong Unity.
2. Mo lai project bang Unity Hub.
3. Dam bao may co internet.
4. Neu van loi, xoa folder `Library` trong project roi mo lai.

## Cach build ra file cho nguoi khac choi

1. Mo Unity.
2. Vao **File > Build Profiles** hoac **File > Build Settings**.
3. Chon platform **Windows**.
4. Dam bao scene dau tien la:

   ```text
   Assets/Scenes/main menu.unity
   ```

5. Bam **Build**.
6. Chon thu muc output, vi du:

   ```text
   Builds/Windows
   ```

7. Sau khi build xong, nen nen ca thu muc build thanh `.zip` roi gui cho nguoi khac.

Nguoi choi chi can chay file `.exe`, khong can cai Unity.

## Ghi chu cho nguoi clone bang Git

Neu dung Git thay vi Download ZIP:

```bash
git clone https://github.com/phongnk123nk/escape-the-humg.git
```

Sau do mo folder clone bang Unity Hub.
