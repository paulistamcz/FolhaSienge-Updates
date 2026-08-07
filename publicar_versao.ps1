# ============================================================
# Publica uma nova versão do FolhaSienge no GitHub Releases.
# Uso:  .\publicar_versao.ps1 1.0.1  (ou apenas .\publicar_versao.ps1)
# Sem argumento, usa a versão atual do FolhaSienge.csproj.
# ============================================================
param([string]$Versao = "")

$ErrorActionPreference = "Stop"
$proj = "C:\Users\Eduardo\Desktop\13 - PROJETO - RELATORIOS DOMINIO\FolhaSienge"
$temp = "C:\Users\Eduardo\AppData\Local\Temp\opencode\folhasienge-updates"

if ([string]::IsNullOrWhiteSpace($Versao)) {
    $csproj = Get-Content "$proj\FolhaSienge.csproj" -Raw
    if ($csproj -match "<Version>([^<]+)</Version>") { $Versao = $Matches[1] }
}
if ([string]::IsNullOrWhiteSpace($Versao)) { throw "Não consegui detectar a versão. Passe como argumento." }
Write-Host "==> Versão: $Versao"

Write-Host "==> Publicando build..."
dotnet publish "$proj\FolhaSienge.csproj" -c Release -r win-x64 --self-contained false -o "$proj\publicado" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Falha no publish." }

Write-Host "==> Compactando pacote..."
$stage = Join-Path $temp $Versao
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null
Copy-Item "$proj\publicado\*" $stage -Recurse -Force
$zip = Join-Path $temp "FolhaSienge_$Versao.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path "$stage\*" -DestinationPath $zip -Force

Write-Host "==> Criando Release v$Versao no GitHub..."
gh release create "v$Versao" $zip --repo paulistamcz/FolhaSienge-Updates `
    --title "Versão $Versao" --notes "Atualização do FolhaSienge para a versão $Versao."

Write-Host "==> Publicando código-fonte (branch main)..."
Push-Location $proj
try {
    git add -A
    git commit -m "release: v$Versao" --allow-empty
    git push origin main 2>&1 | Out-Null
} finally { Pop-Location }

Write-Host "==> Concluído! Release em https://github.com/paulistamcz/FolhaSienge-Updates/releases/tag/v$Versao"
