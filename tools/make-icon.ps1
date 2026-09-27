# Regenerates assets\FontDrop.ico (16-256 px, PNG-compressed entries).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$output = Join-Path $root 'assets\FontDrop.ico'
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null

function New-IconPng([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = [Math]::Max(1, $size * 0.04)
    $r = $size * 0.22
    $w = $size - 2 * $pad
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($pad, $pad, $r * 2, $r * 2, 180, 90)
    $path.AddArc($pad + $w - $r * 2, $pad, $r * 2, $r * 2, 270, 90)
    $path.AddArc($pad + $w - $r * 2, $pad + $w - $r * 2, $r * 2, $r * 2, 0, 90)
    $path.AddArc($pad, $pad + $w - $r * 2, $r * 2, $r * 2, 90, 90)
    $path.CloseFigure()
    $rect = New-Object System.Drawing.RectangleF $pad, $pad, $w, $w
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, ([System.Drawing.Color]::FromArgb(58, 123, 255)), ([System.Drawing.Color]::FromArgb(24, 64, 196)), 90
    $g.FillPath($brush, $path)

    $font = New-Object System.Drawing.Font 'Georgia', ($size * 0.42), ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = 'Center'
    $format.LineAlignment = 'Center'
    $textRect = New-Object System.Drawing.RectangleF 0, ($size * -0.08), $size, $size
    $g.DrawString('Aa', $font, [System.Drawing.Brushes]::White, $textRect, $format)

    # Drop arrow
    $cx = $size / 2
    $top = $size * 0.66
    $arrow = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($cx - $size * 0.16), $top),
        (New-Object System.Drawing.PointF ($cx + $size * 0.16), $top),
        (New-Object System.Drawing.PointF $cx, ($top + $size * 0.16)))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 214, 90))), $arrow)

    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    return ,$ms.ToArray()
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$images = @($sizes | ForEach-Object { ,(New-IconPng $_) })
$fs = [System.IO.File]::Create($output)
$bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]; $data = $images[$i]
    $bw.Write([byte]($s % 256)); $bw.Write([byte]($s % 256)); $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([UInt16]1); $bw.Write([UInt16]32)
    $bw.Write([UInt32]$data.Length); $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($data in $images) { $bw.Write($data) }
$bw.Dispose()
Write-Output $output
