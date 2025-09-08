$branch = "dev"

if (Test-Path $PSScriptRoot/dev-scripts -PathType Container) {
	Get-Item $PSScriptRoot/dev-scripts | Remove-Item -Recurse -Force
}

Invoke-WebRequest `
	-Uri https://github.com/RushuiGuan/dev-scripts/archive/refs/heads/$branch.zip `
	-OutFile $PSScriptRoot/dev-scripts.zip

Expand-Archive -Path $PSScriptRoot/dev-scripts.zip -DestinationPath $PSScriptRoot -Force
Remove-Item -Path $PSScriptRoot/dev-scripts.zip

Move-Item -Path $PSScriptRoot/dev-scripts-$branch -Destination $PSScriptRoot/dev-scripts
Import-Module $PSScriptRoot/dev-scripts/dev-scripts.psm1