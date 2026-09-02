# GitHub Releases 發布準備

目前專案已具備單檔程式與 Inno Setup 安裝腳本。安裝 Inno Setup 6 後，編譯 `installer\DeskBound.iss` 會在 `outputs` 產生 `桌伴-Setup-0.12.0.exe`。

GitHub Releases 建議每次上傳：

- `桌伴-Setup-版本.exe`：一般使用者安裝或更新
- `桌伴.exe`：免安裝版
- 發行說明：列出新功能、修正與是否需要重新啟動

程式內自動檢查更新需要確定 GitHub 儲存庫網址後才能安全綁定；在網址尚未確定前，不會放入無效或冒充的更新來源。
