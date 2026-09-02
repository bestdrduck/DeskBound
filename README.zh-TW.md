<p align="center">
  <img src="assets/DeskBound-logo.png" width="104" alt="桌伴 Logo">
</p>

<h1 align="center">桌伴 DeskBound</h1>

<p align="center">讓桌面保持整齊，也保留你原本的使用方式。</p>

<p align="center">
  <a href="README.md">English</a> · <strong>繁體中文</strong> ·
  <a href="https://github.com/bestdrduck/DeskBound/releases/latest">下載</a>
</p>

桌伴是一款輕量的 Windows 10/11 桌面整理工具。它能把檔案、資料夾與捷徑收進可移動、可分頁的桌面區塊，同時讓每個項目保持為真正的 Windows 檔案。

## 下載

請從 [GitHub Releases](https://github.com/bestdrduck/DeskBound/releases/latest) 下載 `DeskBound.exe`。第一次啟動會保持空白，不會自動搬動原本的桌面項目。

## 主要功能

- 可移動、縮放、收合並支援多分頁的桌面區塊
- 自由拖入與移出項目，或使用桌面收件匣收納新項目
- 資料夾瀏覽、搜尋、排序、縮圖與多選
- 智慧整理、版面快照、情境配置與移動復原
- 自訂外觀樣式並支援 Wallpaper Engine
- 0.13.0 起支援 SHA-256 驗證的安全更新
- 等等……

## 資料安全

移除區塊或分頁不會刪除檔案；遇到同名項目不會直接覆蓋，版面設定會保留備份，更新失敗也會還原上一版程式。

## 建置

```powershell
.\build.ps1
```

本機建置輸出位於 `outputs\桌伴.exe`。
