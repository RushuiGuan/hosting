if ($IsWindows) {
	set-alias -name daemon -value $env:InstallDirectory\Sample.Daemon\Sample.Daemon.exe
	set-alias -name api -value $env:InstallDirectory\Sample.WebApi\Sample.WebApi.exe
} else {
	set-alias -name daemon -value $env:InstallDirectory/Sample.Daemon/Sample.Daemon
	set-alias -name api -value $env:InstallDirectory/Sample.WebApi/Sample.WebApi
}