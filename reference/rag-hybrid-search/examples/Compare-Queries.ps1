$ErrorActionPreference = 'Stop'
$referencePath = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $referencePath
try {
    New-Item -ItemType Directory -Path artifacts -Force | Out-Null
    $cases = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'queries.json') -Raw | ConvertFrom-Json
    foreach ($case in $cases) {
        Write-Output ("Caso {0}: esperados {1}. {2}" -f $case.id, ($case.expectedDocuments -join ', '), $case.observation)
        foreach ($mode in @('vector', 'bm25', 'hybrid')) {
            $arguments = @('run', '--no-build', '--project', 'src/RagHybridSearch', '--', 'query', $case.query, '--mode', $mode, '--search-only')
            if ($case.department) { $arguments += @('--department', $case.department) }
            if ($case.type) { $arguments += @('--type', $case.type) }
            if ($case.year) { $arguments += @('--year', [string]$case.year) }
            # Argumentos separados: la pregunta se pasa como texto, nunca como un comando construido.
            & dotnet @arguments | Tee-Object -FilePath (Join-Path 'artifacts' ("{0}-{1}.txt" -f $case.id, $mode))
            if ($LASTEXITCODE -ne 0) { throw "Falló $($case.id) en modo $mode." }
        }
    }
}
finally { Pop-Location }
