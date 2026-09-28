# Draws docs\images\social-preview.png (1280x640), the image GitHub shows when the repository is shared:
# the icon, the name and the English screenshot on a plain background.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$output = Join-Path $root 'docs\images\social-preview.png'
$icon = [System.Drawing.Image]::FromFile((Join-Path $root 'docs\images\icon.png'))
$shot = [System.Drawing.Image]::FromFile((Join-Path $root 'docs\images\screenshot-en.png'))

$W = 1280; $H = 640
$bmp = New-Object System.Drawing.Bitmap $W, $H
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.TextRenderingHint = 'AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::FromArgb(243, 243, 243))

# Left: icon, name and one line about what it does.
$g.DrawImage($icon, 72, 170, 96, 96)
$g.DrawString('FontDrop', (New-Object System.Drawing.Font 'Segoe UI Semibold', 60, ([System.Drawing.GraphicsUnit]::Pixel)), (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(32, 32, 32))), 64, 280)
$text = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(96, 96, 96))
$g.DrawString("Batch font installer`nfor Windows", (New-Object System.Drawing.Font 'Segoe UI', 28, ([System.Drawing.GraphicsUnit]::Pixel)), $text, (New-Object System.Drawing.RectangleF 70, 370, 330, 100))

# Right: the app as it is.
$scale = 0.95
$sw = [int]($shot.Width * $scale); $sh = [int]($shot.Height * $scale)
$sx = $W - 56 - $sw; $sy = [int](($H - $sh) / 2)
$g.DrawImage($shot, $sx, $sy, $sw, $sh)
$g.DrawRectangle((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(200, 200, 200)), 1), $sx - 1, $sy - 1, $sw + 1, $sh + 1)

$g.Dispose(); $icon.Dispose(); $shot.Dispose()
$bmp.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output $output
