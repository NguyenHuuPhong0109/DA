$ErrorActionPreference = "Stop"

$outDir = Join-Path $PSScriptRoot "..\DACN_HTQL\wwwroot\images\specialties"
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
}

$icons = @(
    @{ File = "noi-tong-quat.svg";      Id = "stethoscope";        Cat = "devices" },
    @{ File = "tai-mui-hong.svg";       Id = "ear-nose-throat";     Cat = "symbols" },
    @{ File = "da-lieu.svg";            Id = "dermatology";         Cat = "specialties" },
    @{ File = "nhi-khoa.svg";           Id = "pediatrics";          Cat = "specialties" },
    @{ File = "tim-mach.svg";           Id = "cardiology";          Cat = "specialties" },
    @{ File = "co-xuong-khop.svg";      Id = "orthopaedics";        Cat = "specialties" },
    @{ File = "tieu-hoa-gan-mat.svg";   Id = "gastroenterology";    Cat = "specialties" },
    @{ File = "san-phu-khoa.svg";       Id = "gynecology";          Cat = "specialties" },
    @{ File = "mat.svg";                Id = "eye";                 Cat = "body" },
    @{ File = "rang-ham-mat.svg";       Id = "tooth";               Cat = "body" },
    @{ File = "mac-dinh.svg";           Id = "hospital";            Cat = "places" }
)

function Extract-InnerSvg($svgContent) {
    if ($svgContent -match '(?s)<svg[^>]*>(.*)</svg>') {
        return $matches[1].Trim()
    }
    return $svgContent
}

Write-Host "Starting duotone SVG generation for $($icons.Count) icons..."

foreach ($ic in $icons) {
    $outlineUrl = "https://raw.githubusercontent.com/resolvetosavelives/healthicons/main/public/icons/svg/outline/$($ic.Cat)/$($ic.Id).svg"
    $filledUrl  = "https://raw.githubusercontent.com/resolvetosavelives/healthicons/main/public/icons/svg/filled/$($ic.Cat)/$($ic.Id).svg"

    Write-Host "Processing $($ic.File) (Id: $($ic.Id), Cat: $($ic.Cat))..."
    $outlineSvg = (Invoke-WebRequest -Uri $outlineUrl -UseBasicParsing -TimeoutSec 15).Content
    $filledSvg  = (Invoke-WebRequest -Uri $filledUrl -UseBasicParsing -TimeoutSec 15).Content

    $outlineInner = Extract-InnerSvg $outlineSvg
    $filledInner  = Extract-InnerSvg $filledSvg

    # Thay the fill="currentColor" hoac dam bao path ke thua mau tu the <g>
    $filledInnerClean = $filledInner -replace 'fill="currentColor"', 'fill="#BBF7D0"' -replace 'fill="#[0-9a-fA-F]{3,6}"', 'fill="#BBF7D0"'
    $outlineInnerClean = $outlineInner -replace 'fill="currentColor"', 'fill="#15803D"' -replace 'fill="#[0-9a-fA-F]{3,6}"', 'fill="#15803D"'

    $duotoneSvg = @"
<svg width="48" height="48" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
  <g id="layer-filled" fill="#BBF7D0" opacity="0.75" transform="translate(1.2, 1.2) scale(0.95)">
$filledInnerClean
  </g>
  <g id="layer-outline" fill="#15803D">
$outlineInnerClean
  </g>
</svg>
"@

    $destPath = Join-Path $outDir $ic.File
    [System.IO.File]::WriteAllText($destPath, $duotoneSvg, [System.Text.Encoding]::UTF8)
    $fi = Get-Item $destPath
    Write-Host "  -> Created $($ic.File) ($($fi.Length) bytes)"
}

Write-Host "`nVerifying all generated SVG files:"
$allSvg = Get-ChildItem -Path $outDir -Filter "*.svg"
foreach ($f in $allSvg) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $xmlValid = $false
    try {
        [xml]$x = $content
        $xmlValid = $true
    } catch {
        $xmlValid = $false
    }
    Write-Host "  - $($f.Name): $($f.Length) bytes | XML Valid: $xmlValid"
}
