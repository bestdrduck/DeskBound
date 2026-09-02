$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent $PSScriptRoot
$assetDirectory = Join-Path $projectRoot "assets"
$mainPng = Join-Path $assetDirectory "DeskBound-logo.png"
$iconPath = Join-Path $assetDirectory "DeskBound.ico"

function New-RoundedPath {
    param(
        [float]$X,
        [float]$Y,
        [float]$Width,
        [float]$Height,
        [float]$Radius
    )

    $diameter = $Radius * 2
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    $path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    $path.AddArc($X + $Width - $diameter, $Y + $Height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-LogoBitmap {
    param([int]$Size)

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bitmap.SetResolution(96, 96)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    $scale = $Size / 512.0
    $outer = New-RoundedPath (28 * $scale) (28 * $scale) (456 * $scale) (456 * $scale) (112 * $scale)
    $gradient = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(66 * $scale, 54 * $scale),
        [System.Drawing.PointF]::new(448 * $scale, 464 * $scale),
        ([System.Drawing.Color]::FromArgb(255, 38, 203, 224)),
        ([System.Drawing.Color]::FromArgb(255, 90, 76, 238)))
    $graphics.FillPath($gradient, $outer)

    $inner = New-RoundedPath (90 * $scale) (92 * $scale) (332 * $scale) (328 * $scale) (66 * $scale)
    $innerBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(238, 12, 25, 48))
    $graphics.FillPath($innerBrush, $inner)

    $railPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(245, 247, 251, 255), (24 * $scale))
    $railPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $railPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($railPen, 138 * $scale, 170 * $scale, 374 * $scale, 170 * $scale)

    $postColors = @(
        [System.Drawing.Color]::FromArgb(255, 63, 226, 237),
        [System.Drawing.Color]::FromArgb(248, 247, 251, 255),
        [System.Drawing.Color]::FromArgb(255, 164, 150, 255)
    )
    $postXs = @(146, 238, 330)
    $postHeights = @(154, 198, 136)
    for ($index = 0; $index -lt 3; $index++) {
        $height = $postHeights[$index]
        $post = New-RoundedPath ($postXs[$index] * $scale) (196 * $scale) (36 * $scale) ($height * $scale) (18 * $scale)
        $postBrush = [System.Drawing.SolidBrush]::new($postColors[$index])
        $graphics.FillPath($postBrush, $post)
        $postBrush.Dispose()
        $post.Dispose()
    }

    $graphics.Dispose()
    $gradient.Dispose()
    $innerBrush.Dispose()
    $railPen.Dispose()
    $inner.Dispose()
    $outer.Dispose()
    return $bitmap
}

$primary = New-LogoBitmap 512
$primary.Save($mainPng, [System.Drawing.Imaging.ImageFormat]::Png)
$primary.Dispose()

$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$images = New-Object System.Collections.Generic.List[byte[]]
foreach ($size in $sizes) {
    $bitmap = New-LogoBitmap $size
    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $images.Add($stream.ToArray())
    $stream.Dispose()
    $bitmap.Dispose()
}

$file = [System.IO.File]::Create($iconPath)
$writer = [System.IO.BinaryWriter]::new($file)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizes.Count)
$offset = 6 + (16 * $sizes.Count)
for ($index = 0; $index -lt $sizes.Count; $index++) {
    $size = $sizes[$index]
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]$images[$index].Length)
    $writer.Write([uint32]$offset)
    $offset += $images[$index].Length
}
foreach ($image in $images) {
    $writer.Write($image)
}
$writer.Dispose()
$file.Dispose()

Write-Host "Generated $mainPng"
Write-Host "Generated $iconPath"
