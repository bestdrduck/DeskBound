﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Drawing = System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Shapes = System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;

[assembly: AssemblyTitle("桌伴")]
[assembly: AssemblyProduct("桌伴")]
[assembly: AssemblyDescription("輕量、漂亮且支援動態桌布的 Windows 桌面圍欄")]
[assembly: AssemblyVersion("0.15.7.0")]
[assembly: AssemblyFileVersion("0.15.7.0")]

namespace DeskBound
{
    internal static class AppBrand
    {
        private static ImageSource logo;
        private static readonly Dictionary<string, ImageSource> images = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        public static ImageSource Logo
        {
            get
            {
                if (logo != null) return logo;
                try
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DeskBound.logo.png"))
                    {
                        if (stream == null) return null;
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                        logo = image;
                    }
                }
                catch { logo = null; }
                return logo;
            }
        }

        public static ImageSource EmbeddedImage(string resourceName)
        {
            ImageSource cached;
            if (images.TryGetValue(resourceName, out cached)) return cached;
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DeskBound." + resourceName))
                {
                    if (stream == null) return null;
                    BitmapImage image = new BitmapImage();
                    image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze();
                    images[resourceName] = image;
                    return image;
                }
            }
            catch { return null; }
        }
    }

    internal static class I18n
    {
        private static bool english;
        private static string option = "System";
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "桌伴", "DeskBound" },
            { "桌伴控制中心", "DeskBound Control Center" },
            { "桌面圍欄控制中心", "Desktop panel control center" },
            { "工作區", "Workspace" },
            { "圍欄管理", "Panel management" },
            { "外觀與排列", "Appearance" },
            { "使用說明", "Help" },
            { "說明", "Help" },
            { "系統狀態", "System status" },
            { "●  桌伴正在執行", "●  DeskBound is running" },
            { "●  運作正常", "●  Running normally" },
            { "你的桌面，現在井然有序。", "Your desktop, finally in order." },
            { "從這裡建立圍欄、快速整理桌面，或調整整套外觀。", "Create panels, organize quickly, and shape the whole desktop from here." },
            { "目前版面", "Current layout" },
            { "建立圍欄", "Create panels" },
            { "新增一個空白空間，或直接連結現有資料夾", "Start with an empty space or link an existing folder" },
            { "＋  新增空白圍欄", "+  New empty panel" },
            { "▣  新增資料夾圍欄", "▣  New folder panel" },
            { "建立乾淨區域，拖入後安全收納", "Create a clean space and safely collect dropped items" },
            { "直接查看指定資料夾，不搬動內容", "View a folder directly without moving its contents" },
            { "快速功能", "Quick actions" },
            { "常用操作集中在這裡，需要時再展開更多設定", "Common actions stay close, with more settings when you need them" },
            { "顯示與尋找", "Show & find" },
            { "控制圍欄可見狀態，快速找到內容", "Control visibility and quickly locate content" },
            { "顯示全部", "Show all" },
            { "隱藏全部", "Hide all" },
            { "快速查看", "Quick Peek" },
            { "搜尋全部", "Search all" },
            { "版面與快照", "Layouts & snapshots" },
            { "排列、保存或切換整套桌面配置", "Arrange, save, or switch the complete desktop layout" },
            { "情境配置", "Scenes" },
            { "自動排列", "Auto arrange" },
            { "建立快照", "Create snapshot" },
            { "還原快照", "Restore snapshot" },
            { "智慧工具", "Smart tools" },
            { "自動分類與桌面新項目收納", "Automatic sorting and collection of new desktop items" },
            { "智慧整理", "Smart organize" },
            { "分類規則", "Classification rules" },
            { "桌面收件匣", "Desktop Inbox" },
            { "圍欄資料夾", "Panel folders" },
            { "內容與復原", "Content & recovery" },
            { "查看搬移紀錄，處理不再需要的版面", "Review moves and clean up layouts you no longer need" },
            { "復原紀錄", "Move history" },
            { "清除全部", "Clear all" },
            { "自動化", "Automation" },
            { "控制啟動方式與桌面新項目的去向", "Control startup and where new desktop items go" },
            { "登入 Windows 時自動啟動桌伴", "Start DeskBound when I sign in to Windows" },
            { "高優先啟動（登入後立即啟動）", "Early startup (immediately after sign-in)" },
            { "使用登入排程提早啟動，不提高 CPU 或管理員權限。", "Start through a logon task without elevating CPU priority or administrator privileges." },
            { "無法設定登入排程；原有開機啟動方式已保留。", "The logon task could not be configured. The previous startup method has been kept." },
            { "依檔案類型自動整理桌面新項目", "Automatically organize new desktop items by file type" },
            { "將桌面新出現的項目自動收入「桌面收件匣」", "Automatically collect new desktop items in Desktop Inbox" },
            { "目前的圍欄", "Your panels" },
            { "管理顯示、內容資料夾與個別外觀", "Manage visibility, content folders, and individual appearance" },
            { "讓每個圍欄都像你的桌面。", "Make every panel feel at home." },
            { "需要時，再回來這裡。", "Come back whenever you need it." },
            { "整理桌面，也整理思緒。", "Organize your desktop and your thoughts." },
            { "桌面控制中心", "Desktop control center" },
            { "設計你的桌面圍欄", "Design your desktop panels" },
            { "選擇圍欄後直接調整；預覽與桌面上的圍欄會同步更新。", "Choose a panel and adjust it directly; the preview and desktop update together." },
            { "介面語言", "Interface language" },
            { "跟隨 Windows 或選擇顯示語言；切換後會重新啟動桌伴", "Follow Windows or choose a display language; DeskBound restarts after switching" },
            { "跟隨系統", "Follow system" },
            { "繁體中文", "Traditional Chinese" },
            { "正在編輯", "Editing" },
            { "即時預覽", "Live preview" },
            { "顯示目前圍欄的配色、透明度、圓角與陰影", "Preview the current panel's color, opacity, corners, and shadow" },
            { "圍欄樣式", "Panel style" },
            { "四種不同材質與邊框表現", "Four distinct materials and border treatments" },
            { "晶透玻璃", "Clear glass" },
            { "輕盈通透", "Light and transparent" },
            { "經典柵欄", "Classic fence" },
            { "深色實用", "Dark and practical" },
            { "柔霧面板", "Soft frost" },
            { "柔和厚實", "Soft and substantial" },
            { "強調框線", "Accent outline" },
            { "清楚醒目", "Clear and vivid" },
            { "強調色", "Accent color" },
            { "套用到外框、選取狀態與滾動條", "Used for borders, selection, and scrollbars" },
            { "系統", "System" },
            { "藍紫", "Indigo" },
            { "青綠", "Teal" },
            { "暖橘", "Warm orange" },
            { "玫紅", "Rose" },
            { "天藍", "Sky blue" },
            { "紫晶", "Amethyst" },
            { "透明度", "Opacity" },
            { "拖曳時會同步更新預覽與桌面圍欄", "Updates the preview and desktop panel while you drag" },
            { "標題列", "Title bar" },
            { "可關閉圍欄名稱，保留項目數量與搜尋功能", "Hide the panel name while keeping item count and search" },
            { "顯示圍欄名稱", "Show panel name" },
            { "隱藏圍欄名稱", "Hide panel name" },
            { "圖示大小", "Icon size" },
            { "調整圍欄內容的密度", "Adjust the density of panel contents" },
            { "檢視方式", "View mode" },
            { "圖示格狀", "Icon grid" },
            { "精簡清單", "Compact list" },
            { "檔案", "File" },
            { "大量項目可改用清單；每個分頁會記住自己的選擇", "Use a list for larger collections; every tab remembers its own choice" },
            { "小", "Small" },
            { "中", "Medium" },
            { "大", "Large" },
            { "排列方式", "Sort order" },
            { "選擇圍欄內項目的預設順序", "Choose the default order of panel items" },
            { "名稱", "Name" },
            { "最近修改", "Recently modified" },
            { "檔案類型", "File type" },
            { "圓角", "Corner radius" },
            { "可覆蓋樣式原本的圓角尺寸", "Override the style's default corner radius" },
            { "樣式預設", "Style default" },
            { "俐落 8", "Sharp 8" },
            { "平衡 14", "Balanced 14" },
            { "圓潤 22", "Round 22" },
            { "陰影", "Shadow" },
            { "控制圍欄與桌布之間的立體層次", "Control depth between the panel and wallpaper" },
            { "關閉", "Off" },
            { "加強", "Strong" },
            { "智慧收合", "Smart collapse" },
            { "滑鼠移開後自動縮成標題列，移回立即展開", "Collapse to the title bar when the pointer leaves and expand on return" },
            { "手動收合", "Manual" },
            { "滑鼠感應", "Pointer aware" },
            { "動態桌布模式", "Animated wallpaper mode" },
            { "針對 Wallpaper Engine 降低昂貴陰影並保持邊框清楚", "Reduce expensive shadows while keeping borders clear with Wallpaper Engine" },
            { "一般模式", "Normal mode" },
            { "動態桌布最佳化", "Optimize for animated wallpaper" },
            { "桌伴使用說明", "DeskBound help" },
            { "把桌面內容整理進可移動、可分頁的圍欄，同時保留真正的檔案與資料夾。", "Organize desktop content into movable, tabbed panels while keeping real files and folders." },
            { "1. 在「圍欄管理」新增空白圍欄或資料夾圍欄。\n2. 把桌面檔案拖入空白圍欄，檔案會安全移到文件中的專用收納資料夾。\n3. 拖出圍欄即可移回桌面或其他檔案總管資料夾。", "1. Open Panel management and create an empty or folder panel.\n2. Drag desktop files into an empty panel; they move safely to its dedicated folder in Documents.\n3. Drag an item out to move it back to the desktop or another File Explorer folder." },
            { "按圍欄標題列的 ＋ 新增分頁。檔案也可以直接拖到指定分頁標籤；在分頁上按右鍵可重新命名、改用資料夾、排序或移除。移除分頁不會刪除檔案。", "Use + in the panel title bar to add a tab. You can drag files directly onto a tab; right-click it to rename, link a folder, reorder, or remove it. Removing a tab never deletes files." },
            { "按 Ctrl + Alt + P，圍欄會暫時顯示在其他程式上方，方便快速開啟或拖放檔案；再按一次就回到正常桌面層。", "Press Ctrl + Alt + P to temporarily show panels above other apps for quick access and drag-and-drop. Press it again to return them to the desktop layer." },
            { "Ctrl + F 搜尋目前分頁；Menu 的「搜尋所有圍欄」可跨圍欄搜尋並直接定位。Ctrl + 點擊可多選；Ctrl + A 選取目前顯示的項目；Ctrl + Z 復原上一批移動。", "Ctrl + F searches the current tab. Search all panels locates items across panels. Ctrl + click selects multiple items, Ctrl + A selects visible items, and Ctrl + Z restores the last move." },
            { "雙擊資料夾會直接在圍欄內開啟，標題列會顯示返回按鈕；拖入檔案時會放到目前正在瀏覽的子資料夾。", "Double-click a folder to browse it inside the panel. The title bar shows a Back button, and dropped files go into the folder currently being viewed." },
            { "啟用後會近乎即時監看之後新出現在桌面的項目；啟用前已有的桌面內容不會突然被移動。可直接在收件匣標題列切換「監看中／已暫停」，新項目會安全移入文件中的專用收納資料夾。", "When enabled, Desktop Inbox watches for new desktop items almost instantly. Existing items are left alone. Pause or resume it from the title bar; collected items move safely to a dedicated folder in Documents." },
            { "情境配置可保存並切換整套圍欄版面；智慧分類規則可自訂副檔名與檔名關鍵字；復原紀錄最多保留最近 40 次搬移。", "Scenes save and switch complete panel layouts. Classification rules support custom extensions and filename keywords, while move history keeps the 40 latest operations." },
            { "Win + D 或右下角顯示桌面後，圍欄應留在桌面層。Ctrl + Alt + Space 可隱藏或顯示全部。建立版面快照只保存位置與設定，不會複製或移動檔案。", "Panels remain on the desktop after Win + D or Show Desktop. Ctrl + Alt + Space hides or shows all panels. Layout snapshots save positions and settings without copying or moving files." },
            { "桌伴使用獨立透明桌面視窗，不建立全螢幕遮罩。若動態桌布效能較吃緊，可到「外觀與排列」開啟動態桌布最佳化。", "DeskBound uses independent transparent desktop windows rather than a full-screen overlay. If an animated wallpaper is demanding, enable animated wallpaper optimization under Appearance." },
            { "刪除圍欄或分頁只會移除版面入口，不會刪除資料。發生同名檔案時會自動改名而不是覆蓋；版面設定也會保留上一版備份。", "Deleting a panel or tab only removes its layout entry; it never deletes data. Name collisions are renamed instead of overwritten, and the previous layout is kept as a backup." },
            { "軟體更新", "Software updates" },
            { "自動檢查更新", "Automatically check for updates" },
            { "啟動時及執行期間定期檢查；找到新版後會先詢問，不會強制安裝。", "Check at startup and periodically while running. DeskBound asks before installing an update." },
            { "檢查更新", "Check for updates" },
            { "開始使用", "Getting started" },
            { "圍欄分頁", "Panel tabs" },
            { "快速查看圍欄", "Quick Peek" },
            { "搜尋、選取與滾動", "Search, selection, and scrolling" },
            { "圍欄內瀏覽資料夾", "Browse folders inside a panel" },
            { "情境、規則與復原紀錄", "Scenes, rules, and move history" },
            { "版面與顯示", "Layout and visibility" },
            { "資料安全", "Data safety" },
            { "新增空白圍欄", "New empty panel" },
            { "新增資料夾圍欄…", "New folder panel…" },
            { "開啟圍欄資料夾", "Open panel folder" },
            { "顯示／隱藏", "Show / hide" },
            { "智慧整理桌面…", "Smart organize desktop…" },
            { "智慧分類規則…", "Classification rules…" },
            { "重新載入桌面層", "Reload desktop layer" },
            { "隨 Windows 自動啟動", "Start with Windows" },
            { "檢查更新…", "Check for updates…" },
            { "結束桌伴", "Exit DeskBound" },
            { "桌伴保留了原本資料", "DeskBound kept your existing data" },
            { "版面設定暫時無法讀取，因此桌伴沒有建立或覆寫新的空白設定。", "The layout could not be read, so DeskBound did not create or overwrite it with a blank layout." },
            { "已復原原本的桌伴版面", "Your DeskBound layout was restored" },
            { "已從版面備份還原。", "The layout was restored from its backup." },
            { "找到既有的圍欄資料夾，已重新接回版面。", "Existing panel folders were found and reconnected to the layout." },
            { "偏好設定暫時無法讀取，因此桌伴不會用預設值覆寫原檔。", "Preferences could not be read, so DeskBound will not overwrite them with defaults." },
            { "已復原原本的桌伴設定", "Your DeskBound preferences were restored" },
            { "已從偏好設定備份還原。", "Preferences were restored from their backup." },
            { "搜尋所有圍欄", "Search all panels" },
            { "搜尋所有圍欄…", "Search all panels…" },
            { "移動與復原紀錄", "Move & undo history" },
            { "移動與復原紀錄…", "Move & undo history…" },
            { "選取一筆紀錄即可安全搬回原處；同名檔案不會被覆蓋", "Select a move to safely restore it; existing files are never overwritten" },
            { "情境配置…", "Scenes…" },
            { "保存並快速切換工作、遊戲或學習版面", "Save and quickly switch work, gaming, or study layouts" },
            { "先比對檔名關鍵字，再依副檔名分類；使用逗號分隔多個條件", "Filename keywords are matched first, then extensions; separate multiple rules with commas" },
            { "開啟桌面收件匣", "Open Desktop Inbox" },
            { "監看桌面新項目", "Watch for new desktop items" },
            { "重新命名", "Rename" },
            { "重新命名分頁", "Rename tab" },
            { "改用現有資料夾…", "Use existing folder…" },
            { "開啟分頁資料夾", "Open tab folder" },
            { "向左移", "Move left" },
            { "向右移", "Move right" },
            { "移除分頁", "Remove tab" },
            { "重新整理", "Refresh" },
            { "搜尋項目…    Ctrl+F", "Search items…    Ctrl+F" },
            { "收合／展開", "Collapse / expand" },
            { "鎖定位置與大小", "Lock position and size" },
            { "復原上次移動    Ctrl+Z", "Undo last move    Ctrl+Z" },
            { "分頁", "Tabs" },
            { "新增空白分頁", "New empty tab" },
            { "新增資料夾分頁…", "New folder tab…" },
            { "重新命名目前分頁…", "Rename current tab…" },
            { "移除目前分頁", "Remove current tab" },
            { "排序", "Sort" },
            { "名稱（資料夾優先）", "Name (folders first)" },
            { "外觀與排列設定…", "Appearance & layout…" },
            { "刪除圍欄", "Delete panel" },
            { "開啟", "Open" },
            { "在檔案總管中顯示", "Show in File Explorer" },
            { "移回桌面", "Move back to desktop" },
            { "從圍欄移除", "Remove from panel" },
            { "返回上一層資料夾", "Back to parent folder" },
            { "新增分頁", "New tab" },
            { "搜尋項目（Ctrl+F）", "Search items (Ctrl+F)" },
            { "開啟或暫停桌面收件匣監看", "Start or pause Desktop Inbox monitoring" },
            { "正在監看桌面；點一下暫停", "Watching the desktop; click to pause" },
            { "桌面監看已暫停；點一下繼續", "Desktop monitoring is paused; click to resume" },
            { "●  監看中", "●  Watching" },
            { "○  已暫停", "○  Paused" },
            { "將項目拖曳到這裡", "Drag items here" },
            { "檔案會安全移入圍欄資料夾", "Files are safely moved into the panel folder" },
            { "換個關鍵字再試試看", "Try a different search" },
            { "載入中", "Loading" },
            { "正在移回桌面…", "Moving back to desktop…" },
            { "正在復原…", "Restoring…" },
            { "正在移入…", "Moving items…" },
            { "取消", "Cancel" },
            { "確認", "Confirm" },
            { "確定", "OK" },
            { "知道了", "OK" },
            { "尚未檢查更新", "Updates have not been checked yet" },
            { "正在檢查 GitHub 最新版本…", "Checking GitHub for the latest version…" },
            { "暫時無法連線到 GitHub", "Unable to connect to GitHub right now" },
            { "桌伴有新版本", "A DeskBound update is available" },
            { "已推出，點一下即可更新。", " is available. Click to update." },
            { "有新版本 ", "Version " },
            { " 可安裝", " is ready to install" },
            { "已是最新版本 ", "Up to date · " },
            { "目前已是最新版本 ", "You already have the latest version, " },
            { "正在下載版本 ", "Downloading version " },
            { "安裝更新", "Install update" },
            { "無法檢查更新", "Unable to check for updates" },
            { "無法下載更新", "Unable to download the update" },
            { "無法安裝更新", "Unable to install the update" },
            { "下載更新失敗", "Update download failed" },
            { "無法啟動更新程式", "Unable to start the updater" },
            { "新圍欄", "New panel" },
            { "工作", "Work" },
            { "主要", "Main" },
            { "常用", "Favorites" },
            { "資料夾", "Folders" },
            { "遊戲", "Games" },
            { "遊戲收藏", "Game collection" },
            { "文件", "Documents" },
            { "圖片", "Images" },
            { "下載", "Downloads" },
            { "壓縮檔", "Archives" },
            { "安裝程式", "Installers" },
            { "影音", "Media" },
            { "捷徑", "Shortcuts" },
            { "桌面", "Desktop" },
            { "恢復預設", "Restore defaults" },
            { "儲存規則", "Save rules" },
            { "分類", "Category" },
            { "副檔名", "Extensions" },
            { "檔名關鍵字（可留空）", "Filename keywords (optional)" },
            { "保存目前情境", "Save current scene" },
            { "切換到選取情境", "Switch to selected scene" },
            { "刪除", "Delete" },
            { "輸入檔名、資料夾名稱或路徑", "Enter a filename, folder name, or path" },
            { "找不到符合的項目", "No matching items" },
            { "還沒有可復原的搬移紀錄", "No moves available to restore" },
            { "最多保留最近 40 次搬移", "Keeps the 40 most recent moves" },
            { "先選取一筆紀錄", "Select a move first" },
            { "復原這筆搬移", "Restore this move" },
            { "重新命名圍欄", "Rename panel" },
            { "圍欄名稱", "Panel name" },
            { "目前沒有可設定的圍欄\n請先到「圍欄管理」新增一個圍欄", "There are no panels to customize yet.\nCreate one under Panel management first." },
            { "目前是空白桌面\n按上方按鈕新增第一個圍欄", "Your desktop layout is empty.\nUse the buttons above to create your first panel." },
            { "輸入文字即可搜尋所有圍欄與分頁\n雙擊結果會切換到該位置並選取項目", "Type to search every panel and tab.\nDouble-click a result to reveal and select it." },
            { "尚未建立情境\n先將圍欄排成想要的樣子，再按「保存目前情境」", "No scenes yet.\nArrange your panels, then choose Save current scene." },
            { "無法切換語言", "Unable to switch language" },
            { "選擇要顯示在新圍欄中的資料夾", "Choose a folder to display in the new panel" },
            { "選擇新分頁要顯示的資料夾", "Choose a folder for the new tab" },
            { "選擇要顯示在圍欄中的資料夾", "Choose a folder to display in the panel" },
            { "輸入檔名篩選", "Filter by filename" },
            { "桌伴已啟動", "DeskBound is running" },
            { "從系統匣開啟控制中心，或按 Ctrl + Alt + Space 顯示／隱藏圍欄。", "Open Control Center from the system tray, or press Ctrl + Alt + Space to show or hide panels." }
        };

        public static bool IsEnglish { get { return english; } }
        public static string Option { get { return option; } }

        public static void Configure(string value)
        {
            option = string.IsNullOrWhiteSpace(value) ? "System" : value;
            english = string.Equals(option, "en-US", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(option, "System", StringComparison.OrdinalIgnoreCase) &&
                 !CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase));
        }

        public static string T(string value)
        {
            if (!english || string.IsNullOrEmpty(value)) return value;
            if (value.StartsWith("要安裝桌伴 ", StringComparison.Ordinal))
            {
                int marker = value.IndexOf(" 嗎？", StringComparison.Ordinal);
                string version = marker > 6 ? value.Substring(6, marker - 6) : "the latest version";
                return "Install DeskBound " + version + "?\n\nDeskBound will download the official Setup installer from GitHub Releases, verify its integrity, and keep the current installation location. Your panels and files will not be moved.";
            }
            if (value.StartsWith("桌伴已更新完成。", StringComparison.Ordinal))
                return value.Replace("桌伴已更新完成。", "DeskBound was updated successfully.").Replace("目前版本：", "Current version: ");
            if (value.StartsWith("更新沒有完成，桌伴已保留原本版本。", StringComparison.Ordinal))
                return value.Replace("更新沒有完成，桌伴已保留原本版本。", "The update did not finish. DeskBound kept the previous version.");
            string translated;
            if (En.TryGetValue(value, out translated)) return translated;
            translated = value;
            foreach (KeyValuePair<string, string> pair in En.OrderByDescending(pair => pair.Key.Length))
                translated = translated.Replace(pair.Key, pair.Value);
            translated = translated.Replace(" 個圍欄", " panels").Replace(" 個項目", " items").Replace(" 個已選取", " selected");
            translated = translated.Replace("版本 ", "Version ").Replace("安裝 ", "Install ");
            return translated;
        }

        public static string DashboardDate(DateTime value)
        {
            return english ? value.ToString("MMM d, dddd", CultureInfo.GetCultureInfo("en-US")) : value.ToString("M月d日 dddd");
        }

        public static void Apply(DependencyObject root)
        {
            Apply(root, new HashSet<DependencyObject>());
        }

        private static void Apply(DependencyObject node, HashSet<DependencyObject> visited)
        {
            if (!english || node == null || !visited.Add(node)) return;
            FrameworkElement framework = node as FrameworkElement;
            if (framework != null && string.Equals(framework.Tag as string, "i18n-skip", StringComparison.Ordinal)) return;
            Window window = node as Window;
            if (window != null) window.Title = T(window.Title);
            TextBlock text = node as TextBlock;
            if (text != null) text.Text = T(text.Text);
            ContentControl content = node as ContentControl;
            if (content != null && content.Content is string) content.Content = T((string)content.Content);
            HeaderedItemsControl header = node as HeaderedItemsControl;
            if (header != null && header.Header is string) header.Header = T((string)header.Header);
            if (framework != null && framework.ToolTip is string) framework.ToolTip = T((string)framework.ToolTip);
            foreach (object child in LogicalTreeHelper.GetChildren(node))
            {
                DependencyObject dependency = child as DependencyObject;
                if (dependency != null) Apply(dependency, visited);
            }
            ItemsControl items = node as ItemsControl;
            if (items != null)
                foreach (object item in items.Items)
                {
                    DependencyObject dependency = item as DependencyObject;
                    if (dependency != null) Apply(dependency, visited);
                }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args != null && (args.Contains("--repair-startup") || args.Contains("--remove-startup")))
            {
                try
                {
                    if (args.Contains("--remove-startup")) StartupManager.RemoveForCurrentExecutable();
                    else StartupManager.RepairForCurrentExecutable();
                }
                catch { Environment.ExitCode = 1; }
                return;
            }
            if (args != null && args.Length > 1 && string.Equals(args[0], "--restart-after", StringComparison.OrdinalIgnoreCase))
            {
                int previousId;
                if (int.TryParse(args[1], out previousId))
                {
                    try
                    {
                        Process previous = Process.GetProcessById(previousId);
                        if (!previous.HasExited) previous.WaitForExit(15000);
                    }
                    catch (ArgumentException) { }
                }
            }

            if (args != null && args.Length > 0 && string.Equals(args[0], "--apply-update", StringComparison.OrdinalIgnoreCase))
            {
                Environment.ExitCode = UpdateInstaller.Apply(args);
                return;
            }

            if (args != null && args.Any(a => string.Equals(a, "--storage-self-test", StringComparison.OrdinalIgnoreCase)))
            {
                Environment.ExitCode = ManagedStorage.RunSelfTest();
                return;
            }

            bool created;
            using (Mutex mutex = new Mutex(true, "DeskBound.DesktopFences.Singleton", out created))
            {
                if (!created)
                {
                    AppDialog.Show("桌伴已經在執行。", "桌伴", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                NativeMethods.EnableBestDpiMode();
                Application app = new Application();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                UiStyles.InstallApplicationTheme(app);
                bool preview = args != null && args.Any(a => string.Equals(a, "--preview", StringComparison.OrdinalIgnoreCase));
                bool diagnostics = args != null && args.Any(a => string.Equals(a, "--diagnostics", StringComparison.OrdinalIgnoreCase));
                bool openCenter = args != null && args.Any(a => string.Equals(a, "--control-center", StringComparison.OrdinalIgnoreCase));
                bool captureCenter = args != null && args.Any(a => string.Equals(a, "--capture-control-center", StringComparison.OrdinalIgnoreCase));
                bool captureAppearance = args != null && args.Any(a => string.Equals(a, "--capture-appearance", StringComparison.OrdinalIgnoreCase));
                bool captureAppearanceControls = args != null && args.Any(a => string.Equals(a, "--capture-appearance-controls", StringComparison.OrdinalIgnoreCase));
                bool captureHelp = args != null && args.Any(a => string.Equals(a, "--capture-help", StringComparison.OrdinalIgnoreCase));
                bool captureInbox = args != null && args.Any(a => string.Equals(a, "--capture-inbox", StringComparison.OrdinalIgnoreCase));
                bool captureHistory = args != null && args.Any(a => string.Equals(a, "--capture-history", StringComparison.OrdinalIgnoreCase));
                bool captureFenceTabs = args != null && args.Any(a => string.Equals(a, "--capture-fence-tabs", StringComparison.OrdinalIgnoreCase));
                bool captureListView = args != null && args.Any(a => string.Equals(a, "--capture-list-view", StringComparison.OrdinalIgnoreCase));
                bool enableDesktopInbox = args != null && args.Any(a => string.Equals(a, "--enable-desktop-inbox", StringComparison.OrdinalIgnoreCase));
                DeskBoundManager manager = new DeskBoundManager(app, preview, diagnostics);
                manager.Start();
                string updateResult = UpdateInstaller.ConsumeResult();
                if (!string.IsNullOrWhiteSpace(updateResult))
                {
                    app.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        bool failed = updateResult.StartsWith("ERROR|", StringComparison.Ordinal);
                        string detail = updateResult.Contains("|") ? updateResult.Substring(updateResult.IndexOf('|') + 1) : updateResult;
                        AppDialog.Show(failed ? "更新沒有完成，桌伴已保留原本版本。\n\n" + detail : "桌伴已更新完成。\n\n目前版本：" + detail,
                            failed ? "更新未完成" : "更新完成", MessageBoxButton.OK,
                            failed ? MessageBoxImage.Warning : MessageBoxImage.Information);
                    }));
                }
                if (openCenter) app.Dispatcher.BeginInvoke(new Action(manager.ShowControlCenter));
                if (captureCenter) app.Dispatcher.BeginInvoke(new Action(manager.CaptureControlCenterAndExit));
                if (captureAppearance) app.Dispatcher.BeginInvoke(new Action(delegate { manager.CaptureControlCenterPageAndExit("Appearance", "DeskBound-appearance-preview.png"); }));
                if (captureAppearanceControls) app.Dispatcher.BeginInvoke(new Action(delegate { manager.CaptureControlCenterPageAndExit("AppearanceControls", "桌伴-appearance-controls-preview.png"); }));
                if (captureHelp) app.Dispatcher.BeginInvoke(new Action(delegate { manager.CaptureControlCenterPageAndExit("Help", "DeskBound-help-preview.png"); }));
                if (captureInbox) app.Dispatcher.BeginInvoke(new Action(manager.CaptureInboxAndExit));
                if (captureHistory) app.Dispatcher.BeginInvoke(new Action(manager.CaptureMoveHistoryAndExit));
                if (captureFenceTabs) app.Dispatcher.BeginInvoke(new Action(manager.CaptureFenceTabsAndExit));
                if (captureListView) app.Dispatcher.BeginInvoke(new Action(manager.CaptureListViewAndExit));
                if (enableDesktopInbox) app.Dispatcher.BeginInvoke(new Action(manager.ShowDesktopInbox));
                app.Run();
                manager.Dispose();
            }
        }
    }

    internal sealed class DeskBoundManager : IDisposable
    {
        private readonly Application app;
        private readonly LayoutStore store;
        private readonly List<FenceWindow> fences = new List<FenceWindow>();
        private readonly DispatcherTimer saveTimer;
        private readonly DispatcherTimer desktopTimer;
        private readonly DispatcherTimer organizerTimer;
        private readonly DispatcherTimer desktopInboxDebounceTimer;
        private readonly DispatcherTimer updateTimer;
        private readonly AppSettingsStore settingsStore;
        private readonly AppSettingsModel settings;
        private Forms.NotifyIcon tray;
        private MenuItem autoStartTrayItem;
        private ContextMenu trayMenu;
        private HotkeySink hotkey;
        private ControlCenterWindow controlCenter;
        private MoveHistoryWindow moveHistoryWindow;
        private GlobalSearchWindow globalSearchWindow;
        private SceneSwitcherWindow sceneSwitcherWindow;
        private RuleEditorWindow ruleEditorWindow;
        private UpdateRelease pendingUpdate;
        private bool checkingForUpdates;
        private bool downloadingUpdate;
        private string updateStatus = "尚未檢查更新";
        private DateTime? lastUpdateAttemptUtc;
        private bool visible = true;
        private bool peeking;
        private bool visibleBeforePeek = true;
        private bool exiting;
        private bool organizingDesktop;
        private bool collectingDesktopInbox;
        private bool allowLayoutReduction;
        private HashSet<string> desktopInboxBaseline = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private FileSystemWatcher desktopInboxWatcher;
        public bool PreviewMode { get; private set; }
        public bool DiagnosticsMode { get; private set; }

        public DeskBoundManager(Application application, bool previewMode, bool diagnosticsMode)
        {
            app = application;
            PreviewMode = previewMode;
            DiagnosticsMode = diagnosticsMode;
            store = new LayoutStore();
            settingsStore = new AppSettingsStore();
            settings = settingsStore.Load();
            string languageOverride = Environment.GetEnvironmentVariable("DESKBOUND_UI_LANGUAGE");
            I18n.Configure(string.IsNullOrWhiteSpace(languageOverride) ? settings.UiLanguage : languageOverride);
            updateStatus = I18n.T("尚未檢查更新");
            saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            saveTimer.Tick += delegate { saveTimer.Stop(); SaveNow(); };
            desktopTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            desktopTimer.Tick += delegate { ReattachDesktopWindows(); };
            organizerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            organizerTimer.Tick += delegate
            {
                if (settings.DesktopInboxEnabled) CollectDesktopInbox(false);
                else if (settings.AutoOrganizeDesktop) OrganizeDesktop(false);
            };
            desktopInboxDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(160) };
            desktopInboxDebounceTimer.Tick += delegate
            {
                desktopInboxDebounceTimer.Stop();
                CollectDesktopInbox(false);
            };
            updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(30) };
            updateTimer.Tick += delegate
            {
                if (settings.AutoCheckUpdates && ShouldAutomaticallyCheckForUpdates()) CheckForUpdates(false);
            };
        }

        public void Start()
        {
            CreateTray();
            hotkey = new HotkeySink(ToggleVisible, TogglePeek);
            List<FenceModel> models = store.Load();

            // Preview mode is an internal visual-QA surface.  It must stay useful even
            // when the real first-run layout is (correctly) empty, and it never saves.
            if (PreviewMode && models.Count == 0)
                models.Add(CreatePreviewModel());

            foreach (FenceModel model in models)
                AddFence(model, false);

            if (!PreviewMode) desktopTimer.Start();
            ResetDesktopInboxBaseline();
            ConfigureDesktopInboxWatcher();
            if (!PreviewMode && (settings.AutoOrganizeDesktop || settings.DesktopInboxEnabled)) organizerTimer.Start();
            if (!PreviewMode)
            {
                UpdateService.CleanupStaleDownloads();
                updateTimer.Start();
                if (settings.AutoCheckUpdates && ShouldAutomaticallyCheckForUpdates())
                    app.Dispatcher.BeginInvoke(new Action(delegate { CheckForUpdates(false); }));
            }
            if (!store.LoadFailed) SaveSoon();
            if (models.Count == 0)
                app.Dispatcher.BeginInvoke(new Action(ShowControlCenter));
            else
                tray.ShowBalloonTip(1800, I18n.T("桌伴已啟動"), I18n.T("從系統匣開啟控制中心，或按 Ctrl + Alt + Space 顯示／隱藏圍欄。"), Forms.ToolTipIcon.Info);
            if (store.LoadFailed)
                tray.ShowBalloonTip(4500, I18n.T("桌伴保留了原本資料"), I18n.T("版面設定暫時無法讀取，因此桌伴沒有建立或覆寫新的空白設定。"), Forms.ToolTipIcon.Warning);
            else if (store.RecoveredFromBackup || store.RecoveredFromFolders)
                tray.ShowBalloonTip(4500, I18n.T("已復原原本的桌伴版面"), I18n.T(store.RecoveredFromBackup ? "已從版面備份還原。" : "找到既有的圍欄資料夾，已重新接回版面。"), Forms.ToolTipIcon.Info);
            if (settingsStore.LoadFailed)
                tray.ShowBalloonTip(4500, I18n.T("桌伴保留了原本資料"), I18n.T("偏好設定暫時無法讀取，因此桌伴不會用預設值覆寫原檔。"), Forms.ToolTipIcon.Warning);
            else if (settingsStore.RecoveredFromBackup)
                tray.ShowBalloonTip(3500, I18n.T("已復原原本的桌伴設定"), I18n.T("已從偏好設定備份還原。"), Forms.ToolTipIcon.Info);
        }

        private void CreateTray()
        {
            tray = new Forms.NotifyIcon();
            tray.Text = I18n.T("桌伴");
            try { tray.Icon = Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location); }
            catch { tray.Icon = Drawing.SystemIcons.Application; }
            tray.Visible = true;
            trayMenu = BuildTrayMenu();
            tray.MouseUp += delegate(object sender, Forms.MouseEventArgs e)
            {
                if (e.Button != Forms.MouseButtons.Right) return;
                app.Dispatcher.BeginInvoke(new Action(delegate
                {
                    autoStartTrayItem.IsChecked = StartupManager.IsEnabled();
                    trayMenu.Placement = PlacementMode.MousePoint;
                    trayMenu.IsOpen = true;
                }));
            };
            tray.DoubleClick += delegate { ShowControlCenter(); };
            tray.BalloonTipClicked += delegate
            {
                app.Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (pendingUpdate != null) InstallPendingUpdate();
                    else ShowControlCenter();
                }));
            };
        }

        private ContextMenu BuildTrayMenu()
        {
            ContextMenu menu = new ContextMenu();
            UiStyles.PrepareDarkContextMenu(menu, AccentPalette.ReadWindowsAccent());
            menu.Items.Add(TrayMenuItem("桌伴控制中心", delegate { ShowControlCenter(); }));
            menu.Items.Add(TrayMenuItem("搜尋所有圍欄", delegate { ShowGlobalSearch(); }));
            menu.Items.Add(TrayMenuItem("移動與復原紀錄", delegate { ShowMoveHistory(); }));
            menu.Items.Add(TrayMenuItem("情境配置", delegate { ShowScenes(); }));
            menu.Items.Add(new Separator());
            menu.Items.Add(TrayMenuItem("新增空白圍欄", delegate { CreateBlankFence(); }));
            menu.Items.Add(TrayMenuItem("新增資料夾圍欄…", delegate { CreateFolderFence(); }));
            menu.Items.Add(TrayMenuItem("開啟圍欄資料夾", delegate { OpenStorageRoot(); }));
            menu.Items.Add(TrayMenuItem("顯示／隱藏", delegate { ToggleVisible(); }));
            menu.Items.Add(TrayMenuItem("快速查看圍欄", delegate { TogglePeek(); }));
            menu.Items.Add(TrayMenuItem("智慧整理桌面…", delegate { OrganizeDesktopInteractive(); }));
            menu.Items.Add(TrayMenuItem("智慧分類規則…", delegate { ShowRuleEditor(); }));
            menu.Items.Add(TrayMenuItem("桌面收件匣", delegate { ShowDesktopInbox(); }));
            menu.Items.Add(TrayMenuItem("重新載入桌面層", delegate { ReattachDesktopWindows(true); }));
            menu.Items.Add(new Separator());
            autoStartTrayItem = TrayMenuItem("隨 Windows 自動啟動", delegate { SetAutoStart(!StartupManager.IsEnabled()); });
            autoStartTrayItem.IsCheckable = true;
            autoStartTrayItem.IsChecked = StartupManager.IsEnabled();
            menu.Items.Add(autoStartTrayItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(TrayMenuItem("檢查更新…", delegate { CheckForUpdates(true); }));
            menu.Items.Add(TrayMenuItem("結束桌伴", delegate { Exit(); }));
            return menu;
        }

        private bool ShouldAutomaticallyCheckForUpdates()
        {
            if (checkingForUpdates || downloadingUpdate || pendingUpdate != null) return false;
            if (lastUpdateAttemptUtc.HasValue && DateTime.UtcNow - lastUpdateAttemptUtc.Value < TimeSpan.FromMinutes(15)) return false;
            if (!settings.LastUpdateCheckUtc.HasValue) return true;
            return DateTime.UtcNow - settings.LastUpdateCheckUtc.Value > TimeSpan.FromHours(6);
        }

        public bool IsAutoCheckUpdatesEnabled()
        {
            return settings.AutoCheckUpdates;
        }

        public void SetAutoCheckUpdatesEnabled(bool enabled)
        {
            settings.AutoCheckUpdates = enabled;
            settingsStore.Save(settings);
            NotifyUpdateUi();
            if (enabled && ShouldAutomaticallyCheckForUpdates()) CheckForUpdates(false);
        }

        public string GetUpdateStatus()
        {
            return I18n.T(updateStatus);
        }

        public string GetUiLanguage()
        {
            return string.IsNullOrWhiteSpace(settings.UiLanguage) ? "System" : settings.UiLanguage;
        }

        public void SetUiLanguage(string language)
        {
            string value = string.IsNullOrWhiteSpace(language) ? "System" : language;
            if (string.Equals(GetUiLanguage(), value, StringComparison.OrdinalIgnoreCase)) return;
            SaveNow();
            settings.UiLanguage = value;
            settingsStore.Save(settings);
            if (PreviewMode) return;
            try
            {
                Process.Start(new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
                {
                    UseShellExecute = true,
                    Arguments = "--restart-after " + Process.GetCurrentProcess().Id
                });
                Exit();
            }
            catch (Exception ex)
            {
                AppDialog.Show(ex.Message, "無法切換語言", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetPendingUpdateVersion()
        {
            return pendingUpdate == null ? null : pendingUpdate.Version.ToString(3);
        }

        public void CheckForUpdates(bool interactive)
        {
            if (checkingForUpdates || downloadingUpdate)
            {
                if (interactive) AppDialog.Show("桌伴正在檢查或下載更新，請稍候。", "檢查更新");
                return;
            }

            checkingForUpdates = true;
            lastUpdateAttemptUtc = DateTime.UtcNow;
            updateStatus = "正在檢查 GitHub 最新版本…";
            NotifyUpdateUi();
            Task.Factory.StartNew<UpdateRelease>(delegate { return UpdateService.GetLatestRelease(); }).ContinueWith(delegate(Task<UpdateRelease> task)
            {
                app.Dispatcher.BeginInvoke(new Action(delegate
                {
                    checkingForUpdates = false;
                    if (task.IsFaulted)
                    {
                        Exception error = task.Exception == null ? null : task.Exception.GetBaseException();
                        updateStatus = "暫時無法連線到 GitHub";
                        NotifyUpdateUi();
                        if (interactive) AppDialog.Show(error == null ? "請稍後再試。" : error.Message, "無法檢查更新", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    settings.LastUpdateCheckUtc = DateTime.UtcNow;
                    settingsStore.Save(settings);

                    UpdateRelease latest = task.Result;
                    Version current = Assembly.GetExecutingAssembly().GetName().Version;
                    if (latest != null && latest.Version.CompareTo(current) > 0 && !string.IsNullOrWhiteSpace(latest.DownloadUrl))
                    {
                        pendingUpdate = latest;
                        updateStatus = "有新版本 " + latest.Version.ToString(3) + " 可安裝";
                        NotifyUpdateUi();
                        if (interactive) InstallPendingUpdate();
                        else tray.ShowBalloonTip(8000, I18n.T("桌伴有新版本"), I18n.T("版本 " + latest.Version.ToString(3) + " 已推出，點一下即可更新。"), Forms.ToolTipIcon.Info);
                    }
                    else
                    {
                        pendingUpdate = null;
                        updateStatus = "已是最新版本 " + current.ToString(3);
                        NotifyUpdateUi();
                        if (interactive) AppDialog.Show("目前已是最新版本 " + current.ToString(3) + "。", "檢查更新");
                    }
                }));
            });
        }

        public void InstallPendingUpdate()
        {
            if (pendingUpdate == null)
            {
                CheckForUpdates(true);
                return;
            }
            if (downloadingUpdate) return;
            UpdateRelease release = pendingUpdate;
            if (AppDialog.Show("要安裝桌伴 " + release.Version.ToString(3) + " 嗎？\n\n程式會從官方 GitHub Release 下載正式安裝程式、核對檔案完整性，並沿用目前的安裝位置。圍欄設定與檔案不會被移動。",
                "安裝更新", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            downloadingUpdate = true;
            updateStatus = "正在下載版本 " + release.Version.ToString(3) + "…";
            NotifyUpdateUi();
            Task.Factory.StartNew(delegate { return UpdateService.DownloadRelease(release); }).ContinueWith(delegate(Task<string> task)
            {
                app.Dispatcher.BeginInvoke(new Action(delegate
                {
                    downloadingUpdate = false;
                    if (task.IsFaulted)
                    {
                        Exception error = task.Exception == null ? null : task.Exception.GetBaseException();
                        updateStatus = "下載更新失敗";
                        NotifyUpdateUi();
                        AppDialog.Show(error == null ? "請稍後再試。" : error.Message, "無法下載更新", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    try
                    {
                        string installer = task.Result;
                        ProcessStartInfo start = new ProcessStartInfo(installer)
                        {
                            UseShellExecute = true,
                            Arguments = UpdateInstaller.BuildSetupArguments(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        };
                        Process.Start(start);
                        Exit();
                    }
                    catch (Exception ex)
                    {
                        updateStatus = "無法啟動更新程式";
                        NotifyUpdateUi();
                        AppDialog.Show(ex.Message, "無法安裝更新", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }));
            });
        }

        private void NotifyUpdateUi()
        {
            if (controlCenter != null) controlCenter.RefreshUpdateStatus();
        }

        private static MenuItem TrayMenuItem(string label, Action action)
        {
            MenuItem item = new MenuItem { Header = I18n.T(label) };
            item.Click += delegate { if (action != null) action(); };
            return item;
        }

        private FenceModel CreateNewModel()
        {
            int offset = fences.Count * 26;
            return new FenceModel
            {
                Id = Guid.NewGuid().ToString("N"), Title = I18n.T("新圍欄"),
                X = 70 + offset, Y = 100 + offset, Width = 350, Height = 260,
                Accent = AccentPalette.ToHex(AccentPalette.ReadWindowsAccent()), Opacity = 0.86,
                Items = new List<string>()
            };
        }

        private FenceModel CreatePreviewModel()
        {
            FenceModel model = CreateNewModel();
            model.Title = "選取與樣式預覽";
            model.X = 150;
            model.Y = 120;
            model.Width = 430;
            model.Height = 330;
            model.Items = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                Environment.GetFolderPath(Environment.SpecialFolder.Recent),
                Environment.GetFolderPath(Environment.SpecialFolder.SendTo),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Templates),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
            }.Where(p => !string.IsNullOrEmpty(p) && Directory.Exists(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return model;
        }

        public void CreateBlankFence()
        {
            AddFence(CreateNewModel(), false);
        }

        public void CreateFolderFence()
        {
            using (Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = I18n.T("選擇要顯示在新圍欄中的資料夾");
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
                FenceModel model = CreateNewModel();
                model.PortalPath = dialog.SelectedPath;
                model.Title = new DirectoryInfo(dialog.SelectedPath).Name;
                AddFence(model, false);
            }
        }

        public void ShowControlCenter()
        {
            if (controlCenter == null)
            {
                controlCenter = new ControlCenterWindow(this);
                controlCenter.Closed += delegate { controlCenter = null; };
            }
            controlCenter.RefreshContent();
            I18n.Apply(controlCenter);
            if (!controlCenter.IsVisible) controlCenter.Show();
            controlCenter.Activate();
        }

        public void CaptureControlCenterAndExit()
        {
            CaptureControlCenterPageAndExit("Manage", "DeskBound-control-center-preview.png");
        }

        public void CaptureControlCenterPageAndExit(string page, string fileName)
        {
            ShowControlCenter();
            controlCenter.ShowPageForCapture(page);
            DispatcherTimer captureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(850) };
            captureTimer.Tick += delegate
            {
                captureTimer.Stop();
                try
                {
                    controlCenter.UpdateLayout();
                    int width = Math.Max(1, (int)Math.Ceiling(controlCenter.ActualWidth));
                    int height = Math.Max(1, (int)Math.Ceiling(controlCenter.ActualHeight));
                    RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(controlCenter);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (FileStream output = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)))
                        encoder.Save(output);
                }
                catch { }
                Exit();
            };
            captureTimer.Start();
        }

        public void CaptureInboxAndExit()
        {
            FenceWindow inbox = EnsureDesktopInboxFence();
            if (inbox == null) { Exit(); return; }
            inbox.Show();
            DispatcherTimer captureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
            captureTimer.Tick += delegate
            {
                captureTimer.Stop();
                try { inbox.SaveVisualPreview(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeskBound-inbox-preview.png")); }
                catch { }
                Exit();
            };
            captureTimer.Start();
        }

        public void CaptureMoveHistoryAndExit()
        {
            ShowMoveHistory();
            DispatcherTimer captureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(850) };
            captureTimer.Tick += delegate
            {
                captureTimer.Stop();
                try
                {
                    moveHistoryWindow.UpdateLayout();
                    int width = Math.Max(1, (int)Math.Ceiling(moveHistoryWindow.ActualWidth));
                    int height = Math.Max(1, (int)Math.Ceiling(moveHistoryWindow.ActualHeight));
                    RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(moveHistoryWindow);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (FileStream output = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "桌伴-history-preview.png"))) encoder.Save(output);
                }
                catch { }
                Exit();
            };
            captureTimer.Start();
        }

        public void CaptureFenceTabsAndExit()
        {
            FenceModel model = CreatePreviewModel();
            model.Title = I18n.T("遊戲");
            model.Width = 460; model.Height = 360;
            model.Tabs.Clear();
            FenceTabModel common = new FenceTabModel { Title = I18n.T("常用"), Accent = model.Accent, Items = model.Items.Take(4).ToList() };
            FenceTabModel folders = new FenceTabModel { Title = I18n.T("資料夾"), Accent = model.Accent, Items = model.Items.Skip(4).Take(4).ToList() };
            FenceTabModel games = new FenceTabModel { Title = I18n.T("遊戲收藏"), Accent = model.Accent, Items = model.Items.Skip(8).Take(5).ToList() };
            model.Tabs.Add(common); model.Tabs.Add(folders); model.Tabs.Add(games); model.ActiveTabId = games.Id;
            FenceWindow preview = new FenceWindow(model, this);
            preview.Show();
            DispatcherTimer captureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            captureTimer.Tick += delegate
            {
                captureTimer.Stop();
                try { preview.SaveVisualPreview(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "桌伴-tabs-preview.png")); }
                catch { }
                preview.SwitchTabForVisualTest(common.Id);
                DispatcherTimer motionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(175) };
                motionTimer.Tick += delegate
                {
                    motionTimer.Stop();
                    try { preview.SaveVisualPreview(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "桌伴-tabs-motion-preview.png")); }
                    catch { }
                    DispatcherTimer finishTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(360) };
                    finishTimer.Tick += delegate { finishTimer.Stop(); Exit(); };
                    finishTimer.Start();
                };
                motionTimer.Start();
            };
            captureTimer.Start();
        }

        public void CaptureListViewAndExit()
        {
            FenceModel model = CreatePreviewModel();
            string previewRoot = Path.Combine(Path.GetTempPath(), "DeskBound-list-preview-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(previewRoot);
            model.Items = new[] { "Projects", "Documents", "Downloads", "Screenshots", "Design Assets", "Archive" }
                .Select(name => Directory.CreateDirectory(Path.Combine(previewRoot, name)).FullName).ToList();
            model.Title = I18n.T("精簡清單");
            model.Width = 390; model.Height = 390; model.ItemView = "List";
            model.Tabs.Clear();
            FenceTabModel tab = new FenceTabModel
            {
                Title = I18n.T("常用"), Accent = model.Accent, Items = model.Items, ItemView = "List"
            };
            model.Tabs.Add(tab); model.ActiveTabId = tab.Id;
            FenceWindow preview = new FenceWindow(model, this);
            preview.Show();
            DispatcherTimer captureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(900) };
            captureTimer.Tick += delegate
            {
                captureTimer.Stop();
                try { preview.SaveVisualPreview(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeskBound-list-view-preview.png")); }
                catch { }
                try { if (Directory.Exists(previewRoot)) Directory.Delete(previewRoot, true); } catch { }
                Exit();
            };
            captureTimer.Start();
        }

        public void ShowAppearanceSettings(FenceWindow fence)
        {
            ShowControlCenter();
            if (controlCenter != null) controlCenter.ShowAppearance(fence);
        }

        public void SaveFromControlCenter()
        {
            saveTimer.Stop();
            saveTimer.Start();
        }

        public IList<FenceWindow> GetFences()
        {
            return fences.ToArray();
        }

        public void RecordMoveHistory(string label, IEnumerable<MoveRecord> moves)
        {
            List<MoveRecord> records = (moves ?? Enumerable.Empty<MoveRecord>()).Take(200).ToList();
            if (records.Count == 0) return;
            if (settings.MoveHistory == null) settings.MoveHistory = new List<MoveHistoryEntry>();
            settings.MoveHistory.Insert(0, new MoveHistoryEntry
            {
                Id = Guid.NewGuid().ToString("N"), Timestamp = DateTime.Now,
                Label = string.IsNullOrWhiteSpace(label) ? "移動項目" : label, Moves = records
            });
            settings.MoveHistory = settings.MoveHistory.Take(40).ToList();
            settingsStore.Save(settings);
            if (moveHistoryWindow != null && moveHistoryWindow.IsVisible) moveHistoryWindow.RefreshContent();
        }

        public IList<MoveHistoryEntry> GetMoveHistory()
        {
            return (settings.MoveHistory ?? new List<MoveHistoryEntry>()).ToArray();
        }

        public void ShowMoveHistory()
        {
            if (moveHistoryWindow == null)
            {
                moveHistoryWindow = new MoveHistoryWindow(this);
                moveHistoryWindow.Closed += delegate { moveHistoryWindow = null; };
            }
            moveHistoryWindow.RefreshContent();
            I18n.Apply(moveHistoryWindow);
            if (!moveHistoryWindow.IsVisible) moveHistoryWindow.Show();
            moveHistoryWindow.Activate();
        }

        public void UndoHistoryEntry(MoveHistoryEntry entry)
        {
            if (entry == null || entry.Moves == null || entry.Moves.Count == 0) return;
            int originalCount = entry.Moves.Count;
            Task.Factory.StartNew(delegate { return ManagedStorage.Undo(entry.Moves); })
                .ContinueWith(task =>
                {
                    MoveBatchResult result = task.Result;
                    if (result.Moves.Count == 0) settings.MoveHistory.RemoveAll(item => item.Id == entry.Id);
                    else entry.Moves = result.Moves;
                    settingsStore.Save(settings);
                    foreach (FenceWindow fence in fences) fence.RefreshFromManager();
                    if (moveHistoryWindow != null) moveHistoryWindow.RefreshContent();
                    int restored = originalCount - result.Moves.Count;
                    if (restored > 0 && result.Errors.Count == 0)
                        AppDialog.Show("已復原 " + restored + " 個項目。", "復原完成", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result.Errors.Count > 0)
                        AppDialog.Show(string.Join(Environment.NewLine, result.Errors.Take(8)), "部分項目無法復原", MessageBoxButton.OK, MessageBoxImage.Warning);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void ShowGlobalSearch()
        {
            if (globalSearchWindow == null)
            {
                globalSearchWindow = new GlobalSearchWindow(this);
                globalSearchWindow.Closed += delegate { globalSearchWindow = null; };
            }
            I18n.Apply(globalSearchWindow);
            if (!globalSearchWindow.IsVisible) globalSearchWindow.Show();
            globalSearchWindow.Activate();
            globalSearchWindow.FocusSearch();
        }

        public List<GlobalSearchResult> SearchAll(string query)
        {
            List<GlobalSearchResult> results = new List<GlobalSearchResult>();
            string needle = (query ?? "").Trim();
            if (needle.Length == 0) return results;
            foreach (FenceWindow fence in fences)
            {
                fence.SyncActiveTabState();
                IEnumerable<FenceTabModel> tabs = fence.Model.Tabs ?? new List<FenceTabModel>();
                foreach (FenceTabModel tab in tabs)
                {
                    List<string> paths = new List<string>();
                    string folder = !string.IsNullOrEmpty(tab.PortalPath) ? tab.PortalPath : tab.ManagedPath;
                    try
                    {
                        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                            paths.AddRange(Directory.EnumerateFileSystemEntries(folder).Take(1200));
                    }
                    catch { }
                    if (tab.Items != null) paths.AddRange(tab.Items.Where(path => File.Exists(path) || Directory.Exists(path)));
                    foreach (string path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        string name = DisplayName(path);
                        if (name.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                            path.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) < 0) continue;
                        results.Add(new GlobalSearchResult
                        {
                            Fence = fence, FenceTitle = fence.Model.Title, TabId = tab.Id,
                            TabTitle = tab.Title, Path = path, Name = name
                        });
                        if (results.Count >= 250) return results;
                    }
                }
            }
            return results.OrderBy(result => result.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public void RevealSearchResult(GlobalSearchResult result)
        {
            if (result == null || result.Fence == null) return;
            SetAllVisible(true);
            result.Fence.RevealItem(result.TabId, result.Path);
        }

        private static string DisplayName(string path)
        {
            string name = Path.GetFileName((path ?? "").TrimEnd(Path.DirectorySeparatorChar));
            string extension = Path.GetExtension(name);
            if (string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".url", StringComparison.OrdinalIgnoreCase))
                name = Path.GetFileNameWithoutExtension(name);
            return string.IsNullOrEmpty(name) ? path : name;
        }

        public bool ShouldKeepFenceVisible(FenceWindow fence)
        {
            return !PreviewMode && !exiting && (visible || peeking) && fence != null && fences.Contains(fence);
        }

        public void AddFence(FenceModel model, bool chooseSource)
        {
            FenceWindow window = new FenceWindow(model, this);
            fences.Add(window);
            I18n.Apply(window);
            window.Show();
            window.AttachToDesktop();
            if (chooseSource)
                window.ChooseFolder();
            SaveSoon();
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void RemoveFence(FenceWindow window)
        {
            if (!fences.Contains(window)) return;
            allowLayoutReduction = true;
            fences.Remove(window);
            window.CloseFromManager();
            SaveSoon();
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void SaveSoon()
        {
            saveTimer.Stop();
            saveTimer.Start();
            if (controlCenter != null && controlCenter.IsVisible) controlCenter.RefreshContent();
        }

        public void SaveCritical()
        {
            saveTimer.Stop();
            SaveNow();
            if (controlCenter != null && controlCenter.IsVisible) controlCenter.RefreshContent();
        }

        public void Log(string message)
        {
            if (!DiagnosticsMode) return;
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeskBound-diagnostics.log"),
                    DateTime.Now.ToString("O") + " " + message + Environment.NewLine);
            }
            catch { }
        }

        private void SaveNow()
        {
            if (exiting || PreviewMode) return;
            foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
            if (store.Save(fences.Select(f => f.Model).ToList(), allowLayoutReduction))
                allowLayoutReduction = false;
        }

        private void ToggleVisible()
        {
            SetAllVisible(!visible);
        }

        public void SetAllVisible(bool show)
        {
            if (peeking) EndPeek();
            visible = show;
            foreach (FenceWindow fence in fences)
            {
                if (show) { fence.Show(); fence.AttachToDesktop(); }
                else fence.Hide();
            }
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void TogglePeek()
        {
            if (peeking) { EndPeek(); return; }
            peeking = true;
            visibleBeforePeek = visible;
            foreach (FenceWindow fence in fences)
            {
                if (!fence.IsVisible) fence.Show();
                fence.SetPeekMode(true);
            }
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        private void EndPeek()
        {
            if (!peeking) return;
            peeking = false;
            foreach (FenceWindow fence in fences)
            {
                fence.SetPeekMode(false);
                if (!visibleBeforePeek) fence.Hide();
            }
            visible = visibleBeforePeek;
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void SnapFence(FenceWindow moving)
        {
            if (moving == null || (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) return;
            const double threshold = 14;
            double x = moving.Model.X;
            double y = moving.Model.Y;
            double width = moving.Width;
            double height = moving.Height;
            Forms.Screen screen = Forms.Screen.FromPoint(new Drawing.Point((int)Math.Round(x + width / 2), (int)Math.Round(y + HeaderForSnap / 2)));
            Drawing.Rectangle area = screen.WorkingArea;
            x = SnapValue(x, area.Left, threshold);
            x = SnapValue(x, area.Right - width, threshold);
            y = SnapValue(y, area.Top, threshold);
            y = SnapValue(y, area.Bottom - height, threshold);
            foreach (FenceWindow other in fences)
            {
                if (object.ReferenceEquals(other, moving)) continue;
                x = SnapValue(x, other.Model.X, threshold);
                x = SnapValue(x, other.Model.X + other.Width, threshold);
                x = SnapValue(x, other.Model.X - width, threshold);
                x = SnapValue(x, other.Model.X + other.Width - width, threshold);
                y = SnapValue(y, other.Model.Y, threshold);
                y = SnapValue(y, other.Model.Y + other.Height, threshold);
                y = SnapValue(y, other.Model.Y - height, threshold);
                y = SnapValue(y, other.Model.Y + other.Height - height, threshold);
            }
            moving.Model.X = x;
            moving.Model.Y = y;
        }

        private const double HeaderForSnap = 44;
        private static double SnapValue(double value, double target, double threshold)
        {
            return Math.Abs(value - target) <= threshold ? target : value;
        }

        public void CreateLayoutSnapshot()
        {
            if (PreviewMode) return;
            foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
            SaveCritical();
            try
            {
                string path = LayoutSnapshotStore.Create(fences.Select(f => f.Model).ToList());
                AppDialog.Show("已保存目前版面。\n\n" + path + "\n\n快照只記錄圍欄配置，不會複製或移動檔案。",
                    "版面快照已建立", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { AppDialog.Show(ex.Message, "無法建立版面快照", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        public bool IsAutoOrganizeEnabled()
        {
            return settings.AutoOrganizeDesktop;
        }

        public bool SetAutoOrganizeEnabled(bool enabled)
        {
            if (enabled && !settings.AutoOrganizeDesktop)
            {
                MessageBoxResult answer = AppDialog.Show(
                    "開啟後，桌面上新出現的圖片、文件、壓縮檔、安裝程式、影音與捷徑，會移入「智慧整理」圍欄的分類分頁。\n\n不符合規則的檔案和所有資料夾會留在原位；功能可隨時關閉。",
                    "開啟自動整理", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes) return false;
            }
            settings.AutoOrganizeDesktop = enabled;
            if (enabled)
            {
                settings.DesktopInboxEnabled = false;
                ConfigureDesktopInboxWatcher();
                desktopInboxDebounceTimer.Stop();
                foreach (FenceWindow fence in fences.Where(f => f.Model.IsDesktopInbox))
                    fence.UpdateInboxMonitorState();
            }
            settingsStore.Save(settings);
            if (enabled)
            {
                organizerTimer.Start();
                OrganizeDesktop(false);
            }
            else if (!settings.DesktopInboxEnabled) organizerTimer.Stop();
            return true;
        }

        public bool IsDesktopInboxEnabled()
        {
            return settings.DesktopInboxEnabled;
        }

        public void SetDesktopInboxEnabled(bool enabled)
        {
            settings.DesktopInboxEnabled = enabled;
            if (enabled)
            {
                settings.AutoOrganizeDesktop = false;
                EnsureDesktopInboxFence();
                ResetDesktopInboxBaseline();
                ConfigureDesktopInboxWatcher();
                organizerTimer.Start();
            }
            else
            {
                ConfigureDesktopInboxWatcher();
                desktopInboxDebounceTimer.Stop();
                if (!settings.AutoOrganizeDesktop) organizerTimer.Stop();
            }
            settingsStore.Save(settings);
            foreach (FenceWindow fence in fences.Where(f => f.Model.IsDesktopInbox))
                fence.UpdateInboxMonitorState();
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void ShowDesktopInbox()
        {
            // Opening this feature means the user intends to use it. Previously this
            // only created the fence while its collector silently remained disabled.
            if (!settings.DesktopInboxEnabled) SetDesktopInboxEnabled(true);
            FenceWindow inbox = EnsureDesktopInboxFence();
            if (inbox == null) return;
            visible = true;
            inbox.Show();
            inbox.AttachToDesktop();
            inbox.Activate();
            SaveSoon();
        }

        private FenceWindow EnsureDesktopInboxFence()
        {
            FenceWindow existing = fences.FirstOrDefault(f => f.Model.IsDesktopInbox);
            if (existing != null) return existing;
            FenceModel model = CreateNewModel();
            model.Title = I18n.T("桌面收件匣");
            model.IsDesktopInbox = true;
            model.Accent = "#52C7A5";
            model.Width = 370;
            model.Height = 285;
            AddFence(model, false);
            return fences.FirstOrDefault(f => f.Model.IsDesktopInbox);
        }

        private void ResetDesktopInboxBaseline()
        {
            desktopInboxBaseline = new HashSet<string>(EnumerateDesktopInboxCandidates(), StringComparer.OrdinalIgnoreCase);
        }

        private void ConfigureDesktopInboxWatcher()
        {
            if (desktopInboxWatcher != null)
            {
                try { desktopInboxWatcher.EnableRaisingEvents = false; desktopInboxWatcher.Dispose(); }
                catch { }
                desktopInboxWatcher = null;
            }
            if (PreviewMode || !settings.DesktopInboxEnabled) return;
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop)) return;
            try
            {
                desktopInboxWatcher = new FileSystemWatcher(desktop)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime
                };
                desktopInboxWatcher.Created += delegate { QueueDesktopInboxCollection(); };
                desktopInboxWatcher.Renamed += delegate { QueueDesktopInboxCollection(); };
                desktopInboxWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                if (desktopInboxWatcher != null) { try { desktopInboxWatcher.Dispose(); } catch { } }
                desktopInboxWatcher = null;
            }
        }

        private void QueueDesktopInboxCollection()
        {
            if (!settings.DesktopInboxEnabled || exiting) return;
            try
            {
                app.Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (!settings.DesktopInboxEnabled || exiting) return;
                    desktopInboxDebounceTimer.Stop();
                    desktopInboxDebounceTimer.Start();
                }));
            }
            catch { }
        }

        private static List<string> EnumerateDesktopInboxCandidates()
        {
            List<string> result = new List<string>();
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop)) return result;
            try
            {
                foreach (string path in Directory.EnumerateFileSystemEntries(desktop))
                {
                    try
                    {
                        if (string.Equals(Path.GetFileName(path), "desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;
                        if (IsDeskBoundDesktopShortcut(path)) continue;
                        if (IsIncompleteDownloadPath(path)) continue;
                        FileAttributes attributes = File.GetAttributes(path);
                        if ((attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0) continue;
                        result.Add(path);
                    }
                    catch { }
                }
            }
            catch { }
            return result;
        }

        internal static bool IsDeskBoundDesktopShortcut(string path)
        {
            string name = Path.GetFileName(path ?? "");
            return string.Equals(name, "桌伴.lnk", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "DeskBound.lnk", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsIncompleteDownloadPath(string path)
        {
            string lowerName = Path.GetFileName(path ?? "").ToLowerInvariant();
            return lowerName.EndsWith(".crdownload") || lowerName.EndsWith(".part") || lowerName.EndsWith(".partial") ||
                lowerName.EndsWith(".download") || lowerName.EndsWith(".opdownload") || lowerName.EndsWith(".tmp");
        }

        internal static List<string> FindNewDesktopInboxItems(IEnumerable<string> baseline, IEnumerable<string> current)
        {
            HashSet<string> known = new HashSet<string>(baseline ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            return (current ?? Enumerable.Empty<string>()).Where(path => !string.IsNullOrEmpty(path) && !known.Contains(path))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void CollectDesktopInbox(bool interactive)
        {
            if (PreviewMode || collectingDesktopInbox || !settings.DesktopInboxEnabled) return;
            List<string> current = EnumerateDesktopInboxCandidates();
            // Keep only baseline entries that still exist. Do not mark a newly found
            // item as seen before it has moved: browsers commonly keep downloads
            // locked for a while, and those items must be retried on later ticks.
            desktopInboxBaseline.IntersectWith(current);
            List<string> incoming = FindNewDesktopInboxItems(desktopInboxBaseline, current);
            if (incoming.Count == 0) return;
            FenceWindow inbox = EnsureDesktopInboxFence();
            if (inbox == null) return;
            string destination;
            try
            {
                destination = ManagedStorage.EnsureFolder(inbox.Model);
                inbox.SyncActiveTabState();
            }
            catch { return; }
            collectingDesktopInbox = true;
            Task.Factory.StartNew(delegate { return ManagedStorage.MoveInto(incoming, destination); })
                .ContinueWith(task =>
                {
                    collectingDesktopInbox = false;
                    MoveBatchResult result = task.Result;
                    if (result.Moves.Count > 0)
                    {
                        RecordMoveHistory("桌面收件匣", result.Moves);
                        inbox.RefreshFromManager();
                        SaveCritical();
                        if (interactive) AppDialog.Show("已收入 " + result.Moves.Count + " 個桌面項目。", "桌面收件匣");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OrganizeDesktopInteractive()
        {
            Dictionary<string, List<string>> groups = DesktopAutoOrganizer.Analyze(settings.OrganizerExtensions, settings.OrganizerKeywords);
            int total = groups.Sum(g => g.Value.Count);
            if (total == 0)
            {
                AppDialog.Show("桌面目前沒有符合分類規則的檔案。\n資料夾與無法判斷類型的項目不會被移動。",
                    "智慧整理", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string details = string.Join("\n", groups.Where(g => g.Value.Count > 0).Select(g => "• " + DesktopAutoOrganizer.CategoryTitle(g.Key) + "：" + g.Value.Count + " 個"));
            if (AppDialog.Show("找到 " + total + " 個可整理項目：\n\n" + details +
                "\n\n將建立或更新「智慧整理」圍欄。檔案會安全移入分類分頁，名稱衝突時不會覆蓋。",
                "預覽智慧整理", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            OrganizeDesktop(true, groups);
        }

        public void ShowRuleEditor()
        {
            if (ruleEditorWindow == null)
            {
                ruleEditorWindow = new RuleEditorWindow(this);
                ruleEditorWindow.Closed += delegate { ruleEditorWindow = null; };
            }
            ruleEditorWindow.LoadRules();
            I18n.Apply(ruleEditorWindow);
            if (!ruleEditorWindow.IsVisible) ruleEditorWindow.Show();
            ruleEditorWindow.Activate();
        }

        public string GetOrganizerExtensionRule(string key)
        {
            string value;
            if (settings.OrganizerExtensions != null && settings.OrganizerExtensions.TryGetValue(key, out value)) return value;
            return DesktopAutoOrganizer.DefaultExtensionText(key);
        }

        public string GetOrganizerKeywordRule(string key)
        {
            string value;
            return settings.OrganizerKeywords != null && settings.OrganizerKeywords.TryGetValue(key, out value) ? value : "";
        }

        public void SaveOrganizerRules(Dictionary<string, string> extensions, Dictionary<string, string> keywords)
        {
            settings.OrganizerExtensions = extensions ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            settings.OrganizerKeywords = keywords ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            settingsStore.Save(settings);
            if (ruleEditorWindow != null) ruleEditorWindow.Hide();
        }

        private void OrganizeDesktop(bool interactive)
        {
            OrganizeDesktop(interactive, DesktopAutoOrganizer.Analyze(settings.OrganizerExtensions, settings.OrganizerKeywords));
        }

        private void OrganizeDesktop(bool interactive, Dictionary<string, List<string>> groups)
        {
            if (PreviewMode || organizingDesktop || groups == null || groups.Sum(g => g.Value.Count) == 0) return;
            organizingDesktop = true;
            FenceWindow existing = fences.FirstOrDefault(f => f.Model.IsAutoOrganizer);
            if (existing != null) existing.SyncActiveTabState();
            FenceModel model = existing == null ? CreateNewModel() : existing.Model;
            bool isNew = existing == null;
            if (isNew)
            {
                model.Title = I18n.T("智慧整理");
                model.IsAutoOrganizer = true;
                model.Tabs.Clear();
                model.ActiveTabId = null;
            }
            Task.Factory.StartNew(delegate
            {
                OrganizerRunResult result = new OrganizerRunResult { Model = model, IsNew = isNew };
                foreach (KeyValuePair<string, List<string>> group in groups)
                {
                    if (group.Value.Count == 0) continue;
                    FenceTabModel tab = model.Tabs.FirstOrDefault(t => string.Equals(t.RuleKey, group.Key, StringComparison.OrdinalIgnoreCase));
                    if (tab == null)
                    {
                        tab = new FenceTabModel { Title = DesktopAutoOrganizer.CategoryTitle(group.Key), RuleKey = group.Key, Accent = model.Accent };
                        model.Tabs.Add(tab);
                    }
                    string folder = ManagedStorage.EnsureFolder(model, tab);
                    MoveBatchResult move = ManagedStorage.MoveInto(group.Value, folder);
                    if (move.Moves.Count > 0) tab.LastMoves = move.Moves.Take(200).ToList();
                    result.Moved += move.Moves.Count;
                    result.Errors.AddRange(move.Errors);
                }
                if (model.Tabs.Count > 0 && string.IsNullOrEmpty(model.ActiveTabId)) model.ActiveTabId = model.Tabs[0].Id;
                return result;
            }).ContinueWith(task =>
            {
                organizingDesktop = false;
                OrganizerRunResult result = task.Result;
                if (result.Moved > 0)
                {
                    if (result.IsNew) AddFence(result.Model, false);
                    else existing.RefreshFromManager();
                    SaveCritical();
                    if (interactive) AppDialog.Show("已整理 " + result.Moved + " 個桌面項目。\n可在「智慧整理」圍欄上方切換分類分頁。",
                        "智慧整理完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if (result.Errors.Count > 0 && interactive)
                    AppDialog.Show(string.Join(Environment.NewLine, result.Errors.Take(8)), "部分項目無法整理", MessageBoxButton.OK, MessageBoxImage.Warning);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void RestoreLayoutSnapshot()
        {
            if (PreviewMode) return;
            using (Forms.OpenFileDialog dialog = new Forms.OpenFileDialog())
            {
                dialog.Title = "選擇要還原的桌伴版面快照";
                dialog.Filter = "桌伴版面快照 (*.json)|*.json";
                dialog.InitialDirectory = LayoutSnapshotStore.GetFolder();
                if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
                if (AppDialog.Show("要還原選取的版面嗎？\n目前版面會先自動保存一份快照；所有檔案都不會被移動或刪除。",
                    "還原版面快照", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                try
                {
                    foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
                    LayoutSnapshotStore.Create(fences.Select(f => f.Model).ToList(), "還原前");
                    ApplyLayoutModels(LayoutSnapshotStore.Load(dialog.FileName));
                }
                catch (Exception ex) { AppDialog.Show(ex.Message, "無法還原版面快照", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
        }

        private void ApplyLayoutModels(List<FenceModel> models)
        {
            allowLayoutReduction = true;
            foreach (FenceWindow fence in fences.ToArray()) { fences.Remove(fence); fence.CloseFromManager(); }
            foreach (FenceModel model in models ?? new List<FenceModel>()) AddFence(model, false);
            visible = true;
            SaveCritical();
            ResetDesktopInboxBaseline();
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void CreateScene()
        {
            RenameDialog dialog = new RenameDialog("新情境", "建立情境配置", "情境名稱");
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.Value)) return;
            foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
            SaveCritical();
            string path = LayoutSnapshotStore.Create(fences.Select(f => f.Model).ToList(), "情境-" + dialog.Value.Trim());
            AppDialog.Show("已保存「" + dialog.Value.Trim() + "」。\n只記錄圍欄配置，不會複製檔案。", "情境已建立", MessageBoxButton.OK, MessageBoxImage.Information);
            if (sceneSwitcherWindow != null) sceneSwitcherWindow.RefreshContent();
        }

        public IList<SceneInfo> GetScenes()
        {
            List<SceneInfo> scenes = new List<SceneInfo>();
            try
            {
                foreach (string path in Directory.EnumerateFiles(LayoutSnapshotStore.GetFolder(), "*-情境-*.json").OrderByDescending(File.GetLastWriteTime))
                {
                    string file = Path.GetFileNameWithoutExtension(path);
                    int marker = file.IndexOf("-情境-", StringComparison.OrdinalIgnoreCase);
                    scenes.Add(new SceneInfo { Path = path, Name = marker >= 0 ? file.Substring(marker + 4) : file, Modified = File.GetLastWriteTime(path) });
                }
            }
            catch { }
            return scenes;
        }

        public void ShowScenes()
        {
            if (sceneSwitcherWindow == null)
            {
                sceneSwitcherWindow = new SceneSwitcherWindow(this);
                sceneSwitcherWindow.Closed += delegate { sceneSwitcherWindow = null; };
            }
            sceneSwitcherWindow.RefreshContent();
            I18n.Apply(sceneSwitcherWindow);
            if (!sceneSwitcherWindow.IsVisible) sceneSwitcherWindow.Show();
            sceneSwitcherWindow.Activate();
        }

        public void RestoreScene(SceneInfo scene)
        {
            if (scene == null || !File.Exists(scene.Path)) return;
            try
            {
                foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
                LayoutSnapshotStore.Create(fences.Select(f => f.Model).ToList(), "切換情境前");
                ApplyLayoutModels(LayoutSnapshotStore.Load(scene.Path));
                if (sceneSwitcherWindow != null) sceneSwitcherWindow.Hide();
            }
            catch (Exception ex) { AppDialog.Show(ex.Message, "無法切換情境", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        public void DeleteScene(SceneInfo scene)
        {
            if (scene == null || !File.Exists(scene.Path)) return;
            if (AppDialog.Show("刪除情境「" + scene.Name + "」？\n只會刪除配置，不會刪除任何圍欄檔案。", "刪除情境", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            try { File.Delete(scene.Path); }
            catch (Exception ex) { AppDialog.Show(ex.Message, "無法刪除情境", MessageBoxButton.OK, MessageBoxImage.Warning); }
            if (sceneSwitcherWindow != null) sceneSwitcherWindow.RefreshContent();
        }

        public void ArrangeFences()
        {
            Rect area = SystemParameters.WorkArea;
            double x = area.Left + 28;
            double y = area.Top + 34;
            double rowHeight = 0;
            foreach (FenceWindow fence in fences)
            {
                double width = Math.Max(250, fence.Width);
                double height = fence.Model.Collapsed ? 44 : Math.Max(150, fence.Height);
                if (x + width > area.Right - 28 && x > area.Left + 28)
                {
                    x = area.Left + 28;
                    y += rowHeight + 22;
                    rowHeight = 0;
                }
                fence.Model.X = x;
                fence.Model.Y = y;
                fence.RestackDesktop();
                x += width + 22;
                rowHeight = Math.Max(rowHeight, height);
            }
            SetAllVisible(true);
            SaveSoon();
        }

        public void ClearAllFences()
        {
            if (fences.Count == 0) return;
            MessageBoxResult result = AppDialog.Show("要移除所有圍欄嗎？只會清除版面，所有檔案仍保留在各自資料夾。", "清除桌伴版面",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            foreach (FenceWindow fence in fences.ToArray())
                RemoveFence(fence);
            SaveSoon();
            if (controlCenter != null) controlCenter.RefreshContent();
        }

        public void RevealFence(FenceWindow fence)
        {
            visible = true;
            fence.Show();
            fence.AttachToDesktop();
            if (controlCenter != null) controlCenter.Hide();
        }

        public void OpenStorageRoot()
        {
            try
            {
                string root = ManagedStorage.GetRoot();
                Directory.CreateDirectory(root);
                Process.Start(new ProcessStartInfo(root) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AppDialog.Show(ex.Message, "無法開啟圍欄資料夾", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool IsAutoStartEnabled()
        {
            return StartupManager.IsEnabled();
        }

        public bool IsHighPriorityStartupEnabled() { return StartupManager.IsHighPriorityEnabled(); }

        public bool SetHighPriorityStartup(bool enabled)
        {
            try
            {
                StartupManager.SetHighPriority(enabled);
                if (autoStartTrayItem != null) autoStartTrayItem.IsChecked = StartupManager.IsEnabled();
                return true;
            }
            catch (Exception ex)
            {
                AppDialog.Show(I18n.T("無法設定登入排程；原有開機啟動方式已保留。") + "\n\n" + ex.Message,
                    "無法更新開機啟動", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public bool SetAutoStart(bool enabled)
        {
            try
            {
                StartupManager.SetEnabled(enabled);
                if (autoStartTrayItem != null) autoStartTrayItem.IsChecked = enabled;
                return true;
            }
            catch (Exception ex)
            {
                AppDialog.Show(ex.Message, "無法更新開機啟動", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (autoStartTrayItem != null) autoStartTrayItem.IsChecked = StartupManager.IsEnabled();
                return false;
            }
        }

        private void ReattachDesktopWindows()
        {
            ReattachDesktopWindows(false);
        }

        private void ReattachDesktopWindows(bool force)
        {
            if (peeking) return;
            IntPtr host = DesktopHost.FindDesktopHost();
            if (host == IntPtr.Zero) return;
            foreach (FenceWindow fence in fences)
            {
                if (fence.IsShellDragActive) continue;
                if (force || fence.DesktopHostHandle != host || !NativeMethods.IsWindow(fence.DesktopHostHandle))
                    fence.AttachToDesktop();
                else
                    fence.RestackDesktop();
            }
        }

        private void Exit()
        {
            exiting = true;
            saveTimer.Stop();
            if (!PreviewMode)
            {
                foreach (FenceWindow fence in fences) fence.SyncActiveTabState();
                if (store.Save(fences.Select(f => f.Model).ToList(), allowLayoutReduction))
                    allowLayoutReduction = false;
            }
            foreach (FenceWindow fence in fences.ToArray())
                fence.CloseFromManager();
            if (controlCenter != null) controlCenter.CloseForExit();
            if (tray != null) tray.Visible = false;
            app.Shutdown();
        }

        public void Dispose()
        {
            desktopTimer.Stop();
            organizerTimer.Stop();
            updateTimer.Stop();
            desktopInboxDebounceTimer.Stop();
            saveTimer.Stop();
            if (desktopInboxWatcher != null)
            {
                try { desktopInboxWatcher.EnableRaisingEvents = false; desktopInboxWatcher.Dispose(); }
                catch { }
                desktopInboxWatcher = null;
            }
            if (hotkey != null) hotkey.Dispose();
            if (tray != null) tray.Dispose();
        }
    }

    internal sealed class RuleEditorWindow : Window
    {
        private readonly DeskBoundManager manager;
        private readonly Dictionary<string, TextBox> extensionBoxes = new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TextBox> keywordBoxes = new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);

        public RuleEditorWindow(DeskBoundManager owner)
        {
            manager = owner;
            Width = 720; Height = 560; MinWidth = 620; MinHeight = 450;
            WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanResizeWithGrip; WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = "智慧分類規則"; Icon = AppBrand.Logo;
            Border shell = new Border { CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, 124, 140, 255)), Background = new SolidColorBrush(MediaColor.FromArgb(250, 17, 20, 29)), Padding = new Thickness(22), Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 28, ShadowDepth = 7, Opacity = 0.43 } };
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(68) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(58) });
            Grid header = new Grid { Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titles.Children.Add(new TextBlock { Text = "智慧分類規則", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.SemiBold });
            titles.Children.Add(new TextBlock { Text = "先比對檔名關鍵字，再依副檔名分類；使用逗號分隔多個條件", Foreground = new SolidColorBrush(MediaColor.FromArgb(150, 255, 255, 255)), FontSize = 11.5 });
            header.Children.Add(titles);
            Button close = new Button { Content = "×", Width = 34, Height = 34, FontSize = 20, Foreground = Brushes.White, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(8) };
            close.Click += delegate { Hide(); }; header.Children.Add(close); Grid.SetColumn(close, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove(); };
            root.Children.Add(header);

            Grid table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(94) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            table.RowDefinitions.Add(new RowDefinition { Height = new GridLength(34) });
            AddCell(table, new TextBlock { Text = "分類", Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center }, 0, 0);
            AddCell(table, new TextBlock { Text = "副檔名", Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center }, 0, 1);
            AddCell(table, new TextBlock { Text = "檔名關鍵字（可留空）", Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center }, 0, 2);
            int row = 1;
            foreach (string key in DesktopAutoOrganizer.CategoryKeys)
            {
                table.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) });
                AddCell(table, new TextBlock { Text = DesktopAutoOrganizer.CategoryTitle(key), Foreground = Brushes.White, FontSize = 13, VerticalAlignment = VerticalAlignment.Center }, row, 0);
                TextBox extensions = RuleBox(); extensionBoxes[key] = extensions; AddCell(table, extensions, row, 1);
                TextBox keywords = RuleBox(); keywordBoxes[key] = keywords; AddCell(table, keywords, row, 2);
                row++;
            }
            ScrollViewer scroll = new ScrollViewer { Content = table, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            scroll.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.ReadWindowsAccent());
            root.Children.Add(scroll); Grid.SetRow(scroll, 1);

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            Button reset = RuleButton("恢復預設"); reset.Click += delegate { ResetDefaults(); };
            Button save = RuleButton("儲存規則"); save.Background = new SolidColorBrush(MediaColor.FromArgb(125, 124, 140, 255)); save.Click += delegate { SaveRules(); };
            actions.Children.Add(reset); actions.Children.Add(save); root.Children.Add(actions); Grid.SetRow(actions, 2);
            shell.Child = root; Content = shell;
        }

        private static void AddCell(Grid grid, UIElement element, int row, int column)
        {
            if (element is FrameworkElement) ((FrameworkElement)element).Margin = new Thickness(5, 4, 5, 4);
            grid.Children.Add(element); Grid.SetRow(element, row); Grid.SetColumn(element, column);
        }

        private static TextBox RuleBox()
        {
            return new TextBox { FontSize = 12.5, Padding = new Thickness(8, 7, 8, 7), Foreground = Brushes.White, CaretBrush = Brushes.White, Background = new SolidColorBrush(MediaColor.FromArgb(35, 255, 255, 255)), BorderBrush = new SolidColorBrush(MediaColor.FromArgb(58, 255, 255, 255)), BorderThickness = new Thickness(1) };
        }

        private static Button RuleButton(string text)
        {
            return new Button { Content = text, Height = 36, MinWidth = 104, Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(15, 0, 15, 0), Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromArgb(45, 255, 255, 255)), BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(9), Cursor = Cursors.Hand };
        }

        public void LoadRules()
        {
            foreach (string key in DesktopAutoOrganizer.CategoryKeys)
            {
                extensionBoxes[key].Text = manager.GetOrganizerExtensionRule(key);
                keywordBoxes[key].Text = manager.GetOrganizerKeywordRule(key);
            }
        }

        private void ResetDefaults()
        {
            foreach (string key in DesktopAutoOrganizer.CategoryKeys) { extensionBoxes[key].Text = DesktopAutoOrganizer.DefaultExtensionText(key); keywordBoxes[key].Text = ""; }
        }

        private void SaveRules()
        {
            manager.SaveOrganizerRules(extensionBoxes.ToDictionary(pair => pair.Key, pair => pair.Value.Text, StringComparer.OrdinalIgnoreCase),
                keywordBoxes.ToDictionary(pair => pair.Key, pair => pair.Value.Text, StringComparer.OrdinalIgnoreCase));
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null) { T value = node as T; if (value != null) return value; node = VisualTreeHelper.GetParent(node); }
            return null;
        }
    }

    internal sealed class SceneInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime Modified { get; set; }
        public override string ToString() { return Name + "   ·   " + Modified.ToString("MM/dd  HH:mm"); }
    }

    internal sealed class SceneSwitcherWindow : Window
    {
        private readonly DeskBoundManager manager;
        private readonly ListBox list;
        private readonly TextBlock empty;

        public SceneSwitcherWindow(DeskBoundManager owner)
        {
            manager = owner;
            Width = 520; Height = 420; MinWidth = 430; MinHeight = 320;
            WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanResizeWithGrip; WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = "情境配置"; Icon = AppBrand.Logo;
            Border shell = new Border { CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, 124, 140, 255)), Background = new SolidColorBrush(MediaColor.FromArgb(250, 17, 20, 29)), Padding = new Thickness(20), Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 28, ShadowDepth = 7, Opacity = 0.43 } };
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(58) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) });
            Grid header = new Grid { Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titles.Children.Add(new TextBlock { Text = "情境配置", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.SemiBold });
            titles.Children.Add(new TextBlock { Text = "保存並快速切換工作、遊戲或學習版面", Foreground = new SolidColorBrush(MediaColor.FromArgb(145, 255, 255, 255)), FontSize = 11.5 });
            header.Children.Add(titles);
            Button close = new Button { Content = "×", Width = 34, Height = 34, FontSize = 20, Foreground = Brushes.White, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(8) };
            close.Click += delegate { Hide(); }; header.Children.Add(close); Grid.SetColumn(close, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove(); };
            root.Children.Add(header);
            Grid content = new Grid();
            list = new ListBox { Background = new SolidColorBrush(MediaColor.FromArgb(26, 255, 255, 255)), BorderBrush = new SolidColorBrush(MediaColor.FromArgb(58, 255, 255, 255)), BorderThickness = new Thickness(1), Foreground = Brushes.White, FontSize = 13.5, Padding = new Thickness(7) };
            list.MouseDoubleClick += delegate { SceneInfo scene = list.SelectedItem as SceneInfo; if (scene != null) manager.RestoreScene(scene); };
            content.Children.Add(list);
            empty = new TextBlock { Text = "尚未建立情境\n先將圍欄排成想要的樣子，再按「保存目前情境」", TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(MediaColor.FromArgb(150, 255, 255, 255)), FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
            content.Children.Add(empty); root.Children.Add(content); Grid.SetRow(content, 1);
            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            Button create = SceneButton("保存目前情境"); create.Click += delegate { manager.CreateScene(); };
            Button apply = SceneButton("切換到選取情境"); apply.Click += delegate { SceneInfo scene = list.SelectedItem as SceneInfo; if (scene != null) manager.RestoreScene(scene); };
            Button delete = SceneButton("刪除"); delete.Click += delegate { SceneInfo scene = list.SelectedItem as SceneInfo; if (scene != null) manager.DeleteScene(scene); };
            actions.Children.Add(create); actions.Children.Add(apply); actions.Children.Add(delete); root.Children.Add(actions); Grid.SetRow(actions, 2);
            shell.Child = root; Content = shell;
        }

        private static Button SceneButton(string label)
        {
            return new Button { Content = label, Height = 36, MinWidth = 84, Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(13, 0, 13, 0), Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromArgb(48, 255, 255, 255)), BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(9), Cursor = Cursors.Hand };
        }

        public void RefreshContent()
        {
            list.ItemsSource = null; list.ItemsSource = manager.GetScenes();
            empty.Visibility = list.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null) { T value = node as T; if (value != null) return value; node = VisualTreeHelper.GetParent(node); }
            return null;
        }
    }

    internal sealed class GlobalSearchResult
    {
        public FenceWindow Fence { get; set; }
        public string FenceTitle { get; set; }
        public string TabId { get; set; }
        public string TabTitle { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name + Environment.NewLine + FenceTitle + "  ›  " + TabTitle;
        }
    }

    internal sealed class GlobalSearchWindow : Window
    {
        private readonly DeskBoundManager manager;
        private readonly TextBox search;
        private readonly ListBox results;
        private readonly TextBlock hint;
        private readonly DispatcherTimer debounce;

        public GlobalSearchWindow(DeskBoundManager owner)
        {
            manager = owner;
            Width = 620; Height = 500; MinWidth = 500; MinHeight = 360;
            WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanResizeWithGrip; WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = "搜尋所有圍欄"; Icon = AppBrand.Logo;
            Border shell = new Border
            {
                CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, 124, 140, 255)),
                Background = new SolidColorBrush(MediaColor.FromArgb(250, 17, 20, 29)), Padding = new Thickness(20),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 30, ShadowDepth = 8, Opacity = 0.44 }
            };
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(52) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid header = new Grid { Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.Children.Add(new TextBlock { Text = "搜尋所有圍欄", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });
            Button close = new Button { Content = "×", Width = 34, Height = 34, FontSize = 20, Foreground = Brushes.White, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(8) };
            close.Click += delegate { Hide(); };
            header.Children.Add(close); Grid.SetColumn(close, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove(); };
            root.Children.Add(header);

            Border searchShell = new Border { CornerRadius = new CornerRadius(11), Background = new SolidColorBrush(MediaColor.FromArgb(34, 255, 255, 255)), BorderBrush = new SolidColorBrush(MediaColor.FromArgb(65, 255, 255, 255)), BorderThickness = new Thickness(1), Margin = new Thickness(0, 2, 0, 8) };
            Grid searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.Children.Add(new TextBlock { Text = "\uE721", FontFamily = new FontFamily("Segoe MDL2 Assets"), Foreground = new SolidColorBrush(MediaColor.FromArgb(190, 255, 255, 255)), FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
            search = new TextBox { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.White, CaretBrush = Brushes.White, FontSize = 14, Padding = new Thickness(0, 8, 8, 8), ToolTip = "輸入檔名、資料夾名稱或路徑" };
            searchGrid.Children.Add(search); Grid.SetColumn(search, 1); searchShell.Child = searchGrid;
            root.Children.Add(searchShell); Grid.SetRow(searchShell, 1);

            Grid content = new Grid();
            results = new ListBox { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.White, FontSize = 13, Padding = new Thickness(0, 4, 0, 4) };
            results.MouseDoubleClick += delegate { OpenSelected(); };
            results.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { OpenSelected(); e.Handled = true; } };
            content.Children.Add(results);
            hint = new TextBlock { Text = "輸入文字即可搜尋所有圍欄與分頁\n雙擊結果會切換到該位置並選取項目", TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(MediaColor.FromArgb(145, 255, 255, 255)), FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
            content.Children.Add(hint); root.Children.Add(content); Grid.SetRow(content, 2);
            shell.Child = root; Content = shell;

            debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
            debounce.Tick += delegate { debounce.Stop(); RunSearch(); };
            search.TextChanged += delegate { debounce.Stop(); debounce.Start(); };
            KeyDown += delegate(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Hide(); };
        }

        public void FocusSearch()
        {
            Dispatcher.BeginInvoke(new Action(delegate { search.Focus(); search.SelectAll(); }));
        }

        private void RunSearch()
        {
            List<GlobalSearchResult> found = manager.SearchAll(search.Text);
            results.ItemsSource = found;
            hint.Text = I18n.T(string.IsNullOrWhiteSpace(search.Text) ? "輸入文字即可搜尋所有圍欄與分頁\n雙擊結果會切換到該位置並選取項目" : "找不到符合的項目");
            hint.Visibility = found.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenSelected()
        {
            GlobalSearchResult selected = results.SelectedItem as GlobalSearchResult;
            if (selected == null) return;
            manager.RevealSearchResult(selected);
            Hide();
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null) { T value = node as T; if (value != null) return value; node = VisualTreeHelper.GetParent(node); }
            return null;
        }
    }

    internal sealed class MoveHistoryWindow : Window
    {
        private readonly DeskBoundManager manager;
        private readonly StackPanel historyList;
        private readonly TextBlock empty;
        private readonly Button undoButton;
        private readonly Dictionary<string, Border> historyCards = new Dictionary<string, Border>();
        private MoveHistoryEntry selectedEntry;

        public MoveHistoryWindow(DeskBoundManager owner)
        {
            manager = owner;
            Width = 660; Height = 520; MinWidth = 520; MinHeight = 390;
            WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanResizeWithGrip; WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = "移動與復原紀錄"; Icon = AppBrand.Logo;

            Border shell = new Border
            {
                CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(48, 65, 79)),
                Background = new LinearGradientBrush(MediaColor.FromRgb(13, 20, 28), MediaColor.FromRgb(17, 25, 35), 35),
                Padding = new Thickness(22),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 34, ShadowDepth = 9, Opacity = 0.48 }
            };
            shell.Opacity = 0;
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(64) });
            Grid header = new Grid { Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titles.Children.Add(new TextBlock { Text = "移動與復原紀錄", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White });
            titles.Children.Add(new TextBlock { Text = "選取一筆紀錄即可安全搬回原處；同名檔案不會被覆蓋", FontSize = 11.5, Margin = new Thickness(0, 5, 0, 0), Foreground = new SolidColorBrush(MediaColor.FromRgb(137, 157, 175)) });
            header.Children.Add(titles);
            Button close = new Button { Content = "×", Width = 34, Height = 34, FontSize = 20, Foreground = Brushes.White, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(8) };
            close.Click += delegate { Close(); };
            header.Children.Add(close); Grid.SetColumn(close, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove(); };
            root.Children.Add(header);

            Grid content = new Grid();
            Border contentShell = new Border
            {
                Background = new SolidColorBrush(MediaColor.FromRgb(11, 17, 24)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(34, 48, 60)), BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14), Padding = new Thickness(8)
            };
            ScrollViewer scroller = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(4, 4, 2, 4)
            };
            scroller.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.ReadWindowsAccent());
            historyList = new StackPanel();
            scroller.Content = historyList;
            contentShell.Child = scroller;
            content.Children.Add(contentShell);
            empty = new TextBlock { Text = "還沒有可復原的搬移紀錄", Foreground = new SolidColorBrush(MediaColor.FromRgb(128, 149, 167)), FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
            content.Children.Add(empty);
            root.Children.Add(content); Grid.SetRow(content, 1);

            Grid actions = new Grid { VerticalAlignment = VerticalAlignment.Bottom };
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock selectionHint = new TextBlock { Text = "最多保留最近 40 次搬移", Foreground = new SolidColorBrush(MediaColor.FromRgb(104, 126, 144)), FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
            actions.Children.Add(selectionHint);
            undoButton = new Button { Content = "先選取一筆紀錄", MinWidth = 156, Height = 40, Padding = new Thickness(18, 0, 18, 0), Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromRgb(52, 75, 84)), BorderBrush = new SolidColorBrush(MediaColor.FromRgb(70, 95, 103)), BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10), Cursor = Cursors.Hand, IsEnabled = false };
            undoButton.Click += delegate { if (selectedEntry != null) manager.UndoHistoryEntry(selectedEntry); };
            actions.Children.Add(undoButton); Grid.SetColumn(undoButton, 1);
            root.Children.Add(actions); Grid.SetRow(actions, 2);
            shell.Child = root; Content = shell;
            Loaded += delegate { shell.BeginAnimation(UIElement.OpacityProperty, UiStyles.EaseDouble(0, 1, 220)); };
        }

        public void RefreshContent()
        {
            string selectedId = selectedEntry == null ? null : selectedEntry.Id;
            IList<MoveHistoryEntry> entries = manager.GetMoveHistory();
            historyList.Children.Clear();
            historyCards.Clear();
            selectedEntry = null;
            foreach (MoveHistoryEntry entry in entries)
            {
                Border card = BuildHistoryCard(entry);
                historyCards[entry.Id] = card;
                historyList.Children.Add(card);
                if (!string.IsNullOrEmpty(selectedId) && string.Equals(entry.Id, selectedId, StringComparison.Ordinal))
                    selectedEntry = entry;
            }
            empty.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateSelectionVisuals();
        }

        private Border BuildHistoryCard(MoveHistoryEntry entry)
        {
            Border card = new Border
            {
                CornerRadius = new CornerRadius(11), BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(35, 50, 63)),
                Background = new SolidColorBrush(MediaColor.FromRgb(20, 29, 39)),
                Padding = new Thickness(12, 10, 12, 10), Margin = new Thickness(0, 0, 5, 7),
                Cursor = Cursors.Hand, Tag = entry
            };
            Grid row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Border icon = new Border
            {
                Width = 36, Height = 36, CornerRadius = new CornerRadius(11),
                Background = new LinearGradientBrush(MediaColor.FromRgb(53, 100, 100), MediaColor.FromRgb(43, 67, 86), 35),
                VerticalAlignment = VerticalAlignment.Center
            };
            icon.Child = new TextBlock { Text = "↶", Foreground = new SolidColorBrush(MediaColor.FromRgb(153, 239, 214)), FontSize = 20, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            row.Children.Add(icon);
            StackPanel copy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            copy.Children.Add(new TextBlock { Text = entry.Label, Foreground = Brushes.White, FontSize = 13.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            string names = string.Join("、", (entry.Moves ?? new List<MoveRecord>()).Take(2).Select(m => Path.GetFileName(m.DestinationPath)).Where(n => !string.IsNullOrWhiteSpace(n)));
            string details = entry.Timestamp.ToString("MM/dd  HH:mm") + "  ·  " + (entry.Moves == null ? 0 : entry.Moves.Count) + " 個項目";
            if (!string.IsNullOrWhiteSpace(names)) details += "  ·  " + names;
            copy.Children.Add(new TextBlock { Text = details, Foreground = new SolidColorBrush(MediaColor.FromRgb(126, 147, 165)), FontSize = 10.5, Margin = new Thickness(0, 4, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis });
            row.Children.Add(copy); Grid.SetColumn(copy, 1);
            TextBlock arrow = new TextBlock { Text = "›", Foreground = new SolidColorBrush(MediaColor.FromRgb(107, 135, 155)), FontSize = 24, Margin = new Thickness(12, 0, 3, 0), VerticalAlignment = VerticalAlignment.Center };
            row.Children.Add(arrow); Grid.SetColumn(arrow, 2);
            card.Child = row;
            card.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                selectedEntry = entry;
                UpdateSelectionVisuals();
                if (e.ClickCount == 2) manager.UndoHistoryEntry(entry);
            };
            card.MouseEnter += delegate { if (selectedEntry == null || selectedEntry.Id != entry.Id) card.Background = new SolidColorBrush(MediaColor.FromRgb(25, 37, 48)); };
            card.MouseLeave += delegate { if (selectedEntry == null || selectedEntry.Id != entry.Id) card.Background = new SolidColorBrush(MediaColor.FromRgb(20, 29, 39)); };
            return card;
        }

        private void UpdateSelectionVisuals()
        {
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            foreach (KeyValuePair<string, Border> pair in historyCards)
            {
                bool selected = selectedEntry != null && string.Equals(pair.Key, selectedEntry.Id, StringComparison.Ordinal);
                pair.Value.Background = selected ? new SolidColorBrush(MediaColor.FromArgb(72, accent.R, accent.G, accent.B)) : new SolidColorBrush(MediaColor.FromRgb(20, 29, 39));
                pair.Value.BorderBrush = selected ? new SolidColorBrush(MediaColor.FromArgb(225, accent.R, accent.G, accent.B)) : new SolidColorBrush(MediaColor.FromRgb(35, 50, 63));
                pair.Value.BorderThickness = selected ? new Thickness(1.5) : new Thickness(1);
            }
            undoButton.IsEnabled = selectedEntry != null;
            undoButton.Content = I18n.T(selectedEntry == null ? "先選取一筆紀錄" : "復原這筆搬移");
            undoButton.Background = selectedEntry == null
                ? new SolidColorBrush(MediaColor.FromRgb(52, 75, 84))
                : new SolidColorBrush(MediaColor.FromArgb(190, accent.R, accent.G, accent.B));
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null) { T value = node as T; if (value != null) return value; node = VisualTreeHelper.GetParent(node); }
            return null;
        }
    }

    internal sealed class ControlCenterWindow : Window
    {
        private readonly DeskBoundManager manager;
        private readonly StackPanel fenceList;
        private readonly TextBlock summaryText;
        private readonly ScrollViewer managePage;
        private readonly ScrollViewer appearancePage;
        private readonly ScrollViewer helpPage;
        private readonly StackPanel appearanceBody;
        private Border appearancePreviewShell;
        private Button manageTabButton;
        private Button appearanceTabButton;
        private Button helpTabButton;
        private CheckBox autoOrganizeCheck;
        private CheckBox desktopInboxCheck;
        private CheckBox autoUpdateCheck;
        private TextBlock updateStatusText;
        private Button updateActionButton;
        private TextBlock dashboardTitle;
        private TextBlock dashboardSubtitle;
        private string currentDashboardPage = "Manage";
        private int pageAnimationGeneration;
        private string selectedAppearanceFenceId;
        private bool rebuildingAppearance;
        private bool closeAllowed;

        public ControlCenterWindow(DeskBoundManager owner)
        {
            manager = owner;
            Title = "桌伴控制中心";
            Icon = AppBrand.Logo;
            Width = 1180;
            Height = 760;
            MinWidth = 960;
            MinHeight = 620;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = true;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Border shell = new Border
            {
                CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(39, 51, 63)),
                Background = new LinearGradientBrush(MediaColor.FromRgb(11, 17, 24), MediaColor.FromRgb(15, 23, 32),
                    new System.Windows.Point(0, 0), new System.Windows.Point(1, 1)),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 38, ShadowDepth = 10, Opacity = 0.48, Color = MediaColors.Black
                }
            };
            Grid root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(214) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            shell.Child = root; Content = shell;
            ScaleTransform windowEntranceScale = new ScaleTransform(0.985, 0.985);
            shell.RenderTransform = windowEntranceScale;
            shell.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            shell.Opacity = 0;

            Border sidebar = new Border
            {
                Background = new SolidColorBrush(MediaColor.FromRgb(14, 22, 30)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(36, 48, 59)), BorderThickness = new Thickness(0, 0, 1, 0),
                CornerRadius = new CornerRadius(16, 0, 0, 16), Child = BuildDashboardSidebar()
            };
            root.Children.Add(sidebar);

            Grid main = new Grid();
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(104) });
            main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.Children.Add(main); Grid.SetColumn(main, 1);

            Grid header = BuildDashboardHeader();
            main.Children.Add(header);

            managePage = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(34, 4, 34, 30)
            };
            managePage.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.ReadWindowsAccent());
            StackPanel body = new StackPanel();
            managePage.Content = body;
            main.Children.Add(managePage); Grid.SetRow(managePage, 1);

            Border hero = DashboardCard(22);
            hero.Margin = new Thickness(0, 0, 0, 18);
            hero.Background = new LinearGradientBrush(MediaColor.FromRgb(34, 46, 59), MediaColor.FromRgb(26, 37, 48), 12);
            Grid heroGrid = new Grid();
            heroGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            heroGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel heroCopy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            heroCopy.Children.Add(new TextBlock
            {
                Text = "你的桌面，現在井然有序。", Foreground = Brushes.White, FontSize = 22,
                FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 7)
            });
            heroCopy.Children.Add(new TextBlock
            {
                Text = "從這裡建立圍欄、快速整理桌面，或調整整套外觀。",
                Foreground = new SolidColorBrush(MediaColor.FromRgb(157, 174, 190)), FontSize = 12.5
            });
            heroGrid.Children.Add(heroCopy);
            Border heroCount = new Border
            {
                MinWidth = 128, Height = 74, Padding = new Thickness(16, 10, 16, 10), CornerRadius = new CornerRadius(13),
                Background = new SolidColorBrush(MediaColor.FromRgb(55, 68, 82)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(72, 87, 102)), BorderThickness = new Thickness(1)
            };
            StackPanel heroCountStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            summaryText = new TextBlock { Text = "0 個圍欄", Foreground = Brushes.White, FontSize = 20, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center };
            heroCountStack.Children.Add(summaryText);
            heroCountStack.Children.Add(new TextBlock { Text = "目前版面", Foreground = new SolidColorBrush(MediaColor.FromRgb(157, 174, 190)), FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) });
            heroCount.Child = heroCountStack; heroGrid.Children.Add(heroCount); Grid.SetColumn(heroCount, 1);
            hero.Child = heroGrid; AddCardMotion(hero); body.Children.Add(hero);

            body.Children.Add(DashboardSectionTitle("建立圍欄", "新增一個空白空間，或直接連結現有資料夾"));

            UniformGrid primaryActions = new UniformGrid { Columns = 2, Margin = new Thickness(-5, 0, -5, 19) };
            primaryActions.Children.Add(ActionButton("＋  新增空白圍欄", "建立乾淨區域，拖入後安全收納", true, delegate { manager.CreateBlankFence(); }));
            primaryActions.Children.Add(ActionButton("▣  新增資料夾圍欄", "直接查看指定資料夾，不搬動內容", false, delegate { manager.CreateFolderFence(); }));
            body.Children.Add(primaryActions);

            body.Children.Add(DashboardSectionTitle("快速功能", "常用操作集中在這裡，需要時再展開更多設定"));
            UniformGrid featureGroups = new UniformGrid { Columns = 2, Margin = new Thickness(-6, 0, -6, 18) };
            featureGroups.Children.Add(FeatureGroupCard("\uE7B3", "顯示與尋找", "控制圍欄可見狀態，快速找到內容",
                SmallActionButton("顯示全部", delegate { manager.SetAllVisible(true); }),
                SmallActionButton("隱藏全部", delegate { manager.SetAllVisible(false); }),
                SmallActionButton("快速查看", delegate { manager.TogglePeek(); }),
                SmallActionButton("搜尋全部", delegate { manager.ShowGlobalSearch(); })));
            featureGroups.Children.Add(FeatureGroupCard("\uE81E", "版面與快照", "排列、保存或切換整套桌面配置",
                SmallActionButton("情境配置", delegate { manager.ShowScenes(); }),
                SmallActionButton("自動排列", delegate { manager.ArrangeFences(); }),
                SmallActionButton("建立快照", delegate { manager.CreateLayoutSnapshot(); }),
                SmallActionButton("還原快照", delegate { manager.RestoreLayoutSnapshot(); })));
            featureGroups.Children.Add(FeatureGroupCard("\uE945", "智慧工具", "自動分類與桌面新項目收納",
                SmallActionButton("智慧整理", delegate { manager.OrganizeDesktopInteractive(); }),
                SmallActionButton("分類規則", delegate { manager.ShowRuleEditor(); }),
                SmallActionButton("桌面收件匣", delegate { manager.ShowDesktopInbox(); }),
                SmallActionButton("圍欄資料夾", delegate { manager.OpenStorageRoot(); })));
            featureGroups.Children.Add(FeatureGroupCard("\uE777", "內容與復原", "查看搬移紀錄，處理不再需要的版面",
                SmallActionButton("復原紀錄", delegate { manager.ShowMoveHistory(); }),
                SmallActionButton("清除全部", delegate { manager.ClearAllFences(); })));
            body.Children.Add(featureGroups);

            Border automationCard = DashboardCard(18);
            automationCard.Margin = new Thickness(0, 0, 0, 20);
            StackPanel automationBody = new StackPanel();
            automationBody.Children.Add(new TextBlock { Text = "自動化", Foreground = Brushes.White, FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            automationBody.Children.Add(new TextBlock { Text = "控制啟動方式與桌面新項目的去向", Foreground = new SolidColorBrush(MediaColor.FromRgb(139, 157, 174)), FontSize = 11.5, Margin = new Thickness(0, 0, 0, 14) });
            CheckBox autoStart = new CheckBox
            {
                Content = "登入 Windows 時自動啟動桌伴", IsChecked = manager.IsAutoStartEnabled(),
                Foreground = new SolidColorBrush(MediaColor.FromArgb(195, 255, 255, 255)), FontSize = 12,
                Margin = new Thickness(3, 0, 0, 11), Cursor = Cursors.Hand
            };
            CheckBox earlyStart = new CheckBox
            {
                Content = I18n.T("高優先啟動（登入後立即啟動）"), IsChecked = manager.IsHighPriorityStartupEnabled(),
                IsEnabled = autoStart.IsChecked == true,
                Foreground = new SolidColorBrush(MediaColor.FromArgb(195, 255, 255, 255)), FontSize = 12,
                Margin = new Thickness(22, 0, 0, 5), Cursor = Cursors.Hand
            };
            autoStart.Click += delegate
            {
                bool requested = autoStart.IsChecked == true;
                if (!manager.SetAutoStart(requested)) autoStart.IsChecked = manager.IsAutoStartEnabled();
                earlyStart.IsChecked = manager.IsHighPriorityStartupEnabled();
                earlyStart.IsEnabled = autoStart.IsChecked == true;
            };
            automationBody.Children.Add(autoStart);
            earlyStart.Click += delegate
            {
                manager.SetHighPriorityStartup(earlyStart.IsChecked == true);
                earlyStart.IsChecked = manager.IsHighPriorityStartupEnabled();
                autoStart.IsChecked = manager.IsAutoStartEnabled();
            };
            automationBody.Children.Add(earlyStart);
            automationBody.Children.Add(new TextBlock
            {
                Text = I18n.T("使用登入排程提早啟動，不提高 CPU 或管理員權限。"),
                Foreground = new SolidColorBrush(MediaColor.FromRgb(139, 157, 174)), FontSize = 11,
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(22, 0, 0, 14)
            });

            autoOrganizeCheck = new CheckBox
            {
                Content = "依檔案類型自動整理桌面新項目", IsChecked = manager.IsAutoOrganizeEnabled(),
                Foreground = new SolidColorBrush(MediaColor.FromArgb(195, 255, 255, 255)), FontSize = 12,
                Margin = new Thickness(3, 0, 0, 11), Cursor = Cursors.Hand,
                ToolTip = "圖片、文件、壓縮檔、安裝程式、影音與捷徑會進入智慧整理圍欄；資料夾不會被移動"
            };
            autoOrganizeCheck.Click += delegate
            {
                bool requested = autoOrganizeCheck.IsChecked == true;
                if (!manager.SetAutoOrganizeEnabled(requested)) autoOrganizeCheck.IsChecked = manager.IsAutoOrganizeEnabled();
                if (desktopInboxCheck != null) desktopInboxCheck.IsChecked = manager.IsDesktopInboxEnabled();
            };
            automationBody.Children.Add(autoOrganizeCheck);

            desktopInboxCheck = new CheckBox
            {
                Content = "將桌面新出現的項目自動收入「桌面收件匣」",
                IsChecked = manager.IsDesktopInboxEnabled(),
                Foreground = new SolidColorBrush(MediaColor.FromArgb(195, 255, 255, 255)), FontSize = 12,
                Margin = new Thickness(3, 0, 0, 2), Cursor = Cursors.Hand,
                ToolTip = "啟用當下的桌面內容不會移動；之後新出現的檔案與資料夾才會被收入"
            };
            desktopInboxCheck.Click += delegate
            {
                manager.SetDesktopInboxEnabled(desktopInboxCheck.IsChecked == true);
                autoOrganizeCheck.IsChecked = manager.IsAutoOrganizeEnabled();
            };
            automationBody.Children.Add(desktopInboxCheck);
            automationCard.Child = automationBody; AddCardMotion(automationCard); body.Children.Add(automationCard);

            body.Children.Add(DashboardSectionTitle("目前的圍欄", "管理顯示、內容資料夾與個別外觀"));

            fenceList = new StackPanel();
            body.Children.Add(fenceList);
            body.Children.Add(new TextBlock
            {
                Text = "快捷鍵  Ctrl + Alt + Space 顯示／隱藏  ·  Ctrl + Alt + P 快速查看圍欄  ·  按住 Alt 拖曳可暫時停用磁吸",
                Foreground = new SolidColorBrush(MediaColor.FromArgb(115, 255, 255, 255)), FontSize = 11,
                Margin = new Thickness(2, 18, 0, 0)
            });

            appearanceBody = new StackPanel();
            appearancePage = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(34, 4, 34, 30), Visibility = Visibility.Collapsed,
                Content = appearanceBody
            };
            appearancePage.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.ReadWindowsAccent());
            main.Children.Add(appearancePage); Grid.SetRow(appearancePage, 1);

            helpPage = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(34, 4, 34, 30), Visibility = Visibility.Collapsed,
                Content = BuildHelpPage()
            };
            helpPage.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.ReadWindowsAccent());
            main.Children.Add(helpPage); Grid.SetRow(helpPage, 1);

            Closing += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (!closeAllowed) { e.Cancel = true; Hide(); }
            };
            Loaded += delegate
            {
                I18n.Apply(this);
                shell.BeginAnimation(UIElement.OpacityProperty, EaseDouble(0, 1, 220, 0));
                AnimateScale(windowEntranceScale, 1.0, 260);
                AnimatePageChildren(managePage);
                AnimateDashboardHeader();
            };
        }

        private Grid BuildDashboardSidebar()
        {
            Grid sidebar = new Grid { Margin = new Thickness(16, 18, 16, 16) };
            sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(84) });
            sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid brand = new Grid { Margin = new Thickness(3, 2, 3, 10) };
            brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Border logo = new Border
            {
                Width = 42, Height = 42, CornerRadius = new CornerRadius(13),
                Background = new SolidColorBrush(MediaColor.FromRgb(35, 49, 62)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(52, 68, 82)), BorderThickness = new Thickness(1)
            };
            logo.Child = new System.Windows.Controls.Image { Source = AppBrand.Logo, Width = 34, Height = 34, Stretch = Stretch.Uniform };
            brand.Children.Add(logo);
            StackPanel brandText = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            brandText.Children.Add(new TextBlock { Text = "桌伴", Foreground = Brushes.White, FontSize = 16, FontWeight = FontWeights.Bold });
            brandText.Children.Add(new TextBlock { Text = "DeskBound", Foreground = new SolidColorBrush(MediaColor.FromRgb(138, 160, 181)), FontSize = 10.5, FontWeight = FontWeights.Medium, Margin = new Thickness(0, 2, 0, 0) });
            brand.Children.Add(brandText); Grid.SetColumn(brandText, 1);
            sidebar.Children.Add(brand);

            StackPanel navigation = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
            navigation.Children.Add(new TextBlock { Text = "工作區", Foreground = new SolidColorBrush(MediaColor.FromRgb(102, 122, 140)), FontSize = 10.5, Margin = new Thickness(10, 0, 0, 9) });
            manageTabButton = SidebarNavigationButton("sidebar-manage.png", "圍欄管理");
            appearanceTabButton = SidebarNavigationButton("sidebar-appearance.png", "外觀與排列");
            helpTabButton = SidebarNavigationButton("sidebar-help.png", "使用說明");
            manageTabButton.Click += delegate { SwitchPage("Manage"); };
            appearanceTabButton.Click += delegate { SwitchPage("Appearance"); };
            helpTabButton.Click += delegate { SwitchPage("Help"); };
            navigation.Children.Add(manageTabButton);
            navigation.Children.Add(appearanceTabButton);
            navigation.Children.Add(helpTabButton);
            sidebar.Children.Add(navigation); Grid.SetRow(navigation, 1);

            Border status = new Border
            {
                Padding = new Thickness(14, 13, 14, 13), CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(MediaColor.FromRgb(21, 31, 41)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(43, 57, 69)), BorderThickness = new Thickness(1)
            };
            StackPanel statusContent = new StackPanel();
            statusContent.Children.Add(new TextBlock { Text = "系統狀態", Foreground = new SolidColorBrush(MediaColor.FromRgb(123, 145, 164)), FontSize = 10.5 });
            statusContent.Children.Add(new TextBlock { Text = "●  桌伴正在執行", Foreground = new SolidColorBrush(MediaColor.FromRgb(104, 229, 191)), FontSize = 11.5, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 5) });
            statusContent.Children.Add(new TextBlock { Text = "版本 " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3), Foreground = new SolidColorBrush(MediaColor.FromRgb(105, 124, 141)), FontSize = 10 });
            status.Child = statusContent; sidebar.Children.Add(status); Grid.SetRow(status, 2);
            SetTabAppearance("Manage");
            return sidebar;
        }

        private Button SidebarNavigationButton(string imageName, string label)
        {
            StackPanel content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Border badge = new Border
            {
                Width = 28, Height = 28, CornerRadius = new CornerRadius(9), Margin = new Thickness(0, 0, 11, 0),
                Background = Brushes.Transparent
            };
            badge.Child = new System.Windows.Controls.Image { Source = AppBrand.EmbeddedImage(imageName), Width = 24, Height = 24, Stretch = Stretch.Uniform };
            content.Children.Add(badge);
            content.Children.Add(new TextBlock { Text = label, FontSize = 12.5, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });
            Button button = new Button
            {
                Content = content, Height = 48, Margin = new Thickness(0, 0, 0, 7), Padding = new Thickness(10, 0, 13, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left, Foreground = Brushes.White, Cursor = Cursors.Hand,
                Background = Brushes.Transparent, BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10)
            };
            AddButtonMotion(button, 1.018);
            return button;
        }

        private Grid BuildDashboardHeader()
        {
            Grid header = new Grid { Margin = new Thickness(34, 18, 22, 8), Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel copy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            dashboardSubtitle = new TextBlock
            {
                Text = I18n.DashboardDate(DateTime.Now) + "  ·  " + I18n.T("桌面控制中心"),
                Foreground = new SolidColorBrush(MediaColor.FromRgb(117, 139, 158)), FontSize = 10.5,
                Margin = new Thickness(1, 0, 0, 7)
            };
            dashboardTitle = new TextBlock
            {
                Text = I18n.T("整理桌面，也整理思緒。"), Foreground = Brushes.White, FontSize = 25,
                FontWeight = FontWeights.SemiBold
            };
            copy.Children.Add(dashboardSubtitle); copy.Children.Add(dashboardTitle); header.Children.Add(copy);

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
            Border healthy = new Border
            {
                Height = 34, Padding = new Thickness(12, 0, 12, 0), CornerRadius = new CornerRadius(11),
                Background = new SolidColorBrush(MediaColor.FromRgb(22, 39, 42)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(42, 79, 72)), BorderThickness = new Thickness(1),
                Child = new TextBlock { Text = "●  運作正常", Foreground = new SolidColorBrush(MediaColor.FromRgb(105, 229, 191)), FontSize = 11, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center }
            };
            actions.Children.Add(healthy);
            Button close = new Button
            {
                Content = "×", Width = 34, Height = 34, Margin = new Thickness(8, 0, 0, 0), Foreground = Brushes.White,
                FontSize = 20, Background = new SolidColorBrush(MediaColor.FromRgb(25, 35, 45)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(45, 58, 70)), BorderThickness = new Thickness(1),
                Style = UiStyles.GhostButton(10), Cursor = Cursors.Hand
            };
            close.Click += delegate { Hide(); }; actions.Children.Add(close);
            header.Children.Add(actions); Grid.SetColumn(actions, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove();
            };
            return header;
        }

        private Border DashboardCard(double padding)
        {
            return new Border
            {
                Padding = new Thickness(padding), CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(MediaColor.FromRgb(22, 31, 40)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(43, 56, 68)), BorderThickness = new Thickness(1)
            };
        }

        private TextBlock DashboardSectionTitle(string title, string subtitle)
        {
            return new TextBlock
            {
                Text = title + "   ·   " + subtitle, Foreground = new SolidColorBrush(MediaColor.FromRgb(201, 211, 220)),
                FontSize = 12.5, FontWeight = FontWeights.SemiBold, Margin = new Thickness(2, 0, 0, 10)
            };
        }

        private Border FeatureGroupCard(string glyph, string title, string subtitle, params Button[] actions)
        {
            Border card = DashboardCard(17);
            card.Margin = new Thickness(6);
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid heading = new Grid();
            heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Border icon = new Border
            {
                Width = 36, Height = 36, CornerRadius = new CornerRadius(11),
                Background = new SolidColorBrush(MediaColor.FromRgb(35, 41, 68)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(55, 64, 100)), BorderThickness = new Thickness(1)
            };
            icon.Child = new TextBlock
            {
                Text = glyph, FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 16,
                Foreground = new SolidColorBrush(MediaColor.FromRgb(156, 154, 255)),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            heading.Children.Add(icon);
            StackPanel copy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            copy.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold });
            copy.Children.Add(new TextBlock { Text = subtitle, Foreground = new SolidColorBrush(MediaColor.FromRgb(125, 145, 163)), FontSize = 10.5, Margin = new Thickness(0, 3, 0, 0) });
            heading.Children.Add(copy); Grid.SetColumn(copy, 1); root.Children.Add(heading);

            UniformGrid actionGrid = new UniformGrid { Columns = 2, Margin = new Thickness(-4, 13, -4, -4) };
            foreach (Button action in actions) actionGrid.Children.Add(action);
            root.Children.Add(actionGrid); Grid.SetRow(actionGrid, 1);
            card.Child = root;
            AddCardMotion(card);
            return card;
        }

        private void AddCardMotion(Border card)
        {
            ScaleTransform scale = new ScaleTransform(1, 1);
            card.RenderTransform = scale;
            card.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            SolidColorBrush initialBorder = card.BorderBrush as SolidColorBrush;
            MediaColor restingColor = initialBorder == null ? MediaColor.FromRgb(43, 56, 68) : initialBorder.Color;
            MediaColor hoverColor = MediaColor.FromArgb(restingColor.A,
                (byte)Math.Min(255, restingColor.R + 25), (byte)Math.Min(255, restingColor.G + 25), (byte)Math.Min(255, restingColor.B + 25));
            card.MouseEnter += delegate
            {
                AnimateScale(scale, 1.008, 150);
                SolidColorBrush border = card.BorderBrush as SolidColorBrush;
                if (border != null) border.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(hoverColor, TimeSpan.FromMilliseconds(150)));
            };
            card.MouseLeave += delegate
            {
                AnimateScale(scale, 1.0, 180);
                SolidColorBrush border = card.BorderBrush as SolidColorBrush;
                if (border != null) border.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(restingColor, TimeSpan.FromMilliseconds(180)));
            };
        }

        private void AddButtonMotion(Button button, double hoverScale)
        {
            ScaleTransform scale = new ScaleTransform(1, 1);
            button.RenderTransform = scale;
            button.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            button.MouseEnter += delegate { if (button.IsEnabled) AnimateScale(scale, hoverScale, 120); };
            button.MouseLeave += delegate { AnimateScale(scale, 1.0, 150); };
            button.PreviewMouseLeftButtonDown += delegate { if (button.IsEnabled) AnimateScale(scale, 0.975, 70); };
            button.PreviewMouseLeftButtonUp += delegate { if (button.IsEnabled) AnimateScale(scale, button.IsMouseOver ? hoverScale : 1.0, 110); };
        }

        private static DoubleAnimation EaseDouble(double from, double to, double milliseconds, double delay)
        {
            return new DoubleAnimation
            {
                From = from, To = to, Duration = TimeSpan.FromMilliseconds(milliseconds), BeginTime = TimeSpan.FromMilliseconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private static void AnimateScale(ScaleTransform scale, double target, double milliseconds)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, EaseDouble(scale.ScaleX, target, milliseconds, 0));
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, EaseDouble(scale.ScaleY, target, milliseconds, 0));
        }

        private void AnimatePageChildren(FrameworkElement page)
        {
            ScrollViewer viewer = page as ScrollViewer;
            Panel panel = viewer == null ? null : viewer.Content as Panel;
            if (panel == null) return;
            int index = 0;
            foreach (UIElement child in panel.Children)
            {
                if (child.Visibility != Visibility.Visible) continue;
                TranslateTransform shift = new TranslateTransform(0, 12);
                ScaleTransform retainedScale = child.RenderTransform as ScaleTransform;
                TransformGroup oldGroup = child.RenderTransform as TransformGroup;
                if (retainedScale == null && oldGroup != null)
                    retainedScale = oldGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (retainedScale != null)
                {
                    TransformGroup composed = new TransformGroup();
                    composed.Children.Add(retainedScale);
                    composed.Children.Add(shift);
                    child.RenderTransform = composed;
                }
                else child.RenderTransform = shift;
                child.Opacity = 0;
                double delay = 65 + Math.Min(index, 10) * 32;
                child.BeginAnimation(UIElement.OpacityProperty, EaseDouble(0, 1, 210, delay));
                shift.BeginAnimation(TranslateTransform.YProperty, EaseDouble(12, 0, 240, delay));
                index++;
            }
        }

        private void AnimateDashboardHeader()
        {
            if (dashboardTitle == null || dashboardSubtitle == null) return;
            TranslateTransform shift = new TranslateTransform(0, -5);
            dashboardTitle.RenderTransform = shift;
            dashboardTitle.BeginAnimation(UIElement.OpacityProperty, EaseDouble(0.25, 1, 190, 35));
            shift.BeginAnimation(TranslateTransform.YProperty, EaseDouble(-5, 0, 210, 35));
            dashboardSubtitle.BeginAnimation(UIElement.OpacityProperty, EaseDouble(0.25, 1, 180, 0));
        }

        private void AnimatePageSwitch(FrameworkElement previous, FrameworkElement next)
        {
            int generation = ++pageAnimationGeneration;
            FrameworkElement[] pages = { managePage, appearancePage, helpPage };
            foreach (FrameworkElement page in pages)
            {
                page.BeginAnimation(UIElement.OpacityProperty, null);
                if (!object.ReferenceEquals(page, previous) && !object.ReferenceEquals(page, next))
                {
                    page.Visibility = Visibility.Collapsed;
                    page.Opacity = 1;
                    page.RenderTransform = Transform.Identity;
                }
            }
            previous.Visibility = Visibility.Visible;
            previous.Opacity = 1;
            previous.IsHitTestVisible = false;
            next.Visibility = Visibility.Visible;
            next.IsHitTestVisible = true;
            next.Opacity = 0;
            TranslateTransform nextShift = new TranslateTransform(18, 0);
            TranslateTransform previousShift = new TranslateTransform(0, 0);
            next.RenderTransform = nextShift;
            previous.RenderTransform = previousShift;

            DoubleAnimation fadeOut = EaseDouble(previous.Opacity, 0, 120, 0);
            fadeOut.Completed += delegate
            {
                if (generation != pageAnimationGeneration) return;
                previous.Visibility = Visibility.Collapsed;
                previous.Opacity = 1;
                previous.RenderTransform = Transform.Identity;
            };
            previous.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            previousShift.BeginAnimation(TranslateTransform.XProperty, EaseDouble(0, -10, 130, 0));
            next.BeginAnimation(UIElement.OpacityProperty, EaseDouble(0, 1, 220, 55));
            nextShift.BeginAnimation(TranslateTransform.XProperty, EaseDouble(18, 0, 245, 55));
            AnimatePageChildren(next);
            AnimateDashboardHeader();
        }

        private Grid BuildHeader()
        {
            Grid header = new Grid { Margin = new Thickness(18, 10, 12, 4), Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border mark = new Border
            {
                Width = 40, Height = 40, CornerRadius = new CornerRadius(12),
                Background = Brushes.Transparent
            };
            mark.Child = new System.Windows.Controls.Image
            {
                Source = AppBrand.Logo, Width = 40, Height = 40, Stretch = Stretch.Uniform
            };
            header.Children.Add(mark);

            StackPanel titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titles.Children.Add(new TextBlock { Text = "桌伴", Foreground = Brushes.White, FontSize = 15, FontWeight = FontWeights.SemiBold });
            titles.Children.Add(new TextBlock
            {
                Text = "桌面圍欄控制中心", Foreground = new SolidColorBrush(MediaColor.FromArgb(145, 255, 255, 255)), FontSize = 11
            });
            header.Children.Add(titles);
            Grid.SetColumn(titles, 1);

            Button close = new Button
            {
                Content = "×", Width = 34, Height = 34, Foreground = Brushes.White, FontSize = 20,
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Style = UiStyles.GhostButton(8), Cursor = Cursors.Hand
            };
            close.Click += delegate { Hide(); };
            header.Children.Add(close);
            Grid.SetColumn(close, 2);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null)
                    DragMove();
            };
            return header;
        }

        private Grid BuildPageTabs()
        {
            Grid tabs = new Grid { Margin = new Thickness(26, 0, 26, 6) };
            tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            manageTabButton = PageTabButton("圍欄管理", "\uE8A7");
            appearanceTabButton = PageTabButton("外觀與排列", "\uE790");
            helpTabButton = PageTabButton("說明", "\uE736");
            manageTabButton.Click += delegate { SwitchPage("Manage"); };
            appearanceTabButton.Click += delegate { SwitchPage("Appearance"); };
            helpTabButton.Click += delegate { SwitchPage("Help"); };
            tabs.Children.Add(manageTabButton);
            tabs.Children.Add(appearanceTabButton);
            Grid.SetColumn(appearanceTabButton, 1);
            tabs.Children.Add(helpTabButton);
            Grid.SetColumn(helpTabButton, 2);
            SetTabAppearance("Manage");
            return tabs;
        }

        private Button PageTabButton(string label, string glyph)
        {
            StackPanel content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(new TextBlock
            {
                Text = glyph, FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                FontSize = 14, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center
            });
            content.Children.Add(new TextBlock { Text = label, FontSize = 12.5, FontWeight = FontWeights.SemiBold });
            return new Button
            {
                Content = content, Height = 38, MinWidth = 128, Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(14, 0, 14, 0), Cursor = Cursors.Hand, Foreground = Brushes.White,
                BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10)
            };
        }

        private void SwitchPage(bool appearance)
        {
            SwitchPage(appearance ? "Appearance" : "Manage");
        }

        private void SwitchPage(string page)
        {
            if (managePage == null || appearancePage == null || helpPage == null) return;
            bool appearance = string.Equals(page, "Appearance", StringComparison.OrdinalIgnoreCase);
            bool help = string.Equals(page, "Help", StringComparison.OrdinalIgnoreCase);
            string normalized = appearance ? "Appearance" : (help ? "Help" : "Manage");
            FrameworkElement next = appearance ? (FrameworkElement)appearancePage : (help ? (FrameworkElement)helpPage : managePage);
            FrameworkElement previous = string.Equals(currentDashboardPage, "Appearance", StringComparison.OrdinalIgnoreCase)
                ? (FrameworkElement)appearancePage : (string.Equals(currentDashboardPage, "Help", StringComparison.OrdinalIgnoreCase)
                    ? (FrameworkElement)helpPage : managePage);

            if (appearance) RefreshAppearance(manager.GetFences());
            SetTabAppearance(page);
            if (dashboardTitle != null)
            {
                dashboardTitle.Text = I18n.T(appearance ? "讓每個圍欄都像你的桌面。" : (help ? "需要時，再回來這裡。" : "整理桌面，也整理思緒。"));
                dashboardSubtitle.Text = I18n.DashboardDate(DateTime.Now) + "  ·  " +
                    I18n.T(appearance ? "外觀與排列" : (help ? "使用說明" : "桌面控制中心"));
            }
            if (object.ReferenceEquals(previous, next)) { AnimateDashboardHeader(); return; }
            currentDashboardPage = normalized;
            AnimatePageSwitch(previous, next);
        }

        private void SetTabAppearance(bool appearance)
        {
            SetTabAppearance(appearance ? "Appearance" : "Manage");
        }

        private void SetTabAppearance(string page)
        {
            if (manageTabButton == null || appearanceTabButton == null || helpTabButton == null) return;
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            Button[] buttons = { manageTabButton, appearanceTabButton, helpTabButton };
            string[] names = { "Manage", "Appearance", "Help" };
            for (int i = 0; i < buttons.Length; i++)
            {
                bool selected = string.Equals(page, names[i], StringComparison.OrdinalIgnoreCase);
                AnimateButtonBrush(buttons[i], Control.BackgroundProperty, selected
                    ? MediaColor.FromArgb(92, accent.R, accent.G, accent.B) : MediaColor.FromArgb(18, 255, 255, 255));
                AnimateButtonBrush(buttons[i], Control.BorderBrushProperty, selected
                    ? MediaColor.FromArgb(190, accent.R, accent.G, accent.B) : MediaColor.FromArgb(35, 255, 255, 255));
            }
        }

        private void AnimateButtonBrush(Button button, DependencyProperty property, MediaColor target)
        {
            SolidColorBrush currentBrush = button.GetValue(property) as SolidColorBrush;
            MediaColor current = currentBrush == null ? MediaColors.Transparent : currentBrush.Color;
            SolidColorBrush animated = new SolidColorBrush(current);
            button.SetValue(property, animated);
            animated.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(current, target, TimeSpan.FromMilliseconds(190))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        }

        private StackPanel BuildHelpPage()
        {
            StackPanel body = new StackPanel();
            body.Children.Add(new TextBlock
            {
                Text = "桌伴使用說明", Foreground = Brushes.White, FontSize = 20,
                FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 5)
            });
            body.Children.Add(new TextBlock
            {
                Text = "把桌面內容整理進可移動、可分頁的圍欄，同時保留真正的檔案與資料夾。",
                Foreground = new SolidColorBrush(MediaColor.FromArgb(165, 255, 255, 255)),
                FontSize = 12.5, Margin = new Thickness(0, 0, 0, 16)
            });
            body.Children.Add(BuildUpdateCard());
            body.Children.Add(HelpCard("開始使用",
                "1. 在「圍欄管理」新增空白圍欄或資料夾圍欄。\n" +
                "2. 把桌面檔案拖入空白圍欄，檔案會安全移到文件中的專用收納資料夾。\n" +
                "3. 拖出圍欄即可移回桌面或其他檔案總管資料夾。"));
            body.Children.Add(HelpCard("圍欄分頁",
                "按圍欄標題列的 ＋ 新增分頁。檔案也可以直接拖到指定分頁標籤；在分頁上按右鍵可重新命名、改用資料夾、排序或移除。移除分頁不會刪除檔案。"));
            body.Children.Add(HelpCard("快速查看圍欄",
                "按 Ctrl + Alt + P，圍欄會暫時顯示在其他程式上方，方便快速開啟或拖放檔案；再按一次就回到正常桌面層。"));
            body.Children.Add(HelpCard("搜尋、選取與滾動",
                "Ctrl + F 搜尋目前分頁；Menu 的「搜尋所有圍欄」可跨圍欄搜尋並直接定位。Ctrl + 點擊可多選；Ctrl + A 選取目前顯示的項目；Ctrl + Z 復原上一批移動。"));
            body.Children.Add(HelpCard("圍欄內瀏覽資料夾",
                "雙擊資料夾會直接在圍欄內開啟，標題列會顯示返回按鈕；拖入檔案時會放到目前正在瀏覽的子資料夾。"));
            body.Children.Add(HelpCard("桌面收件匣",
                "啟用後會近乎即時監看之後新出現在桌面的項目；啟用前已有的桌面內容不會突然被移動。可直接在收件匣標題列切換「監看中／已暫停」，新項目會安全移入文件中的專用收納資料夾。"));
            body.Children.Add(HelpCard("情境、規則與復原紀錄",
                "情境配置可保存並切換整套圍欄版面；智慧分類規則可自訂副檔名與檔名關鍵字；復原紀錄最多保留最近 40 次搬移。"));
            body.Children.Add(HelpCard("版面與顯示",
                "Win + D 或右下角顯示桌面後，圍欄應留在桌面層。Ctrl + Alt + Space 可隱藏或顯示全部。建立版面快照只保存位置與設定，不會複製或移動檔案。"));
            body.Children.Add(HelpCard("Wallpaper Engine",
                "桌伴使用獨立透明桌面視窗，不建立全螢幕遮罩。若動態桌布效能較吃緊，可到「外觀與排列」開啟動態桌布最佳化。"));
            body.Children.Add(HelpCard("資料安全",
                "刪除圍欄或分頁只會移除版面入口，不會刪除資料。發生同名檔案時會自動改名而不是覆蓋；版面設定也會保留上一版備份。"));
            body.Children.Add(new TextBlock
            {
                Text = "桌伴 " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
                Foreground = new SolidColorBrush(MediaColor.FromArgb(110, 255, 255, 255)),
                FontSize = 11, Margin = new Thickness(2, 18, 0, 0)
            });
            return body;
        }

        private Border BuildUpdateCard()
        {
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            Border card = new Border
            {
                CornerRadius = new CornerRadius(14), Margin = new Thickness(0, 0, 0, 14),
                Padding = new Thickness(17, 15, 15, 15),
                Background = new LinearGradientBrush(MediaColor.FromRgb(24, 36, 49), MediaColor.FromRgb(21, 29, 41), 16),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(120, accent.R, accent.G, accent.B)),
                BorderThickness = new Thickness(1)
            };
            Grid layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel copy = new StackPanel();
            copy.Children.Add(new TextBlock
            {
                Text = "軟體更新", Foreground = Brushes.White, FontSize = 14.5,
                FontWeight = FontWeights.SemiBold
            });
            updateStatusText = new TextBlock
            {
                Text = manager.GetUpdateStatus(), Foreground = new SolidColorBrush(MediaColor.FromRgb(153, 175, 193)),
                FontSize = 11.5, Margin = new Thickness(0, 5, 0, 9)
            };
            copy.Children.Add(updateStatusText);
            autoUpdateCheck = new CheckBox
            {
                Content = I18n.T("自動檢查更新"), IsChecked = manager.IsAutoCheckUpdatesEnabled(),
                Foreground = new SolidColorBrush(MediaColor.FromRgb(197, 210, 221)), FontSize = 11.5,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = I18n.T("啟動時及執行期間定期檢查；找到新版後會先詢問，不會強制安裝。")
            };
            autoUpdateCheck.Checked += delegate { manager.SetAutoCheckUpdatesEnabled(true); };
            autoUpdateCheck.Unchecked += delegate { manager.SetAutoCheckUpdatesEnabled(false); };
            copy.Children.Add(autoUpdateCheck);
            copy.Children.Add(new TextBlock
            {
                Text = I18n.T("啟動時及執行期間定期檢查；找到新版後會先詢問，不會強制安裝。"),
                Foreground = new SolidColorBrush(MediaColor.FromRgb(126, 148, 166)), FontSize = 10.5,
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(22, 5, 16, 0)
            });
            layout.Children.Add(copy);

            updateActionButton = new Button
            {
                Content = "檢查更新", MinWidth = 112, Height = 40, Padding = new Thickness(16, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White,
                Background = new SolidColorBrush(MediaColor.FromArgb(180, accent.R, accent.G, accent.B)),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(230, accent.R, accent.G, accent.B)),
                BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10), Cursor = Cursors.Hand
            };
            updateActionButton.Click += delegate
            {
                if (manager.GetPendingUpdateVersion() == null) manager.CheckForUpdates(true);
                else manager.InstallPendingUpdate();
            };
            AddButtonMotion(updateActionButton, 1.025);
            layout.Children.Add(updateActionButton); Grid.SetColumn(updateActionButton, 1);
            card.Child = layout;
            AddCardMotion(card);
            RefreshUpdateStatus();
            return card;
        }

        public void RefreshUpdateStatus()
        {
            if (updateStatusText != null) updateStatusText.Text = manager.GetUpdateStatus();
            if (autoUpdateCheck != null && autoUpdateCheck.IsChecked != manager.IsAutoCheckUpdatesEnabled())
                autoUpdateCheck.IsChecked = manager.IsAutoCheckUpdatesEnabled();
            if (updateActionButton != null)
            {
                string version = manager.GetPendingUpdateVersion();
                updateActionButton.Content = string.IsNullOrEmpty(version) ? "檢查更新" : "安裝 " + version;
            }
        }

        private Border HelpCard(string title, string description)
        {
            Border card = new Border
            {
                CornerRadius = new CornerRadius(12), Margin = new Thickness(0, 0, 0, 9),
                Padding = new Thickness(16, 13, 16, 14),
                Background = new SolidColorBrush(MediaColor.FromArgb(20, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(36, 255, 255, 255)),
                BorderThickness = new Thickness(1)
            };
            StackPanel content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = title, Foreground = Brushes.White, FontSize = 13.5,
                FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5)
            });
            content.Children.Add(new TextBlock
            {
                Text = description, Foreground = new SolidColorBrush(MediaColor.FromArgb(175, 255, 255, 255)),
                FontSize = 12, TextWrapping = TextWrapping.Wrap, LineHeight = 20
            });
            card.Child = content;
            AddCardMotion(card);
            return card;
        }

        public void ShowAppearance(FenceWindow fence)
        {
            if (fence != null) selectedAppearanceFenceId = fence.Model.Id;
            SwitchPage(true);
        }

        public void ShowPageForCapture(string page)
        {
            if (string.Equals(page, "AppearanceControls", StringComparison.OrdinalIgnoreCase))
            {
                SwitchPage("Appearance");
                UpdateLayout();
                appearancePage.ScrollToVerticalOffset(560);
                return;
            }
            SwitchPage(page);
        }

        private void RefreshAppearance(IList<FenceWindow> fences)
        {
            if (appearanceBody == null || rebuildingAppearance) return;
            rebuildingAppearance = true;
            try
            {
                appearanceBody.Children.Clear();
                appearanceBody.Children.Add(new TextBlock
                {
                    Text = "設計你的桌面圍欄", Foreground = Brushes.White, FontSize = 20,
                    FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 5)
                });
                appearanceBody.Children.Add(new TextBlock
                {
                    Text = "選擇圍欄後直接調整；預覽與桌面上的圍欄會同步更新。",
                    Foreground = new SolidColorBrush(MediaColor.FromArgb(165, 255, 255, 255)),
                    FontSize = 12.5, Margin = new Thickness(0, 0, 0, 16)
                });

                appearanceBody.Children.Add(SectionTitle("介面語言", "跟隨 Windows 或選擇顯示語言；切換後會重新啟動桌伴"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "跟隨系統", "繁體中文", "English" },
                    new[] { "System", "zh-TW", "en-US" }, manager.GetUiLanguage(),
                    delegate(string value) { manager.SetUiLanguage(value); }));

                if (fences.Count == 0)
                {
                    Border empty = new Border
                    {
                        Height = 150, CornerRadius = new CornerRadius(14),
                        Background = new SolidColorBrush(MediaColor.FromArgb(18, 255, 255, 255)),
                        BorderBrush = new SolidColorBrush(MediaColor.FromArgb(38, 255, 255, 255)), BorderThickness = new Thickness(1)
                    };
                    empty.Child = new TextBlock
                    {
                        Text = "目前沒有可設定的圍欄\n請先到「圍欄管理」新增一個圍欄",
                        Foreground = new SolidColorBrush(MediaColor.FromArgb(180, 255, 255, 255)), FontSize = 13,
                        TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    appearanceBody.Children.Add(empty);
                    return;
                }

                FenceWindow selectedFence = fences.FirstOrDefault(f => string.Equals(f.Model.Id, selectedAppearanceFenceId, StringComparison.OrdinalIgnoreCase));
                if (selectedFence == null) selectedFence = fences[0];
                selectedAppearanceFenceId = selectedFence.Model.Id;
                FenceModel model = selectedFence.Model;

                StackPanel pickerSection = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                pickerSection.Children.Add(new TextBlock
                {
                    Text = "正在編輯", Foreground = new SolidColorBrush(MediaColor.FromArgb(175, 255, 255, 255)),
                    FontSize = 12, Margin = new Thickness(1, 0, 0, 7)
                });
                WrapPanel picker = new WrapPanel { Margin = new Thickness(-3, 0, 0, 0) };
                foreach (FenceWindow fence in fences)
                {
                    FenceWindow choiceFence = fence;
                    bool isSelected = object.ReferenceEquals(choiceFence, selectedFence);
                    MediaColor choiceAccent = AccentPalette.Parse(choiceFence.Model.Accent);
                    Button choice = new Button
                    {
                        Content = choiceFence.Model.Title, MinWidth = 120, Height = 36, Margin = new Thickness(3, 0, 5, 5),
                        Padding = new Thickness(13, 0, 13, 0), Foreground = Brushes.White, Cursor = Cursors.Hand,
                        Background = new SolidColorBrush(isSelected
                            ? MediaColor.FromArgb(78, choiceAccent.R, choiceAccent.G, choiceAccent.B) : MediaColor.FromArgb(18, 255, 255, 255)),
                        BorderBrush = new SolidColorBrush(isSelected
                            ? MediaColor.FromArgb(220, choiceAccent.R, choiceAccent.G, choiceAccent.B) : MediaColor.FromArgb(38, 255, 255, 255)),
                        BorderThickness = new Thickness(isSelected ? 2 : 1), Style = UiStyles.GhostButton(9)
                    };
                    choice.Click += delegate
                    {
                        selectedAppearanceFenceId = choiceFence.Model.Id;
                        rebuildingAppearance = false;
                        RefreshAppearance(manager.GetFences());
                    };
                    picker.Children.Add(choice);
                }
                pickerSection.Children.Add(picker);
                appearanceBody.Children.Add(pickerSection);

                appearanceBody.Children.Add(SectionTitle("即時預覽", "顯示目前圍欄的配色、透明度、圓角與陰影"));
                appearanceBody.Children.Add(CreateAppearancePreview(model));

                appearanceBody.Children.Add(SectionTitle("圍欄樣式", "四種不同材質與邊框表現"));
                UniformGrid styles = new UniformGrid { Columns = 4, Margin = new Thickness(-4, 0, -4, 4) };
                styles.Children.Add(StyleChoiceButton(selectedFence, "晶透玻璃", "輕盈通透", "Glass"));
                styles.Children.Add(StyleChoiceButton(selectedFence, "經典柵欄", "深色實用", "Classic"));
                styles.Children.Add(StyleChoiceButton(selectedFence, "柔霧面板", "柔和厚實", "Frost"));
                styles.Children.Add(StyleChoiceButton(selectedFence, "強調框線", "清楚醒目", "Outline"));
                appearanceBody.Children.Add(styles);

                appearanceBody.Children.Add(SectionTitle("強調色", "套用到外框、選取狀態與滾動條"));
                WrapPanel colors = new WrapPanel { Margin = new Thickness(-3, 0, 0, 4) };
                AddColorChoice(colors, selectedFence, "系統", AccentPalette.ToHex(AccentPalette.ReadWindowsAccent()));
                AddColorChoice(colors, selectedFence, "藍紫", "#7C8CFF");
                AddColorChoice(colors, selectedFence, "青綠", "#36CFC9");
                AddColorChoice(colors, selectedFence, "暖橘", "#FF9F5A");
                AddColorChoice(colors, selectedFence, "玫紅", "#F06AA6");
                AddColorChoice(colors, selectedFence, "天藍", "#4DB6FF");
                AddColorChoice(colors, selectedFence, "紫晶", "#B77CFF");
                appearanceBody.Children.Add(colors);

                appearanceBody.Children.Add(SectionTitle("透明度", "拖曳時會同步更新預覽與桌面圍欄"));
                Grid opacityRow = new Grid { Margin = new Thickness(2, 0, 2, 6) };
                opacityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                opacityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(62) });
                Slider opacity = new Slider
                {
                    Minimum = 0.20, Maximum = 1.0, Value = AppearanceMath.NormalizeOpacity(model.Opacity),
                    TickFrequency = 0.01, SmallChange = 0.01, LargeChange = 0.05,
                    IsSnapToTickEnabled = false, IsMoveToPointEnabled = true,
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBlock opacityValue = new TextBlock
                {
                    Text = Math.Round(opacity.Value * 100) + "%", Foreground = Brushes.White,
                    FontSize = 12.5, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center
                };
                opacity.ValueChanged += delegate
                {
                    if (rebuildingAppearance) return;
                    model.Opacity = opacity.Value;
                    opacityValue.Text = Math.Round(opacity.Value * 100) + "%";
                    ApplyPreviewSkin(appearancePreviewShell, model.FenceStyle, AccentPalette.Parse(model.Accent), model.Opacity,
                        EffectiveCornerRadius(model), model.ShadowStyle);
                    selectedFence.ApplyAppearanceFromControlCenter(false, false);
                    manager.SaveFromControlCenter();
                };
                opacityRow.Children.Add(opacity);
                opacityRow.Children.Add(opacityValue);
                Grid.SetColumn(opacityValue, 1);
                appearanceBody.Children.Add(opacityRow);

                appearanceBody.Children.Add(SectionTitle("標題列", "可關閉圍欄名稱，保留項目數量與搜尋功能"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "顯示圍欄名稱", "隱藏圍欄名稱" },
                    new[] { "Show", "Hide" }, model.HideTitle ? "Hide" : "Show",
                    delegate(string value) { model.HideTitle = value == "Hide"; CommitAppearance(selectedFence, false, false); }));

                appearanceBody.Children.Add(SectionTitle("圖示大小", "調整圍欄內容的密度"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "小", "中", "大" },
                    new[] { 0.82, 1.0, 1.20 }, model.ItemScale,
                    delegate(double value) { model.ItemScale = value; CommitAppearance(selectedFence, true, false); }));

                appearanceBody.Children.Add(SectionTitle("檢視方式", "大量項目可改用清單；每個分頁會記住自己的選擇"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "圖示格狀", "精簡清單" },
                    new[] { "Grid", "List" }, string.IsNullOrEmpty(model.ItemView) ? "Grid" : model.ItemView,
                    delegate(string value) { model.ItemView = value; CommitAppearance(selectedFence, true, false); }));

                appearanceBody.Children.Add(SectionTitle("排列方式", "選擇圍欄內項目的預設順序"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "名稱", "最近修改", "檔案類型" },
                    new[] { "Name", "Modified", "Type" }, model.ItemSort,
                    delegate(string value) { model.ItemSort = value; CommitAppearance(selectedFence, false, true); }));

                appearanceBody.Children.Add(SectionTitle("圓角", "可覆蓋樣式原本的圓角尺寸"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "樣式預設", "俐落 8", "平衡 14", "圓潤 22" },
                    new[] { 0.0, 8.0, 14.0, 22.0 }, model.CornerRadius,
                    delegate(double value) { model.CornerRadius = value; CommitAppearance(selectedFence, false, false); }));

                appearanceBody.Children.Add(SectionTitle("陰影", "控制圍欄與桌布之間的立體層次"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "樣式預設", "關閉", "加強" },
                    new[] { "Style", "None", "Strong" }, string.IsNullOrEmpty(model.ShadowStyle) ? "Style" : model.ShadowStyle,
                    delegate(string value) { model.ShadowStyle = value; CommitAppearance(selectedFence, false, false); }));

                appearanceBody.Children.Add(SectionTitle("智慧收合", "滑鼠移開後自動縮成標題列，移回立即展開"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "手動收合", "滑鼠感應" },
                    new[] { "Manual", "Auto" }, model.AutoCollapse ? "Auto" : "Manual",
                    delegate(string value)
                    {
                        model.AutoCollapse = value == "Auto";
                        selectedFence.ApplyBehaviorSettings();
                        manager.SaveFromControlCenter();
                    }));

                appearanceBody.Children.Add(SectionTitle("動態桌布模式", "針對 Wallpaper Engine 降低昂貴陰影並保持邊框清楚"));
                appearanceBody.Children.Add(SegmentedChoices(new[] { "一般模式", "動態桌布最佳化" },
                    new[] { "Normal", "Dynamic" }, model.DynamicWallpaperMode ? "Dynamic" : "Normal",
                    delegate(string value)
                    {
                        model.DynamicWallpaperMode = value == "Dynamic";
                        CommitAppearance(selectedFence, false, false);
                    }));
            }
            finally
            {
                rebuildingAppearance = false;
                I18n.Apply(appearanceBody);
            }
        }

        private TextBlock SectionTitle(string title, string subtitle)
        {
            return new TextBlock
            {
                Text = title + "  ·  " + subtitle, Foreground = new SolidColorBrush(MediaColor.FromArgb(205, 255, 255, 255)),
                FontSize = 12.5, FontWeight = FontWeights.SemiBold, Margin = new Thickness(1, 18, 0, 9)
            };
        }

        private Border CreateAppearancePreview(FenceModel model)
        {
            Border stage = new Border
            {
                Height = 190, CornerRadius = new CornerRadius(14), Padding = new Thickness(22),
                Background = new LinearGradientBrush(MediaColor.FromRgb(31, 36, 48), MediaColor.FromRgb(19, 22, 31), 45),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(40, 255, 255, 255)), BorderThickness = new Thickness(1)
            };
            appearancePreviewShell = new Border
            {
                Width = 520, Height = 145, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center, BorderThickness = new Thickness(1)
            };
            ApplyPreviewSkin(appearancePreviewShell, model.FenceStyle, AccentPalette.Parse(model.Accent), model.Opacity,
                EffectiveCornerRadius(model), model.ShadowStyle);
            Grid preview = new Grid();
            preview.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38) });
            preview.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid previewHeader = new Grid { Margin = new Thickness(14, 0, 10, 0) };
            previewHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            previewHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            previewHeader.Children.Add(new TextBlock
            {
                Text = (model.HideTitle ? "" : model.Title + "   ") + "6 個項目", Foreground = Brushes.White, FontSize = 12.5,
                FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
            });
            previewHeader.Children.Add(new TextBlock
            {
                Text = "\uE721    \uE70E", FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                Foreground = Brushes.White, FontSize = 13, VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(previewHeader.Children[1], 1);
            preview.Children.Add(previewHeader);
            UniformGrid samples = new UniformGrid { Columns = 5, Margin = new Thickness(12, 2, 12, 12) };
            string[] glyphs = { "\uE8B7", "\uE8A5", "\uE7C3", "\uE8B5", "\uE8D4" };
            string[] labels = { "工作", "文件", "圖片", "下載", "常用" };
            for (int i = 0; i < glyphs.Length; i++)
            {
                StackPanel sample = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                sample.Children.Add(new TextBlock
                {
                    Text = glyphs[i], FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 25,
                    Foreground = new SolidColorBrush(MediaColor.FromArgb(235, 255, 255, 255)), HorizontalAlignment = HorizontalAlignment.Center
                });
                sample.Children.Add(new TextBlock
                {
                    Text = labels[i], Foreground = new SolidColorBrush(MediaColor.FromArgb(210, 255, 255, 255)),
                    FontSize = 10.5, Margin = new Thickness(0, 4, 0, 0), HorizontalAlignment = HorizontalAlignment.Center
                });
                samples.Children.Add(sample);
            }
            preview.Children.Add(samples);
            Grid.SetRow(samples, 1);
            appearancePreviewShell.Child = preview;
            stage.Child = appearancePreviewShell;
            return stage;
        }

        private Button StyleChoiceButton(FenceWindow fence, string title, string subtitle, string value)
        {
            FenceModel model = fence.Model;
            bool selected = string.Equals(model.FenceStyle, value, StringComparison.OrdinalIgnoreCase);
            StackPanel content = new StackPanel { Margin = new Thickness(5) };
            Border miniature = new Border { Height = 43, Margin = new Thickness(0, 0, 0, 7) };
            ApplyPreviewSkin(miniature, value, AccentPalette.Parse(model.Accent), model.Opacity, 9, "None");
            content.Children.Add(miniature);
            content.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 12, FontWeight = FontWeights.SemiBold });
            content.Children.Add(new TextBlock
            {
                Text = subtitle, Foreground = new SolidColorBrush(MediaColor.FromArgb(130, 255, 255, 255)), FontSize = 10.5,
                Margin = new Thickness(0, 2, 0, 0)
            });
            MediaColor accent = AccentPalette.Parse(model.Accent);
            Button button = new Button
            {
                Content = content, Height = 105, Margin = new Thickness(4), Padding = new Thickness(6), Cursor = Cursors.Hand,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(selected
                    ? MediaColor.FromArgb(76, accent.R, accent.G, accent.B) : MediaColor.FromArgb(18, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(selected
                    ? MediaColor.FromArgb(220, accent.R, accent.G, accent.B) : MediaColor.FromArgb(38, 255, 255, 255)),
                BorderThickness = new Thickness(selected ? 2 : 1), Style = UiStyles.GhostButton(12)
            };
            button.Click += delegate { model.FenceStyle = value; CommitAppearance(fence, false, false); };
            AddButtonMotion(button, 1.015);
            return button;
        }

        private void AddColorChoice(Panel panel, FenceWindow fence, string label, string value)
        {
            MediaColor color = AccentPalette.Parse(value);
            bool selected = string.Equals(AccentPalette.ToHex(AccentPalette.Parse(fence.Model.Accent)), AccentPalette.ToHex(color), StringComparison.OrdinalIgnoreCase);
            StackPanel content = new StackPanel { Margin = new Thickness(3) };
            Border swatch = new Border
            {
                Width = 27, Height = 27, CornerRadius = new CornerRadius(14), HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(color), BorderBrush = selected ? Brushes.White : new SolidColorBrush(MediaColor.FromArgb(90, 255, 255, 255)),
                BorderThickness = new Thickness(selected ? 2 : 1)
            };
            content.Children.Add(swatch);
            content.Children.Add(new TextBlock
            {
                Text = label, Foreground = Brushes.White, FontSize = 10.5, HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            });
            Button button = new Button
            {
                Content = content, Width = 72, Height = 62, Margin = new Thickness(3), Cursor = Cursors.Hand,
                Background = new SolidColorBrush(selected ? MediaColor.FromArgb(48, color.R, color.G, color.B) : MediaColor.FromArgb(15, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(selected ? MediaColor.FromArgb(190, color.R, color.G, color.B) : MediaColor.FromArgb(30, 255, 255, 255)),
                BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10)
            };
            button.Click += delegate { fence.Model.Accent = value; CommitAppearance(fence, false, false); };
            AddButtonMotion(button, 1.025);
            panel.Children.Add(button);
        }

        private UniformGrid SegmentedChoices(string[] labels, double[] values, double current, Action<double> changed)
        {
            UniformGrid grid = new UniformGrid { Columns = labels.Length, Margin = new Thickness(0, 0, 0, 5) };
            for (int i = 0; i < labels.Length; i++)
            {
                double value = values[i];
                bool selected = Math.Abs(current - value) < 0.02;
                Button button = SegmentButton(labels[i], selected);
                button.Click += delegate { changed(value); };
                grid.Children.Add(button);
            }
            return grid;
        }

        private UniformGrid SegmentedChoices(string[] labels, string[] values, string current, Action<string> changed)
        {
            UniformGrid grid = new UniformGrid { Columns = labels.Length, Margin = new Thickness(0, 0, 0, 5) };
            for (int i = 0; i < labels.Length; i++)
            {
                string value = values[i];
                bool selected = string.Equals(current, value, StringComparison.OrdinalIgnoreCase);
                Button button = SegmentButton(labels[i], selected);
                button.Click += delegate { changed(value); };
                grid.Children.Add(button);
            }
            return grid;
        }

        private Button SegmentButton(string label, bool selected)
        {
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            Button button = new Button
            {
                Content = label, Height = 38, Margin = new Thickness(0, 0, 7, 0), Cursor = Cursors.Hand,
                Foreground = Brushes.White, Background = new SolidColorBrush(selected
                    ? MediaColor.FromArgb(82, accent.R, accent.G, accent.B) : MediaColor.FromArgb(18, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(selected
                    ? MediaColor.FromArgb(210, accent.R, accent.G, accent.B) : MediaColor.FromArgb(38, 255, 255, 255)),
                BorderThickness = new Thickness(selected ? 2 : 1), Style = UiStyles.GhostButton(9)
            };
            AddButtonMotion(button, 1.015);
            return button;
        }

        private void CommitAppearance(FenceWindow fence, bool rebuildItems, bool reloadItems)
        {
            fence.ApplyAppearanceFromControlCenter(rebuildItems, reloadItems);
            manager.SaveFromControlCenter();
            rebuildingAppearance = false;
            RefreshAppearance(manager.GetFences());
        }

        private static double EffectiveCornerRadius(FenceModel model)
        {
            if (model.CornerRadius >= 4) return model.CornerRadius;
            if (string.Equals(model.FenceStyle, "Classic", StringComparison.OrdinalIgnoreCase)) return 7;
            if (string.Equals(model.FenceStyle, "Frost", StringComparison.OrdinalIgnoreCase)) return 17;
            if (string.Equals(model.FenceStyle, "Outline", StringComparison.OrdinalIgnoreCase)) return 13;
            return 14;
        }

        private static void ApplyPreviewSkin(Border shell, string style, MediaColor accent, double opacity, double radius, string shadowStyle)
        {
            if (shell == null) return;
            byte alphaTop = AppearanceMath.SurfaceAlpha(opacity);
            byte alphaBottom = AppearanceMath.SurfaceBottomAlpha(opacity);
            style = string.IsNullOrEmpty(style) ? "Glass" : style;
            if (string.Equals(style, "Classic", StringComparison.OrdinalIgnoreCase))
            {
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(alphaTop, 55, 56, 61), MediaColor.FromArgb(alphaBottom, 29, 30, 34), 90);
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(145, accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(1);
            }
            else if (string.Equals(style, "Frost", StringComparison.OrdinalIgnoreCase))
            {
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(alphaTop, 58, 63, 77),
                    MediaColor.FromArgb(alphaBottom, 31, 35, 47), 45);
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(72, 255, 255, 255));
                shell.BorderThickness = new Thickness(1);
            }
            else if (string.Equals(style, "Outline", StringComparison.OrdinalIgnoreCase))
            {
                byte outlineTint = AppearanceMath.OutlineTintAlpha(opacity);
                byte outlineBase = AppearanceMath.OutlineBaseAlpha(opacity);
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(outlineTint, accent.R, accent.G, accent.B),
                    MediaColor.FromArgb(outlineBase, 10, 13, 20), 45);
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(AppearanceMath.OutlineBorderAlpha(opacity), accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(2);
            }
            else
            {
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(alphaTop, 27, 31, 42), MediaColor.FromArgb(alphaBottom, 13, 16, 24), 45);
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(1);
            }
            shell.CornerRadius = new CornerRadius(Math.Max(4, radius));
            double shadowOpacity = string.Equals(shadowStyle, "None", StringComparison.OrdinalIgnoreCase) ? 0 :
                (string.Equals(shadowStyle, "Strong", StringComparison.OrdinalIgnoreCase) ? 0.58 : 0.34);
            shell.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = MediaColors.Black, BlurRadius = shadowOpacity > 0.5 ? 34 : 22,
                ShadowDepth = shadowOpacity > 0.5 ? 8 : 5, Opacity = shadowOpacity
            };
        }

        private Button ActionButton(string title, string subtitle, bool primary, Action action)
        {
            StackPanel content = new StackPanel { Margin = new Thickness(5, 3, 5, 3) };
            content.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold });
            content.Children.Add(new TextBlock
            {
                Text = subtitle, Foreground = new SolidColorBrush(MediaColor.FromArgb(155, 255, 255, 255)), FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0)
            });
            Button button = new Button
            {
                Content = content, Height = 86, Margin = new Thickness(5), Padding = new Thickness(15),
                HorizontalContentAlignment = HorizontalAlignment.Left, Cursor = Cursors.Hand,
                Background = primary ? new SolidColorBrush(MediaColor.FromRgb(49, 66, 85)) : new SolidColorBrush(MediaColor.FromRgb(23, 33, 43)),
                BorderBrush = primary ? new SolidColorBrush(MediaColor.FromRgb(82, 116, 151)) : new SolidColorBrush(MediaColor.FromRgb(45, 59, 72)),
                BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(14)
            };
            button.Click += delegate { action(); RefreshContent(); };
            AddButtonMotion(button, 1.012);
            return button;
        }

        private Button SmallActionButton(string label, Action action)
        {
            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            content.Children.Add(new TextBlock { Text = label, FontSize = 11.5, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });
            TextBlock arrow = new TextBlock { Text = "→", FontSize = 14, Foreground = new SolidColorBrush(MediaColor.FromRgb(123, 145, 164)), VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(arrow); Grid.SetColumn(arrow, 1);
            Button button = new Button
            {
                Content = content, Height = 52, Margin = new Thickness(4), Padding = new Thickness(13, 0, 12, 0), Cursor = Cursors.Hand,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, Foreground = Brushes.White,
                Background = new SolidColorBrush(MediaColor.FromRgb(21, 30, 39)),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(42, 55, 67)), BorderThickness = new Thickness(1),
                Style = UiStyles.GhostButton(12)
            };
            button.Click += delegate { action(); RefreshContent(); };
            AddButtonMotion(button, 1.018);
            return button;
        }

        public void RefreshContent()
        {
            if (fenceList == null) return;
            if (autoOrganizeCheck != null) autoOrganizeCheck.IsChecked = manager.IsAutoOrganizeEnabled();
            if (desktopInboxCheck != null) desktopInboxCheck.IsChecked = manager.IsDesktopInboxEnabled();
            RefreshUpdateStatus();
            IList<FenceWindow> fences = manager.GetFences();
            RefreshAppearance(fences);
            summaryText.Text = I18n.T(fences.Count + " 個圍欄");
            fenceList.Children.Clear();
            if (fences.Count == 0)
            {
                Border empty = new Border
                {
                    Height = 105, CornerRadius = new CornerRadius(12),
                    Background = new SolidColorBrush(MediaColor.FromArgb(18, 255, 255, 255)),
                    BorderBrush = new SolidColorBrush(MediaColor.FromArgb(35, 255, 255, 255)), BorderThickness = new Thickness(1)
                };
                empty.Child = new TextBlock
                {
                    Text = "目前是空白桌面\n按上方按鈕新增第一個圍欄",
                    Foreground = new SolidColorBrush(MediaColor.FromArgb(175, 255, 255, 255)), FontSize = 12.5,
                    TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                fenceList.Children.Add(empty);
                I18n.Apply(this);
                return;
            }

            foreach (FenceWindow fence in fences)
                fenceList.Children.Add(CreateFenceRow(fence));
            I18n.Apply(this);
        }

        private Border CreateFenceRow(FenceWindow fence)
        {
            Border row = new Border
            {
                CornerRadius = new CornerRadius(12), Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(16, 12, 11, 12),
                Background = new SolidColorBrush(fence.Model.IsDesktopInbox ? MediaColor.FromRgb(20, 42, 43) : MediaColor.FromRgb(21, 30, 39)),
                BorderBrush = new SolidColorBrush(fence.Model.IsDesktopInbox ? MediaColor.FromRgb(55, 116, 100) : MediaColor.FromRgb(42, 55, 67)), BorderThickness = new Thickness(1)
            };
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = (fence.Model.IsDesktopInbox ? "⇣  " : "") + (fence.Model.Locked ? "🔒  " : "") + fence.Model.Title,
                Foreground = fence.Model.IsDesktopInbox ? new SolidColorBrush(MediaColor.FromRgb(180, 255, 232)) : Brushes.White,
                FontSize = 13, FontWeight = FontWeights.SemiBold
            });
            string source;
            string contentFolder = null;
            if (!string.IsNullOrEmpty(fence.Model.PortalPath))
            {
                contentFolder = fence.Model.PortalPath;
                source = "資料夾入口 · " + fence.Model.PortalPath;
            }
            else if (!string.IsNullOrEmpty(fence.Model.ManagedPath))
            {
                contentFolder = fence.Model.ManagedPath;
                source = "受管理圍欄 · " + fence.Model.ManagedPath;
            }
            else if (fence.Model.Items != null && fence.Model.Items.Count > 0)
                source = "舊版虛擬集合 · " + fence.Model.Items.Count + " 個項目";
            else
                source = "空白圍欄 · 拖入項目後建立專屬資料夾";
            info.Children.Add(new TextBlock
            {
                Text = source, Foreground = new SolidColorBrush(MediaColor.FromArgb(130, 255, 255, 255)), FontSize = 10.5,
                TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 380, Margin = new Thickness(0, 3, 0, 0)
            });
            grid.Children.Add(info);
            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal };
            if (fence.Model.IsDesktopInbox)
            {
                Button monitor = RowButton(manager.IsDesktopInboxEnabled() ? "暫停監看" : "開始監看");
                monitor.Foreground = new SolidColorBrush(MediaColor.FromRgb(167, 248, 223));
                monitor.Click += delegate { manager.SetDesktopInboxEnabled(!manager.IsDesktopInboxEnabled()); RefreshContent(); };
                actions.Children.Add(monitor);
            }
            Button appearance = RowButton("外觀");
            appearance.Click += delegate { ShowAppearance(fence); };
            actions.Children.Add(appearance);
            Button manage = RowButton("管理");
            manage.Click += delegate { fence.OpenManagementMenu(manage); };
            actions.Children.Add(manage);
            Button show = RowButton("顯示");
            show.Click += delegate { manager.RevealFence(fence); };
            actions.Children.Add(show);
            if (!string.IsNullOrEmpty(contentFolder) && Directory.Exists(contentFolder))
            {
                string folderToOpen = contentFolder;
                Button open = RowButton("資料夾");
                open.Click += delegate { Process.Start(new ProcessStartInfo(folderToOpen) { UseShellExecute = true }); };
                actions.Children.Add(open);
            }
            Button remove = RowButton("刪除");
            remove.Click += delegate
            {
                string keepNote = !string.IsNullOrEmpty(fence.Model.ManagedPath)
                    ? "\n檔案會保留在：\n" + fence.Model.ManagedPath
                    : "\n不會刪除任何檔案。";
                MessageBoxResult result = AppDialog.Show("移除「" + fence.Model.Title + "」圍欄？" + keepNote, "刪除圍欄",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) { manager.RemoveFence(fence); RefreshContent(); }
            };
            actions.Children.Add(remove);
            grid.Children.Add(actions);
            Grid.SetColumn(actions, 1);
            row.Child = grid;
            AddCardMotion(row);
            return row;
        }

        private Button RowButton(string label)
        {
            Button button = new Button
            {
                Content = label, MinWidth = 58, Height = 31, Margin = new Thickness(6, 0, 0, 0), Cursor = Cursors.Hand,
                Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromArgb(24, 255, 255, 255)),
                BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(8)
            };
            AddButtonMotion(button, 1.025);
            return button;
        }

        public void CloseForExit()
        {
            closeAllowed = true;
            Close();
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null)
            {
                T value = node as T;
                if (value != null) return value;
                node = VisualTreeHelper.GetParent(node);
            }
            return null;
        }
    }

    internal sealed class FenceWindow : Window
    {
        private const double HeaderHeight = 44;
        private readonly DeskBoundManager manager;
        private readonly Border shell;
        private readonly Grid layout;
        private Grid headerPanel;
        private Border tabBar;
        private UniformGrid tabPanel;
        private Border tabSelectionPill;
        private TranslateTransform tabSelectionTransform;
        private int renderedActiveTabIndex = -1;
        private StackPanel headerControls;
        private TextBlock titleText;
        private TextBlock countText;
        private readonly WrapPanel itemPanel;
        private readonly ScrollViewer scroller;
        private readonly Grid contentArea;
        private readonly Border searchPanel;
        private readonly TextBox searchBox;
        private readonly TextBlock searchHint;
        private readonly List<string> allItemPaths = new List<string>();
        private readonly Border statusToast;
        private readonly TextBlock statusText;
        private readonly Border dropOverlay;
        private readonly Dictionary<string, ToggleButton> itemButtons = new Dictionary<string, ToggleButton>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> selectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private List<MoveRecord> lastMoveRecords = new List<MoveRecord>();
        private MenuItem undoMoveItem;
        private ContextMenu mainMenu;
        private System.Windows.Point itemDragStart;
        private string armedItemPath;
        private bool itemDragArmed;
        private bool shellDragActive;
        private Button rollButton;
        private Button backButton;
        private Button inboxMonitorButton;
        private Border resizeHandle;
        private readonly DispatcherTimer refreshTimer;
        private readonly DispatcherTimer statusTimer;
        private readonly DispatcherTimer autoCollapseTimer;
        private int refreshGeneration;
        private FileSystemWatcher watcher;
        private HwndSource nativeSource;
        private bool closeAllowed;
        private bool visibilityRecoveryQueued;
        private bool dragging;
        private bool resizing;
        private System.Windows.Point pointerStart;
        private double xStart, yStart, widthStart, heightStart;
        private double expandedHeight;
        private bool autoCollapsedVisual;
        private int collapseAnimationGeneration;
        private int tabTransitionGeneration;
        private bool tabTransitionPending;
        private int tabTransitionDirection;
        private string browseFolder;
        private readonly Stack<string> browseHistory = new Stack<string>();
        private string pendingSelectionPath;

        public FenceModel Model { get; private set; }
        public IntPtr DesktopHostHandle { get; private set; }
        public bool DesktopEmbedded { get; private set; }
        public bool IsShellDragActive { get { return shellDragActive; } }

        public void SaveVisualPreview(string path)
        {
            UpdateLayout();
            int width = Math.Max(1, (int)Math.Ceiling(ActualWidth));
            int height = Math.Max(1, (int)Math.Ceiling(ActualHeight));
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(this);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream output = File.Create(path)) encoder.Save(output);
        }

        public void SwitchTabForVisualTest(string id)
        {
            SwitchTab(id);
        }

        public FenceWindow(FenceModel model, DeskBoundManager owner)
        {
            Model = model;
            manager = owner;
            EnsureTabModel();
            LoadActiveTabState();
            lastMoveRecords = model.LastMoves ?? new List<MoveRecord>();
            model.LastMoves = lastMoveRecords;
            autoCollapsedVisual = model.AutoCollapse;
            expandedHeight = Math.Max(180, model.Height);
            Width = Math.Max(250, model.Width);
            Height = model.Collapsed || autoCollapsedVisual ? HeaderHeight : expandedHeight;
            EnsureModelOnScreen(model, Width, Height);
            Left = model.X;
            Top = model.Y;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Title = manager.PreviewMode ? I18n.T("桌伴預覽 - ") + model.Title : model.Title;
            Icon = AppBrand.Logo;
            ShowInTaskbar = manager.PreviewMode;
            ShowActivated = manager.PreviewMode;
            ResizeMode = ResizeMode.NoResize;
            SnapsToDevicePixels = true;

            shell = new Border
            {
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(1),
                ClipToBounds = true,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 22, ShadowDepth = 5, Opacity = 0.34, Color = MediaColors.Black
                }
            };
            layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HeaderHeight - 2) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            shell.Child = layout;
            Content = shell;

            headerPanel = BuildHeader();
            layout.Children.Add(headerPanel);
            Grid.SetRow(headerPanel, 0);

            tabBar = BuildTabBar();
            layout.Children.Add(tabBar);
            Grid.SetRow(tabBar, 1);

            contentArea = new Grid();
            contentArea.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentArea.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            searchBox = new TextBox
            {
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"), FontSize = 12.5,
                Padding = new Thickness(28, 7, 32, 7), CaretBrush = Brushes.White, ToolTip = "輸入檔名篩選"
            };
            searchHint = new TextBlock
            {
                Text = "搜尋這個圍欄…", Foreground = new SolidColorBrush(MediaColor.FromArgb(105, 255, 255, 255)),
                FontSize = 12.5, Margin = new Thickness(30, 0, 34, 0), VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            searchPanel = BuildSearchPanel();
            searchPanel.Visibility = Visibility.Collapsed;
            contentArea.Children.Add(searchPanel);
            Grid.SetRow(searchPanel, 0);

            scroller = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(10, 8, 6, 12)
            };
            scroller.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(AccentPalette.Parse(Model.Accent));
            itemPanel = new WrapPanel { Orientation = Orientation.Horizontal, Background = Brushes.Transparent };
            itemPanel.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (FindParent<ToggleButton>(e.OriginalSource as DependencyObject) == null)
                    ClearSelection();
            };
            scroller.Content = itemPanel;
            contentArea.Children.Add(scroller);
            Grid.SetRow(scroller, 1);
            dropOverlay = BuildDropOverlay();
            contentArea.Children.Add(dropOverlay);
            Grid.SetRow(dropOverlay, 1);
            statusText = new TextBlock
            {
                Foreground = Brushes.White, FontSize = 11.5, FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            statusToast = new Border
            {
                Child = statusText, Visibility = Visibility.Collapsed, IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10), Padding = new Thickness(12, 7, 12, 7), CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(MediaColor.FromArgb(225, 39, 45, 62)),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(120, 255, 255, 255)), BorderThickness = new Thickness(1)
            };
            contentArea.Children.Add(statusToast);
            Grid.SetRow(statusToast, 1);
            layout.Children.Add(contentArea);
            Grid.SetRow(contentArea, 2);

            resizeHandle = new Border
            {
                Width = 22, Height = 22, HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom, Cursor = Cursors.SizeNWSE,
                Background = Brushes.Transparent
            };
            resizeHandle.Child = new TextBlock
            {
                Text = "⌟", FontSize = 17, Foreground = new SolidColorBrush(MediaColor.FromArgb(115, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            resizeHandle.MouseLeftButtonDown += BeginResize;
            layout.Children.Add(resizeHandle);
            Grid.SetRowSpan(resizeHandle, 3);

            ApplyStyle();
            RebuildTabs();
            ApplyHeaderDisplay();
            ApplyLockState();
            ApplyCollapsedState(false);
            BuildContextMenu();
            AllowDrop = true;
            DragEnter += OnDragEnter;
            DragOver += OnDragOver;
            DragLeave += OnDragLeave;
            Drop += OnDrop;
            Loaded += OnLoaded;
            SourceInitialized += OnSourceInitialized;
            Closing += OnClosing;
            Closed += delegate { manager.Log("Fence closed: " + Model.Title); };
            PreviewMouseMove += OnPointerMove;
            PreviewMouseLeftButtonUp += EndPointerAction;
            PreviewKeyDown += delegate(object sender, KeyEventArgs e)
            {
                bool control = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (e.Key == Key.F && control)
                {
                    ToggleSearch(true); e.Handled = true;
                }
                else if (e.Key == Key.A && control && !searchBox.IsKeyboardFocusWithin)
                {
                    SelectAllVisible(); e.Handled = true;
                }
                else if (e.Key == Key.Z && control && !searchBox.IsKeyboardFocusWithin && lastMoveRecords.Count > 0)
                {
                    UndoLastMove(); e.Handled = true;
                }
                else if (e.Key == Key.Enter && !searchBox.IsKeyboardFocusWithin && selectedPaths.Count == 1)
                {
                    OpenPath(selectedPaths.First()); e.Handled = true;
                }
                else if (e.Key == Key.Escape && searchPanel.Visibility == Visibility.Visible)
                {
                    ToggleSearch(false); e.Handled = true;
                }
            };
            Deactivated += delegate { if (dragging || resizing) EndPointerAction(this, null); };

            refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            refreshTimer.Tick += delegate { refreshTimer.Stop(); RefreshItems(); };
            statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.2) };
            statusTimer.Tick += delegate { statusTimer.Stop(); HideStatusToast(); };
            autoCollapseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(650) };
            autoCollapseTimer.Tick += delegate
            {
                autoCollapseTimer.Stop();
                if (Model.AutoCollapse && !IsMouseOver && !dragging && !resizing)
                {
                    autoCollapsedVisual = true;
                    ApplyCollapsedState(true);
                }
            };
            MouseEnter += delegate
            {
                AnimateOpacity(headerControls, 1.0, 130);
                autoCollapseTimer.Stop();
                if (Model.AutoCollapse && autoCollapsedVisual)
                {
                    autoCollapsedVisual = false;
                    ApplyCollapsedState(true);
                }
            };
            MouseLeave += delegate
            {
                AnimateOpacity(headerControls, Model.IsDesktopInbox ? 0.88 : 0.58, 180);
                if (Model.AutoCollapse) { autoCollapseTimer.Stop(); autoCollapseTimer.Start(); }
            };
        }

        private Grid BuildHeader()
        {
            Grid header = new Grid { Background = Brushes.Transparent, Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Model.IsDesktopInbox ? 32 : 16) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (Model.IsDesktopInbox)
            {
                Border inboxMark = new Border
                {
                    Width = 23, Height = 23, Margin = new Thickness(6, 0, 3, 0), CornerRadius = new CornerRadius(8),
                    Background = new SolidColorBrush(MediaColor.FromArgb(105, 82, 222, 183)),
                    BorderBrush = new SolidColorBrush(MediaColor.FromArgb(175, 132, 255, 220)), BorderThickness = new Thickness(1)
                };
                inboxMark.Child = new TextBlock
                {
                    Text = "⇣", Foreground = Brushes.White, FontSize = 15, FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -2, 0, 0)
                };
                header.Children.Add(inboxMark);
                Grid.SetColumn(inboxMark, 0);
            }
            else
            {
                Shapes.Ellipse dot = new Shapes.Ellipse { Width = 7, Height = 7, Margin = new Thickness(7, 0, 0, 0) };
                dot.SetBinding(Shapes.Shape.FillProperty, new System.Windows.Data.Binding("BorderBrush") { Source = shell });
                header.Children.Add(dot);
                Grid.SetColumn(dot, 0);
            }

            StackPanel titles = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            titleText = new TextBlock
            {
                Text = Model.Title, FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"),
                FontSize = 13.5, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center
            };
            countText = new TextBlock
            {
                Text = "", FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
                Foreground = new SolidColorBrush(MediaColor.FromArgb(150, 255, 255, 255)),
                Margin = new Thickness(9, 1, 0, 0), VerticalAlignment = VerticalAlignment.Center
            };
            titles.Children.Add(titleText);
            titles.Children.Add(countText);
            header.Children.Add(titles);
            Grid.SetColumn(titles, 1);

            headerControls = new StackPanel
            {
                Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 5, 0), Opacity = Model.IsDesktopInbox ? 0.88 : 0.58
            };
            backButton = HeaderButton("\uE72B", "返回上一層資料夾");
            backButton.Visibility = Visibility.Collapsed;
            backButton.Click += delegate { NavigateBack(); };
            Button addTabButton = HeaderButton("\uE710", "新增分頁");
            addTabButton.Click += delegate { AddBlankTab(); };
            Button searchButton = HeaderButton("\uE721", "搜尋項目（Ctrl+F）");
            searchButton.Click += delegate { ToggleSearch(searchPanel.Visibility != Visibility.Visible); };
            rollButton = HeaderButton(Model.Collapsed ? "\uE70D" : "\uE70E", "收合／展開");
            rollButton.Click += delegate { ToggleCollapsed(); };
            if (Model.IsDesktopInbox)
            {
                inboxMonitorButton = new Button
                {
                    Width = 27, Height = 27, Margin = new Thickness(2, 0, 4, 0), Padding = new Thickness(0),
                    Foreground = Brushes.White, BorderThickness = new Thickness(1), Cursor = Cursors.Hand,
                    FontSize = 10.5, FontWeight = FontWeights.SemiBold, Style = UiStyles.GhostButton(9),
                    ToolTip = "開啟或暫停桌面收件匣監看"
                };
                inboxMonitorButton.Content = new Border
                {
                    Width = 8, Height = 8, CornerRadius = new CornerRadius(4),
                    BorderThickness = new Thickness(1)
                };
                inboxMonitorButton.Click += delegate
                {
                    manager.SetDesktopInboxEnabled(!manager.IsDesktopInboxEnabled());
                    UpdateInboxMonitorState();
                };
                headerControls.Children.Add(inboxMonitorButton);
                UpdateInboxMonitorState();
            }
            headerControls.Children.Add(backButton);
            headerControls.Children.Add(addTabButton);
            headerControls.Children.Add(searchButton);
            headerControls.Children.Add(rollButton);
            header.Children.Add(headerControls);
            Grid.SetColumn(headerControls, 2);

            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 2) { ToggleCollapsed(); e.Handled = true; return; }
                if (FindParent<Button>(e.OriginalSource as DependencyObject) != null) return;
                BeginDrag(e);
            };
            return header;
        }

        public void UpdateInboxMonitorState()
        {
            if (!Model.IsDesktopInbox || inboxMonitorButton == null) return;
            bool enabled = manager.IsDesktopInboxEnabled();
            Border statusDot = inboxMonitorButton.Content as Border;
            if (statusDot != null)
            {
                statusDot.Background = enabled
                    ? new SolidColorBrush(MediaColor.FromRgb(111, 239, 195)) : Brushes.Transparent;
                statusDot.BorderBrush = new SolidColorBrush(enabled
                    ? MediaColor.FromRgb(173, 255, 226) : MediaColor.FromArgb(145, 255, 255, 255));
            }
            inboxMonitorButton.ToolTip = I18n.T(enabled
                ? "正在監看桌面；點一下暫停" : "桌面監看已暫停；點一下繼續");
            inboxMonitorButton.Background = Brushes.Transparent;
            inboxMonitorButton.BorderBrush = Brushes.Transparent;
            inboxMonitorButton.Foreground = new SolidColorBrush(enabled
                ? MediaColor.FromRgb(211, 255, 241) : MediaColor.FromArgb(170, 255, 255, 255));
        }

        private Border BuildTabBar()
        {
            tabPanel = new UniformGrid { Rows = 1 };
            tabSelectionTransform = new TranslateTransform();
            Border underline = new Border
            {
                Width = 30, Height = 2.5, CornerRadius = new CornerRadius(2),
                Background = new SolidColorBrush(MediaColor.FromArgb(235, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 2)
            };
            tabSelectionPill = new Border
            {
                Height = 32, CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = tabSelectionTransform, Child = underline, IsHitTestVisible = false
            };
            Grid tabHost = new Grid { ClipToBounds = true };
            tabHost.Children.Add(tabSelectionPill);
            tabHost.Children.Add(tabPanel);
            tabPanel.SizeChanged += delegate { UpdateTabSelectionIndicator(false); };
            Border bar = new Border
            {
                Child = tabHost, Height = 40, Margin = new Thickness(9, 0, 9, 5),
                Padding = new Thickness(4, 3, 4, 3), CornerRadius = new CornerRadius(11),
                Background = new LinearGradientBrush(MediaColor.FromArgb(47, 255, 255, 255), MediaColor.FromArgb(27, 255, 255, 255), 90),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(52, 255, 255, 255)),
                BorderThickness = new Thickness(1)
            };
            return bar;
        }

        private void RebuildTabs()
        {
            if (tabPanel == null || tabBar == null) return;
            tabPanel.Children.Clear();
            foreach (FenceTabModel tab in Model.Tabs)
            {
                bool active = string.Equals(tab.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase);
                MediaColor accent = AccentPalette.Parse(string.IsNullOrEmpty(tab.Accent) ? Model.Accent : tab.Accent);
                TextBlock tabLabel = new TextBlock
                {
                    Text = tab.Title, TextAlignment = TextAlignment.Center,
                    FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"),
                    FontSize = 14, FontWeight = active ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = Brushes.White, TextWrapping = TextWrapping.NoWrap
                };
                Viewbox tabLabelHost = new Viewbox
                {
                    Child = tabLabel, Stretch = Stretch.Uniform, StretchDirection = StretchDirection.Both,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    MaxHeight = 19, Margin = new Thickness(10, 3, 10, 3)
                };
                Grid tabContent = new Grid();
                tabContent.Children.Add(tabLabelHost);
                Button button = new Button
                {
                    Content = tabContent,
                    Tag = tab, Height = 32, HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Padding = new Thickness(0), Margin = new Thickness(2, 0, 2, 0),
                    Foreground = active ? Brushes.White : new SolidColorBrush(MediaColor.FromArgb(215, 255, 255, 255)), FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"),
                    Background = Brushes.Transparent, BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    Style = UiStyles.GhostButton(8), ToolTip = TabFolderDescription(tab), AllowDrop = true,
                    Effect = null
                };
                if (active)
                {
                    ScaleTransform activeScale = new ScaleTransform(0.96, 0.96);
                    button.RenderTransform = activeScale;
                    button.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    activeScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                        CreateEaseAnimation(0.96, 1.0, 170));
                    activeScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                        CreateEaseAnimation(0.96, 1.0, 170));
                }
                button.Click += delegate { SwitchTab(tab.Id); };
                button.DragOver += delegate(object sender, DragEventArgs e)
                {
                    e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Move : DragDropEffects.None;
                    e.Handled = true;
                };
                button.Drop += delegate(object sender, DragEventArgs e) { DropOnTab(tab, e); };
                button.ContextMenu = BuildTabContextMenu(tab);
                tabPanel.Children.Add(button);
            }
            UpdateTabSelectionIndicator(renderedActiveTabIndex >= 0);
            tabBar.Visibility = Model.Collapsed || Model.Tabs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateTabSelectionIndicator(bool animate)
        {
            if (tabPanel == null || tabSelectionPill == null || tabSelectionTransform == null || Model.Tabs == null || Model.Tabs.Count == 0) return;
            int targetIndex = Model.Tabs.FindIndex(tab => string.Equals(tab.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase));
            if (targetIndex < 0) targetIndex = 0;
            int capturedIndex = targetIndex;
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (tabPanel.ActualWidth <= 0 || Model.Tabs.Count == 0) return;
                double cellWidth = tabPanel.ActualWidth / Model.Tabs.Count;
                double targetWidth = Math.Max(20, cellWidth - 4);
                double targetX = capturedIndex * cellWidth + 2;
                MediaColor accent = AccentPalette.Parse(string.IsNullOrEmpty(Model.Tabs[capturedIndex].Accent) ? Model.Accent : Model.Tabs[capturedIndex].Accent);
                tabSelectionPill.Background = new LinearGradientBrush(MediaColor.FromArgb(165, accent.R, accent.G, accent.B), MediaColor.FromArgb(94, accent.R, accent.G, accent.B), 90);
                tabSelectionPill.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(220, accent.R, accent.G, accent.B));
                tabSelectionPill.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = accent, BlurRadius = 11, ShadowDepth = 0, Opacity = 0.28 };

                double fromX = tabSelectionTransform.X;
                double fromWidth = double.IsNaN(tabSelectionPill.Width) ? targetWidth : tabSelectionPill.Width;
                tabSelectionTransform.BeginAnimation(TranslateTransform.XProperty, null);
                tabSelectionPill.BeginAnimation(FrameworkElement.WidthProperty, null);
                tabSelectionTransform.X = targetX;
                tabSelectionPill.Width = targetWidth;
                if (animate && renderedActiveTabIndex >= 0 && renderedActiveTabIndex != capturedIndex)
                {
                    tabSelectionTransform.BeginAnimation(TranslateTransform.XProperty, CreateEaseAnimation(fromX, targetX, 240));
                    tabSelectionPill.BeginAnimation(FrameworkElement.WidthProperty, CreateEaseAnimation(fromWidth, targetWidth, 220));
                    tabSelectionPill.BeginAnimation(UIElement.OpacityProperty, CreateEaseAnimation(0.72, 1, 190));
                }
                else tabSelectionPill.Opacity = 1;
                renderedActiveTabIndex = capturedIndex;
            }), DispatcherPriority.Loaded);
        }

        private ContextMenu BuildTabContextMenu(FenceTabModel tab)
        {
            ContextMenu menu = new ContextMenu();
            UiStyles.PrepareDarkContextMenu(menu, AccentPalette.Parse(Model.Accent));
            MenuItem rename = new MenuItem { Header = "重新命名分頁" };
            rename.Click += delegate { RenameTab(tab); };
            MenuItem folder = new MenuItem { Header = "改用現有資料夾…" };
            folder.Click += delegate { ChooseFolderForTab(tab); };
            MenuItem open = new MenuItem { Header = "開啟分頁資料夾", IsEnabled = Directory.Exists(GetTabContentFolder(tab)) };
            open.Click += delegate { OpenPath(GetTabContentFolder(tab)); };
            MenuItem left = new MenuItem { Header = "向左移", IsEnabled = Model.Tabs.IndexOf(tab) > 0 };
            left.Click += delegate { MoveTab(tab, -1); };
            MenuItem right = new MenuItem { Header = "向右移", IsEnabled = Model.Tabs.IndexOf(tab) < Model.Tabs.Count - 1 };
            right.Click += delegate { MoveTab(tab, 1); };
            MenuItem remove = new MenuItem { Header = "移除分頁", IsEnabled = Model.Tabs.Count > 1 };
            remove.Click += delegate { RemoveTab(tab); };
            menu.Items.Add(rename);
            menu.Items.Add(folder);
            menu.Items.Add(open);
            menu.Items.Add(new Separator());
            menu.Items.Add(left);
            menu.Items.Add(right);
            menu.Items.Add(new Separator());
            menu.Items.Add(remove);
            I18n.Apply(menu);
            return menu;
        }

        private static string TabFolderDescription(FenceTabModel tab)
        {
            string path = !string.IsNullOrEmpty(tab.PortalPath) ? tab.PortalPath : tab.ManagedPath;
            return string.IsNullOrEmpty(path) ? tab.Title + "（空白分頁）" : tab.Title + Environment.NewLine + path;
        }

        private Button HeaderButton(string glyph, string tip)
        {
            return new Button
            {
                Content = glyph, ToolTip = tip, Width = 30, Height = 30, Margin = new Thickness(1, 0, 1, 0),
                FontSize = 14, FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), Foreground = Brushes.White,
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                Padding = new Thickness(0), Style = UiStyles.GhostButton(7)
            };
        }

        private Border BuildSearchPanel()
        {
            Border panel = new Border
            {
                Height = 38, Margin = new Thickness(10, 2, 10, 2), CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(MediaColor.FromArgb(36, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(52, 255, 255, 255)), BorderThickness = new Thickness(1)
            };
            Grid grid = new Grid();
            grid.Children.Add(searchBox);
            grid.Children.Add(searchHint);
            grid.Children.Add(new TextBlock
            {
                Text = "⌕", FontFamily = new FontFamily("Segoe UI Symbol"), FontSize = 16,
                Foreground = new SolidColorBrush(MediaColor.FromArgb(180, 255, 255, 255)),
                Margin = new Thickness(9, 0, 0, 1), HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false
            });
            Button close = new Button
            {
                Content = "×", Width = 28, Height = 28, Margin = new Thickness(0, 0, 4, 0),
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White, Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                FontSize = 16, Cursor = Cursors.Hand, Style = UiStyles.GhostButton(7)
            };
            close.Click += delegate { ToggleSearch(false); };
            grid.Children.Add(close);
            panel.Child = grid;
            searchBox.TextChanged += delegate
            {
                searchHint.Visibility = string.IsNullOrEmpty(searchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
                RenderItems();
            };
            return panel;
        }

        private Border BuildDropOverlay()
        {
            MediaColor accent = AccentPalette.Parse(Model.Accent);
            StackPanel content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            content.Children.Add(new TextBlock
            {
                Text = "\uE898", FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                FontSize = 34, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            content.Children.Add(new TextBlock
            {
                Text = "放入這個圍欄", FontSize = 14, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center
            });
            return new Border
            {
                Child = content, Margin = new Thickness(10, 6, 10, 10), CornerRadius = new CornerRadius(13),
                Background = new SolidColorBrush(MediaColor.FromArgb(218, 18, 23, 34)),
                BorderBrush = new SolidColorBrush(MediaColor.FromArgb(245, accent.R, accent.G, accent.B)),
                BorderThickness = new Thickness(2), Visibility = Visibility.Collapsed, Opacity = 0,
                IsHitTestVisible = false
            };
        }

        private static DoubleAnimation CreateEaseAnimation(double from, double to, double milliseconds)
        {
            return new DoubleAnimation
            {
                From = from, To = to, Duration = TimeSpan.FromMilliseconds(milliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private static void AnimateOpacity(UIElement element, double target, double milliseconds)
        {
            if (element == null) return;
            element.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(target, TimeSpan.FromMilliseconds(milliseconds))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        }

        private static void AnimateScale(ScaleTransform transform, double target, double milliseconds)
        {
            if (transform == null) return;
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateEaseAnimation(transform.ScaleX, target, milliseconds));
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateEaseAnimation(transform.ScaleY, target, milliseconds));
        }

        private void ToggleSearch(bool show)
        {
            if (show && Model.Collapsed)
            {
                Model.Collapsed = false;
                ApplyCollapsedState(true);
                manager.SaveSoon();
            }
            searchPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (show)
                Dispatcher.BeginInvoke(new Action(delegate { searchBox.Focus(); searchBox.SelectAll(); }));
            else
            {
                searchBox.Text = "";
                Keyboard.ClearFocus();
            }
        }

        private void EnsureTabModel()
        {
            if (Model.Tabs == null) Model.Tabs = new List<FenceTabModel>();
            if (Model.Tabs.Count == 0)
            {
                FenceTabModel migrated = new FenceTabModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Title = string.IsNullOrWhiteSpace(Model.Title) ? "主要" : Model.Title,
                    Accent = Model.Accent,
                    PortalPath = Model.PortalPath,
                    ManagedPath = Model.ManagedPath,
                    ItemView = Model.ItemView,
                    Items = Model.Items ?? new List<string>(),
                    LastMoves = Model.LastMoves ?? new List<MoveRecord>()
                };
                Model.Tabs.Add(migrated);
                Model.ActiveTabId = migrated.Id;
            }
            foreach (FenceTabModel tab in Model.Tabs)
            {
                if (string.IsNullOrEmpty(tab.Id)) tab.Id = Guid.NewGuid().ToString("N");
                if (string.IsNullOrWhiteSpace(tab.Title)) tab.Title = I18n.T("分頁");
                if (string.IsNullOrEmpty(tab.Accent)) tab.Accent = Model.Accent;
                if (tab.Items == null) tab.Items = new List<string>();
                if (tab.LastMoves == null) tab.LastMoves = new List<MoveRecord>();
                if (string.IsNullOrEmpty(tab.ItemView)) tab.ItemView = "Grid";
            }
            if (!Model.Tabs.Any(t => string.Equals(t.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase)))
                Model.ActiveTabId = Model.Tabs[0].Id;
        }

        private FenceTabModel ActiveTab()
        {
            if (Model.Tabs == null) return null;
            return Model.Tabs.FirstOrDefault(t => string.Equals(t.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase));
        }

        public void SyncActiveTabState()
        {
            FenceTabModel active = ActiveTab();
            if (active == null) return;
            active.PortalPath = Model.PortalPath;
            active.ManagedPath = Model.ManagedPath;
            active.Items = Model.Items ?? new List<string>();
            active.LastMoves = lastMoveRecords ?? new List<MoveRecord>();
            active.ItemView = string.IsNullOrEmpty(Model.ItemView) ? "Grid" : Model.ItemView;
            if (string.IsNullOrEmpty(active.Accent)) active.Accent = Model.Accent;
        }

        private void LoadActiveTabState()
        {
            FenceTabModel active = ActiveTab();
            if (active == null) return;
            Model.PortalPath = active.PortalPath;
            Model.ManagedPath = active.ManagedPath;
            Model.Items = active.Items ?? new List<string>();
            Model.LastMoves = active.LastMoves ?? new List<MoveRecord>();
            Model.ItemView = string.IsNullOrEmpty(active.ItemView) ? "Grid" : active.ItemView;
            active.Items = Model.Items;
            active.LastMoves = Model.LastMoves;
            lastMoveRecords = Model.LastMoves;
        }

        private void SwitchTab(string id)
        {
            if (string.IsNullOrEmpty(id) || string.Equals(Model.ActiveTabId, id, StringComparison.OrdinalIgnoreCase)) return;
            int oldIndex = Model.Tabs.FindIndex(tab => string.Equals(tab.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase));
            int newIndex = Model.Tabs.FindIndex(tab => string.Equals(tab.Id, id, StringComparison.OrdinalIgnoreCase));
            int direction = newIndex >= oldIndex ? 1 : -1;
            int generation = ++tabTransitionGeneration;
            TranslateTransform slide = contentArea.RenderTransform as TranslateTransform;
            if (slide == null)
            {
                slide = new TranslateTransform();
                contentArea.RenderTransform = slide;
            }
            DoubleAnimation fadeOut = CreateEaseAnimation(contentArea.Opacity, 0, 95);
            DoubleAnimation moveOut = CreateEaseAnimation(slide.X, -7 * direction, 95);
            fadeOut.Completed += delegate
            {
                if (generation != tabTransitionGeneration) return;
                SyncActiveTabState();
                Model.ActiveTabId = id;
                LoadActiveTabState();
                ResetBrowseState();
                searchBox.Text = "";
                searchPanel.Visibility = Visibility.Collapsed;
                ClearSelection();
                ConfigureWatcher();
                RebuildTabs();
                BuildContextMenu();
                tabTransitionPending = true;
                tabTransitionDirection = direction;
                contentArea.BeginAnimation(UIElement.OpacityProperty, null);
                contentArea.Opacity = 0;
                slide.BeginAnimation(TranslateTransform.XProperty, null);
                slide.X = 7 * direction;
                RefreshItems();
                manager.SaveSoon();
            };
            contentArea.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            slide.BeginAnimation(TranslateTransform.XProperty, moveOut);
        }

        private void AnimateTabContentIn()
        {
            TranslateTransform slide = contentArea.RenderTransform as TranslateTransform;
            if (slide == null)
            {
                slide = new TranslateTransform(7 * tabTransitionDirection, 0);
                contentArea.RenderTransform = slide;
            }
            contentArea.BeginAnimation(UIElement.OpacityProperty, CreateEaseAnimation(0, 1, 175));
            slide.BeginAnimation(TranslateTransform.XProperty, CreateEaseAnimation(7 * tabTransitionDirection, 0, 190));
        }

        private void AddBlankTab()
        {
            SyncActiveTabState();
            FenceTabModel tab = new FenceTabModel
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = I18n.T("分頁") + " " + (Model.Tabs.Count + 1),
                Accent = Model.Accent
            };
            Model.Tabs.Add(tab);
            Model.ActiveTabId = tab.Id;
            LoadActiveTabState();
            ResetBrowseState();
            RebuildTabs();
            ConfigureWatcher();
            RefreshItems();
            BuildContextMenu();
            manager.SaveCritical();
            ShowStatus("＋ 已新增「" + tab.Title + "」");
        }

        private void AddFolderTab()
        {
            using (Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = I18n.T("選擇新分頁要顯示的資料夾");
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
                SyncActiveTabState();
                FenceTabModel tab = new FenceTabModel
                {
                    Id = Guid.NewGuid().ToString("N"), Title = new DirectoryInfo(dialog.SelectedPath).Name,
                    Accent = Model.Accent, PortalPath = dialog.SelectedPath
                };
                Model.Tabs.Add(tab);
                Model.ActiveTabId = tab.Id;
                LoadActiveTabState();
                ResetBrowseState();
                RebuildTabs();
                ConfigureWatcher();
                RefreshItems();
                BuildContextMenu();
                manager.SaveCritical();
            }
        }

        private void RenameTab(FenceTabModel tab)
        {
            RenameDialog dialog = new RenameDialog(tab.Title);
            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.Value)) return;
            tab.Title = dialog.Value.Trim();
            RebuildTabs();
            manager.SaveSoon();
        }

        private void ChooseFolderForTab(FenceTabModel tab)
        {
            using (Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = I18n.T("選擇「" + tab.Title + "」分頁要顯示的資料夾");
                dialog.ShowNewFolderButton = true;
                if (!string.IsNullOrEmpty(tab.PortalPath) && Directory.Exists(tab.PortalPath)) dialog.SelectedPath = tab.PortalPath;
                if (dialog.ShowDialog() != Forms.DialogResult.OK) return;
                tab.PortalPath = dialog.SelectedPath;
                tab.Items.Clear();
                if (string.Equals(tab.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase))
                {
                    LoadActiveTabState(); ResetBrowseState(); ConfigureWatcher(); RefreshItems();
                }
                RebuildTabs();
                manager.SaveCritical();
            }
        }

        private void MoveTab(FenceTabModel tab, int direction)
        {
            int oldIndex = Model.Tabs.IndexOf(tab);
            int newIndex = oldIndex + direction;
            if (oldIndex < 0 || newIndex < 0 || newIndex >= Model.Tabs.Count) return;
            Model.Tabs.RemoveAt(oldIndex);
            Model.Tabs.Insert(newIndex, tab);
            RebuildTabs();
            manager.SaveSoon();
        }

        private void RemoveTab(FenceTabModel tab)
        {
            if (Model.Tabs.Count <= 1) return;
            string kept = GetTabContentFolder(tab);
            string note = string.IsNullOrEmpty(kept) ? "不會刪除任何檔案。" : "檔案會保留在：\n" + kept;
            if (AppDialog.Show("移除分頁「" + tab.Title + "」？\n" + note, "移除分頁",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            bool active = string.Equals(tab.Id, Model.ActiveTabId, StringComparison.OrdinalIgnoreCase);
            if (active) SyncActiveTabState();
            int index = Model.Tabs.IndexOf(tab);
            Model.Tabs.Remove(tab);
            if (active)
            {
                Model.ActiveTabId = Model.Tabs[Math.Min(index, Model.Tabs.Count - 1)].Id;
                LoadActiveTabState(); ResetBrowseState(); ConfigureWatcher(); ClearSelection(); RefreshItems();
            }
            RebuildTabs();
            BuildContextMenu();
            manager.SaveCritical();
        }

        private string GetTabContentFolder(FenceTabModel tab)
        {
            if (tab == null) return null;
            if (!string.IsNullOrEmpty(tab.PortalPath) && Directory.Exists(tab.PortalPath)) return tab.PortalPath;
            if (!string.IsNullOrEmpty(tab.ManagedPath) && Directory.Exists(tab.ManagedPath)) return tab.ManagedPath;
            return null;
        }

        private void DropOnTab(FenceTabModel tab, DragEventArgs e)
        {
            ShowDropOverlay(false);
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            string destination;
            try { destination = !string.IsNullOrEmpty(tab.PortalPath) && Directory.Exists(tab.PortalPath) ? tab.PortalPath : ManagedStorage.EnsureFolder(Model, tab); }
            catch (Exception ex)
            {
                AppDialog.Show(ex.Message, "無法建立分頁資料夾", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (!string.Equals(Model.ActiveTabId, tab.Id, StringComparison.OrdinalIgnoreCase)) SwitchTab(tab.Id);
            countText.Text = I18n.T("正在移入「" + tab.Title + "」…");
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            Task.Factory.StartNew(delegate { return ManagedStorage.MoveInto(paths, destination); })
                .ContinueWith(task => HandleMoveResult(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BuildContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            UiStyles.PrepareDarkContextMenu(menu, AccentPalette.Parse(Model.Accent));
            MenuItem center = new MenuItem { Header = "桌伴控制中心…" };
            center.Click += delegate { manager.ShowControlCenter(); };
            MenuItem globalSearch = new MenuItem { Header = "搜尋所有圍欄…" };
            globalSearch.Click += delegate { manager.ShowGlobalSearch(); };
            MenuItem history = new MenuItem { Header = "移動與復原紀錄…" };
            history.Click += delegate { manager.ShowMoveHistory(); };
            MenuItem scenes = new MenuItem { Header = "情境配置…" };
            scenes.Click += delegate { manager.ShowScenes(); };
            MenuItem rules = new MenuItem { Header = "智慧分類規則…" };
            rules.Click += delegate { manager.ShowRuleEditor(); };
            MenuItem inbox = new MenuItem { Header = "開啟桌面收件匣" };
            inbox.Click += delegate { manager.ShowDesktopInbox(); };
            MenuItem inboxMonitoring = null;
            if (Model.IsDesktopInbox)
            {
                inboxMonitoring = new MenuItem
                {
                    Header = "監看桌面新項目", IsCheckable = true, IsChecked = manager.IsDesktopInboxEnabled()
                };
                inboxMonitoring.Click += delegate { manager.SetDesktopInboxEnabled(!manager.IsDesktopInboxEnabled()); BuildContextMenu(); };
            }
            MenuItem add = new MenuItem { Header = "新增空白圍欄" };
            add.Click += delegate { manager.CreateBlankFence(); };
            MenuItem addFolder = new MenuItem { Header = "新增資料夾圍欄…" };
            addFolder.Click += delegate { manager.CreateFolderFence(); };
            MenuItem rename = new MenuItem { Header = "重新命名" };
            rename.Click += delegate { Rename(); };
            MenuItem folder = new MenuItem { Header = "改用現有資料夾…" };
            folder.Click += delegate { ChooseFolder(); };
            MenuItem openFolder = new MenuItem { Header = "開啟圍欄資料夾", IsEnabled = Directory.Exists(GetContentFolder()) };
            openFolder.Click += delegate { OpenPath(GetContentFolder()); };
            MenuItem refresh = new MenuItem { Header = "重新整理" };
            refresh.Click += delegate { RefreshItems(); };
            MenuItem search = new MenuItem { Header = "搜尋項目…    Ctrl+F" };
            search.Click += delegate { ToggleSearch(true); };
            MenuItem collapse = new MenuItem { Header = "收合／展開" };
            collapse.Click += delegate { ToggleCollapsed(); };
            MenuItem lockFence = new MenuItem { Header = "鎖定位置與大小", IsCheckable = true, IsChecked = Model.Locked };
            lockFence.Click += delegate
            {
                Model.Locked = !Model.Locked;
                ApplyLockState();
                BuildContextMenu();
                manager.SaveSoon();
            };
            undoMoveItem = new MenuItem { Header = "復原上次移動    Ctrl+Z", IsEnabled = lastMoveRecords.Count > 0 };
            undoMoveItem.Click += delegate { UndoLastMove(); };

            MenuItem tabsMenu = new MenuItem { Header = "分頁" };
            MenuItem newTab = new MenuItem { Header = "新增空白分頁" };
            newTab.Click += delegate { AddBlankTab(); };
            MenuItem newFolderTab = new MenuItem { Header = "新增資料夾分頁…" };
            newFolderTab.Click += delegate { AddFolderTab(); };
            FenceTabModel activeTab = ActiveTab();
            MenuItem renameTab = new MenuItem { Header = "重新命名目前分頁…", IsEnabled = activeTab != null };
            renameTab.Click += delegate { if (activeTab != null) RenameTab(activeTab); };
            MenuItem removeTab = new MenuItem { Header = "移除目前分頁", IsEnabled = activeTab != null && Model.Tabs.Count > 1 };
            removeTab.Click += delegate { if (activeTab != null) RemoveTab(activeTab); };
            tabsMenu.Items.Add(newTab);
            tabsMenu.Items.Add(newFolderTab);
            tabsMenu.Items.Add(new Separator());
            tabsMenu.Items.Add(renameTab);
            tabsMenu.Items.Add(removeTab);

            MenuItem sorting = new MenuItem { Header = "排序" };
            AddSortItem(sorting, "名稱（資料夾優先）", "Name");
            AddSortItem(sorting, "最近修改", "Modified");
            AddSortItem(sorting, "檔案類型", "Type");
            MenuItem iconSize = new MenuItem { Header = "圖示大小" };
            AddItemScaleItem(iconSize, "小", 0.82);
            AddItemScaleItem(iconSize, "中", 1.0);
            AddItemScaleItem(iconSize, "大", 1.20);
            MenuItem viewMode = new MenuItem { Header = "檢視方式" };
            AddItemViewItem(viewMode, "圖示格狀", "Grid");
            AddItemViewItem(viewMode, "精簡清單", "List");

            MenuItem appearance = new MenuItem { Header = "外觀與排列設定…" };
            appearance.Click += delegate { manager.ShowAppearanceSettings(this); };

            MenuItem remove = new MenuItem { Header = "刪除圍欄" };
            remove.Click += delegate
            {
                string note = !string.IsNullOrEmpty(Model.ManagedPath)
                    ? "\n檔案會保留在：\n" + Model.ManagedPath
                    : "\n不會刪除任何檔案。";
                MessageBoxResult answer = AppDialog.Show("移除「" + Model.Title + "」圍欄？" + note,
                    "刪除圍欄", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Yes) manager.RemoveFence(this);
            };

            if (inboxMonitoring != null) { menu.Items.Add(inboxMonitoring); menu.Items.Add(new Separator()); }
            menu.Items.Add(center);
            menu.Items.Add(globalSearch);
            menu.Items.Add(history);
            menu.Items.Add(scenes);
            menu.Items.Add(inbox);
            menu.Items.Add(rules);
            menu.Items.Add(new Separator());
            menu.Items.Add(add);
            menu.Items.Add(addFolder);
            menu.Items.Add(new Separator());
            menu.Items.Add(rename);
            menu.Items.Add(folder);
            menu.Items.Add(openFolder);
            menu.Items.Add(refresh);
            menu.Items.Add(search);
            menu.Items.Add(collapse);
            menu.Items.Add(lockFence);
            menu.Items.Add(undoMoveItem);
            menu.Items.Add(tabsMenu);
            menu.Items.Add(sorting);
            menu.Items.Add(iconSize);
            menu.Items.Add(viewMode);
            menu.Items.Add(appearance);
            menu.Items.Add(new Separator());
            menu.Items.Add(remove);
            mainMenu = menu;
            // The uncluttered header has no ellipsis button. Right-clicking the fence
            // background or title opens the same full management menu; item and tab
            // buttons keep their own specialized context menus.
            I18n.Apply(menu);
            ContextMenu = menu;
        }

        public void OpenManagementMenu(FrameworkElement anchor)
        {
            BuildContextMenu();
            if (mainMenu == null || anchor == null) return;
            mainMenu.PlacementTarget = anchor;
            mainMenu.Placement = PlacementMode.Bottom;
            mainMenu.IsOpen = true;
        }

        private void AddColorItem(MenuItem parent, string label, string value)
        {
            MenuItem item = new MenuItem { Header = label, Tag = value };
            item.Click += delegate
            {
                Model.Accent = value ?? AccentPalette.ToHex(AccentPalette.ReadWindowsAccent());
                ApplyStyle();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void AddFenceStyleItem(MenuItem parent, string label, string value)
        {
            MenuItem item = new MenuItem { Header = label, IsCheckable = true, IsChecked = string.Equals(Model.FenceStyle, value, StringComparison.OrdinalIgnoreCase) };
            item.Click += delegate
            {
                Model.FenceStyle = value;
                ApplyStyle();
                BuildContextMenu();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void AddOpacityItem(MenuItem parent, string label, double value)
        {
            MenuItem item = new MenuItem { Header = label, Tag = value };
            item.Click += delegate
            {
                Model.Opacity = value;
                ApplyStyle();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void AddSortItem(MenuItem parent, string label, string value)
        {
            MenuItem item = new MenuItem
            {
                Header = label, IsCheckable = true,
                IsChecked = string.Equals(Model.ItemSort, value, StringComparison.OrdinalIgnoreCase)
            };
            item.Click += delegate
            {
                Model.ItemSort = value;
                BuildContextMenu();
                RefreshItems();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void AddItemScaleItem(MenuItem parent, string label, double value)
        {
            MenuItem item = new MenuItem
            {
                Header = label, IsCheckable = true, IsChecked = Math.Abs(Model.ItemScale - value) < 0.02
            };
            item.Click += delegate
            {
                Model.ItemScale = value;
                BuildContextMenu();
                RenderItems();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void AddItemViewItem(MenuItem parent, string label, string value)
        {
            MenuItem item = new MenuItem
            {
                Header = label, IsCheckable = true,
                IsChecked = string.Equals(Model.ItemView, value, StringComparison.OrdinalIgnoreCase)
            };
            item.Click += delegate
            {
                Model.ItemView = value;
                SyncActiveTabState();
                BuildContextMenu();
                RenderItems();
                manager.SaveSoon();
            };
            parent.Items.Add(item);
        }

        private void ApplyStyle()
        {
            MediaColor accent = AccentPalette.Parse(Model.Accent);
            byte alphaTop = AppearanceMath.SurfaceAlpha(Model.Opacity);
            byte alphaBottom = AppearanceMath.SurfaceBottomAlpha(Model.Opacity);
            string style = string.IsNullOrEmpty(Model.FenceStyle) ? "Glass" : Model.FenceStyle;
            System.Windows.Media.Effects.DropShadowEffect shadow = shell.Effect as System.Windows.Media.Effects.DropShadowEffect;
            if (shadow == null)
            {
                shadow = new System.Windows.Media.Effects.DropShadowEffect { Color = MediaColors.Black, ShadowDepth = 5 };
                shell.Effect = shadow;
            }

            if (string.Equals(style, "Classic", StringComparison.OrdinalIgnoreCase))
            {
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(alphaTop, 55, 56, 61), MediaColor.FromArgb(alphaBottom, 29, 30, 34),
                    new System.Windows.Point(0, 0), new System.Windows.Point(0, 1));
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(145, accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(1);
                shell.CornerRadius = new CornerRadius(7);
                headerPanel.Background = new SolidColorBrush(MediaColor.FromArgb(42, accent.R, accent.G, accent.B));
                shadow.BlurRadius = 13; shadow.ShadowDepth = 3; shadow.Opacity = 0.30;
            }
            else if (string.Equals(style, "Frost", StringComparison.OrdinalIgnoreCase))
            {
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(alphaTop, 58, 63, 77),
                    MediaColor.FromArgb(alphaBottom, 31, 35, 47), new System.Windows.Point(0, 0), new System.Windows.Point(1, 1));
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(72, 255, 255, 255));
                shell.BorderThickness = new Thickness(1);
                shell.CornerRadius = new CornerRadius(17);
                headerPanel.Background = new SolidColorBrush(MediaColor.FromArgb(13, 255, 255, 255));
                shadow.BlurRadius = 30; shadow.ShadowDepth = 7; shadow.Opacity = 0.36;
            }
            else if (string.Equals(style, "Outline", StringComparison.OrdinalIgnoreCase))
            {
                byte outlineTint = AppearanceMath.OutlineTintAlpha(Model.Opacity);
                byte outlineBase = AppearanceMath.OutlineBaseAlpha(Model.Opacity);
                shell.Background = new LinearGradientBrush(MediaColor.FromArgb(outlineTint, accent.R, accent.G, accent.B),
                    MediaColor.FromArgb(outlineBase, 10, 13, 20), new System.Windows.Point(0, 0), new System.Windows.Point(1, 1));
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(AppearanceMath.OutlineBorderAlpha(Model.Opacity), accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(2);
                shell.CornerRadius = new CornerRadius(13);
                headerPanel.Background = new SolidColorBrush(MediaColor.FromArgb(AppearanceMath.OutlineHeaderAlpha(Model.Opacity), accent.R, accent.G, accent.B));
                shadow.BlurRadius = 16; shadow.ShadowDepth = 2; shadow.Opacity = 0.20;
            }
            else
            {
                MediaColor top = MediaColor.FromArgb(alphaTop, 27, 31, 42);
                MediaColor bottom = MediaColor.FromArgb(alphaBottom, 13, 16, 24);
                shell.Background = new LinearGradientBrush(top, bottom, new System.Windows.Point(0, 0), new System.Windows.Point(1, 1));
                shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(210, accent.R, accent.G, accent.B));
                shell.BorderThickness = new Thickness(1);
                shell.CornerRadius = new CornerRadius(14);
                headerPanel.Background = Brushes.Transparent;
                shadow.BlurRadius = 22; shadow.ShadowDepth = 5; shadow.Opacity = 0.34;
            }

            if (Model.CornerRadius >= 4)
                shell.CornerRadius = new CornerRadius(Math.Min(30, Model.CornerRadius));
            if (string.Equals(Model.ShadowStyle, "None", StringComparison.OrdinalIgnoreCase))
                shadow.Opacity = 0;
            else if (string.Equals(Model.ShadowStyle, "Strong", StringComparison.OrdinalIgnoreCase))
            {
                shadow.BlurRadius = Math.Max(30, shadow.BlurRadius + 8);
                shadow.ShadowDepth = Math.Max(7, shadow.ShadowDepth + 2);
                shadow.Opacity = Math.Max(0.52, shadow.Opacity);
            }
            if (Model.DynamicWallpaperMode)
            {
                shell.Effect = null;
                SolidColorBrush border = shell.BorderBrush as SolidColorBrush;
                if (border != null && border.Color.A < 190)
                {
                    MediaColor color = border.Color;
                    shell.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(190, color.R, color.G, color.B));
                }
            }

            if (Model.IsDesktopInbox)
            {
                // The inbox is intentionally recognizable even when the user uses the
                // same appearance preset for every other fence.
                shell.BorderBrush = new LinearGradientBrush(MediaColor.FromArgb(235, 87, 230, 190),
                    MediaColor.FromArgb(180, 73, 161, 255), new System.Windows.Point(0, 0), new System.Windows.Point(1, 1));
                shell.BorderThickness = new Thickness(2);
                headerPanel.Background = new LinearGradientBrush(MediaColor.FromArgb(68, 65, 218, 174),
                    MediaColor.FromArgb(28, 84, 159, 255), new System.Windows.Point(0, 0), new System.Windows.Point(1, 0));
            }

            foreach (ToggleButton button in itemButtons.Values)
                button.Style = UiStyles.ItemToggleButton(10, accent);
            scroller.Resources[typeof(ScrollBar)] = UiStyles.DarkScrollBar(accent);
            if (dropOverlay != null)
                dropOverlay.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(245, accent.R, accent.G, accent.B));
        }

        public void ApplyAppearanceFromControlCenter(bool rebuildItems, bool reloadItems)
        {
            ApplyStyle();
            ApplyHeaderDisplay();
            if (reloadItems) RefreshItems();
            else if (rebuildItems) RenderItems();
            BuildContextMenu();
        }

        private void ApplyHeaderDisplay()
        {
            if (titleText == null || countText == null) return;
            titleText.Visibility = Model.HideTitle ? Visibility.Collapsed : Visibility.Visible;
            countText.Margin = Model.HideTitle ? new Thickness(0, 1, 0, 0) : new Thickness(9, 1, 0, 0);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AttachToDesktop();
            ConfigureWatcher();
            RefreshItems();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            nativeSource = HwndSource.FromHwnd(hwnd);
            if (nativeSource != null) nativeSource.AddHook(NativeWindowProc);
        }

        private IntPtr NativeWindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (closeAllowed || !manager.ShouldKeepFenceVisible(this)) return IntPtr.Zero;
            if (message == NativeMethods.WM_SYSCOMMAND && (wParam.ToInt64() & 0xFFF0) == NativeMethods.SC_MINIMIZE)
            {
                handled = true;
                QueueDesktopVisibilityRecovery();
            }
            else if (message == NativeMethods.WM_WINDOWPOSCHANGING && lParam != IntPtr.Zero)
            {
                NativeMethods.WINDOWPOS position = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeMethods.WINDOWPOS));
                if ((position.flags & NativeMethods.SWP_HIDEWINDOW) != 0)
                {
                    position.flags = (position.flags & ~NativeMethods.SWP_HIDEWINDOW) | NativeMethods.SWP_SHOWWINDOW;
                    Marshal.StructureToPtr(position, lParam, false);
                    QueueDesktopVisibilityRecovery();
                }
            }
            else if (message == NativeMethods.WM_SHOWWINDOW && wParam == IntPtr.Zero)
                QueueDesktopVisibilityRecovery();
            return IntPtr.Zero;
        }

        private void QueueDesktopVisibilityRecovery()
        {
            if (visibilityRecoveryQueued) return;
            visibilityRecoveryQueued = true;
            Dispatcher.BeginInvoke(new Action(delegate
            {
                visibilityRecoveryQueued = false;
                if (!manager.ShouldKeepFenceVisible(this) || closeAllowed) return;
                if (!IsVisible) Show();
                AttachToDesktop();
                RestackDesktop();
            }), DispatcherPriority.Background);
        }

        public void RefreshFromManager()
        {
            EnsureTabModel();
            LoadActiveTabState();
            RebuildTabs();
            ConfigureWatcher();
            BuildContextMenu();
            RefreshItems();
        }

        public void AttachToDesktop()
        {
            if (!IsLoaded || manager.PreviewMode) return;
            bool embedded;
            int attachError;
            DesktopHostHandle = DesktopHost.Attach(this, Model.X, Model.Y, Width, Height, out embedded, out attachError);
            DesktopEmbedded = embedded;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            manager.Log(string.Format("Fence '{0}' hwnd=0x{1:X} host=0x{2:X} owner=0x{3:X} parent=0x{4:X} embedded={5} error={6}",
                Model.Title, hwnd.ToInt64(), DesktopHostHandle.ToInt64(),
                NativeMethods.GetWindow(hwnd, NativeMethods.GW_OWNER).ToInt64(), NativeMethods.GetParent(hwnd).ToInt64(),
                embedded, attachError));
        }

        public void RestackDesktop()
        {
            if (!IsLoaded || manager.PreviewMode) return;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            manager.Log(string.Format("Restack '{0}' hwnd=0x{1:X} validBefore={2} visibleBefore={3} iconicBefore={4} desktopActive={5}",
                Model.Title, hwnd.ToInt64(), NativeMethods.IsWindow(hwnd), NativeMethods.IsWindowVisible(hwnd),
                NativeMethods.IsIconic(hwnd), DesktopHost.IsDesktopActive(DesktopHostHandle, hwnd)));
            DesktopHost.Move(this, DesktopHostHandle, DesktopEmbedded, Model.X, Model.Y, Width, Height);
            manager.Log(string.Format("Restack '{0}' validAfter={1} visibleAfter={2} iconicAfter={3}",
                Model.Title, NativeMethods.IsWindow(hwnd), NativeMethods.IsWindowVisible(hwnd), NativeMethods.IsIconic(hwnd)));
        }

        public void SetPeekMode(bool enabled)
        {
            if (!IsLoaded || manager.PreviewMode) return;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (enabled)
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, (int)Math.Round(Model.X), (int)Math.Round(Model.Y),
                    Math.Max(1, (int)Math.Round(Width)), Math.Max(1, (int)Math.Round(Height)),
                    NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
            }
            else
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                AttachToDesktop();
            }
        }

        private void BeginDrag(MouseButtonEventArgs e)
        {
            if (Model.Locked) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            NativeMethods.POINT p;
            NativeMethods.GetCursorPos(out p);
            pointerStart = new System.Windows.Point(p.X, p.Y);
            xStart = Model.X;
            yStart = Model.Y;
            dragging = true;
            CaptureMouse();
            e.Handled = true;
        }

        private void BeginResize(object sender, MouseButtonEventArgs e)
        {
            if (Model.Collapsed || Model.Locked) return;
            NativeMethods.POINT p;
            NativeMethods.GetCursorPos(out p);
            pointerStart = new System.Windows.Point(p.X, p.Y);
            widthStart = Width;
            heightStart = Height;
            resizing = true;
            CaptureMouse();
            e.Handled = true;
        }

        private void OnPointerMove(object sender, MouseEventArgs e)
        {
            if (!dragging && !resizing) return;
            NativeMethods.POINT p;
            NativeMethods.GetCursorPos(out p);
            double dx = p.X - pointerStart.X;
            double dy = p.Y - pointerStart.Y;
            if (dragging)
            {
                Model.X = xStart + dx;
                Model.Y = yStart + dy;
            }
            else
            {
                Width = Math.Max(250, widthStart + dx);
                Height = Math.Max(150, heightStart + dy);
                expandedHeight = Height;
                Model.Width = Width;
                Model.Height = expandedHeight;
                statusText.Text = Math.Round(Width) + " × " + Math.Round(Height);
                statusToast.BeginAnimation(UIElement.OpacityProperty, null);
                statusToast.Visibility = Visibility.Visible;
                statusToast.Opacity = 1;
                statusTimer.Stop();
            }
            if (manager.PreviewMode) { Left = Model.X; Top = Model.Y; }
            else DesktopHost.Move(this, DesktopHostHandle, DesktopEmbedded, Model.X, Model.Y, Width, Height);
        }

        private void EndPointerAction(object sender, MouseButtonEventArgs e)
        {
            if (!dragging && !resizing) return;
            bool finishedResize = resizing;
            dragging = false;
            resizing = false;
            ReleaseMouseCapture();
            Model.X = Math.Round(Model.X / 4.0) * 4.0;
            Model.Y = Math.Round(Model.Y / 4.0) * 4.0;
            manager.SnapFence(this);
            Model.X = Math.Max(SystemParameters.VirtualScreenLeft - Width + 84,
                Math.Min(Model.X, SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 84));
            Model.Y = Math.Max(SystemParameters.VirtualScreenTop,
                Math.Min(Model.Y, SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - HeaderHeight));
            Model.Width = Width;
            if (!Model.Collapsed) Model.Height = expandedHeight = Height;
            if (manager.PreviewMode) { Left = Model.X; Top = Model.Y; }
            else DesktopHost.Move(this, DesktopHostHandle, DesktopEmbedded, Model.X, Model.Y, Width, Height);
            manager.SaveSoon();
            if (finishedResize)
            {
                if (string.Equals(Model.ItemView, "List", StringComparison.OrdinalIgnoreCase)) RenderItems();
                ShowStatus("尺寸  " + Math.Round(Width) + " × " + Math.Round(Height));
            }
        }

        private static void EnsureModelOnScreen(FenceModel model, double width, double height)
        {
            Rect virtualArea = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            Rect header = new Rect(model.X, model.Y, Math.Max(84, width), HeaderHeight);
            Rect visible = Rect.Intersect(virtualArea, header);
            if (visible.IsEmpty || visible.Width < 84 || visible.Height < 20)
            {
                Rect work = SystemParameters.WorkArea;
                model.X = work.Left + 36;
                model.Y = work.Top + 36;
            }
        }

        private void ToggleCollapsed()
        {
            if (Model.AutoCollapse)
            {
                Model.AutoCollapse = false;
                Model.Collapsed = autoCollapsedVisual;
                autoCollapsedVisual = false;
            }
            else Model.Collapsed = !Model.Collapsed;
            ApplyCollapsedState(true);
            manager.SaveSoon();
        }

        private void ApplyCollapsedState(bool animate)
        {
            bool collapsed = Model.AutoCollapse ? autoCollapsedVisual : Model.Collapsed;
            resizeHandle.Visibility = collapsed || Model.Locked ? Visibility.Collapsed : Visibility.Visible;
            rollButton.Content = collapsed ? "\uE70D" : "\uE70E";
            if (collapsed)
            {
                if (Height > HeaderHeight + 2) expandedHeight = Height;
            }
            double targetHeight = collapsed ? HeaderHeight : Math.Max(150, expandedHeight);
            int generation = ++collapseAnimationGeneration;
            if (!animate || !IsLoaded || Math.Abs(Height - targetHeight) < 1)
            {
                BeginAnimation(Window.HeightProperty, null);
                Height = targetHeight;
                contentArea.BeginAnimation(UIElement.OpacityProperty, null);
                contentArea.Opacity = 1;
                contentArea.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
                if (tabBar != null)
                {
                    tabBar.BeginAnimation(UIElement.OpacityProperty, null);
                    tabBar.Opacity = 1;
                    tabBar.Visibility = collapsed || Model.Tabs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
                }
                DesktopHost.Move(this, DesktopHostHandle, DesktopEmbedded, Model.X, Model.Y, Width, Height);
                return;
            }

            double startHeight = Height;
            BeginAnimation(Window.HeightProperty, null);
            Height = startHeight;
            contentArea.Visibility = Visibility.Visible;
            if (tabBar != null) tabBar.Visibility = Model.Tabs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
            if (collapsed)
            {
                contentArea.BeginAnimation(UIElement.OpacityProperty, CreateEaseAnimation(contentArea.Opacity, 0, 115));
                if (tabBar != null) tabBar.BeginAnimation(UIElement.OpacityProperty, CreateEaseAnimation(tabBar.Opacity, 0, 100));
            }
            else
            {
                contentArea.Opacity = 0;
                DoubleAnimation contentFade = CreateEaseAnimation(0, 1, 180);
                contentFade.BeginTime = TimeSpan.FromMilliseconds(35);
                contentArea.BeginAnimation(UIElement.OpacityProperty, contentFade);
                if (tabBar != null)
                {
                    tabBar.Opacity = 0;
                    DoubleAnimation tabFade = CreateEaseAnimation(0, 1, 150);
                    tabFade.BeginTime = TimeSpan.FromMilliseconds(25);
                    tabBar.BeginAnimation(UIElement.OpacityProperty, tabFade);
                }
            }
            System.Windows.Media.Animation.DoubleAnimation animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = startHeight, To = targetHeight, Duration = TimeSpan.FromMilliseconds(collapsed ? 170 : 210),
                EasingFunction = new System.Windows.Media.Animation.CubicEase
                {
                    EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                },
                FillBehavior = System.Windows.Media.Animation.FillBehavior.Stop
            };
            animation.Completed += delegate
            {
                if (generation != collapseAnimationGeneration) return;
                BeginAnimation(Window.HeightProperty, null);
                Height = targetHeight;
                bool stillCollapsed = Model.AutoCollapse ? autoCollapsedVisual : Model.Collapsed;
                contentArea.Visibility = stillCollapsed ? Visibility.Collapsed : Visibility.Visible;
                contentArea.BeginAnimation(UIElement.OpacityProperty, null);
                contentArea.Opacity = 1;
                if (tabBar != null)
                {
                    tabBar.Visibility = stillCollapsed || Model.Tabs.Count < 2 ? Visibility.Collapsed : Visibility.Visible;
                    tabBar.BeginAnimation(UIElement.OpacityProperty, null);
                    tabBar.Opacity = 1;
                }
                DesktopHost.Move(this, DesktopHostHandle, DesktopEmbedded, Model.X, Model.Y, Width, Height);
            };
            BeginAnimation(Window.HeightProperty, animation, System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace);
        }

        public void ApplyBehaviorSettings()
        {
            autoCollapseTimer.Stop();
            autoCollapsedVisual = Model.AutoCollapse && !IsMouseOver;
            ApplyCollapsedState(true);
            BuildContextMenu();
            manager.SaveSoon();
        }

        private void ApplyLockState()
        {
            if (headerPanel != null) headerPanel.Cursor = Model.Locked ? Cursors.Arrow : Cursors.SizeAll;
            if (resizeHandle != null)
                resizeHandle.Visibility = Model.Locked || (Model.AutoCollapse ? autoCollapsedVisual : Model.Collapsed) ? Visibility.Collapsed : Visibility.Visible;
        }

        public void ChooseFolder()
        {
            using (Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = I18n.T("選擇要顯示在圍欄中的資料夾");
                dialog.ShowNewFolderButton = true;
                if (!string.IsNullOrEmpty(Model.PortalPath) && Directory.Exists(Model.PortalPath))
                    dialog.SelectedPath = Model.PortalPath;
                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    Model.PortalPath = dialog.SelectedPath;
                    Model.Items.Clear();
                    if (Model.Title == "新圍欄") Model.Title = new DirectoryInfo(dialog.SelectedPath).Name;
                    titleText.Text = Model.Title;
                    SyncActiveTabState();
                    ResetBrowseState();
                    RebuildTabs();
                    ConfigureWatcher();
                    RefreshItems();
                    manager.SaveSoon();
                }
            }
        }

        private void Rename()
        {
            RenameDialog dialog = new RenameDialog(Model.Title);
            bool? result = dialog.ShowDialog();
            if (result == true && !string.IsNullOrWhiteSpace(dialog.Value))
            {
                Model.Title = dialog.Value.Trim();
                titleText.Text = Model.Title;
                manager.SaveSoon();
            }
        }

        private void ConfigureWatcher()
        {
            if (watcher != null) { watcher.Dispose(); watcher = null; }
            string folder = GetContentFolder();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;
            try
            {
                watcher = new FileSystemWatcher(folder);
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
                FileSystemEventHandler changed = delegate { Dispatcher.BeginInvoke(new Action(QueueRefresh)); };
                RenamedEventHandler renamed = delegate { Dispatcher.BeginInvoke(new Action(QueueRefresh)); };
                watcher.Created += changed;
                watcher.Deleted += changed;
                watcher.Changed += changed;
                watcher.Renamed += renamed;
                watcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        private void QueueRefresh()
        {
            if (shellDragActive)
            {
                return;
            }
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        private void RefreshItems()
        {
            if (shellDragActive)
            {
                return;
            }
            countText.Text = I18n.T("載入中");
            int generation = ++refreshGeneration;
            string folder = GetContentFolder();
            string legacyTabContainer = GetLegacyTabContainer(folder);
            bool includePins = string.IsNullOrEmpty(Model.PortalPath);
            List<string> pinned = new List<string>(Model.Items ?? new List<string>());
            Task.Factory.StartNew(delegate
            {
                try
                {
                    List<string> paths = new List<string>();
                    if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                        paths.AddRange(Directory.EnumerateFileSystemEntries(folder)
                            .Where(path => !PathsEqual(path, legacyTabContainer)).Take(5000));
                    if (includePins)
                        paths.AddRange(pinned.Where(p => File.Exists(p) || Directory.Exists(p)));
                    return SortPaths(paths.Distinct(StringComparer.OrdinalIgnoreCase), Model.ItemSort).Take(600).ToList();
                }
                catch { return new List<string>(); }
            }).ContinueWith(task =>
            {
                if (generation == refreshGeneration) DisplayItems(task.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string GetLegacyTabContainer(string activeFolder)
        {
            if (string.IsNullOrEmpty(activeFolder) || Model.Tabs == null || Model.Tabs.Count < 2) return null;
            try
            {
                string candidate = Path.Combine(activeFolder, "分頁");
                if (!Directory.Exists(candidate)) return null;
                bool belongsToTabs = Model.Tabs.Any(tab => tab != null &&
                    !string.IsNullOrEmpty(tab.ManagedPath) && IsPathInside(tab.ManagedPath, candidate));
                return belongsToTabs ? candidate : null;
            }
            catch { return null; }
        }

        private static bool PathsEqual(string first, string second)
        {
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) return false;
            try
            {
                return string.Equals(Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
                    Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void DisplayItems(List<string> paths)
        {
            allItemPaths.Clear();
            allItemPaths.AddRange(paths);
            selectedPaths.RemoveWhere(p => !allItemPaths.Contains(p, StringComparer.OrdinalIgnoreCase));
            RenderItems();
            if (!string.IsNullOrEmpty(pendingSelectionPath))
            {
                ToggleButton selected;
                if (itemButtons.TryGetValue(pendingSelectionPath, out selected))
                {
                    SelectItem(selected, pendingSelectionPath, false);
                    Dispatcher.BeginInvoke(new Action(selected.BringIntoView), DispatcherPriority.Loaded);
                }
                pendingSelectionPath = null;
            }
            if (tabTransitionPending)
            {
                tabTransitionPending = false;
                AnimateTabContentIn();
            }
        }

        private void RenderItems()
        {
            itemPanel.Children.Clear();
            itemButtons.Clear();
            bool listView = string.Equals(Model.ItemView, "List", StringComparison.OrdinalIgnoreCase);
            itemPanel.Orientation = listView ? Orientation.Vertical : Orientation.Horizontal;
            string query = searchBox == null ? "" : (searchBox.Text ?? "").Trim();
            IEnumerable<string> visiblePaths = allItemPaths;
            if (!string.IsNullOrEmpty(query))
                visiblePaths = visiblePaths.Where(p => FriendlyName(p).IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    p.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
            List<string> filtered = visiblePaths.ToList();
            foreach (string path in filtered)
                itemPanel.Children.Add(CreateItem(path));
            UpdateSelectionSummary();
            if (filtered.Count == 0)
            {
                Border empty = new Border
                {
                    Width = Math.Max(190, Width - 38), Height = 132, Margin = new Thickness(5, 12, 5, 5),
                    CornerRadius = new CornerRadius(13), BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(MediaColor.FromArgb(60, 255, 255, 255)),
                    Background = new LinearGradientBrush(MediaColor.FromArgb(26, 255, 255, 255),
                        MediaColor.FromArgb(10, 255, 255, 255), new System.Windows.Point(0, 0), new System.Windows.Point(0, 1))
                };
                StackPanel emptyContent = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
                };
                emptyContent.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrEmpty(query) ? "\uE8B7" : "\uE721",
                    FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 27,
                    Foreground = new SolidColorBrush(MediaColor.FromArgb(225, 255, 255, 255)),
                    HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8)
                });
                emptyContent.Children.Add(new TextBlock
                {
                    Text = !string.IsNullOrEmpty(query) ? "找不到符合「" + query + "」的項目" : "將項目拖曳到這裡",
                    Foreground = Brushes.White, FontSize = 13, FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center
                });
                emptyContent.Children.Add(new TextBlock
                {
                    Text = !string.IsNullOrEmpty(query) ? "換個關鍵字再試試看" : "檔案會安全移入圍欄資料夾",
                    Foreground = new SolidColorBrush(MediaColor.FromArgb(145, 255, 255, 255)), FontSize = 11,
                    TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
                empty.Child = emptyContent;
                itemPanel.Children.Add(empty);
            }
            I18n.Apply(itemPanel);
        }

        private static IEnumerable<string> SortPaths(IEnumerable<string> paths, string mode)
        {
            if (string.Equals(mode, "Modified", StringComparison.OrdinalIgnoreCase))
                return paths.OrderByDescending(SafeModifiedTime).ThenBy(p => Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase);
            if (string.Equals(mode, "Type", StringComparison.OrdinalIgnoreCase))
                return paths.OrderBy(p => Directory.Exists(p) ? 0 : 1)
                    .ThenBy(p => Directory.Exists(p) ? "" : Path.GetExtension(p), StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(p => Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase);
            return paths.OrderBy(p => Directory.Exists(p) ? 0 : 1)
                .ThenBy(p => Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase);
        }

        private static DateTime SafeModifiedTime(string path)
        {
            try { return Directory.Exists(path) ? Directory.GetLastWriteTimeUtc(path) : File.GetLastWriteTimeUtc(path); }
            catch { return DateTime.MinValue; }
        }

        private FrameworkElement CreateItem(string path)
        {
            double scale = Math.Max(0.75, Math.Min(1.30, Model.ItemScale));
            bool listView = string.Equals(Model.ItemView, "List", StringComparison.OrdinalIgnoreCase);
            ToggleButton button = new ToggleButton
            {
                Width = listView ? Math.Max(190, Width - 38) : Math.Round(88 * scale),
                Height = listView ? Math.Round(54 * scale) : Math.Round(84 * scale),
                Margin = listView ? new Thickness(2, 1.5, 2, 1.5) : new Thickness(2),
                Padding = listView ? new Thickness(10, 5, 10, 5) : new Thickness(5),
                HorizontalContentAlignment = listView ? HorizontalAlignment.Stretch : HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand, ToolTip = path, Tag = path,
                IsChecked = selectedPaths.Contains(path), Style = UiStyles.ItemToggleButton(10, AccentPalette.Parse(Model.Accent))
            };
            ScaleTransform hoverScale = new ScaleTransform(1, 1);
            button.RenderTransform = hoverScale;
            button.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            button.MouseEnter += delegate { AnimateScale(hoverScale, 1.045, 125); };
            button.MouseLeave += delegate { AnimateScale(hoverScale, 1.0, 150); };
            System.Windows.Controls.Image icon = new System.Windows.Controls.Image
            {
                Width = Math.Round((listView ? 34 : 42) * scale), Height = Math.Round((listView ? 34 : 42) * scale),
                Margin = listView ? new Thickness(0, 0, 10, 0) : new Thickness(0, 2, 0, 5), Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);
            // Shell icon handlers can be slow at login; never block the panel's UI.
            ShellIconCache.GetAsync(path).ContinueWith(task =>
            {
                if (!Dispatcher.HasShutdownStarted && task.Status == TaskStatus.RanToCompletion)
                    icon.Source = task.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            TextBlock label = new TextBlock
            {
                Text = FriendlyName(path), Foreground = Brushes.White, FontSize = Math.Max(10.2, 11.5 * scale),
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"),
                TextAlignment = listView ? TextAlignment.Left : TextAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = listView ? double.PositiveInfinity : Math.Round(78 * scale),
                MaxHeight = listView ? Math.Round(19 * scale) : Math.Round(34 * scale),
                TextWrapping = listView ? TextWrapping.NoWrap : TextWrapping.Wrap,
                Tag = "i18n-skip"
            };
            if (listView)
            {
                Grid row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.Children.Add(icon);
                StackPanel text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                text.Children.Add(label);
                string extension = Directory.Exists(path) ? I18n.T("資料夾") : Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
                if (string.IsNullOrEmpty(extension)) extension = I18n.T("檔案");
                text.Children.Add(new TextBlock
                {
                    Text = extension, Foreground = new SolidColorBrush(MediaColor.FromArgb(135, 255, 255, 255)),
                    FontSize = Math.Max(9, 9.5 * scale), TextTrimming = TextTrimming.CharacterEllipsis,
                    Tag = "i18n-skip"
                });
                row.Children.Add(text); Grid.SetColumn(text, 1);
                button.Content = row;
            }
            else
            {
                StackPanel stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                stack.Children.Add(icon);
                stack.Children.Add(label);
                button.Content = stack;
            }
            button.Click += delegate
            {
                bool extend = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                SelectItem(button, path, extend);
            };
            button.PreviewMouseRightButtonDown += delegate
            {
                bool extend = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (!selectedPaths.Contains(path)) SelectItem(button, path, extend);
            };
            button.MouseDoubleClick += delegate
            {
                if (Directory.Exists(path)) NavigateIntoFolder(path);
                else OpenPath(path);
            };
            button.PreviewMouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                AnimateScale(hoverScale, 0.965, 70);
                itemDragStart = e.GetPosition(this);
                armedItemPath = path;
                itemDragArmed = true;
            };
            button.PreviewMouseLeftButtonUp += delegate
            {
                AnimateScale(hoverScale, button.IsMouseOver ? 1.045 : 1.0, 110);
                itemDragArmed = false;
                armedItemPath = null;
            };
            button.PreviewMouseMove += BeginItemDrag;
            itemButtons[path] = button;

            ContextMenu menu = new ContextMenu();
            UiStyles.PrepareDarkContextMenu(menu, AccentPalette.Parse(Model.Accent));
            MenuItem open = new MenuItem { Header = "開啟" };
            open.Click += delegate { OpenPath(path); };
            MenuItem reveal = new MenuItem { Header = "在檔案總管中顯示" };
            reveal.Click += delegate { RevealPath(path); };
            menu.Items.Add(open);
            menu.Items.Add(reveal);
            if (IsPathInside(path, GetContentFolder()))
            {
                menu.Items.Add(new Separator());
                MenuItem returnDesktop = new MenuItem { Header = "移回桌面" };
                returnDesktop.Click += delegate
                {
                    MoveItemsToDesktop(selectedPaths.Contains(path) ? selectedPaths.ToArray() : new[] { path });
                };
                menu.Items.Add(returnDesktop);
            }
            else if (Model.Items.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
            {
                menu.Items.Add(new Separator());
                MenuItem unpin = new MenuItem { Header = "從圍欄移除" };
                unpin.Click += delegate { Model.Items.Remove(path); RefreshItems(); manager.SaveSoon(); };
                menu.Items.Add(unpin);
            }
            I18n.Apply(menu);
            button.ContextMenu = menu;
            return button;
        }

        private void SelectItem(ToggleButton selected, string path, bool extend)
        {
            if (!extend)
            {
                selectedPaths.Clear();
                foreach (ToggleButton button in itemButtons.Values)
                    if (!object.ReferenceEquals(button, selected)) button.IsChecked = false;
                selected.IsChecked = true;
                selectedPaths.Add(path);
            }
            else if (selected.IsChecked == true)
                selectedPaths.Add(path);
            else
                selectedPaths.Remove(path);
            UpdateSelectionSummary();
        }

        private void ClearSelection()
        {
            selectedPaths.Clear();
            foreach (ToggleButton button in itemButtons.Values) button.IsChecked = false;
            UpdateSelectionSummary();
        }

        private void SelectAllVisible()
        {
            selectedPaths.Clear();
            foreach (KeyValuePair<string, ToggleButton> pair in itemButtons)
            {
                pair.Value.IsChecked = true;
                selectedPaths.Add(pair.Key);
            }
            UpdateSelectionSummary();
        }

        private void UpdateSelectionSummary()
        {
            string location = !string.IsNullOrEmpty(browseFolder) ? FriendlyName(browseFolder) + " · " : "";
            if (selectedPaths.Count > 0) countText.Text = I18n.T(location + selectedPaths.Count + " 個已選取");
            else if (searchPanel.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(searchBox.Text))
                countText.Text = I18n.T(location + itemButtons.Count + " / " + allItemPaths.Count + " 個項目");
            else countText.Text = I18n.T(location + itemButtons.Count + " 個項目");
        }

        private static string FriendlyName(string path)
        {
            if (string.Equals(path, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), StringComparison.OrdinalIgnoreCase)) return "桌面";
            if (string.Equals(path, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), StringComparison.OrdinalIgnoreCase)) return "文件";
            if (string.Equals(path, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), StringComparison.OrdinalIgnoreCase)) return "圖片";
            string downloads = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (string.Equals(path, downloads, StringComparison.OrdinalIgnoreCase)) return "下載";

            string name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));

            // Windows 捷徑在介面中隱藏 .lnk / .url 副檔名，但不修改實際檔案名稱。
            string ext = Path.GetExtension(name);
            if (string.Equals(ext, ".lnk", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ext, ".url", StringComparison.OrdinalIgnoreCase))
            {
                name = Path.GetFileNameWithoutExtension(name);
            }

            return string.IsNullOrEmpty(name) ? path : name;
        }

        private static void OpenPath(string path)
        {
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { AppDialog.Show(ex.Message, "無法開啟", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private static void RevealPath(string path)
        {
            try
            {
                if (Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
                else Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"") { UseShellExecute = true });
            }
            catch { }
        }

        private string GetContentFolder()
        {
            if (!string.IsNullOrEmpty(browseFolder) && Directory.Exists(browseFolder)) return browseFolder;
            return GetBaseContentFolder();
        }

        private string GetBaseContentFolder()
        {
            if (!string.IsNullOrEmpty(Model.PortalPath) && Directory.Exists(Model.PortalPath)) return Model.PortalPath;
            if (!string.IsNullOrEmpty(Model.ManagedPath) && Directory.Exists(Model.ManagedPath)) return Model.ManagedPath;
            return null;
        }

        private void NavigateIntoFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;
            string current = GetContentFolder();
            if (!string.IsNullOrEmpty(current)) browseHistory.Push(current);
            browseFolder = folder;
            backButton.Visibility = Visibility.Visible;
            ClearSelection();
            ConfigureWatcher();
            RefreshItems();
        }

        private void NavigateBack()
        {
            if (browseHistory.Count == 0) { ResetBrowseState(); RefreshItems(); return; }
            string previous = browseHistory.Pop();
            string root = GetBaseContentFolder();
            browseFolder = !string.IsNullOrEmpty(root) && PathsEqual(previous, root) ? null : previous;
            backButton.Visibility = string.IsNullOrEmpty(browseFolder) ? Visibility.Collapsed : Visibility.Visible;
            ClearSelection();
            ConfigureWatcher();
            RefreshItems();
        }

        private void ResetBrowseState()
        {
            browseFolder = null;
            browseHistory.Clear();
            if (backButton != null) backButton.Visibility = Visibility.Collapsed;
        }

        public void RevealItem(string tabId, string path)
        {
            if (!string.IsNullOrEmpty(tabId) && !string.Equals(Model.ActiveTabId, tabId, StringComparison.OrdinalIgnoreCase))
            {
                SyncActiveTabState();
                Model.ActiveTabId = tabId;
                LoadActiveTabState();
            }
            ResetBrowseState();
            pendingSelectionPath = path;
            ConfigureWatcher();
            RebuildTabs();
            BuildContextMenu();
            RefreshItems();
            if (!IsVisible) Show();
            AttachToDesktop();
        }

        private static bool IsPathInside(string path, string folder)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(folder)) return false;
            try
            {
                string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                string fullFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                return fullPath.StartsWith(fullFolder, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void BeginItemDrag(object sender, MouseEventArgs e)
        {
            if (!itemDragArmed || string.IsNullOrEmpty(armedItemPath) || e.LeftButton != MouseButtonState.Pressed) return;
            System.Windows.Point current = e.GetPosition(this);
            if (Math.Abs(current.X - itemDragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(current.Y - itemDragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            string path = armedItemPath;
            itemDragArmed = false;
            armedItemPath = null;
            ToggleButton button = sender as ToggleButton;
            if (button == null) return;
            if (!selectedPaths.Contains(path)) SelectItem(button, path, false);
            string[] paths = selectedPaths.Where(p => File.Exists(p) || Directory.Exists(p)).ToArray();
            if (paths.Length == 0) return;

            DataObject data = new DataObject();
            data.SetData(DataFormats.FileDrop, paths);
            data.SetData("DeskBound.InternalDragSource", Model.Id ?? "");
            shellDragActive = true;
            refreshTimer.Stop();
            try
            {
                using (MemoryStream preferred = new MemoryStream(BitConverter.GetBytes((int)DragDropEffects.Move)))
                {
                    data.SetData("Preferred DropEffect", preferred);
                    System.Windows.DragDrop.DoDragDrop(button, data, DragDropEffects.Move);
                }
            }
            finally
            {
                shellDragActive = false;
                refreshTimer.Stop();
                refreshTimer.Start();
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (!manager.PreviewMode) RestackDesktop();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            e.Handled = true;
        }

        private void HandleMoveResult(MoveBatchResult result)
        {
            if (result.Moves.Count > 0)
            {
                manager.RecordMoveHistory(Model.Title, result.Moves);
                lastMoveRecords = result.Moves.Take(200).ToList();
                Model.LastMoves = lastMoveRecords;
                SyncActiveTabState();
                ConfigureWatcher();
                manager.SaveCritical();
                ShowStatus("✓  已移動 " + result.Moves.Count + " 個項目");
            }
            BuildContextMenu();
            ClearSelection();
            RefreshItems();
            if (result.Errors.Count > 0)
                AppDialog.Show(string.Join(Environment.NewLine, result.Errors.Take(8)), "部分項目無法移動", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MoveItemsToDesktop(IEnumerable<string> paths)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string[] sources = paths.Where(p => File.Exists(p) || Directory.Exists(p)).ToArray();
            if (sources.Length == 0 || !Directory.Exists(desktop)) return;
            countText.Text = I18n.T("正在移回桌面…");
            Task.Factory.StartNew(delegate { return ManagedStorage.MoveInto(sources, desktop); })
                .ContinueWith(task => HandleMoveResult(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UndoLastMove()
        {
            if (lastMoveRecords.Count == 0) return;
            List<MoveRecord> records = new List<MoveRecord>(lastMoveRecords);
            countText.Text = I18n.T("正在復原…");
            Task.Factory.StartNew(delegate { return ManagedStorage.Undo(records); })
                .ContinueWith(task =>
                {
                    int restored = records.Count - task.Result.Moves.Count;
                    lastMoveRecords = task.Result.Moves;
                    Model.LastMoves = lastMoveRecords;
                    SyncActiveTabState();
                    ConfigureWatcher();
                    BuildContextMenu();
                    RefreshItems();
                    manager.SaveCritical();
                    if (restored > 0) ShowStatus("↶  已復原 " + restored + " 個項目");
                    if (task.Result.Errors.Count > 0)
                        AppDialog.Show(string.Join(Environment.NewLine, task.Result.Errors.Take(8)), "部分項目無法復原", MessageBoxButton.OK, MessageBoxImage.Warning);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ShowStatus(string message)
        {
            statusText.Text = I18n.T(message);
            statusToast.Visibility = Visibility.Visible;
            TranslateTransform rise = statusToast.RenderTransform as TranslateTransform;
            if (rise == null)
            {
                rise = new TranslateTransform(0, 6);
                statusToast.RenderTransform = rise;
            }
            statusToast.BeginAnimation(UIElement.OpacityProperty, CreateEaseAnimation(0, 1, 150));
            rise.BeginAnimation(TranslateTransform.YProperty, CreateEaseAnimation(6, 0, 170));
            statusTimer.Stop();
            autoCollapseTimer.Stop();
            statusTimer.Start();
        }

        private void HideStatusToast()
        {
            DoubleAnimation fade = CreateEaseAnimation(statusToast.Opacity, 0, 170);
            fade.Completed += delegate
            {
                statusToast.BeginAnimation(UIElement.OpacityProperty, null);
                statusToast.Opacity = 1;
                statusToast.Visibility = Visibility.Collapsed;
            };
            statusToast.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private bool IsExternalFileDrag(DragEventArgs e)
        {
            if (e == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return false;
            if (!e.Data.GetDataPresent("DeskBound.InternalDragSource")) return true;
            string sourceFence = e.Data.GetData("DeskBound.InternalDragSource") as string;
            return !string.Equals(sourceFence, Model.Id, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowDropOverlay(bool show)
        {
            if (dropOverlay == null) return;
            if (show)
            {
                if (dropOverlay.Visibility != Visibility.Visible)
                {
                    dropOverlay.Visibility = Visibility.Visible;
                    AnimateOpacity(dropOverlay, 1, 130);
                    ScaleTransform scale = new ScaleTransform(0.975, 0.975);
                    dropOverlay.RenderTransform = scale;
                    dropOverlay.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    AnimateScale(scale, 1, 150);
                }
            }
            else
            {
                dropOverlay.BeginAnimation(UIElement.OpacityProperty, null);
                dropOverlay.Opacity = 0;
                dropOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            ShowDropOverlay(IsExternalFileDrag(e));
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Move : DragDropEffects.None;
            ShowDropOverlay(IsExternalFileDrag(e));
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            ShowDropOverlay(false);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            ShowDropOverlay(false);
            string[] paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            string destination;
            try
            {
                destination = !string.IsNullOrEmpty(browseFolder) && Directory.Exists(browseFolder)
                    ? browseFolder
                    : (!string.IsNullOrEmpty(Model.PortalPath) && Directory.Exists(Model.PortalPath)
                        ? Model.PortalPath : ManagedStorage.EnsureFolder(Model, ActiveTab()));
            }
            catch (Exception ex)
            {
                AppDialog.Show(ex.Message, "無法建立圍欄資料夾", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            countText.Text = I18n.T("正在移入…");
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            Task.Factory.StartNew(delegate { return ManagedStorage.MoveInto(paths, destination); })
                .ContinueWith(task => HandleMoveResult(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closeAllowed) { e.Cancel = true; Hide(); return; }
            refreshTimer.Stop();
            statusTimer.Stop();
            if (watcher != null) watcher.Dispose();
            if (nativeSource != null) nativeSource.RemoveHook(NativeWindowProc);
        }

        public void CloseFromManager()
        {
            closeAllowed = true;
            if (watcher != null) watcher.Dispose();
            Close();
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null)
            {
                T found = node as T;
                if (found != null) return found;
                node = VisualTreeHelper.GetParent(node);
            }
            return null;
        }
    }

    internal sealed class RenameDialog : Window
    {
        private readonly TextBox input;
        public string Value { get { return input.Text; } }

        public RenameDialog(string current)
            : this(current, "重新命名圍欄", "圍欄名稱")
        {
        }

        public RenameDialog(string current, string title, string prompt)
        {
            Title = I18n.T(title);
            Width = 390; Height = 174;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Border border = new Border
            {
                CornerRadius = new CornerRadius(16), Padding = new Thickness(20),
                Background = new LinearGradientBrush(MediaColor.FromRgb(16, 23, 32), MediaColor.FromRgb(23, 33, 43), 35),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(50, 66, 79)), BorderThickness = new Thickness(1),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 28, ShadowDepth = 7, Opacity = 0.45 }
            };
            StackPanel stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = I18n.T(prompt), Foreground = Brushes.White, FontSize = 13, Margin = new Thickness(0, 0, 0, 8) });
            input = new TextBox { Text = current, FontSize = 14, Padding = new Thickness(8, 5, 8, 5) };
            stack.Children.Add(input);
            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            Button cancel = new Button { Content = I18n.T("取消"), Width = 82, Height = 36, Margin = new Thickness(0, 0, 8, 0), Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromRgb(30, 42, 53)), BorderBrush = new SolidColorBrush(MediaColor.FromRgb(54, 70, 83)), BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(9), Cursor = Cursors.Hand };
            Button ok = new Button { Content = I18n.T("確定"), Width = 82, Height = 36, Foreground = Brushes.White, Background = new SolidColorBrush(MediaColor.FromArgb(195, accent.R, accent.G, accent.B)), BorderBrush = new SolidColorBrush(accent), BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(9), Cursor = Cursors.Hand };
            cancel.Click += delegate { DialogResult = false; };
            ok.Click += delegate { DialogResult = true; };
            buttons.Children.Add(cancel); buttons.Children.Add(ok); stack.Children.Add(buttons);
            border.Child = stack; Content = border;
            Loaded += delegate { input.Focus(); input.SelectAll(); };
            KeyDown += delegate(object s, KeyEventArgs e) { if (e.Key == Key.Enter) DialogResult = true; if (e.Key == Key.Escape) DialogResult = false; };
        }
    }

    internal sealed class FenceModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Collapsed { get; set; }
        public bool Locked { get; set; }
        public string Accent { get; set; }
        public string FenceStyle { get; set; }
        public string ItemSort { get; set; }
        public string ItemView { get; set; }
        public double ItemScale { get; set; }
        public double Opacity { get; set; }
        public double CornerRadius { get; set; }
        public string ShadowStyle { get; set; }
        public bool HideTitle { get; set; }
        public bool AutoCollapse { get; set; }
        public bool IsAutoOrganizer { get; set; }
        public bool IsDesktopInbox { get; set; }
        public bool DynamicWallpaperMode { get; set; }
        public string PortalPath { get; set; }
        public string ManagedPath { get; set; }
        public List<string> Items { get; set; }
        public List<MoveRecord> LastMoves { get; set; }
        public List<FenceTabModel> Tabs { get; set; }
        public string ActiveTabId { get; set; }

        public FenceModel()
        {
            Items = new List<string>();
            LastMoves = new List<MoveRecord>();
            Tabs = new List<FenceTabModel>();
            Accent = "#7C8CFF";
            FenceStyle = "Glass";
            ItemSort = "Name";
            ItemView = "Grid";
            ItemScale = 1.0;
            Opacity = 0.86;
            CornerRadius = 0;
            ShadowStyle = "Style";
            Width = 350;
            Height = 260;
        }
    }

    internal sealed class FenceTabModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Accent { get; set; }
        public string PortalPath { get; set; }
        public string ManagedPath { get; set; }
        public string RuleKey { get; set; }
        public string ItemView { get; set; }
        public List<string> Items { get; set; }
        public List<MoveRecord> LastMoves { get; set; }

        public FenceTabModel()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = "分頁";
            ItemView = "Grid";
            Items = new List<string>();
            LastMoves = new List<MoveRecord>();
        }
    }

    internal sealed class LayoutStore
    {
        private readonly string root;
        private readonly string path;
        private readonly string backupPath;
        private readonly string historyFolder;
        private readonly string managedRoot;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public bool LoadFailed { get; private set; }
        public bool RecoveredFromBackup { get; private set; }
        public bool RecoveredFromFolders { get; private set; }

        public LayoutStore() : this(null) { }

        internal LayoutStore(string rootOverride) : this(rootOverride, null) { }

        internal LayoutStore(string rootOverride, string managedRootOverride)
        {
            root = rootOverride ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskBound");
            try { Directory.CreateDirectory(root); }
            catch { root = AppDomain.CurrentDomain.BaseDirectory; }
            path = Path.Combine(root, "layout.json");
            backupPath = Path.Combine(root, "layout.backup.json");
            historyFolder = Path.Combine(root, "layout-history");
            managedRoot = managedRootOverride ?? ManagedStorage.GetRoot();
        }

        public List<FenceModel> Load()
        {
            LoadFailed = false;
            RecoveredFromBackup = false;
            RecoveredFromFolders = false;
            bool primaryExists = File.Exists(path);
            bool backupExists = File.Exists(backupPath);
            if (!primaryExists && !backupExists)
            {
                List<FenceModel> discovered = DiscoverManagedFolders();
                RecoveredFromFolders = discovered.Count > 0;
                return Normalize(discovered);
            }

            try
            {
                List<FenceModel> result = null;
                if (primaryExists)
                {
                    try
                    {
                        result = Deserialize(path);
                        EnsureBackup(path, backupPath);
                    }
                    catch { result = null; }
                }
                if (result == null && backupExists)
                {
                    try { result = Deserialize(backupPath); RecoveredFromBackup = true; }
                    catch { result = null; }
                }
                if (result != null) return Normalize(result);

                // Never turn a read error into a fresh empty layout. If the JSON files
                // are damaged, rebuild only from real managed folders and retain the
                // originals for manual recovery.
                List<FenceModel> recovered = DiscoverManagedFolders();
                if (recovered.Count > 0)
                {
                    RecoveredFromFolders = true;
                    PreserveForHistory(primaryExists ? path : backupPath, "unreadable");
                    return Normalize(recovered);
                }
                LoadFailed = true;
                return new List<FenceModel>();
            }
            catch
            {
                LoadFailed = true;
                return new List<FenceModel>();
            }
        }

        public bool Save(List<FenceModel> models, bool allowReduction = false)
        {
            if (LoadFailed) return false;
            models = models ?? new List<FenceModel>();
            string temp = path + ".tmp";
            try
            {
                string json = serializer.Serialize(models);
                if (File.Exists(path) && !allowReduction)
                {
                    List<FenceModel> previous = null;
                    try { previous = Deserialize(path); } catch { }
                    if (previous != null && models.Count < previous.Count) return false;
                }

                File.WriteAllText(temp, json);
                if (File.Exists(path))
                {
                    PreserveForHistory(path, "layout");
                    if (RecoveredFromBackup || RecoveredFromFolders)
                    {
                        File.Delete(path);
                        File.Move(temp, path);
                        File.Copy(path, backupPath, true);
                    }
                    else File.Replace(temp, path, backupPath, true);
                }
                else
                {
                    File.Move(temp, path);
                    EnsureBackup(path, backupPath);
                }
                LoadFailed = false;
                RecoveredFromBackup = false;
                RecoveredFromFolders = false;
                return true;
            }
            catch
            {
                try { if (File.Exists(temp)) File.Delete(temp); } catch { }
                return false;
            }
        }

        private List<FenceModel> Deserialize(string source)
        {
            List<FenceModel> result = serializer.Deserialize<List<FenceModel>>(File.ReadAllText(source));
            if (result == null) throw new InvalidDataException("Layout data is empty.");
            return result;
        }

        private List<FenceModel> Normalize(List<FenceModel> result)
        {
            result = result ?? new List<FenceModel>();
            foreach (FenceModel model in result)
            {
                if (string.IsNullOrEmpty(model.Id)) model.Id = Guid.NewGuid().ToString("N");
                if (model.Items == null) model.Items = new List<string>();
                if (model.LastMoves == null) model.LastMoves = new List<MoveRecord>();
                if (model.Tabs == null) model.Tabs = new List<FenceTabModel>();
                foreach (FenceTabModel tab in model.Tabs)
                {
                    if (string.IsNullOrEmpty(tab.Id)) tab.Id = Guid.NewGuid().ToString("N");
                    if (string.IsNullOrWhiteSpace(tab.Title)) tab.Title = I18n.T("分頁");
                    if (tab.Items == null) tab.Items = new List<string>();
                    if (tab.LastMoves == null) tab.LastMoves = new List<MoveRecord>();
                    if (string.IsNullOrEmpty(tab.ItemView)) tab.ItemView = "Grid";
                }
                if (model.Width < 250) model.Width = 350;
                if (model.Height < 150) model.Height = 260;
                if (string.IsNullOrEmpty(model.Accent)) model.Accent = "#7C8CFF";
                if (string.IsNullOrEmpty(model.FenceStyle)) model.FenceStyle = "Glass";
                if (string.IsNullOrEmpty(model.ItemSort)) model.ItemSort = "Name";
                if (string.IsNullOrEmpty(model.ItemView)) model.ItemView = "Grid";
                if (model.ItemScale < 0.75 || model.ItemScale > 1.30) model.ItemScale = 1.0;
                if (model.Opacity <= 0 || model.Opacity > 1.0) model.Opacity = 0.86;
                if (model.CornerRadius < 0 || model.CornerRadius > 30) model.CornerRadius = 0;
                if (string.IsNullOrEmpty(model.ShadowStyle)) model.ShadowStyle = "Style";
            }
            return result;
        }

        private List<FenceModel> DiscoverManagedFolders()
        {
            List<FenceModel> recovered = new List<FenceModel>();
            if (string.IsNullOrEmpty(managedRoot) || !Directory.Exists(managedRoot)) return recovered;
            DirectoryInfo[] folders;
            try { folders = new DirectoryInfo(managedRoot).GetDirectories(); }
            catch { return recovered; }

            int index = 0;
            foreach (DirectoryInfo folder in folders.OrderBy(item => item.CreationTimeUtc))
            {
                string title;
                if (!TryManagedName(folder.Name, out title) || folder.Name.EndsWith("-分頁", StringComparison.CurrentCultureIgnoreCase)) continue;
                bool hasContent;
                try { hasContent = folder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Any(); }
                catch { hasContent = false; }
                if (!hasContent) continue;

                FenceModel model = new FenceModel
                {
                    Id = Guid.NewGuid().ToString("N"), Title = title,
                    X = 56 + (index % 4) * 390, Y = 72 + (index / 4) * 330,
                    Width = 370, Height = 285, ManagedPath = folder.FullName,
                    Accent = title.IndexOf("收件", StringComparison.CurrentCultureIgnoreCase) >= 0 ? "#52C7A5" : "#4DB6FF",
                    IsDesktopInbox = title.IndexOf("桌面收件", StringComparison.CurrentCultureIgnoreCase) >= 0
                };
                model.Tabs.Clear();
                IEnumerable<DirectoryInfo> tabFolders = Enumerable.Empty<DirectoryInfo>();
                try
                {
                    List<DirectoryInfo> candidates = new List<DirectoryInfo>();
                    DirectoryInfo nested = new DirectoryInfo(Path.Combine(folder.FullName, "分頁"));
                    DirectoryInfo sibling = new DirectoryInfo(folder.FullName + "-分頁");
                    if (nested.Exists) candidates.AddRange(nested.GetDirectories());
                    if (sibling.Exists) candidates.AddRange(sibling.GetDirectories());
                    tabFolders = candidates;
                }
                catch { }
                foreach (DirectoryInfo tabFolder in tabFolders)
                {
                    string tabTitle;
                    if (!TryManagedName(tabFolder.Name, out tabTitle)) tabTitle = tabFolder.Name;
                    model.Tabs.Add(new FenceTabModel { Title = tabTitle, Accent = model.Accent, ManagedPath = tabFolder.FullName });
                }
                FenceTabModel primary = new FenceTabModel { Title = title, Accent = model.Accent, ManagedPath = folder.FullName };
                model.Tabs.Add(primary);
                model.ActiveTabId = model.Tabs[0].Id;
                recovered.Add(model);
                index++;
            }
            return recovered;
        }

        private static bool TryManagedName(string name, out string title)
        {
            title = null;
            if (string.IsNullOrWhiteSpace(name)) return false;
            int dash = name.LastIndexOf('-');
            if (dash <= 0 || name.Length - dash - 1 != 8) return false;
            string suffix = name.Substring(dash + 1);
            if (!suffix.All(Uri.IsHexDigit)) return false;
            title = name.Substring(0, dash).Trim();
            return title.Length > 0;
        }

        private void PreserveForHistory(string source, string label)
        {
            try
            {
                if (string.IsNullOrEmpty(source) || !File.Exists(source)) return;
                Directory.CreateDirectory(historyFolder);
                string destination = Path.Combine(historyFolder, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + label + ".json");
                File.Copy(source, destination, false);
                foreach (FileInfo stale in new DirectoryInfo(historyFolder).GetFiles("*.json").OrderByDescending(file => file.LastWriteTimeUtc).Skip(20))
                    try { stale.Delete(); } catch { }
            }
            catch { }
        }

        private static void EnsureBackup(string source, string destination)
        {
            try
            {
                if (File.Exists(source) && !File.Exists(destination)) File.Copy(source, destination, false);
            }
            catch { }
        }
    }

    internal sealed class MoveRecord
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
    }

    internal sealed class MoveHistoryEntry
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Label { get; set; }
        public List<MoveRecord> Moves { get; set; }

        public MoveHistoryEntry()
        {
            Id = Guid.NewGuid().ToString("N");
            Moves = new List<MoveRecord>();
        }

        public override string ToString()
        {
            return Timestamp.ToString("MM/dd  HH:mm") + "   " + Label + "   ·   " + Moves.Count + " 個項目";
        }
    }

    internal static class LayoutSnapshotStore
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static string GetFolder()
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskBound", "snapshots");
            Directory.CreateDirectory(root);
            return root;
        }

        public static string Create(List<FenceModel> models)
        {
            return Create(models, "版面");
        }

        public static string Create(List<FenceModel> models, string label)
        {
            string safeLabel = string.Join("_", (string.IsNullOrWhiteSpace(label) ? "版面" : label).Split(Path.GetInvalidFileNameChars()));
            string path = Path.Combine(GetFolder(), DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + safeLabel + ".json");
            File.WriteAllText(path, Serializer.Serialize(models ?? new List<FenceModel>()));
            return path;
        }

        public static List<FenceModel> Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) throw new FileNotFoundException("找不到版面快照。", path);
            List<FenceModel> models = Serializer.Deserialize<List<FenceModel>>(File.ReadAllText(path));
            if (models == null) throw new InvalidDataException("版面快照格式不正確。");
            return models;
        }
    }

    internal sealed class AppSettingsModel
    {
        public const int CurrentSchemaVersion = 2;
        public int SchemaVersion { get; set; }
        public string UiLanguage { get; set; }
        public bool AutoOrganizeDesktop { get; set; }
        public bool DesktopInboxEnabled { get; set; }
        public bool AutoCheckUpdates { get; set; }
        public DateTime? LastUpdateCheckUtc { get; set; }
        public List<MoveHistoryEntry> MoveHistory { get; set; }
        public Dictionary<string, string> OrganizerExtensions { get; set; }
        public Dictionary<string, string> OrganizerKeywords { get; set; }

        public AppSettingsModel()
        {
            SchemaVersion = CurrentSchemaVersion;
            UiLanguage = "System";
            AutoCheckUpdates = true;
            MoveHistory = new List<MoveHistoryEntry>();
            OrganizerExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            OrganizerKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal sealed class AppSettingsStore
    {
        private readonly string path;
        private readonly string backupPath;
        private readonly string historyFolder;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        public bool LoadFailed { get; private set; }
        public bool RecoveredFromBackup { get; private set; }

        public AppSettingsStore() : this(null) { }

        internal AppSettingsStore(string rootOverride)
        {
            string root = rootOverride ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskBound");
            Directory.CreateDirectory(root);
            path = Path.Combine(root, "settings.json");
            backupPath = Path.Combine(root, "settings.backup.json");
            historyFolder = Path.Combine(root, "settings-history");
        }

        public AppSettingsModel Load()
        {
            LoadFailed = false;
            RecoveredFromBackup = false;
            if (File.Exists(path))
            {
                try
                {
                    AppSettingsModel loaded = LoadFile(path);
                    EnsureBackup(path, backupPath);
                    return loaded;
                }
                catch { }
            }
            if (File.Exists(backupPath))
            {
                try
                {
                    AppSettingsModel recovered = LoadFile(backupPath);
                    RecoveredFromBackup = true;
                    return recovered;
                }
                catch { }
            }
            if (File.Exists(path) || File.Exists(backupPath)) LoadFailed = true;
            return new AppSettingsModel();
        }

        private AppSettingsModel LoadFile(string source)
        {
            string json = File.ReadAllText(source);
            int sourceSchema = 0;
            Dictionary<string, object> raw = serializer.DeserializeObject(json) as Dictionary<string, object>;
            object schemaValue;
            if (raw != null && raw.TryGetValue("SchemaVersion", out schemaValue) && schemaValue != null)
                int.TryParse(Convert.ToString(schemaValue), out sourceSchema);
            AppSettingsModel model = serializer.Deserialize<AppSettingsModel>(json);
            if (model == null) throw new InvalidDataException("Settings data is empty.");
            if (model.MoveHistory == null) model.MoveHistory = new List<MoveHistoryEntry>();
            if (model.OrganizerExtensions == null) model.OrganizerExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (model.OrganizerKeywords == null) model.OrganizerKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Migrate(model, sourceSchema);
            return model;
        }

        internal static bool Migrate(AppSettingsModel model, int sourceSchema)
        {
            if (model == null || sourceSchema >= AppSettingsModel.CurrentSchemaVersion) return false;
            // Schema 1 formalized existing defaults. Schema 2 adds an explicit UI
            // language preference; older installations follow Windows by default.
            if (sourceSchema < 2 && string.IsNullOrWhiteSpace(model.UiLanguage)) model.UiLanguage = "System";
            model.SchemaVersion = AppSettingsModel.CurrentSchemaVersion;
            return true;
        }

        public bool Save(AppSettingsModel model)
        {
            if (LoadFailed) return false;
            string temp = path + ".tmp";
            try
            {
                File.WriteAllText(temp, serializer.Serialize(model));
                if (File.Exists(path))
                {
                    PreserveForHistory(path);
                    if (RecoveredFromBackup)
                    {
                        File.Delete(path);
                        File.Move(temp, path);
                        File.Copy(path, backupPath, true);
                    }
                    else File.Replace(temp, path, backupPath, true);
                }
                else
                {
                    File.Move(temp, path);
                    EnsureBackup(path, backupPath);
                }
                LoadFailed = false;
                RecoveredFromBackup = false;
                return true;
            }
            catch
            {
                try { if (File.Exists(temp)) File.Delete(temp); } catch { }
                return false;
            }
        }

        private void PreserveForHistory(string source)
        {
            try
            {
                if (!File.Exists(source)) return;
                Directory.CreateDirectory(historyFolder);
                string destination = Path.Combine(historyFolder, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-settings.json");
                File.Copy(source, destination, false);
                foreach (FileInfo stale in new DirectoryInfo(historyFolder).GetFiles("*.json").OrderByDescending(file => file.LastWriteTimeUtc).Skip(20))
                    try { stale.Delete(); } catch { }
            }
            catch { }
        }

        private static void EnsureBackup(string source, string destination)
        {
            try
            {
                if (File.Exists(source) && !File.Exists(destination)) File.Copy(source, destination, false);
            }
            catch { }
        }
    }

    internal sealed class OrganizerRunResult
    {
        public FenceModel Model { get; set; }
        public bool IsNew { get; set; }
        public int Moved { get; set; }
        public List<string> Errors { get; private set; }
        public OrganizerRunResult() { Errors = new List<string>(); }
    }

    internal static class DesktopAutoOrganizer
    {
        private static readonly Dictionary<string, string[]> Extensions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Images", new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".heic", ".avif" } },
            { "Documents", new[] { ".txt", ".md", ".pdf", ".doc", ".docx", ".rtf", ".odt", ".xls", ".xlsx", ".csv", ".ppt", ".pptx", ".ods", ".odp" } },
            { "Archives", new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz" } },
            { "Installers", new[] { ".exe", ".msi", ".msix", ".msixbundle", ".appx", ".appxbundle" } },
            { "Media", new[] { ".mp3", ".wav", ".flac", ".aac", ".m4a", ".ogg", ".mp4", ".mkv", ".mov", ".avi", ".webm" } },
            { "Shortcuts", new[] { ".lnk", ".url" } }
        };

        public static IEnumerable<string> CategoryKeys { get { return Extensions.Keys; } }

        public static string DefaultExtensionText(string key)
        {
            string[] values;
            return Extensions.TryGetValue(key, out values) ? string.Join(", ", values) : "";
        }

        public static Dictionary<string, List<string>> Analyze(Dictionary<string, string> extensionRules, Dictionary<string, string> keywordRules)
        {
            Dictionary<string, List<string>> result = Extensions.Keys.ToDictionary(k => k, k => new List<string>(), StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<string>> parsedExtensions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string[]> parsedKeywords = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in Extensions.Keys)
            {
                string extensionText;
                if (extensionRules == null || !extensionRules.TryGetValue(key, out extensionText)) extensionText = DefaultExtensionText(key);
                parsedExtensions[key] = new HashSet<string>(SplitRule(extensionText).Select(value => value.StartsWith(".") ? value.ToLowerInvariant() : "." + value.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
                string keywordText;
                if (keywordRules == null || !keywordRules.TryGetValue(key, out keywordText)) keywordText = "";
                parsedKeywords[key] = SplitRule(keywordText).ToArray();
            }
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop)) return result;
            try
            {
                foreach (string path in Directory.EnumerateFiles(desktop))
                {
                    try
                    {
                        if (string.Equals(Path.GetFileName(path), "desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;
                        if (DeskBoundManager.IsDeskBoundDesktopShortcut(path)) continue;
                        FileAttributes attributes = File.GetAttributes(path);
                        if ((attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0) continue;
                        string extension = Path.GetExtension(path).ToLowerInvariant();
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        string category = parsedKeywords.FirstOrDefault(pair => pair.Value.Any(keyword => fileName.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)).Key;
                        if (string.IsNullOrEmpty(category)) category = parsedExtensions.FirstOrDefault(pair => pair.Value.Contains(extension)).Key;
                        if (!string.IsNullOrEmpty(category)) result[category].Add(path);
                    }
                    catch { }
                }
            }
            catch { }
            return result;
        }

        private static IEnumerable<string> SplitRule(string value)
        {
            return (value ?? "").Split(new[] { ',', ';', '，', '；', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim()).Where(part => part.Length > 0).Distinct(StringComparer.CurrentCultureIgnoreCase);
        }

        public static string CategoryTitle(string key)
        {
            if (string.Equals(key, "Images", StringComparison.OrdinalIgnoreCase)) return I18n.T("圖片");
            if (string.Equals(key, "Documents", StringComparison.OrdinalIgnoreCase)) return I18n.T("文件");
            if (string.Equals(key, "Archives", StringComparison.OrdinalIgnoreCase)) return I18n.T("壓縮檔");
            if (string.Equals(key, "Installers", StringComparison.OrdinalIgnoreCase)) return I18n.T("安裝程式");
            if (string.Equals(key, "Media", StringComparison.OrdinalIgnoreCase)) return I18n.T("影音");
            if (string.Equals(key, "Shortcuts", StringComparison.OrdinalIgnoreCase)) return I18n.T("捷徑");
            return key;
        }
    }

    internal sealed class MoveBatchResult
    {
        public List<MoveRecord> Moves { get; private set; }
        public List<string> Errors { get; private set; }

        public MoveBatchResult()
        {
            Moves = new List<MoveRecord>();
            Errors = new List<string>();
        }
    }

    internal static class ManagedStorage
    {
        public static string GetRoot()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(documents))
                documents = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents");
            return Path.Combine(documents, "DeskBound Fences");
        }

        public static string EnsureFolder(FenceModel model)
        {
            if (!string.IsNullOrEmpty(model.ManagedPath))
            {
                Directory.CreateDirectory(model.ManagedPath);
                return model.ManagedPath;
            }

            string root = GetRoot();
            string label = SafeName(string.IsNullOrWhiteSpace(model.Title) ? "圍欄" : model.Title.Trim());
            string suffix = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString("N").Substring(0, 8) : model.Id.Substring(0, Math.Min(8, model.Id.Length));
            model.ManagedPath = Path.Combine(root, label + "-" + suffix);
            Directory.CreateDirectory(model.ManagedPath);
            return model.ManagedPath;
        }

        public static string EnsureFolder(FenceModel model, FenceTabModel tab)
        {
            if (tab == null) return EnsureFolder(model);
            if (!string.IsNullOrEmpty(tab.ManagedPath))
            {
                Directory.CreateDirectory(tab.ManagedPath);
                model.ManagedPath = tab.ManagedPath;
                return tab.ManagedPath;
            }

            string root = GetRoot();
            string fenceLabel = SafeName(string.IsNullOrWhiteSpace(model.Title) ? "圍欄" : model.Title.Trim());
            string fenceSuffix = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString("N").Substring(0, 8) : model.Id.Substring(0, Math.Min(8, model.Id.Length));
            string tabLabel = SafeName(string.IsNullOrWhiteSpace(tab.Title) ? "分頁" : tab.Title.Trim());
            string tabSuffix = string.IsNullOrEmpty(tab.Id) ? Guid.NewGuid().ToString("N").Substring(0, 8) : tab.Id.Substring(0, Math.Min(8, tab.Id.Length));
            // Keep new tab storage beside the primary fence folder. Nesting it inside the
            // primary folder made the internal "分頁" container appear as a normal item.
            tab.ManagedPath = Path.Combine(root, fenceLabel + "-" + fenceSuffix + "-分頁", tabLabel + "-" + tabSuffix);
            Directory.CreateDirectory(tab.ManagedPath);
            model.ManagedPath = tab.ManagedPath;
            return tab.ManagedPath;
        }

        public static MoveBatchResult MoveInto(IEnumerable<string> sourcePaths, string destinationFolder)
        {
            MoveBatchResult result = new MoveBatchResult();
            if (string.IsNullOrEmpty(destinationFolder))
            {
                result.Errors.Add("目的資料夾不存在。");
                return result;
            }

            string destination;
            try
            {
                destination = Path.GetFullPath(destinationFolder).TrimEnd(Path.DirectorySeparatorChar);
                Directory.CreateDirectory(destination);
            }
            catch (Exception ex)
            {
                result.Errors.Add("目的資料夾：" + ex.Message);
                return result;
            }
            foreach (string raw in sourcePaths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    string source = Path.GetFullPath(raw).TrimEnd(Path.DirectorySeparatorChar);
                    bool isDirectory = Directory.Exists(source);
                    if (!isDirectory && !File.Exists(source)) throw new FileNotFoundException("來源已不存在", source);
                    if (IsProtectedContainer(source)) throw new IOException("不允許移動系統或使用者主要資料夾。");
                    if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase)) continue;
                    if (isDirectory && IsInside(destination, source)) throw new IOException("不能把資料夾移入它自己裡面。");
                    string parent = Path.GetDirectoryName(source);
                    if (string.Equals(parent, destination, StringComparison.OrdinalIgnoreCase)) continue;

                    string target = UniquePath(Path.Combine(destination, Path.GetFileName(source)), isDirectory);
                    MoveOne(source, target, isDirectory);
                    result.Moves.Add(new MoveRecord { SourcePath = source, DestinationPath = target });
                }
                catch (Exception ex)
                {
                    result.Errors.Add(Path.GetFileName(raw) + "：" + ex.Message);
                }
            }
            return result;
        }

        // The returned Moves collection contains records that still could not be undone.
        public static MoveBatchResult Undo(IEnumerable<MoveRecord> records)
        {
            MoveBatchResult result = new MoveBatchResult();
            foreach (MoveRecord record in records.Reverse())
            {
                try
                {
                    bool isDirectory = Directory.Exists(record.DestinationPath);
                    if (!isDirectory && !File.Exists(record.DestinationPath))
                    {
                        if (Directory.Exists(record.SourcePath) || File.Exists(record.SourcePath)) continue;
                        throw new FileNotFoundException("已找不到移動後的項目", record.DestinationPath);
                    }
                    if (Directory.Exists(record.SourcePath) || File.Exists(record.SourcePath))
                        throw new IOException("原位置已有同名項目，未覆蓋。");
                    string parent = Path.GetDirectoryName(record.SourcePath);
                    if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                    MoveOne(record.DestinationPath, record.SourcePath, isDirectory);
                }
                catch (Exception ex)
                {
                    result.Moves.Add(record);
                    result.Errors.Add(Path.GetFileName(record.DestinationPath) + "：" + ex.Message);
                }
            }
            return result;
        }

        private static void MoveOne(string source, string destination, bool directory)
        {
            try
            {
                if (directory) Directory.Move(source, destination);
                else File.Move(source, destination);
            }
            catch (IOException)
            {
                string sourceRoot = Path.GetPathRoot(Path.GetFullPath(source));
                string destinationRoot = Path.GetPathRoot(Path.GetFullPath(destination));
                if (string.Equals(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase)) throw;
                if (directory) MoveDirectoryAcrossVolumes(source, destination);
                else MoveFileAcrossVolumes(source, destination);
            }
            NotifyShellMove(source, destination, directory);
        }

        private static void NotifyShellMove(string source, string destination, bool directory)
        {
            try
            {
                uint flags = NativeMethods.SHCNF_PATHW | NativeMethods.SHCNF_FLUSH;
                NativeMethods.SHChangeNotify(directory ? NativeMethods.SHCNE_RENAMEFOLDER : NativeMethods.SHCNE_RENAMEITEM,
                    flags, source, destination);

                // Explorer's desktop view can miss a rename event when an item moves
                // between folders. Explicit delete/create and parent-directory events
                // keep the source icon and destination listing in sync immediately.
                NativeMethods.SHChangeNotify(directory ? NativeMethods.SHCNE_RMDIR : NativeMethods.SHCNE_DELETE,
                    flags, source, null);
                NativeMethods.SHChangeNotify(directory ? NativeMethods.SHCNE_MKDIR : NativeMethods.SHCNE_CREATE,
                    flags, destination, null);

                string sourceParent = Path.GetDirectoryName(source);
                string destinationParent = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(sourceParent))
                    NativeMethods.SHChangeNotify(NativeMethods.SHCNE_UPDATEDIR, flags, sourceParent, null);
                if (!string.IsNullOrEmpty(destinationParent) &&
                    !string.Equals(sourceParent, destinationParent, StringComparison.OrdinalIgnoreCase))
                    NativeMethods.SHChangeNotify(NativeMethods.SHCNE_UPDATEDIR, flags, destinationParent, null);
            }
            catch { }
        }

        private static void MoveFileAcrossVolumes(string source, string destination)
        {
            string staging = destination + ".deskbound-part-" + Guid.NewGuid().ToString("N");
            try
            {
                File.Copy(source, staging, false);
                if (new FileInfo(source).Length != new FileInfo(staging).Length)
                    throw new IOException("跨磁碟複製後大小不一致，來源已保留。");
                File.Move(staging, destination);
                try { File.Delete(source); }
                catch
                {
                    try { if (File.Exists(destination)) File.Delete(destination); } catch { }
                    throw;
                }
            }
            finally
            {
                try { if (File.Exists(staging)) File.Delete(staging); } catch { }
            }
        }

        private static void MoveDirectoryAcrossVolumes(string source, string destination)
        {
            if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("跨磁碟移動不支援連結或接合點，來源已保留。");
            string staging = destination + ".deskbound-part-" + Guid.NewGuid().ToString("N");
            bool destinationReady = false;
            try
            {
                CopyDirectoryTree(source, staging);
                Directory.Move(staging, destination);
                destinationReady = true;
                Directory.Delete(source, true);
            }
            catch
            {
                if (!destinationReady)
                {
                    try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { }
                }
                throw;
            }
        }

        private static void CopyDirectoryTree(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.EnumerateFiles(source))
            {
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("資料夾含有連結，跨磁碟搬移已取消，來源仍保留。");
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            }
            foreach (string child in Directory.EnumerateDirectories(source))
            {
                if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("資料夾含有連結，跨磁碟搬移已取消，來源仍保留。");
                CopyDirectoryTree(child, Path.Combine(destination, Path.GetFileName(child)));
            }
        }

        private static string UniquePath(string requested, bool directory)
        {
            if (!File.Exists(requested) && !Directory.Exists(requested)) return requested;
            string folder = Path.GetDirectoryName(requested);
            string extension = directory ? "" : Path.GetExtension(requested);
            string name = directory ? Path.GetFileName(requested) : Path.GetFileNameWithoutExtension(requested);
            for (int number = 2; number < 10000; number++)
            {
                string candidate = Path.Combine(folder, name + " (" + number + ")" + extension);
                if (!File.Exists(candidate) && !Directory.Exists(candidate)) return candidate;
            }
            throw new IOException("同名項目過多，無法產生安全名稱。");
        }

        private static bool IsInside(string path, string folder)
        {
            string candidate = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string container = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return candidate.StartsWith(container, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsProtectedContainer(string path)
        {
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            string root = Path.GetPathRoot(full).TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(full, root, StringComparison.OrdinalIgnoreCase)) return true;
            string[] protectedFolders =
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
            return protectedFolders.Any(p => !string.IsNullOrEmpty(p) && string.Equals(full, Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
        }

        private static string SafeName(string value)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string clean = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim().TrimEnd('.');
            if (string.IsNullOrEmpty(clean)) clean = "圍欄";
            return clean.Length > 48 ? clean.Substring(0, 48) : clean;
        }

        public static int RunSelfTest()
        {
            string testRoot = Path.Combine(Path.GetTempPath(), "DeskBound-storage-selftest-" + Guid.NewGuid().ToString("N"));
            string report = Path.Combine(Environment.CurrentDirectory, "work", "DeskBound-storage-self-test.txt");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(report));
                string incoming = Path.Combine(testRoot, "desktop-sim");
                string fence = Path.Combine(testRoot, "fence");
                string outgoing = Path.Combine(testRoot, "desktop-return");
                Directory.CreateDirectory(incoming);
                Directory.CreateDirectory(fence);
                Directory.CreateDirectory(outgoing);
                string file = Path.Combine(incoming, "alpha.txt");
                string folder = Path.Combine(incoming, "project");
                Directory.CreateDirectory(folder);
                File.WriteAllText(file, "alpha-content");
                File.WriteAllText(Path.Combine(folder, "nested.txt"), "nested-content");

                MoveBatchResult into = MoveInto(new[] { file, folder }, fence);
                Assert(into.Errors.Count == 0 && into.Moves.Count == 2, "move-in batch");
                Assert(!File.Exists(file) && !Directory.Exists(folder), "sources disappear after move-in");
                Assert(File.ReadAllText(Path.Combine(fence, "alpha.txt")) == "alpha-content", "file content preserved");
                Assert(File.ReadAllText(Path.Combine(fence, "project", "nested.txt")) == "nested-content", "folder content preserved");

                MoveBatchResult outward = MoveInto(new[] { Path.Combine(fence, "alpha.txt") }, outgoing);
                Assert(outward.Errors.Count == 0 && File.Exists(Path.Combine(outgoing, "alpha.txt")), "move-out batch");
                MoveBatchResult undoOut = Undo(outward.Moves);
                Assert(undoOut.Errors.Count == 0 && File.Exists(Path.Combine(fence, "alpha.txt")), "undo move-out");

                string collisionSource = Path.Combine(testRoot, "collision", "alpha.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(collisionSource));
                File.WriteAllText(collisionSource, "second-content");
                MoveBatchResult collision = MoveInto(new[] { collisionSource }, fence);
                Assert(collision.Errors.Count == 0 && collision.Moves.Count == 1, "collision move");
                Assert(Path.GetFileName(collision.Moves[0].DestinationPath) == "alpha (2).txt", "collision auto-rename");
                Assert(File.ReadAllText(collision.Moves[0].DestinationPath) == "second-content", "collision content preserved");
                MoveBatchResult undoCollision = Undo(collision.Moves);
                Assert(undoCollision.Errors.Count == 0 && File.Exists(collisionSource), "undo collision move");

                string fallbackFileSource = Path.Combine(testRoot, "fallback-source.txt");
                string fallbackFileTarget = Path.Combine(testRoot, "fallback-target.txt");
                File.WriteAllText(fallbackFileSource, "fallback-file-content");
                MoveFileAcrossVolumes(fallbackFileSource, fallbackFileTarget);
                Assert(!File.Exists(fallbackFileSource) && File.ReadAllText(fallbackFileTarget) == "fallback-file-content", "cross-volume file fallback");
                string fallbackDirSource = Path.Combine(testRoot, "fallback-dir-source");
                string fallbackDirTarget = Path.Combine(testRoot, "fallback-dir-target");
                Directory.CreateDirectory(Path.Combine(fallbackDirSource, "child"));
                File.WriteAllText(Path.Combine(fallbackDirSource, "child", "payload.txt"), "fallback-directory-content");
                MoveDirectoryAcrossVolumes(fallbackDirSource, fallbackDirTarget);
                Assert(!Directory.Exists(fallbackDirSource) &&
                    File.ReadAllText(Path.Combine(fallbackDirTarget, "child", "payload.txt")) == "fallback-directory-content", "cross-volume directory fallback");

                MoveBatchResult undoInto = Undo(into.Moves);
                Assert(undoInto.Errors.Count == 0, "undo initial move");
                Assert(File.ReadAllText(file) == "alpha-content" && File.ReadAllText(Path.Combine(folder, "nested.txt")) == "nested-content", "round trip content preserved");

                string inboxDesktop = Path.Combine(testRoot, "inbox-desktop");
                string inboxDestination = Path.Combine(testRoot, "inbox-destination");
                Directory.CreateDirectory(inboxDesktop);
                Directory.CreateDirectory(inboxDestination);
                string existingDesktopFile = Path.Combine(inboxDesktop, "existing.txt");
                File.WriteAllText(existingDesktopFile, "existing");
                List<string> inboxBaseline = Directory.EnumerateFileSystemEntries(inboxDesktop).ToList();
                string newDesktopFile = Path.Combine(inboxDesktop, "new.txt");
                string newDesktopFolder = Path.Combine(inboxDesktop, "new-folder");
                File.WriteAllText(newDesktopFile, "new-content");
                Directory.CreateDirectory(newDesktopFolder);
                File.WriteAllText(Path.Combine(newDesktopFolder, "inside.txt"), "inside-content");
                List<string> detectedInboxItems = DeskBoundManager.FindNewDesktopInboxItems(inboxBaseline,
                    Directory.EnumerateFileSystemEntries(inboxDesktop));
                Assert(detectedInboxItems.Count == 2 && !detectedInboxItems.Contains(existingDesktopFile, StringComparer.OrdinalIgnoreCase),
                    "inbox detects only new items");
                Assert(DeskBoundManager.IsIncompleteDownloadPath("sample.zip.crdownload") &&
                    DeskBoundManager.IsIncompleteDownloadPath("sample.part") && !DeskBoundManager.IsIncompleteDownloadPath("sample.zip"),
                    "inbox ignores incomplete browser downloads");
                MoveBatchResult inboxMove = MoveInto(detectedInboxItems, inboxDestination);
                Assert(inboxMove.Errors.Count == 0 && !File.Exists(newDesktopFile) && !Directory.Exists(newDesktopFolder) &&
                    File.Exists(Path.Combine(inboxDestination, "new.txt")) && File.Exists(Path.Combine(inboxDestination, "new-folder", "inside.txt")),
                    "inbox moves files and folders");
                MoveBatchResult inboxUndo = Undo(inboxMove.Moves);
                Assert(inboxUndo.Errors.Count == 0 && File.Exists(newDesktopFile) && Directory.Exists(newDesktopFolder), "inbox move undo");

                FenceModel persisted = new FenceModel
                {
                    Id = "selftest", Title = "test", LastMoves = outward.Moves,
                    ItemSort = "Modified", ItemView = "List", ItemScale = 1.20, Locked = true,
                    CornerRadius = 22, ShadowStyle = "Strong", HideTitle = true, AutoCollapse = true, IsDesktopInbox = true,
                    ActiveTabId = "tab-docs", Tabs = new List<FenceTabModel>
                    {
                        new FenceTabModel { Id = "tab-main", Title = "主要", ManagedPath = fence },
                        new FenceTabModel { Id = "tab-docs", Title = "文件", RuleKey = "Documents", PortalPath = incoming, ItemView = "List" }
                    }
                };
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FenceModel restored = serializer.Deserialize<FenceModel>(serializer.Serialize(persisted));
                Assert(restored.LastMoves != null && restored.LastMoves.Count == 1 &&
                    restored.LastMoves[0].SourcePath == outward.Moves[0].SourcePath, "undo history persistence");
                Assert(restored.ItemSort == "Modified" && restored.ItemView == "List" && Math.Abs(restored.ItemScale - 1.20) < 0.01 && restored.Locked &&
                    Math.Abs(restored.CornerRadius - 22) < 0.01 && restored.ShadowStyle == "Strong" && restored.HideTitle && restored.AutoCollapse && restored.IsDesktopInbox,
                    "view preference persistence");
                Assert(restored.Tabs != null && restored.Tabs.Count == 2 && restored.ActiveTabId == "tab-docs" &&
                    restored.Tabs[1].RuleKey == "Documents" && restored.Tabs[1].PortalPath == incoming && restored.Tabs[1].ItemView == "List", "tab persistence");
                AppSettingsModel settingsRoundTrip = new AppSettingsModel
                {
                    DesktopInboxEnabled = true,
                    MoveHistory = new List<MoveHistoryEntry> { new MoveHistoryEntry { Label = "test", Moves = outward.Moves } },
                    OrganizerExtensions = new Dictionary<string, string> { { "Images", ".png, .jpg" } },
                    OrganizerKeywords = new Dictionary<string, string> { { "Images", "截圖" } }
                };
                AppSettingsModel restoredSettings = serializer.Deserialize<AppSettingsModel>(serializer.Serialize(settingsRoundTrip));
                Assert(restoredSettings.DesktopInboxEnabled && restoredSettings.MoveHistory.Count == 1 &&
                    restoredSettings.OrganizerExtensions["Images"].Contains(".png") && restoredSettings.OrganizerKeywords["Images"] == "截圖",
                    "inbox history and rules persistence");
                AppSettingsModel legacySettings = serializer.Deserialize<AppSettingsModel>("{\"DesktopInboxEnabled\":false}");
                Assert(legacySettings.AutoCheckUpdates, "automatic updates enabled for existing settings");
                Assert(DesktopAutoOrganizer.DefaultExtensionText("Images").Contains(".png"), "organizer defaults");

                string layoutRoot = Path.Combine(testRoot, "layout-store");
                LayoutStore layoutStore = new LayoutStore(layoutRoot);
                layoutStore.Save(new List<FenceModel> { new FenceModel { Id = "one", Title = "第一版" } });
                layoutStore.Save(new List<FenceModel> { new FenceModel { Id = "two", Title = "第二版" } });
                File.WriteAllText(Path.Combine(layoutRoot, "layout.json"), "{corrupted");
                List<FenceModel> recoveredLayout = layoutStore.Load();
                Assert(recoveredLayout.Count == 1 && recoveredLayout[0].Title == "第一版", "atomic layout backup recovery");

                string guardedLayoutRoot = Path.Combine(testRoot, "guarded-layout-store");
                LayoutStore guardedLayoutStore = new LayoutStore(guardedLayoutRoot, Path.Combine(testRoot, "no-managed-folders"));
                Assert(guardedLayoutStore.Save(new List<FenceModel>
                {
                    new FenceModel { Id = "keep-one", Title = "保留一" },
                    new FenceModel { Id = "keep-two", Title = "保留二" }
                }), "initial guarded layout save");
                Assert(!guardedLayoutStore.Save(new List<FenceModel> { new FenceModel { Id = "keep-one", Title = "保留一" } }),
                    "unexpected layout reduction blocked");
                Assert(guardedLayoutStore.Load().Count == 2, "blocked layout reduction preserves file");
                Assert(guardedLayoutStore.Save(new List<FenceModel> { new FenceModel { Id = "keep-one", Title = "保留一" } }, true) &&
                    guardedLayoutStore.Load().Count == 1, "explicit layout reduction allowed");

                string orphanRoot = Path.Combine(testRoot, "managed-folders");
                string recoveredFenceFolder = Path.Combine(orphanRoot, "遊戲-1234abcd");
                string recoveredTabFolder = Path.Combine(recoveredFenceFolder, "分頁", "常用-abcd1234");
                Directory.CreateDirectory(recoveredTabFolder);
                File.WriteAllText(Path.Combine(recoveredFenceFolder, "game.url"), "game");
                File.WriteAllText(Path.Combine(recoveredTabFolder, "tool.lnk"), "tool");
                LayoutStore folderRecoveryStore = new LayoutStore(Path.Combine(testRoot, "missing-layout-store"), orphanRoot);
                List<FenceModel> folderRecoveredLayout = folderRecoveryStore.Load();
                Assert(folderRecoveryStore.RecoveredFromFolders && folderRecoveredLayout.Count == 1 &&
                    folderRecoveredLayout[0].Title == "遊戲" && folderRecoveredLayout[0].Tabs.Any(tab => tab.Title == "常用"),
                    "existing managed folder recovery");

                string unreadableRoot = Path.Combine(testRoot, "unreadable-layout-store");
                Directory.CreateDirectory(unreadableRoot);
                string unreadableLayoutPath = Path.Combine(unreadableRoot, "layout.json");
                File.WriteAllText(unreadableLayoutPath, "{still-corrupted");
                LayoutStore unreadableStore = new LayoutStore(unreadableRoot, Path.Combine(testRoot, "empty-managed-root"));
                Assert(unreadableStore.Load().Count == 0 && unreadableStore.LoadFailed &&
                    !unreadableStore.Save(new List<FenceModel>()) && File.ReadAllText(unreadableLayoutPath) == "{still-corrupted",
                    "unreadable layout is never overwritten by empty defaults");

                string settingsRoot = Path.Combine(testRoot, "settings-store");
                AppSettingsStore settingsStoreTest = new AppSettingsStore(settingsRoot);
                Assert(settingsStoreTest.Save(new AppSettingsModel { UiLanguage = "zh-TW" }) &&
                    settingsStoreTest.Save(new AppSettingsModel { UiLanguage = "en-US" }), "atomic settings save");
                File.WriteAllText(Path.Combine(settingsRoot, "settings.json"), "{corrupted");
                AppSettingsStore settingsRecoveryStore = new AppSettingsStore(settingsRoot);
                AppSettingsModel recoveredPreferences = settingsRecoveryStore.Load();
                Assert(settingsRecoveryStore.RecoveredFromBackup && recoveredPreferences.UiLanguage == "zh-TW", "settings backup recovery");

                string unreadableSettingsRoot = Path.Combine(testRoot, "unreadable-settings-store");
                Directory.CreateDirectory(unreadableSettingsRoot);
                string unreadableSettingsPath = Path.Combine(unreadableSettingsRoot, "settings.json");
                File.WriteAllText(unreadableSettingsPath, "{still-corrupted");
                AppSettingsStore unreadableSettingsStore = new AppSettingsStore(unreadableSettingsRoot);
                Assert(unreadableSettingsStore.Load().SchemaVersion == AppSettingsModel.CurrentSchemaVersion && unreadableSettingsStore.LoadFailed &&
                    !unreadableSettingsStore.Save(new AppSettingsModel()) && File.ReadAllText(unreadableSettingsPath) == "{still-corrupted",
                    "unreadable settings are never overwritten by defaults");
                Assert(StartupManager.BuildCommand(@"C:\Program Files\DeskBound\DeskBound.exe") ==
                    "\"C:\\Program Files\\DeskBound\\DeskBound.exe\"", "startup command quoting");
                string startupXml = StartupManager.BuildTaskXml(@"C:\Apps & Tools\桌伴.exe", "S-1-5-21-123-456-789-1001");
                Assert(StartupManager.ReadTaskCommand(startupXml) == @"C:\Apps & Tools\桌伴.exe" &&
                    startupXml.Contains("<RunLevel>LeastPrivilege</RunLevel>") && startupXml.Contains("<LogonType>InteractiveToken</LogonType>") &&
                    startupXml.Contains("<Delay>PT0S</Delay>") && startupXml.Contains("<Priority>6</Priority>") &&
                    startupXml.Contains("<ExecutionTimeLimit>PT0S</ExecutionTimeLimit>"), "early startup task safety and quoting");
                Assert(ShortcutResolver.NormalizeIconPath("C:/Riot Games/Riot Client/RiotClientServices.exe") ==
                    @"C:\Riot Games\Riot Client\RiotClientServices.exe", "forward-slash icon path normalization");
                string iconFixture = Path.Combine(testRoot, "launcher icon.ico");
                // Different Windows shell versions may return either null or a generic
                // icon for a missing path. In both cases, creating the icon afterwards
                // must yield the real image rather than preserving the earlier result.
                ShellIconCache.Get(iconFixture);
                using (FileStream stream = File.Create(iconFixture)) Drawing.SystemIcons.Application.Save(stream);
                string urlFixture = Path.Combine(testRoot, "launcher.url");
                File.WriteAllText(urlFixture, "[InternetShortcut]\r\nURL=https://example.invalid/\r\nIconFile=" +
                    iconFixture.Replace('\\', '/') + "\r\nIconIndex=-42\r\n");
                ShortcutResolver.IconLocation iconLocation = ShortcutResolver.ResolveIconLocation(urlFixture);
                Assert(iconLocation.Path == iconFixture && iconLocation.Index == -42, "shortcut icon resource index retained");
                Assert(ShellIconCache.Get(iconFixture) != null, "late icon file is retried and decoded");
                Task<ImageSource> asyncIcon = ShellIconCache.GetAsync(urlFixture);
                Assert(asyncIcon.Wait(5000) && asyncIcon.Result != null && asyncIcon.Result.IsFrozen,
                    "background STA icon loading returns frozen image");

                Version parsedUpdateVersion;
                Assert(UpdateService.TryParseVersion("v0.13.0", out parsedUpdateVersion) && parsedUpdateVersion == new Version(0, 13, 0),
                    "update version parsing");
                string setupArguments = UpdateInstaller.BuildSetupArguments(@"C:\Apps & Tools\DeskBound");
                Assert(setupArguments.Contains("/DIR=\"C:\\Apps & Tools\\DeskBound\"") && setupArguments.Contains("/RESTARTAPPLICATIONS"),
                    "portable update keeps current install directory");
                Assert(DeskBoundManager.IsDeskBoundDesktopShortcut(@"C:\Users\Test\Desktop\桌伴.lnk") &&
                    DeskBoundManager.IsDeskBoundDesktopShortcut(@"C:\Users\Test\Desktop\DeskBound.lnk") &&
                    !DeskBoundManager.IsDeskBoundDesktopShortcut(@"C:\Users\Test\Desktop\Notes.lnk"), "desktop app shortcut exclusion");
                string updateJson = "{\"tag_name\":\"v0.15.0\",\"html_url\":\"https://github.com/bestdrduck/DeskBound/releases/tag/v0.15.0\",\"assets\":[{\"name\":\"DeskBound-Setup.exe\",\"browser_download_url\":\"https://example.invalid/DeskBound-Setup.exe\",\"size\":376832,\"digest\":\"sha256:abc\"}]}";
                UpdateRelease parsedRelease = UpdateService.ParseLatestRelease(updateJson);
                Assert(parsedRelease.Version == new Version(0, 15, 0) && parsedRelease.DownloadUrl.EndsWith("DeskBound-Setup.exe") && parsedRelease.AssetSize == 376832,
                    "update release parsing");
                string installerFixture = Path.Combine(testRoot, "published-setup.exe");
                byte[] installerBytes = new byte[120000];
                installerBytes[0] = (byte)'M'; installerBytes[1] = (byte)'Z';
                for (int index = 2; index < installerBytes.Length; index++) installerBytes[index] = (byte)(index % 251);
                File.WriteAllBytes(installerFixture, installerBytes);
                string installerDigest;
                using (SHA256 sha = SHA256.Create())
                    installerDigest = BitConverter.ToString(sha.ComputeHash(installerBytes)).Replace("-", "").ToLowerInvariant();
                string isolatedUpdateRoot = Path.Combine(testRoot, "updates");
                UpdateRelease installerRelease = new UpdateRelease
                {
                    Version = new Version(9, 9, 9), DownloadUrl = new Uri(installerFixture).AbsoluteUri,
                    AssetSize = installerBytes.Length, Digest = "sha256:" + installerDigest
                };
                string verifiedInstaller = UpdateService.DownloadRelease(installerRelease, isolatedUpdateRoot);
                Assert(File.Exists(verifiedInstaller) && new FileInfo(verifiedInstaller).Length == installerBytes.Length,
                    "isolated updater download and verification");
                installerRelease.Version = new Version(9, 9, 8);
                installerRelease.Digest = "sha256:" + new string('0', 64);
                bool invalidDigestRejected = false;
                try { UpdateService.DownloadRelease(installerRelease, isolatedUpdateRoot); }
                catch (InvalidDataException) { invalidDigestRejected = true; }
                Assert(invalidDigestRejected && !Directory.EnumerateFiles(isolatedUpdateRoot, "*.download").Any(),
                    "invalid update digest rejection and cleanup");
                AppSettingsModel migratedSettings = new AppSettingsModel { SchemaVersion = 0 };
                Assert(AppSettingsStore.Migrate(migratedSettings, 0) && migratedSettings.SchemaVersion == AppSettingsModel.CurrentSchemaVersion,
                    "settings schema migration");
                Assert(migratedSettings.UiLanguage == "System", "language migration default");
                I18n.Configure("en-US");
                Assert(I18n.T("圍欄管理") == "Panel management" && I18n.DashboardDate(new DateTime(2026, 9, 3)).StartsWith("Sep"),
                    "English localization");
                I18n.Configure("zh-TW");
                Assert(I18n.T("圍欄管理") == "圍欄管理", "Traditional Chinese localization");

                Assert(AppearanceMath.OutlineTintAlpha(0.65) < AppearanceMath.OutlineTintAlpha(0.98) &&
                    AppearanceMath.OutlineBaseAlpha(0.65) < AppearanceMath.OutlineBaseAlpha(0.98), "outline opacity response");
                Assert(AppearanceMath.SurfaceAlpha(0.20) == 51 && AppearanceMath.SurfaceAlpha(1.0) == 255 &&
                    AppearanceMath.OutlineBorderAlpha(0.20) < AppearanceMath.OutlineBorderAlpha(1.0), "opacity endpoints");

                File.WriteAllText(report, "PASS\r\nmove-in: PASS\r\nsource-removal: PASS\r\nmove-out: PASS\r\nundo: PASS\r\nundo-persistence: PASS\r\ntab-persistence: PASS\r\nview-preference-persistence: PASS\r\ninbox-new-item-detection: PASS\r\ninbox-partial-download-guard: PASS\r\ninbox-file-folder-move: PASS\r\ninbox-undo: PASS\r\ninbox-history-rules-persistence: PASS\r\nautomatic-update-default: PASS\r\nsettings-schema-migration: PASS\r\nlanguage-migration-default: PASS\r\nEnglish-localization: PASS\r\nTraditional-Chinese-localization: PASS\r\norganizer-defaults: PASS\r\noutline-opacity-response: PASS\r\nopacity-endpoints: PASS\r\ncollision-no-overwrite: PASS\r\ncross-volume-fallback: PASS\r\nlayout-backup-recovery: PASS\r\nlayout-reduction-guard: PASS\r\nmanaged-folder-recovery: PASS\r\nunreadable-layout-protection: PASS\r\nsettings-backup-recovery: PASS\r\nunreadable-settings-protection: PASS\r\nstartup-command: PASS\r\nupdate-version-parsing: PASS\r\nupdate-release-parsing: PASS\r\ncontent-integrity: PASS\r\n");
                File.AppendAllText(report, "early-startup-task: PASS\r\nicon-path-normalization: PASS\r\nicon-index: PASS\r\nlate-icon-retry: PASS\r\nbackground-icons: PASS\r\nportable-update-directory: PASS\r\ndesktop-shortcut-exclusion: PASS\r\nisolated-update-download: PASS\r\ninvalid-update-cleanup: PASS\r\n");
                return 0;
            }
            catch (Exception ex)
            {
                try { Directory.CreateDirectory(Path.GetDirectoryName(report)); File.WriteAllText(report, "FAIL\r\n" + ex); } catch { }
                return 1;
            }
            finally
            {
                try
                {
                    string temp = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    string resolved = Path.GetFullPath(testRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    if (resolved.StartsWith(temp, StringComparison.OrdinalIgnoreCase) && Directory.Exists(testRoot)) Directory.Delete(testRoot, true);
                }
                catch { }
            }
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("Self-test failed: " + name);
        }
    }

    internal sealed class UpdateRelease
    {
        public Version Version { get; set; }
        public string TagName { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseUrl { get; set; }
        public string Digest { get; set; }
        public long AssetSize { get; set; }
    }

    internal static class UpdateService
    {
        private const string LatestReleaseApi = "https://api.github.com/repos/bestdrduck/DeskBound/releases/latest";

        public static UpdateRelease GetLatestRelease()
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LatestReleaseApi);
            request.Method = "GET";
            request.UserAgent = "DeskBound/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            request.Accept = "application/vnd.github+json";
            request.Headers["X-GitHub-Api-Version"] = "2022-11-28";
            request.Timeout = 15000;
            request.ReadWriteTimeout = 15000;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                return ParseLatestRelease(reader.ReadToEnd());
        }

        internal static UpdateRelease ParseLatestRelease(string json)
        {
            object parsed = new JavaScriptSerializer().DeserializeObject(json);
            Dictionary<string, object> root = parsed as Dictionary<string, object>;
            if (root == null) throw new InvalidDataException("GitHub 回傳了無法辨識的更新資料。");

            string tag = ReadString(root, "tag_name");
            Version version;
            if (!TryParseVersion(tag, out version)) throw new InvalidDataException("GitHub Release 的版本號格式不正確。");

            Dictionary<string, object> selected = null;
            object assetsValue;
            if (root.TryGetValue("assets", out assetsValue))
            {
                IEnumerable<object> assets = assetsValue as IEnumerable<object>;
                if (assets != null)
                {
                    foreach (object item in assets)
                    {
                        Dictionary<string, object> asset = item as Dictionary<string, object>;
                        if (asset == null) continue;
                        string name = ReadString(asset, "name");
                        if (!string.IsNullOrEmpty(name) && name.StartsWith("DeskBound-Setup-", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        { selected = asset; break; }
                        if (string.Equals(name, "DeskBound-Setup.exe", StringComparison.OrdinalIgnoreCase)) selected = asset;
                    }
                }
            }
            if (selected == null) throw new InvalidDataException("最新 Release 中找不到 DeskBound 安裝程式。");

            long size = 0;
            object sizeValue;
            if (selected.TryGetValue("size", out sizeValue) && sizeValue != null) long.TryParse(Convert.ToString(sizeValue), out size);
            return new UpdateRelease
            {
                Version = version,
                TagName = tag,
                DownloadUrl = ReadString(selected, "browser_download_url"),
                ReleaseUrl = ReadString(root, "html_url"),
                Digest = ReadString(selected, "digest"),
                AssetSize = size
            };
        }

        private static string ReadString(Dictionary<string, object> values, string key)
        {
            object value;
            return values.TryGetValue(key, out value) && value != null ? Convert.ToString(value) : null;
        }

        internal static bool TryParseVersion(string tag, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tag)) return false;
            string value = tag.Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase)) value = value.Substring(1);
            int suffix = value.IndexOf('-');
            if (suffix >= 0) value = value.Substring(0, suffix);
            Version parsed;
            if (!Version.TryParse(value, out parsed)) return false;
            version = parsed;
            return true;
        }

        public static string DownloadRelease(UpdateRelease release)
        {
            return DownloadRelease(release, GetUpdateRoot());
        }

        internal static string DownloadRelease(UpdateRelease release, string root)
        {
            if (release == null || string.IsNullOrWhiteSpace(release.DownloadUrl)) throw new ArgumentException("沒有可下載的更新檔案。");
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("更新暫存位置不能是空白。", "root");
            root = Path.GetFullPath(root);
            Directory.CreateDirectory(root);
            string safeVersion = release.Version == null ? "latest" : release.Version.ToString(3);
            string partial = Path.Combine(root, "DeskBound-Setup-" + safeVersion + ".download");
            string completed = Path.Combine(root, "DeskBound-Setup-" + safeVersion + ".exe");
            try { if (File.Exists(partial)) File.Delete(partial); } catch { }

            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.UserAgent] = "DeskBound/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
                    client.Headers[HttpRequestHeader.Accept] = "application/octet-stream";
                    client.DownloadFile(release.DownloadUrl, partial);
                }

                FileInfo downloaded = new FileInfo(partial);
                if (!downloaded.Exists || downloaded.Length < 100000) throw new InvalidDataException("下載的更新檔案不完整。");
                if (release.AssetSize > 0 && downloaded.Length != release.AssetSize) throw new InvalidDataException("更新檔案大小與 GitHub 資料不符。");
                using (FileStream stream = File.OpenRead(partial))
                {
                    if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z') throw new InvalidDataException("下載內容不是有效的 Windows 程式。");
                }
                VerifyDigest(partial, release.Digest);
                if (File.Exists(completed)) File.Delete(completed);
                File.Move(partial, completed);
                return completed;
            }
            catch
            {
                try { if (File.Exists(partial)) File.Delete(partial); } catch { }
                throw;
            }
        }

        internal static void VerifyDigest(string path, string digest)
        {
            if (string.IsNullOrWhiteSpace(digest)) return;
            const string prefix = "sha256:";
            if (!digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("GitHub 提供了不支援的檔案驗證格式。");
            string expected = digest.Substring(prefix.Length).Trim();
            string actual;
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("更新檔案的 SHA-256 驗證失敗，已停止安裝。");
        }

        internal static string GetUpdateRoot()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskBound", "updates");
        }

        public static void CleanupStaleDownloads()
        {
            try
            {
                string root = GetUpdateRoot();
                if (!Directory.Exists(root)) return;
                string current = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                foreach (string file in Directory.EnumerateFiles(root))
                {
                    if (string.Equals(Path.GetFullPath(file), current, StringComparison.OrdinalIgnoreCase)) continue;
                    if (DateTime.UtcNow - File.GetLastWriteTimeUtc(file) > TimeSpan.FromDays(1))
                        File.Delete(file);
                }
            }
            catch { }
        }
    }

    internal static class UpdateInstaller
    {
        private static string ResultPath
        {
            get { return Path.Combine(UpdateService.GetUpdateRoot(), "update-result.txt"); }
        }

        public static int Apply(string[] args)
        {
            string target = null;
            string backup = null;
            try
            {
                if (args == null || args.Length < 5) throw new ArgumentException("更新參數不完整。");
                int parentId;
                if (!int.TryParse(args[1], out parentId)) throw new ArgumentException("更新程序識別碼不正確。");
                string source = Path.GetFullPath(args[2]);
                target = Path.GetFullPath(args[3]);
                string version = args[4];
                bool restart = !args.Skip(5).Any(a => string.Equals(a, "--no-restart", StringComparison.OrdinalIgnoreCase));
                string updateRoot = Path.GetFullPath(UpdateService.GetUpdateRoot()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!source.StartsWith(updateRoot, StringComparison.OrdinalIgnoreCase) || !string.Equals(Path.GetExtension(source), ".exe", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("更新檔案不在桌伴的安全更新資料夾中。");
                if (!string.Equals(Path.GetExtension(target), ".exe", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("更新目標不是 Windows 程式。");

                try
                {
                    Process parent = Process.GetProcessById(parentId);
                    if (!parent.HasExited) parent.WaitForExit(30000);
                }
                catch (ArgumentException) { }

                Directory.CreateDirectory(updateRoot);
                backup = Path.Combine(updateRoot, Path.GetFileNameWithoutExtension(target) + ".previous.exe");
                if (File.Exists(target)) File.Copy(target, backup, true);
                Exception lastError = null;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    try { File.Copy(source, target, true); lastError = null; break; }
                    catch (Exception ex) { lastError = ex; Thread.Sleep(250); }
                }
                if (lastError != null) throw lastError;
                if (!string.Equals(FileDigest(source), FileDigest(target), StringComparison.OrdinalIgnoreCase))
                    throw new IOException("更新後的程式檔案驗證失敗。");

                WriteResult("SUCCESS|" + version);
                if (restart) Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                return 0;
            }
            catch (Exception ex)
            {
                try { if (!string.IsNullOrEmpty(backup) && File.Exists(backup) && !string.IsNullOrEmpty(target)) File.Copy(backup, target, true); } catch { }
                WriteResult("ERROR|" + ex.Message);
                try
                {
                    bool restart = args == null || !args.Skip(5).Any(a => string.Equals(a, "--no-restart", StringComparison.OrdinalIgnoreCase));
                    if (restart && !string.IsNullOrEmpty(target) && File.Exists(target)) Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                }
                catch { }
                return 1;
            }
        }

        internal static string BuildSetupArguments(string installDirectory)
        {
            if (string.IsNullOrWhiteSpace(installDirectory)) throw new ArgumentException("更新位置不能是空白。", "installDirectory");
            string fullDirectory = Path.GetFullPath(installDirectory);
            string trimmed = fullDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string root = Path.GetPathRoot(fullDirectory);
            string directory = string.Equals(trimmed, (root ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase) ? root : trimmed;
            return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS /DIR=" + Quote(directory);
        }

        private static string FileDigest(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void WriteResult(string value)
        {
            try { Directory.CreateDirectory(UpdateService.GetUpdateRoot()); File.WriteAllText(ResultPath, value ?? ""); } catch { }
        }

        public static string ConsumeResult()
        {
            try
            {
                if (!File.Exists(ResultPath)) return null;
                string value = File.ReadAllText(ResultPath);
                File.Delete(ResultPath);
                return value;
            }
            catch { return null; }
        }

        public static string Quote(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
        }
    }

    internal static class StartupManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "DeskBound";
        private static string ExecutablePath { get { return Assembly.GetExecutingAssembly().Location; } }
        private static string UserSid { get { return System.Security.Principal.WindowsIdentity.GetCurrent().User.Value; } }
        internal static string TaskName { get { return "DeskBound-Logon-" + UserSid; } }

        public static string BuildCommand(string executablePath)
        {
            return "\"" + executablePath + "\"";
        }

        public static bool IsEnabled()
        {
            return IsHighPriorityEnabled() || string.Equals(ReadRunCommand(), BuildCommand(ExecutablePath), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHighPriorityEnabled()
        {
            try
            {
                string xml = ReadTaskXml();
                return IsTaskEnabled(xml) && string.Equals(ReadTaskCommand(xml), ExecutablePath, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        public static void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                if (!IsHighPriorityEnabled()) WriteRunCommand(BuildCommand(ExecutablePath));
                return;
            }
            // Do not report disabled while a logon task is still enabled.
            DeleteTask();
            WriteRunCommand(null);
        }

        public static void SetHighPriority(bool enabled)
        {
            if (enabled)
            {
                // Register and verify the replacement before removing normal startup.
                RegisterTask(ExecutablePath);
                if (!IsHighPriorityEnabled()) throw new IOException("Windows did not enable the DeskBound logon task.");
                WriteRunCommand(null);
            }
            else
            {
                string previousRun = ReadRunCommand();
                WriteRunCommand(BuildCommand(ExecutablePath));
                try { DeleteTask(); }
                catch { WriteRunCommand(previousRun); throw; }
            }
        }

        public static void RepairForCurrentExecutable()
        {
            string xml = ReadTaskXml();
            if (IsTaskEnabled(xml))
            {
                RegisterTask(ExecutablePath);
                WriteRunCommand(null);
            }
            else if (!string.IsNullOrWhiteSpace(ReadRunCommand())) WriteRunCommand(BuildCommand(ExecutablePath));
        }

        public static void RemoveForCurrentExecutable()
        {
            if (string.Equals(ReadTaskCommand(ReadTaskXml()), ExecutablePath, StringComparison.OrdinalIgnoreCase)) DeleteTask();
            if (string.Equals(ReadRunCommand(), BuildCommand(ExecutablePath), StringComparison.OrdinalIgnoreCase)) WriteRunCommand(null);
        }

        private static string ReadRunCommand()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey))
                return key == null ? null : key.GetValue(ValueName) as string;
        }

        private static void WriteRunCommand(string command)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (key == null) throw new IOException("無法開啟 Windows 啟動設定。");
                if (command != null) key.SetValue(ValueName, command, RegistryValueKind.String);
                else key.DeleteValue(ValueName, false);
            }
        }

        internal static string BuildTaskXml(string executablePath, string userSid)
        {
            string exe = System.Security.SecurityElement.Escape(executablePath);
            string folder = System.Security.SecurityElement.Escape(Path.GetDirectoryName(executablePath));
            string sid = System.Security.SecurityElement.Escape(userSid);
            return "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" +
                "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">" +
                "<RegistrationInfo><Description>Start DeskBound at user logon without Explorer startup delay.</Description></RegistrationInfo>" +
                "<Triggers><LogonTrigger><Enabled>true</Enabled><UserId>" + sid + "</UserId><Delay>PT0S</Delay></LogonTrigger></Triggers>" +
                "<Principals><Principal id=\"DeskBoundUser\"><UserId>" + sid + "</UserId><LogonType>InteractiveToken</LogonType><RunLevel>LeastPrivilege</RunLevel></Principal></Principals>" +
                "<Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>" +
                "<StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>false</AllowHardTerminate><StartWhenAvailable>true</StartWhenAvailable>" +
                "<RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled>" +
                "<Hidden>false</Hidden><RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>6</Priority></Settings>" +
                "<Actions Context=\"DeskBoundUser\"><Exec><Command>" + exe + "</Command><WorkingDirectory>" + folder + "</WorkingDirectory></Exec></Actions></Task>";
        }

        internal static string ReadTaskCommand(string xml)
        {
            return ReadTaskValue(xml, "/t:Task/t:Actions/t:Exec/t:Command");
        }

        private static bool IsTaskEnabled(string xml)
        {
            return !string.IsNullOrWhiteSpace(xml) &&
                !string.Equals(ReadTaskValue(xml, "/t:Task/t:Settings/t:Enabled"), "false", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadTaskValue(string xml, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            var document = new System.Xml.XmlDocument { XmlResolver = null };
            document.LoadXml(xml);
            var namespaces = new System.Xml.XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("t", "http://schemas.microsoft.com/windows/2004/02/mit/task");
            var node = document.SelectSingleNode(xpath, namespaces);
            return node == null ? null : node.InnerText;
        }

        private static string ReadTaskXml()
        {
            object service = null, folder = null, task = null;
            try
            {
                service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));
                ((dynamic)service).Connect();
                folder = ((dynamic)service).GetFolder("\\");
                task = ((dynamic)folder).GetTask(TaskName);
                return (string)((dynamic)task).Xml;
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == unchecked((int)0x80070002) || ex.ErrorCode == unchecked((int)0x8004130F)) return null;
                throw;
            }
            finally { ReleaseCom(task); ReleaseCom(folder); ReleaseCom(service); }
        }

        private static void RegisterTask(string executablePath)
        {
            object service = null, folder = null, task = null;
            try
            {
                service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));
                ((dynamic)service).Connect();
                folder = ((dynamic)service).GetFolder("\\");
                task = ((dynamic)folder).RegisterTask(TaskName, BuildTaskXml(executablePath, UserSid), 6, UserSid, null, 3, null);
            }
            finally { ReleaseCom(task); ReleaseCom(folder); ReleaseCom(service); }
        }

        private static void DeleteTask()
        {
            if (ReadTaskXml() == null) return;
            object service = null, folder = null;
            try
            {
                service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));
                ((dynamic)service).Connect();
                folder = ((dynamic)service).GetFolder("\\");
                ((dynamic)folder).DeleteTask(TaskName, 0);
            }
            finally { ReleaseCom(folder); ReleaseCom(service); }
        }

        private static void ReleaseCom(object value)
        {
            if (value != null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
        }
    }

    internal sealed class AppDialog : Window
    {
        private MessageBoxResult result;

        private AppDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            message = I18n.T(message);
            title = I18n.T(title);
            result = buttons == MessageBoxButton.YesNo ? MessageBoxResult.No : MessageBoxResult.OK;
            Title = string.IsNullOrWhiteSpace(title) ? "桌伴" : title;
            Icon = AppBrand.Logo;
            Width = 500; MinWidth = 420; MaxWidth = 640; SizeToContent = SizeToContent.Height;
            MaxHeight = 680; WindowStyle = WindowStyle.None; AllowsTransparency = true;
            Background = Brushes.Transparent; ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen; ShowInTaskbar = false;

            Window active = Application.Current == null ? null : Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive && !object.ReferenceEquals(w, this));
            if (active != null) { Owner = active; WindowStartupLocation = WindowStartupLocation.CenterOwner; }

            MediaColor accent = AccentPalette.ReadWindowsAccent();
            Border shell = new Border
            {
                CornerRadius = new CornerRadius(18), BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(MediaColor.FromRgb(51, 67, 80)),
                Background = new LinearGradientBrush(MediaColor.FromRgb(14, 21, 29), MediaColor.FromRgb(19, 28, 38), 35),
                Padding = new Thickness(22),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 34, ShadowDepth = 9, Opacity = 0.5 }
            };
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid header = new Grid { Cursor = Cursors.SizeAll };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel heading = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Border symbol = new Border
            {
                Width = 34, Height = 34, CornerRadius = new CornerRadius(11), Margin = new Thickness(0, 0, 12, 0),
                Background = image == MessageBoxImage.Warning
                    ? new SolidColorBrush(MediaColor.FromRgb(91, 62, 34))
                    : new SolidColorBrush(MediaColor.FromArgb(82, accent.R, accent.G, accent.B))
            };
            symbol.Child = new TextBlock
            {
                Text = image == MessageBoxImage.Warning ? "!" : (image == MessageBoxImage.Question ? "◆" : "i"),
                Foreground = image == MessageBoxImage.Warning ? new SolidColorBrush(MediaColor.FromRgb(255, 191, 102)) : Brushes.White,
                FontSize = 16, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            heading.Children.Add(symbol);
            heading.Children.Add(new TextBlock { Text = Title, Foreground = Brushes.White, FontSize = 18, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });
            header.Children.Add(heading);
            Button close = new Button { Content = "×", Width = 34, Height = 34, Foreground = Brushes.White, FontSize = 19, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Style = UiStyles.GhostButton(9), Cursor = Cursors.Hand };
            close.Click += delegate { Close(); };
            header.Children.Add(close); Grid.SetColumn(close, 1);
            header.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && FindParent<Button>(e.OriginalSource as DependencyObject) == null) DragMove(); };
            root.Children.Add(header);

            ScrollViewer textScroller = new ScrollViewer { MaxHeight = 340, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(46, 20, 2, 22) };
            textScroller.Content = new TextBlock { Text = message ?? "", Foreground = new SolidColorBrush(MediaColor.FromRgb(194, 207, 219)), FontSize = 13, LineHeight = 21, TextWrapping = TextWrapping.Wrap };
            root.Children.Add(textScroller); Grid.SetRow(textScroller, 1);

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            if (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel)
            {
                Button no = DialogButton(I18n.T("取消"), false, accent); no.Click += delegate { result = MessageBoxResult.No; Close(); }; actions.Children.Add(no);
                Button yes = DialogButton(I18n.T("確認"), true, accent); yes.Click += delegate { result = MessageBoxResult.Yes; Close(); }; actions.Children.Add(yes);
            }
            else if (buttons == MessageBoxButton.OKCancel)
            {
                Button cancel = DialogButton(I18n.T("取消"), false, accent); cancel.Click += delegate { result = MessageBoxResult.Cancel; Close(); }; actions.Children.Add(cancel);
                Button ok = DialogButton(I18n.T("確認"), true, accent); ok.Click += delegate { result = MessageBoxResult.OK; Close(); }; actions.Children.Add(ok);
            }
            else
            {
                Button ok = DialogButton(I18n.T("知道了"), true, accent); ok.Click += delegate { result = MessageBoxResult.OK; Close(); }; actions.Children.Add(ok);
            }
            root.Children.Add(actions); Grid.SetRow(actions, 2);
            shell.Child = root; Content = shell;
            PreviewKeyDown += delegate(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); };
        }

        private static Button DialogButton(string text, bool primary, MediaColor accent)
        {
            return new Button
            {
                Content = text, MinWidth = 94, Height = 38, Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(16, 0, 16, 0),
                Foreground = Brushes.White, Cursor = Cursors.Hand, BorderThickness = new Thickness(1), Style = UiStyles.GhostButton(10),
                Background = primary ? new SolidColorBrush(MediaColor.FromArgb(195, accent.R, accent.G, accent.B)) : new SolidColorBrush(MediaColor.FromRgb(28, 39, 49)),
                BorderBrush = primary ? new SolidColorBrush(MediaColor.FromArgb(235, accent.R, accent.G, accent.B)) : new SolidColorBrush(MediaColor.FromRgb(54, 70, 83))
            };
        }

        public static MessageBoxResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            AppDialog dialog = new AppDialog(message, title, buttons, image);
            dialog.ShowDialog();
            return dialog.result;
        }

        private static T FindParent<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null) { T value = node as T; if (value != null) return value; node = VisualTreeHelper.GetParent(node); }
            return null;
        }
    }

    internal static class UiStyles
    {
        public static void InstallApplicationTheme(Application app)
        {
            if (app == null) return;
            MediaColor accent = AccentPalette.ReadWindowsAccent();
            app.Resources[typeof(ScrollBar)] = DarkScrollBar(accent);
            app.Resources[typeof(CheckBox)] = DarkCheckBox(accent);
            app.Resources[typeof(Slider)] = DarkSlider(accent);
            app.Resources[typeof(TextBox)] = DarkTextBox(accent);
            app.Resources[typeof(ListBox)] = DarkListBox();
            app.Resources[typeof(ListBoxItem)] = DarkListBoxItem(accent);
        }

        public static DoubleAnimation EaseDouble(double from, double to, double milliseconds)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(milliseconds))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        public static Style DarkCheckBox(MediaColor accent)
        {
            string accentHex = string.Format("#{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string xaml = @"
<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TargetType=""{x:Type CheckBox}"">
  <Setter Property=""Foreground"" Value=""#EAF0F6"" />
  <Setter Property=""VerticalContentAlignment"" Value=""Center"" />
  <Setter Property=""Template"">
    <Setter.Value>
      <ControlTemplate TargetType=""{x:Type CheckBox}"">
        <Grid Background=""Transparent"">
          <Grid.ColumnDefinitions><ColumnDefinition Width=""Auto""/><ColumnDefinition Width=""*""/></Grid.ColumnDefinitions>
          <Border x:Name=""Box"" Width=""19"" Height=""19"" CornerRadius=""6"" Background=""#141E28"" BorderBrush=""#46596A"" BorderThickness=""1.2"" VerticalAlignment=""Center"">
            <Path x:Name=""Tick"" Data=""M 3,8 L 7,12 L 15,4"" Stroke=""White"" StrokeThickness=""2.2"" StrokeStartLineCap=""Round"" StrokeEndLineCap=""Round"" Visibility=""Collapsed""/>
          </Border>
          <ContentPresenter Grid.Column=""1"" Margin=""10,0,0,0"" VerticalAlignment=""{TemplateBinding VerticalContentAlignment}"" RecognizesAccessKey=""True""/>
        </Grid>
        <ControlTemplate.Triggers>
          <Trigger Property=""IsMouseOver"" Value=""True""><Setter TargetName=""Box"" Property=""BorderBrush"" Value=""#8196A9""/></Trigger>
          <Trigger Property=""IsChecked"" Value=""True""><Setter TargetName=""Box"" Property=""Background"" Value=""" + accentHex + @"""/><Setter TargetName=""Box"" Property=""BorderBrush"" Value=""" + accentHex + @"""/><Setter TargetName=""Tick"" Property=""Visibility"" Value=""Visible""/></Trigger>
          <Trigger Property=""IsEnabled"" Value=""False""><Setter Property=""Opacity"" Value=""0.42""/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>";
            return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
        }

        public static Style DarkTextBox(MediaColor accent)
        {
            string accentHex = string.Format("#{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string xaml = @"
<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TargetType=""{x:Type TextBox}"">
  <Setter Property=""Foreground"" Value=""#F1F5F9""/><Setter Property=""CaretBrush"" Value=""White""/>
  <Setter Property=""SelectionBrush"" Value=""" + accentHex + @"""/><Setter Property=""Background"" Value=""#151F29""/>
  <Setter Property=""BorderBrush"" Value=""#3B4B59""/><Setter Property=""BorderThickness"" Value=""1""/>
  <Setter Property=""Padding"" Value=""10,7""/>
  <Setter Property=""Template""><Setter.Value><ControlTemplate TargetType=""{x:Type TextBox}"">
    <Border x:Name=""Frame"" Background=""{TemplateBinding Background}"" BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" CornerRadius=""8"">
      <ScrollViewer x:Name=""PART_ContentHost"" Margin=""{TemplateBinding Padding}""/>
    </Border>
    <ControlTemplate.Triggers>
      <Trigger Property=""IsKeyboardFocusWithin"" Value=""True""><Setter TargetName=""Frame"" Property=""BorderBrush"" Value=""" + accentHex + @"""/></Trigger>
      <Trigger Property=""IsMouseOver"" Value=""True""><Setter TargetName=""Frame"" Property=""Background"" Value=""#192630""/></Trigger>
      <Trigger Property=""IsEnabled"" Value=""False""><Setter Property=""Opacity"" Value=""0.45""/></Trigger>
    </ControlTemplate.Triggers>
  </ControlTemplate></Setter.Value></Setter>
</Style>";
            return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
        }

        public static Style DarkSlider(MediaColor accent)
        {
            string accentHex = string.Format("#{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string xaml = @"
<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TargetType=""{x:Type Slider}"">
  <Setter Property=""Height"" Value=""28""/><Setter Property=""Focusable"" Value=""False""/>
  <Setter Property=""Template""><Setter.Value><ControlTemplate TargetType=""{x:Type Slider}"">
    <Grid Background=""Transparent"" Margin=""2,0"">
      <Border Height=""5"" CornerRadius=""3"" Background=""#283745"" VerticalAlignment=""Center""/>
      <Track x:Name=""PART_Track"" Minimum=""{TemplateBinding Minimum}"" Maximum=""{TemplateBinding Maximum}"" Value=""{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"" IsDirectionReversed=""False"" VerticalAlignment=""Center"">
        <Track.DecreaseRepeatButton><RepeatButton Command=""{x:Static Slider.DecreaseLarge}"" Height=""5"" Background=""" + accentHex + @""" BorderThickness=""0"" Focusable=""False""><RepeatButton.Template><ControlTemplate TargetType=""RepeatButton""><Border Background=""{TemplateBinding Background}"" CornerRadius=""3""/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton>
        <Track.Thumb><Thumb Width=""18"" Height=""18"" Cursor=""Hand""><Thumb.Template><ControlTemplate TargetType=""Thumb""><Grid><Ellipse Fill=""#121B24"" Stroke=""" + accentHex + @""" StrokeThickness=""3""/><Ellipse Width=""6"" Height=""6"" Fill=""White""/></Grid></ControlTemplate></Thumb.Template></Thumb></Track.Thumb>
        <Track.IncreaseRepeatButton><RepeatButton Command=""{x:Static Slider.IncreaseLarge}"" Opacity=""0"" Focusable=""False""/></Track.IncreaseRepeatButton>
      </Track>
    </Grid>
    <ControlTemplate.Triggers><Trigger Property=""IsEnabled"" Value=""False""><Setter Property=""Opacity"" Value=""0.4""/></Trigger></ControlTemplate.Triggers>
  </ControlTemplate></Setter.Value></Setter>
</Style>";
            return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
        }

        public static Style DarkListBox()
        {
            Style style = new Style(typeof(ListBox));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto));
            style.Setters.Add(new Setter(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled));
            return style;
        }

        public static Style DarkListBoxItem(MediaColor accent)
        {
            string selected = string.Format("#58{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string outline = string.Format("#D8{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string xaml = @"
<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TargetType=""{x:Type ListBoxItem}"">
  <Setter Property=""Foreground"" Value=""#EEF3F8""/><Setter Property=""Padding"" Value=""11,9""/><Setter Property=""Margin"" Value=""2,2""/><Setter Property=""HorizontalContentAlignment"" Value=""Stretch""/>
  <Setter Property=""Template""><Setter.Value><ControlTemplate TargetType=""{x:Type ListBoxItem}"">
    <Border x:Name=""Row"" Background=""Transparent"" BorderBrush=""Transparent"" BorderThickness=""1"" CornerRadius=""8"" Padding=""{TemplateBinding Padding}""><ContentPresenter/></Border>
    <ControlTemplate.Triggers>
      <Trigger Property=""IsMouseOver"" Value=""True""><Setter TargetName=""Row"" Property=""Background"" Value=""#263645""/></Trigger>
      <Trigger Property=""IsSelected"" Value=""True""><Setter TargetName=""Row"" Property=""Background"" Value=""" + selected + @"""/><Setter TargetName=""Row"" Property=""BorderBrush"" Value=""" + outline + @"""/></Trigger>
      <Trigger Property=""IsEnabled"" Value=""False""><Setter Property=""Opacity"" Value=""0.4""/></Trigger>
    </ControlTemplate.Triggers>
  </ControlTemplate></Setter.Value></Setter>
</Style>";
            return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
        }

        public static void PrepareDarkContextMenu(ContextMenu menu, MediaColor accent)
        {
            SolidColorBrush surface = new SolidColorBrush(MediaColor.FromRgb(28, 32, 43));
            SolidColorBrush text = new SolidColorBrush(MediaColor.FromRgb(243, 245, 252));
            SolidColorBrush highlight = new SolidColorBrush(MediaColor.FromArgb(86, accent.R, accent.G, accent.B));
            menu.Background = surface;
            menu.Foreground = text;
            menu.BorderBrush = new SolidColorBrush(MediaColor.FromArgb(205, accent.R, accent.G, accent.B));
            menu.BorderThickness = new Thickness(1);
            menu.Padding = new Thickness(0);
            menu.MaxHeight = Math.Max(420, SystemParameters.WorkArea.Height - 48);
            menu.Resources[SystemColors.MenuBrushKey] = surface;
            menu.Resources[SystemColors.MenuTextBrushKey] = text;
            menu.Resources[SystemColors.HighlightBrushKey] = highlight;
            menu.Resources[SystemColors.HighlightTextBrushKey] = Brushes.White;
            menu.Resources[SystemColors.ControlBrushKey] = surface;
            menu.Resources[SystemColors.ControlLightBrushKey] = surface;
            menu.Resources[SystemColors.ControlLightLightBrushKey] = surface;
            menu.Resources[SystemColors.ControlDarkBrushKey] = surface;
            menu.Resources[SystemColors.WindowBrushKey] = surface;
            menu.Resources[SystemColors.MenuBarBrushKey] = surface;

            ControlTemplate contextTemplate = new ControlTemplate(typeof(ContextMenu));
            FrameworkElementFactory contextBorder = new FrameworkElementFactory(typeof(Border));
            contextBorder.SetValue(Border.BackgroundProperty, surface);
            contextBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(MediaColor.FromArgb(205, accent.R, accent.G, accent.B)));
            contextBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            contextBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(13));
            contextBorder.SetValue(Border.PaddingProperty, new Thickness(8));
            contextBorder.SetValue(Border.EffectProperty, new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = MediaColors.Black, BlurRadius = 24, ShadowDepth = 7, Opacity = 0.48
            });
            FrameworkElementFactory contextScroll = new FrameworkElementFactory(typeof(ScrollViewer));
            contextScroll.SetValue(ScrollViewer.CanContentScrollProperty, true);
            contextScroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            FrameworkElementFactory contextItems = new FrameworkElementFactory(typeof(ItemsPresenter));
            contextScroll.AppendChild(contextItems);
            contextBorder.AppendChild(contextScroll);
            contextTemplate.VisualTree = contextBorder;
            menu.Template = contextTemplate;

            Style itemStyle = new Style(typeof(MenuItem));
            itemStyle.Setters.Add(new Setter(Control.ForegroundProperty, text));
            itemStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            itemStyle.Setters.Add(new Setter(Control.FontFamilyProperty, new FontFamily("Segoe UI Variable Text, Segoe UI")));
            itemStyle.Setters.Add(new Setter(Control.FontSizeProperty, 13.5));
            itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14, 9, 16, 9)));
            itemStyle.Setters.Add(new Setter(Control.MinWidthProperty, 238.0));

            ControlTemplate menuTemplate = new ControlTemplate(typeof(MenuItem));
            FrameworkElementFactory root = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory itemBorder = new FrameworkElementFactory(typeof(Border));
            itemBorder.Name = "ItemBorder";
            itemBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(9));
            itemBorder.SetValue(Border.MarginProperty, new Thickness(2, 2, 2, 2));
            itemBorder.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

            FrameworkElementFactory itemGrid = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            presenter.SetValue(FrameworkElement.MarginProperty, new Thickness(34, 0, 32, 0));
            presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
            check.Name = "CheckMark";
            check.SetValue(TextBlock.TextProperty, "✓");
            check.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            check.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(accent));
            check.SetValue(UIElement.OpacityProperty, 0.0);
            check.SetValue(FrameworkElement.MarginProperty, new Thickness(9, 0, 0, 0));
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            FrameworkElementFactory arrow = new FrameworkElementFactory(typeof(TextBlock));
            arrow.Name = "SubmenuArrow";
            arrow.SetValue(TextBlock.TextProperty, "›");
            arrow.SetValue(TextBlock.FontSizeProperty, 18.0);
            arrow.SetValue(TextBlock.ForegroundProperty, text);
            arrow.SetValue(UIElement.OpacityProperty, 0.0);
            arrow.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0));
            arrow.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            arrow.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            itemGrid.AppendChild(presenter);
            itemGrid.AppendChild(check);
            itemGrid.AppendChild(arrow);
            itemBorder.AppendChild(itemGrid);
            root.AppendChild(itemBorder);

            FrameworkElementFactory popup = new FrameworkElementFactory(typeof(Popup));
            popup.Name = "PART_Popup";
            popup.SetValue(Popup.AllowsTransparencyProperty, true);
            popup.SetValue(Popup.FocusableProperty, false);
            popup.SetValue(Popup.PlacementProperty, PlacementMode.Right);
            popup.SetValue(Popup.HorizontalOffsetProperty, -3.0);
            popup.SetValue(Popup.VerticalOffsetProperty, -7.0);
            popup.SetBinding(Popup.IsOpenProperty, new System.Windows.Data.Binding("IsSubmenuOpen")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            popup.SetBinding(Popup.PlacementTargetProperty, new System.Windows.Data.Binding()
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            FrameworkElementFactory popupBorder = new FrameworkElementFactory(typeof(Border));
            popupBorder.SetValue(Border.BackgroundProperty, surface);
            popupBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(MediaColor.FromArgb(205, accent.R, accent.G, accent.B)));
            popupBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            popupBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
            popupBorder.SetValue(Border.PaddingProperty, new Thickness(8));
            FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer));
            scroll.SetValue(ScrollViewer.CanContentScrollProperty, true);
            scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            FrameworkElementFactory itemsPresenter = new FrameworkElementFactory(typeof(ItemsPresenter));
            scroll.AppendChild(itemsPresenter);
            popupBorder.AppendChild(scroll);
            popup.AppendChild(popupBorder);
            root.AppendChild(popup);
            menuTemplate.VisualTree = root;

            Trigger highlighted = new Trigger { Property = MenuItem.IsHighlightedProperty, Value = true };
            highlighted.Setters.Add(new Setter(Control.BackgroundProperty, highlight));
            highlighted.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            menuTemplate.Triggers.Add(highlighted);
            Trigger checkedItem = new Trigger { Property = MenuItem.IsCheckedProperty, Value = true };
            checkedItem.Setters.Add(new Setter(UIElement.OpacityProperty, 1.0, "CheckMark"));
            menuTemplate.Triggers.Add(checkedItem);
            Trigger hasItems = new Trigger { Property = ItemsControl.HasItemsProperty, Value = true };
            hasItems.Setters.Add(new Setter(UIElement.OpacityProperty, 1.0, "SubmenuArrow"));
            menuTemplate.Triggers.Add(hasItems);
            Trigger disabled = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            disabled.Setters.Add(new Setter(UIElement.OpacityProperty, 0.42, "ItemBorder"));
            menuTemplate.Triggers.Add(disabled);
            itemStyle.Setters.Add(new Setter(Control.TemplateProperty, menuTemplate));
            menu.Resources[typeof(MenuItem)] = itemStyle;

            Style separatorStyle = new Style(typeof(Separator));
            separatorStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 1.0));
            separatorStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(8, 5, 8, 5)));
            separatorStyle.Setters.Add(new Setter(Control.BackgroundProperty,
                new SolidColorBrush(MediaColor.FromArgb(52, 255, 255, 255))));
            menu.Resources[typeof(Separator)] = separatorStyle;
            menu.Resources[typeof(ScrollBar)] = DarkScrollBar(accent);
        }

        public static Style DarkScrollBar(MediaColor accent)
        {
            string normal = string.Format("#A8{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string hover = string.Format("#E8{0:X2}{1:X2}{2:X2}", accent.R, accent.G, accent.B);
            string xaml = @"
<Style xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" TargetType=""{x:Type ScrollBar}"">
  <Setter Property=""Width"" Value=""10"" />
  <Setter Property=""MinWidth"" Value=""10"" />
  <Setter Property=""Opacity"" Value=""0.42"" />
  <Setter Property=""Background"" Value=""Transparent"" />
  <Setter Property=""Template"">
    <Setter.Value>
      <ControlTemplate TargetType=""{x:Type ScrollBar}"">
        <Grid Background=""Transparent"" Margin=""1,3"">
          <Track x:Name=""PART_Track"" IsDirectionReversed=""True"" Focusable=""False"">
            <Track.DecreaseRepeatButton>
              <RepeatButton Command=""{x:Static ScrollBar.PageUpCommand}"" Opacity=""0"" Focusable=""False"" />
            </Track.DecreaseRepeatButton>
            <Track.Thumb>
              <Thumb MinHeight=""30"" Cursor=""Hand"">
                <Thumb.Template>
                  <ControlTemplate TargetType=""{x:Type Thumb}"">
                    <Border x:Name=""Grip"" Margin=""2,0"" CornerRadius=""4"" Background=""" + normal + @""" />
                    <ControlTemplate.Triggers>
                      <Trigger Property=""IsMouseOver"" Value=""True"">
                        <Setter TargetName=""Grip"" Property=""Background"" Value=""" + hover + @""" />
                      </Trigger>
                    </ControlTemplate.Triggers>
                  </ControlTemplate>
                </Thumb.Template>
              </Thumb>
            </Track.Thumb>
            <Track.IncreaseRepeatButton>
              <RepeatButton Command=""{x:Static ScrollBar.PageDownCommand}"" Opacity=""0"" Focusable=""False"" />
            </Track.IncreaseRepeatButton>
          </Track>
        </Grid>
        <ControlTemplate.Triggers>
          <Trigger Property=""IsMouseOver"" Value=""True"">
            <Setter Property=""Opacity"" Value=""1"" />
          </Trigger>
          <Trigger Property=""IsEnabled"" Value=""False"">
            <Setter Property=""Opacity"" Value=""0.35"" />
          </Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>";
            return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
        }

        public static Style GhostButton(double radius)
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
            border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new System.Windows.Data.Binding("HorizontalContentAlignment") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            presenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, new System.Windows.Data.Binding("VerticalContentAlignment") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.AppendChild(presenter);
            template.VisualTree = border;

            Trigger hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(MediaColor.FromArgb(30, 255, 255, 255))));
            template.Triggers.Add(hover);
            Trigger pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(MediaColor.FromArgb(52, 255, 255, 255))));
            template.Triggers.Add(pressed);
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            return style;
        }

        public static Style ItemToggleButton(double radius, MediaColor accent)
        {
            Style style = new Style(typeof(ToggleButton));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            ControlTemplate template = new ControlTemplate(typeof(ToggleButton));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
            border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            FrameworkElementFactory itemGrid = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new System.Windows.Data.Binding("HorizontalContentAlignment") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            presenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, new System.Windows.Data.Binding("VerticalContentAlignment") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            itemGrid.AppendChild(presenter);
            FrameworkElementFactory badge = new FrameworkElementFactory(typeof(Border));
            badge.Name = "SelectionBadge";
            badge.SetValue(FrameworkElement.WidthProperty, 20.0);
            badge.SetValue(FrameworkElement.HeightProperty, 20.0);
            badge.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 5, 5, 0));
            badge.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            badge.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            badge.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            badge.SetValue(Border.BackgroundProperty, new SolidColorBrush(MediaColor.FromRgb(accent.R, accent.G, accent.B)));
            badge.SetValue(Border.BorderBrushProperty, Brushes.White);
            badge.SetValue(Border.BorderThicknessProperty, new Thickness(1.5));
            badge.SetValue(UIElement.OpacityProperty, 0.0);
            FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
            check.SetValue(TextBlock.TextProperty, "✓");
            check.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            check.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
            check.SetValue(TextBlock.FontSizeProperty, 12.0);
            check.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            check.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            badge.AppendChild(check);
            itemGrid.AppendChild(badge);
            border.AppendChild(itemGrid);
            template.VisualTree = border;

            Trigger hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(MediaColor.FromArgb(38, 255, 255, 255))));
            hover.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(MediaColor.FromArgb(78, 255, 255, 255))));
            template.Triggers.Add(hover);
            Trigger selected = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
            selected.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(MediaColor.FromArgb(132, accent.R, accent.G, accent.B))));
            selected.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(MediaColor.FromArgb(255, accent.R, accent.G, accent.B))));
            selected.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(2.0)));
            selected.Setters.Add(new Setter(UIElement.OpacityProperty, 1.0, "SelectionBadge"));
            template.Triggers.Add(selected);
            Trigger pressed = new Trigger { Property = ButtonBase.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(MediaColor.FromArgb(105, accent.R, accent.G, accent.B))));
            template.Triggers.Add(pressed);
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            return style;
        }
    }

    internal static class AppearanceMath
    {
        public static double NormalizeOpacity(double opacity)
        {
            return Math.Max(0.20, Math.Min(1.0, opacity));
        }

        public static byte SurfaceAlpha(double opacity)
        {
            return (byte)Math.Round(NormalizeOpacity(opacity) * 255);
        }

        public static byte SurfaceBottomAlpha(double opacity)
        {
            return (byte)Math.Round(NormalizeOpacity(opacity) * 0.88 * 255);
        }

        public static byte OutlineTintAlpha(double opacity)
        {
            return SurfaceAlpha(opacity);
        }

        public static byte OutlineBaseAlpha(double opacity)
        {
            return SurfaceBottomAlpha(opacity);
        }

        public static byte OutlineBorderAlpha(double opacity)
        {
            double normalized = NormalizeOpacity(opacity);
            return (byte)Math.Round((0.30 + normalized * 0.65) * 255);
        }

        public static byte OutlineHeaderAlpha(double opacity)
        {
            return (byte)Math.Round(NormalizeOpacity(opacity) * 0.22 * 255);
        }
    }

    internal static class AccentPalette
    {
        public static MediaColor ReadWindowsAccent()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    object value = key == null ? null : key.GetValue("ColorizationColor");
                    if (value != null)
                    {
                        uint raw = Convert.ToUInt32(value);
                        return MediaColor.FromRgb((byte)((raw >> 16) & 255), (byte)((raw >> 8) & 255), (byte)(raw & 255));
                    }
                }
            }
            catch { }
            return MediaColor.FromRgb(124, 140, 255);
        }

        public static string ToHex(MediaColor c) { return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B); }

        public static MediaColor Parse(string value)
        {
            try { return (MediaColor)ColorConverter.ConvertFromString(value); }
            catch { return MediaColor.FromRgb(124, 140, 255); }
        }
    }

    internal static class ShortcutResolver
    {
        public static string ResolveIconSource(string path)
        {
            return ResolveIconLocation(path).Path;
        }

        internal sealed class IconLocation
        {
            public string Path;
            public int Index;
        }

        internal static string NormalizeIconPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            string expanded = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"')).Replace('/', '\\');
            try { return System.IO.Path.GetFullPath(expanded); }
            catch { return expanded; }
        }

        internal static IconLocation ResolveIconLocation(string path)
        {
            if (string.IsNullOrEmpty(path)) return new IconLocation { Path = path };
            if (string.Equals(Path.GetExtension(path), ".url", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string iconPath = null;
                    int index = 0;
                    foreach (string line in File.ReadAllLines(path))
                    {
                        if (line.StartsWith("IconFile=", StringComparison.OrdinalIgnoreCase))
                            iconPath = NormalizeIconPath(line.Substring("IconFile=".Length));
                        if (line.StartsWith("IconIndex=", StringComparison.OrdinalIgnoreCase))
                            int.TryParse(line.Substring("IconIndex=".Length).Trim(), out index);
                    }
                    if (File.Exists(iconPath)) return new IconLocation { Path = iconPath, Index = index };
                }
                catch { }
                return new IconLocation { Path = NormalizeIconPath(path) };
            }
            if (!string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase))
                return new IconLocation { Path = NormalizeIconPath(path) };

            object shellLink = null;
            try
            {
                shellLink = new ShellLinkClass();
                ((System.Runtime.InteropServices.ComTypes.IPersistFile)shellLink).Load(path, 0);
                StringBuilder iconLocation = new StringBuilder(1024);
                int iconIndex;
                ((IShellLinkW)shellLink).GetIconLocation(iconLocation, iconLocation.Capacity, out iconIndex);
                string iconPath = NormalizeIconPath(iconLocation.ToString());
                if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
                    return new IconLocation { Path = iconPath, Index = iconIndex };
            }
            catch { }
            finally
            {
                if (shellLink != null && Marshal.IsComObject(shellLink)) Marshal.FinalReleaseComObject(shellLink);
            }
            return new IconLocation { Path = NormalizeIconPath(ResolveTarget(path)) };
        }

        public static string ResolveTarget(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (string.Equals(Path.GetExtension(path), ".url", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        if (!line.StartsWith("IconFile=", StringComparison.OrdinalIgnoreCase)) continue;
                        string iconPath = Environment.ExpandEnvironmentVariables(line.Substring("IconFile=".Length).Trim().Trim('"'));
                        if (File.Exists(iconPath)) return iconPath;
                    }
                }
                catch { }
                return path;
            }
            if (!string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase)) return path;
            object shellLink = null;
            try
            {
                shellLink = new ShellLinkClass();
                System.Runtime.InteropServices.ComTypes.IPersistFile file = (System.Runtime.InteropServices.ComTypes.IPersistFile)shellLink;
                file.Load(path, 0);
                StringBuilder target = new StringBuilder(1024);
                ((IShellLinkW)shellLink).GetPath(target, target.Capacity, IntPtr.Zero, 0);
                string resolved = NormalizeIconPath(target.ToString());
                if (!string.IsNullOrWhiteSpace(resolved) && (File.Exists(resolved) || Directory.Exists(resolved))) return resolved;
            }
            catch { }
            finally
            {
                if (shellLink != null && Marshal.IsComObject(shellLink)) Marshal.FinalReleaseComObject(shellLink);
            }
            return path;
        }

        [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLinkClass { }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder file, int maxPath, IntPtr findData, uint flags);
            void GetIDList(out IntPtr itemIdList);
            void SetIDList(IntPtr itemIdList);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory, int maxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments, int maxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);
            void GetHotkey(out short hotkey);
            void SetHotkey(short hotkey);
            void GetShowCmd(out int showCommand);
            void SetShowCmd(int showCommand);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder iconPath, int maxPath, out int iconIndex);
        }
    }

    internal static class ShellIconCache
    {
        private static readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        private static readonly object cacheLock = new object();
        private static readonly System.Collections.Concurrent.BlockingCollection<Action> iconQueue = new System.Collections.Concurrent.BlockingCollection<Action>();
        private static Thread iconThread;

        public static Task<ImageSource> GetAsync(string path)
        {
            var completion = new TaskCompletionSource<ImageSource>();
            lock (cacheLock)
            {
                if (iconThread == null)
                {
                    iconThread = new Thread(delegate()
                    {
                        foreach (Action action in iconQueue.GetConsumingEnumerable()) action();
                    }) { IsBackground = true, Name = "DeskBound icons" };
                    iconThread.SetApartmentState(ApartmentState.STA);
                    iconThread.Start();
                }
            }
            iconQueue.Add(delegate
            {
                try { completion.SetResult(Get(path)); }
                catch { completion.SetResult(null); }
            });
            return completion.Task;
        }

        public static ImageSource Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            ShortcutResolver.IconLocation location = ShortcutResolver.ResolveIconLocation(path);
            string iconSourcePath = location.Path;
            string key = path + "|" + File.GetLastWriteTimeUtc(path).Ticks + "|" + iconSourcePath + "|" +
                location.Index + "|" + File.GetLastWriteTimeUtc(iconSourcePath).Ticks;
            ImageSource source;
            lock (cacheLock) { if (cache.TryGetValue(key, out source)) return source; }
            source = TryGetThumbnail(path);
            if (source == null) source = TryReadIconFile(iconSourcePath);
            if (source == null && (string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetExtension(path), ".url", StringComparison.OrdinalIgnoreCase)))
                source = TryExtractIcon(iconSourcePath, location.Index);
            if (source == null) source = TryGetHighResolutionIcon(iconSourcePath);
            if (source == null) source = TryGetShellIcon(iconSourcePath);
            if (source == null && !string.Equals(path, iconSourcePath, StringComparison.OrdinalIgnoreCase))
                source = TryGetShellIcon(NormalizeShellPath(path));
            // A failed lookup is not permanent: game launchers may still be updating
            // their icon files at login. Retry on the next refresh.
            if (source != null)
                lock (cacheLock) { if (cache.Count > 800) cache.Clear(); cache[key] = source; }
            return source;
        }

        private static string NormalizeShellPath(string path) { return ShortcutResolver.NormalizeIconPath(path); }

        private static ImageSource TryGetShellIcon(string path)
        {
            ImageSource source = null;
            NativeMethods.SHFILEINFO info = new NativeMethods.SHFILEINFO();
            IntPtr result = NativeMethods.SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON);
            if (result != IntPtr.Zero && info.hIcon != IntPtr.Zero)
            {
                try
                {
                    BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(40, 40));
                    bitmap.Freeze();
                    source = bitmap;
                }
                finally { NativeMethods.DestroyIcon(info.hIcon); }
            }
            return source;
        }

        private static ImageSource TryReadIconFile(string path)
        {
            if (!string.Equals(Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase)) return null;
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    IconBitmapDecoder decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    BitmapFrame frame = decoder.Frames.OrderByDescending(item => Math.Min(item.PixelWidth, 128))
                        .ThenByDescending(item => item.Format.BitsPerPixel).FirstOrDefault();
                    if (frame != null) frame.Freeze();
                    return frame;
                }
            }
            catch { return null; }
        }

        private static ImageSource TryExtractIcon(string path, int index)
        {
            if (!File.Exists(path)) return null;
            IntPtr large = IntPtr.Zero, small = IntPtr.Zero;
            try
            {
                if (NativeMethods.SHDefExtractIcon(path, index, 0, out large, out small, 96 | (32u << 16)) != 0 || large == IntPtr.Zero)
                    return null;
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(large, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmap.Freeze();
                return bitmap;
            }
            catch { return null; }
            finally
            {
                if (large != IntPtr.Zero) NativeMethods.DestroyIcon(large);
                if (small != IntPtr.Zero) NativeMethods.DestroyIcon(small);
            }
        }

        private static ImageSource TryGetThumbnail(string path)
        {
            if (!File.Exists(path) || !ShouldUseThumbnail(path)) return null;
            object shellItem = null;
            IntPtr bitmapHandle = IntPtr.Zero;
            try
            {
                Guid iid = typeof(IShellItemImageFactory).GUID;
                int createResult = NativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out shellItem);
                if (createResult != 0 || shellItem == null) return null;
                IShellItemImageFactory factory = shellItem as IShellItemImageFactory;
                if (factory == null) return null;
                int imageResult = factory.GetImage(new NativeMethods.SIZE { cx = 96, cy = 96 },
                    ShellImageFlags.ThumbnailOnly | ShellImageFlags.BiggerSizeOk, out bitmapHandle);
                if (imageResult != 0 || bitmapHandle == IntPtr.Zero) return null;
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmap.Freeze();
                return bitmap;
            }
            catch { return null; }
            finally
            {
                if (bitmapHandle != IntPtr.Zero) NativeMethods.DeleteObject(bitmapHandle);
                if (shellItem != null && Marshal.IsComObject(shellItem)) Marshal.FinalReleaseComObject(shellItem);
            }
        }

        private static bool HasUsefulHighResolutionContent(BitmapSource bitmap)
        {
            if (bitmap == null || bitmap.PixelWidth <= 64 || bitmap.PixelHeight <= 64) return true;
            try
            {
                BitmapSource source = bitmap;
                if (source.Format != PixelFormats.Bgra32 && source.Format != PixelFormats.Pbgra32)
                {
                    FormatConvertedBitmap converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
                    converted.Freeze();
                    source = converted;
                }
                int stride = source.PixelWidth * 4;
                byte[] pixels = new byte[stride * source.PixelHeight];
                source.CopyPixels(pixels, stride, 0);
                int minX = source.PixelWidth, minY = source.PixelHeight, maxX = -1, maxY = -1;
                int edgeMarginX = Math.Max(3, source.PixelWidth / 32);
                int edgeMarginY = Math.Max(3, source.PixelHeight / 32);
                for (int y = edgeMarginY; y < source.PixelHeight - edgeMarginY; y++)
                {
                    int row = y * stride;
                    for (int x = edgeMarginX; x < source.PixelWidth - edgeMarginX; x++)
                    {
                        if (pixels[row + x * 4 + 3] < 12) continue;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
                if (maxX < minX || maxY < minY) return false;
                double widthRatio = (maxX - minX + 1) / (double)source.PixelWidth;
                double heightRatio = (maxY - minY + 1) / (double)source.PixelHeight;
                return Math.Max(widthRatio, heightRatio) >= 0.46;
            }
            catch { return false; }
        }

        private static ImageSource TryGetHighResolutionIcon(string path)
        {
            if (string.IsNullOrEmpty(path) || (!File.Exists(path) && !Directory.Exists(path))) return null;
            object shellItem = null;
            IntPtr bitmapHandle = IntPtr.Zero;
            try
            {
                Guid iid = typeof(IShellItemImageFactory).GUID;
                int createResult = NativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out shellItem);
                if (createResult != 0 || shellItem == null) return null;
                IShellItemImageFactory factory = shellItem as IShellItemImageFactory;
                if (factory == null) return null;
                int imageResult = factory.GetImage(new NativeMethods.SIZE { cx = 256, cy = 256 },
                    ShellImageFlags.IconOnly | ShellImageFlags.BiggerSizeOk, out bitmapHandle);
                if (imageResult != 0 || bitmapHandle == IntPtr.Zero) return null;
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                if (!HasUsefulHighResolutionContent(bitmap)) return null;
                bitmap.Freeze();
                return bitmap;
            }
            catch { return null; }
            finally
            {
                if (bitmapHandle != IntPtr.Zero) NativeMethods.DeleteObject(bitmapHandle);
                if (shellItem != null && Marshal.IsComObject(shellItem)) Marshal.FinalReleaseComObject(shellItem);
            }
        }

        private static bool ShouldUseThumbnail(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".avif", ".svg",
                ".mp4", ".mkv", ".mov", ".avi", ".webm", ".pdf", ".doc", ".docx", ".ppt", ".pptx" }.Contains(extension);
        }

        [Flags]
        private enum ShellImageFlags
        {
            BiggerSizeOk = 0x1,
            IconOnly = 0x4,
            ThumbnailOnly = 0x8
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B")]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(NativeMethods.SIZE size, ShellImageFlags flags, out IntPtr bitmapHandle);
        }
    }

    internal static class DesktopHost
    {
        public static IntPtr FindDesktopHost()
        {
            IntPtr result = IntPtr.Zero;
            NativeMethods.EnumWindows(delegate(IntPtr top, IntPtr data)
            {
                IntPtr view = NativeMethods.FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (view != IntPtr.Zero)
                {
                    result = top;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            if (result == IntPtr.Zero) result = NativeMethods.FindWindow("Progman", null);
            return result;
        }

        public static IntPtr Attach(Window window, double x, double y, double width, double height, out bool embedded, out int attachError)
        {
            embedded = false;
            attachError = 0;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            IntPtr host = FindDesktopHost();
            if (hwnd == IntPtr.Zero || host == IntPtr.Zero) return IntPtr.Zero;

            long originalStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE).ToInt64();
            long desktopStyle = (originalStyle & ~NativeMethods.WS_CHILD) | NativeMethods.WS_POPUP;
            long exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
            exStyle = (exStyle | NativeMethods.WS_EX_TOOLWINDOW) & ~NativeMethods.WS_EX_APPWINDOW;
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE, new IntPtr(desktopStyle));
            NativeMethods.SetLastError(0);
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWLP_HWNDPARENT, host);
            attachError = Marshal.GetLastWin32Error();
            Move(window, host, embedded, x, y, width, height);
            return host;
        }

        public static void Move(Window window, IntPtr host, bool embedded, double x, double y, double width, double height)
        {
            if (!window.IsLoaded) return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (NativeMethods.IsIconic(hwnd) || !NativeMethods.IsWindowVisible(hwnd))
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOWNOACTIVATE);
            if (!embedded || host == IntPtr.Zero)
            {
                window.Left = x;
                window.Top = y;
                window.Width = width;
                window.Height = height;
                IntPtr aboveDesktop = host == IntPtr.Zero ? IntPtr.Zero : NativeMethods.GetWindow(host, NativeMethods.GW_HWNDPREV);
                IntPtr insertAfter = IsDesktopActive(host, hwnd) || aboveDesktop == IntPtr.Zero ? NativeMethods.HWND_TOP : aboveDesktop;
                NativeMethods.SetWindowPos(hwnd, insertAfter, (int)Math.Round(x), (int)Math.Round(y),
                    Math.Max(1, (int)Math.Round(width)), Math.Max(1, (int)Math.Round(height)),
                    NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
                return;
            }
            NativeMethods.POINT point = new NativeMethods.POINT { X = (int)Math.Round(x), Y = (int)Math.Round(y) };
            NativeMethods.ScreenToClient(host, ref point);
            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOP, point.X, point.Y,
                Math.Max(1, (int)Math.Round(width)), Math.Max(1, (int)Math.Round(height)),
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        public static bool IsDesktopActive(IntPtr host, IntPtr fence)
        {
            IntPtr foreground = NativeMethods.GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;
            if (foreground == fence || foreground == host) return true;
            if (host != IntPtr.Zero && NativeMethods.GetAncestor(foreground, NativeMethods.GA_ROOT) == host) return true;
            StringBuilder className = new StringBuilder(128);
            NativeMethods.GetClassName(foreground, className, className.Capacity);
            string value = className.ToString();
            return string.Equals(value, "Progman", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "WorkerW", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "SHELLDLL_DefView", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class HotkeySink : IDisposable
    {
        private readonly HwndSource source;
        private readonly Action toggleAction;
        private readonly Action peekAction;
        private const int ToggleId = 0xDB01;
        private const int PeekId = 0xDB02;

        public HotkeySink(Action toggleCallback, Action peekCallback)
        {
            toggleAction = toggleCallback;
            peekAction = peekCallback;
            HwndSourceParameters parameters = new HwndSourceParameters("DeskBoundHotkeySink");
            parameters.Width = 0; parameters.Height = 0;
            parameters.WindowStyle = 0;
            source = new HwndSource(parameters);
            source.AddHook(WndProc);
            NativeMethods.RegisterHotKey(source.Handle, ToggleId, NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT, (uint)KeyInterop.VirtualKeyFromKey(Key.Space));
            NativeMethods.RegisterHotKey(source.Handle, PeekId, NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT, (uint)KeyInterop.VirtualKeyFromKey(Key.P));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == ToggleId)
            {
                toggleAction(); handled = true;
            }
            else if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == PeekId)
            {
                peekAction(); handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            NativeMethods.UnregisterHotKey(source.Handle, ToggleId);
            NativeMethods.UnregisterHotKey(source.Handle, PeekId);
            source.RemoveHook(WndProc);
            source.Dispose();
        }
    }

    internal static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int GWLP_HWNDPARENT = -8;
        public const long WS_CHILD = 0x40000000L;
        public const long WS_POPUP = unchecked((long)0x80000000L);
        public const long WS_EX_TOOLWINDOW = 0x00000080L;
        public const long WS_EX_APPWINDOW = 0x00040000L;
        public const long WS_EX_NOACTIVATE = 0x08000000L;
        public const uint GW_HWNDPREV = 3;
        public const uint GW_OWNER = 4;
        public const uint GA_ROOT = 2;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const int SW_SHOWNOACTIVATE = 4;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_SHOWWINDOW = 0x0018;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_SYSCOMMAND = 0x0112;
        public const long SC_MINIMIZE = 0xF020;
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHCNE_RENAMEITEM = 0x00000001;
        public const uint SHCNE_CREATE = 0x00000002;
        public const uint SHCNE_DELETE = 0x00000004;
        public const uint SHCNE_MKDIR = 0x00000008;
        public const uint SHCNE_RMDIR = 0x00000010;
        public const uint SHCNE_UPDATEDIR = 0x00001000;
        public const uint SHCNE_RENAMEFOLDER = 0x00020000;
        public const uint SHCNF_PATHW = 0x0005;
        public const uint SHCNF_FLUSH = 0x1000;
        public const uint SHCNF_FLUSHNOWAIT = 0x2000;
        public static readonly IntPtr HWND_TOP = IntPtr.Zero;
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE { public int cx; public int cy; }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("user32.dll", SetLastError = true)] public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string title);
        [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr SetParent(IntPtr child, IntPtr parent);
        [DllImport("kernel32.dll")] public static extern void SetLastError(uint error);
        [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr hwnd, uint command);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetClassName(IntPtr hwnd, StringBuilder className, int maxCount);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)] private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)] private static extern IntPtr GetWindowLongPtr32(IntPtr hwnd, int index);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)] private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr value);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)] private static extern IntPtr SetWindowLongPtr32(IntPtr hwnd, int index, IntPtr value);
        public static IntPtr GetWindowLongPtr(IntPtr hwnd, int index) { return IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, index) : GetWindowLongPtr32(hwnd, index); }
        public static IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value) { return IntPtr.Size == 8 ? SetWindowLongPtr64(hwnd, index, value) : SetWindowLongPtr32(hwnd, index, value); }
        [DllImport("user32.dll", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int command);
        [DllImport("user32.dll")] public static extern bool ScreenToClient(IntPtr hwnd, ref POINT point);
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT point);
        [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetProcessDPIAware();
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetProcessDpiAwarenessContext(IntPtr value);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SHGetFileInfo(string path, uint attrs, ref SHFILEINFO info, uint size, uint flags);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] public static extern void SHChangeNotify(uint eventId, uint flags, string item1, string item2);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHDefExtractIconW")]
        public static extern int SHDefExtractIcon(string path, int index, uint flags, out IntPtr large, out IntPtr small, uint size);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)] public static extern int SHCreateItemFromParsingName(string path, IntPtr bindContext, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object shellItem);
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr icon);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr handle);

        public static void EnableBestDpiMode()
        {
            try { if (SetProcessDpiAwarenessContext(new IntPtr(-4))) return; }
            catch (EntryPointNotFoundException) { }
            try { SetProcessDPIAware(); } catch { }
        }
    }
}
