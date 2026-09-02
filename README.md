<p align="center">
  <img src="assets/DeskBound-logo.png" width="104" alt="DeskBound logo">
</p>

<h1 align="center">桌伴 DeskBound</h1>

<p align="center">
  讓桌面保持整齊，也保留你原本的使用方式。<br>
  <em>Keep your desktop organized without changing the way you work.</em>
</p>

<p align="center">
  <a href="#繁體中文">繁體中文</a> · <a href="#english">English</a> ·
  <a href="https://github.com/bestdrduck/DeskBound/releases/latest">Download</a>
</p>

> 桌伴是獨立開發的 Windows 桌面整理工具。本專案與 Stardock Corporation 或 Fences® 無關，亦未獲其贊助或認可。

## 繁體中文

桌伴是一款輕量的 Windows 10/11 桌面整理工具。它能把檔案、資料夾與捷徑收進可移動、可分頁的半透明圍欄，並支援 Wallpaper Engine 等動態桌布。程式使用 Windows 內建的 .NET Framework 與 WPF，不依賴 Electron。

### 下載

一般使用者請到 [GitHub Releases](https://github.com/bestdrduck/DeskBound/releases/latest) 下載最新版 `DeskBound.exe`。第一次啟動不會自動搬動任何桌面檔案。

### 0.13.0 功能

- 啟動時會在背景檢查 GitHub Releases；最多每六小時檢查一次，可在「使用說明」關閉
- 發現新版後可從通知、系統匣或控制中心一鍵下載、驗證 SHA-256、自動替換並重新啟動
- 更新失敗時保留上一版程式，圍欄設定與實際檔案不會被更新程序移動

- 桌伴應用程式 Logo，套用到程式、控制中心、系統匣與安裝檔
- 控制中心新增「說明」分頁，集中介紹分頁、快速查看、快捷鍵、Wallpaper Engine 與資料安全
- 圖示優先從 Windows Shell 取得 256×256 版本，放大時更清楚；舊程式沒有高解析資源時仍會安全回退

- 一個圍欄可建立多個分頁；支援切換、重新命名、排序、資料夾入口與直接拖放到指定分頁
- 舊版圍欄會自動成為第一個分頁，不搬動既有檔案
- 智慧整理可先預覽分類數量，再把桌面檔案整理到圖片、文件、壓縮檔、安裝程式、影音與捷徑分頁
- 自動整理新項目預設關閉；資料夾、隱藏項目與無法判斷的類型不會被移動
- 可建立及還原版面快照；還原前會再自動備份，快照不複製或移動檔案
- `Ctrl + Alt + P` 可快速查看圍欄，把它們暫時顯示在其他視窗上方
- 圍欄靠近螢幕或其他圍欄時會磁吸；按住 `Alt` 拖曳可暫時停用
- 智慧收合可在滑鼠移開後縮成標題列，移回立即展開
- 動態桌布模式會移除昂貴陰影並提高邊框辨識度，適合 Wallpaper Engine
- 圖片、影片、PDF 與 Office 文件會優先使用 Windows 原生縮圖

- 空白首次啟動，不會自動加入範例檔案或資料夾
- 首次啟動顯示「桌伴控制中心」全域 Menu
- 新增空白圍欄或資料夾圍欄
- 全域顯示、隱藏、自動排列及清除全部
- 控制中心列出所有圍欄，可快速顯示或刪除
- 控制中心提供獨立的「外觀與排列」頁面，可選擇圍欄並查看即時預覽
- 外觀頁提供樣式縮圖、七種強調色、20–100% 即時透明度滑桿、圖示大小與排列方式
- 新增圓角覆蓋與陰影強度設定，修改後同步套用到桌面圍欄
- 可在外觀頁顯示或隱藏圍欄名稱；項目數量、搜尋與收合仍會保留
- 透明度百分比直接對應圍欄表面 alpha；強調框線、框線亮度與標題底色會同步回應
- 半透明圓角桌面圍欄
- 圍欄內項目支援單擊選取、`Ctrl + 點擊`多選，以及點空白處取消選取；選取時顯示高對比外框與勾選徽章
- 四種完整圍欄樣式：晶透玻璃、經典柵欄、柔霧面板、強調框線
- 圍欄拖曳、縮放、收合、強調色及透明度預設
- 顯示任意資料夾內容，支援檔案系統即時更新
- 從桌面拖入空白圍欄時，項目會實際移入 `文件\DeskBound Fences`，桌面圖示會單獨消失
- 可把圍欄項目拖到桌面或檔案總管移出，也可用右鍵「移回桌面」
- 提供「復原上次移動」；同名項目自動安全重新命名，絕不直接覆蓋
- 刪除圍欄只解除版面，不會刪除圍欄資料夾或其中檔案
- 內容超出圍欄高度時自動顯示垂直滾動條
- 深色強調色滾動條，支援滑鼠滾輪
- 每個圍欄可用 `Ctrl + F` 即時搜尋；`Esc` 關閉搜尋
- 可依名稱、最近修改或檔案類型排序
- 圖示大小可切換小、中、大
- 可鎖定圍欄的位置與大小，避免誤拖
- `Ctrl + A` 選取目前顯示的全部項目，`Enter` 開啟單一選取項目
- 復原紀錄會跨重新啟動保留，`Ctrl + Z` 可快速復原
- 版面採原子儲存並保留上一版備份，設定損壞時可自動回復
- 桌面與文件位於不同磁碟時，先完整複製成功才移除來源
- 控制中心或系統匣可選擇「隨 Windows 自動啟動」，預設關閉
- 儲存位置、大小、顏色、透明度及資料來源
- 系統匣選單與 `Ctrl + Alt + Space` 快速隱藏／顯示
- 每個圍欄都是獨立桌面子視窗，不建立全螢幕覆蓋層，降低 Wallpaper Engine 誤判與效能負擔
- 圍欄維持相容透明 WPF 的頂層視窗，並設為 Windows 桌面宿主的擁有視窗；以工具視窗樣式排除 Alt+Tab，按 Win+D／右下角顯示桌面後會留在桌面層

### 建置

在 Windows PowerShell 執行：

```powershell
.\build.ps1
```

輸出位於 `outputs\桌伴.exe`。為相容舊版，版面設定仍儲存在 `%LOCALAPPDATA%\DeskBound\layout.json`。

### 操作

- 拖曳標題列：移動圍欄
- 拖曳右下角：調整大小
- 雙擊標題列：收合或展開
- 圍欄標題列提供新增分頁、搜尋與收合；完整設定仍集中在控制中心
- 控制中心的圍欄列可開啟「外觀」或「管理」；空白處右鍵不會跳出設定，項目右鍵只顯示檔案操作
- 單擊項目：選取；按住 `Ctrl` 點擊：加入或移出多選；點擊圍欄空白處：取消選取
- 把桌面檔案或資料夾拖入圍欄：實際移入圍欄資料夾，原桌面圖示隨即消失
- 把圍欄項目拖到桌面／檔案總管：實際移出；也可右鍵選擇「移回桌面」
- 控制中心 → 管理 → 復原上次移動：把上一批項目送回原位置
- 控制中心 → 管理 → 改用現有資料夾：讓圍欄直接管理你指定的資料夾
- 搜尋按鈕或 `Ctrl + F`：搜尋目前圍欄；控制中心可調整排序、圖示大小及位置鎖定
- 系統匣雙擊：開啟桌伴控制中心
- `Ctrl + Alt + Space`：隱藏／顯示全部圍欄
- `Ctrl + Alt + P`：切換快速查看圍欄

## English

DeskBound is a lightweight desktop organizer for Windows 10 and Windows 11. It groups files, folders, and shortcuts into movable, tabbed desktop fences while keeping the underlying files real and accessible. Its transparent WPF windows are designed to work alongside animated wallpapers, including Wallpaper Engine, without creating a full-screen overlay.

### Highlights

- Movable, resizable, collapsible desktop fences with multiple tabs
- Drag desktop items into a fence and move them back out at any time
- Folder-backed fences, in-fence folder navigation, search, sorting, and multi-selection
- Desktop Inbox for automatically collecting newly created desktop items
- Smart organization rules, layout snapshots, scenes, and persistent undo history
- Four visual styles, adjustable opacity, accent colors, icon sizes, and title visibility
- Wallpaper Engine optimization and Windows desktop-layer integration
- Background update checks with one-click, SHA-256-verified self-updates
- No sample files are added and no desktop items are moved on first launch

### Download and updates

Download the latest portable build from [GitHub Releases](https://github.com/bestdrduck/DeskBound/releases/latest). Starting with version 0.13.0, DeskBound checks for updates in the background and can safely replace and restart itself after confirmation.

### Data safety

Removing a fence or tab removes only its layout entry—it does not delete your files. File moves avoid overwriting name collisions, layout data keeps a backup, and failed application updates restore the previous executable. Existing settings are stored under `%LOCALAPPDATA%\DeskBound` for backward compatibility.

### Build from source

Run the following command in Windows PowerShell:

```powershell
.\build.ps1
```

The local build is written to `outputs\桌伴.exe`.

> DeskBound is an independent project and is not affiliated with, sponsored by, or endorsed by Stardock Corporation or Fences®.
