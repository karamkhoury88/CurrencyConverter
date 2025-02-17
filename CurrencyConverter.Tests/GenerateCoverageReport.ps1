# Run tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"

# Find the coverage file
$coverageFile = Get-ChildItem -Path ./TestResults -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1

# Generate the report
if ($coverageFile) {
    reportgenerator -reports:$coverageFile.FullName -targetdir:coveragereport -reporttypes:Html
    Write-Host "Coverage report generated at: $PWD/coveragereport/index.html"
} else {
    Write-Host "Coverage file not found."
}