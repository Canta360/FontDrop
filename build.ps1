$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw "C# compiler not found: $csc" }
$bin = Join-Path $root 'bin'
New-Item -ItemType Directory -Force $bin | Out-Null
$output = Join-Path $bin 'FontDrop.exe'
$manifest = Join-Path $root 'FontDrop.manifest'
$icon = Join-Path $root 'assets\FontDrop.ico'
$sources = Get-ChildItem -LiteralPath $root -Filter *.cs -File | Select-Object -ExpandProperty FullName
$references = @('/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll', '/reference:System.Windows.Forms.dll')
& $csc /nologo /target:winexe /platform:anycpu /optimize+ /out:$output "/win32manifest:$manifest" "/win32icon:$icon" @references $sources
if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $LASTEXITCODE" }

$test = Start-Process -FilePath $output -ArgumentList '--self-test' -Wait -PassThru -NoNewWindow
if ($test.ExitCode -ne 0) { throw "Self-test failed: $($test.ExitCode)" }
Write-Output $output
