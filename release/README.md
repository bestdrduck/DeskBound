# GitHub Releases 發布準備

目前專案使用 Inno Setup 6 產生正式安裝檔。編譯 `installer\DeskBound.iss` 會在 `outputs` 產生 `DeskBound-Setup-版本.exe`。

GitHub Releases 建議每次上傳：

- `DeskBound-Setup-版本.exe`：唯一對外發布的安裝檔，可選安裝位置並沿用先前位置升級
- 發行說明：列出新功能、修正與是否需要重新啟動

主程式仍會在本機產生 `outputs\桌伴.exe` 供安裝腳本打包，但不另外上傳到 GitHub Releases。程式內更新會下載正式安裝檔；使用者設定與圍欄資料保存在安裝目錄之外，升級時不會被搬動或清除。
