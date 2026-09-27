$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$output = Join-Path $root 'FontDrop.exe'
$manifest = Join-Path $root 'FontDrop.manifest'
$sources = Get-ChildItem -LiteralPath $root -Filter *.cs -File | Select-Object -ExpandProperty FullName
$references = @('/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll', '/reference:System.Windows.Forms.dll')
& $csc /nologo /target:winexe /platform:anycpu /optimize+ /out:$output "/win32manifest:$manifest" @references $sources
if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $LASTEXITCODE" }
Write-Output $output
