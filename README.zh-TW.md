<p align="center">
  <img src="assets/DeskBound-logo.png" width="104" alt="桌伴 Logo">
</p>

<h1 align="center">桌伴 DeskBound</h1>

<p align="center">替 Windows 桌面上的一切，留一個安靜又好用的位置。</p>

<p align="center">
  <a href="https://github.com/bestdrduck/DeskBound/releases/latest"><img alt="最新版本" src="https://img.shields.io/github/v/release/bestdrduck/DeskBound?display_name=tag&style=flat-square&color=6967e8"></a>
  <img alt="Windows 10 與 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-1777c7?style=flat-square">
  <img alt="WPF" src="https://img.shields.io/badge/UI-WPF-5d60d6?style=flat-square">
</p>

<p align="center">
  <a href="README.md">English</a> · <strong>繁體中文</strong> ·
  <a href="https://github.com/bestdrduck/DeskBound/releases/latest"><strong>下載安裝檔</strong></a>
</p>

<p align="center">
  <img src="assets/screenshots/control-center.png" width="920" alt="桌伴控制中心">
</p>

桌伴是一款輕量的 Windows 10/11 桌面整理工具。它能把檔案、資料夾與捷徑收進可移動、可分頁的桌面區塊，同時讓每個項目保持為真正的 Windows 檔案。

## 為真正使用的桌面而設計

| 自然整理 | 保持掌控 | 符合你的桌面 |
| --- | --- | --- |
| 把項目拖入圍欄、使用分頁，或讓桌面收件匣接住新檔案。 | 隨時把項目拖回桌面；移除圍欄不會刪除其中檔案。 | 依桌布調整樣式、透明度、圖示比例、版面與操作方式。 |

## 主要功能

- 可移動、縮放、收合並支援多分頁的桌面圍欄
- 自由拖入與移出項目，以及可選用的桌面收件匣
- 搜尋、排序、縮圖、多選、智慧整理與復原
- 版面快照、情境配置、自訂外觀與 Wallpaper Engine 支援
- 內建以安裝程式升級並檢查檔案完整性的更新功能
- 等等……

<p align="center">
  <img src="assets/screenshots/desktop-inbox.png" width="360" alt="桌面收件匣">
  &nbsp;&nbsp;
  <img src="assets/screenshots/updates-and-help.png" width="560" alt="使用說明與軟體更新">
</p>

## 安裝與更新

請從 [GitHub Releases](https://github.com/bestdrduck/DeskBound/releases/latest) 下載 `DeskBound-Setup.exe`。第一次安裝可以自訂位置，日後升級會沿用同一位置。首次啟動保持空白，也不會擅自移動原本的桌面項目。

桌伴目前尚未加入程式碼簽章，因此第一次安裝時 Windows SmartScreen 可能會顯示提醒。

從 0.14.0 起，桌伴會下載正式安裝檔，在 GitHub 提供 SHA-256 摘要時先核對內容，再交由 Setup 完成升級。使用舊版 0.13 免安裝檔的使用者，需要先手動執行一次 Setup，之後便能使用新的更新流程。

## 資料獨立保存

設定、版面資料、備份與快照不會放在程式安裝資料夾，因此更新或重新安裝不會將它們移走。帶版本的設定格式會在未來需要時自動遷移；遇到同名檔案也不會直接覆蓋。

## 建置

```powershell
.\build.ps1
```

本機主程式輸出位於 `outputs\桌伴.exe`；GitHub Releases 只發布正式 Setup 安裝檔。
