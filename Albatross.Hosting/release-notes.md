# 9.0.0
Align version with dotnet sdk.
Reference dotnet 9 libraries
# 8.0.5
Add standard jwt token support
# 8.0.3
* Create a property GlbobalExceptionHandler and ways to customize it
* Add the functionality to treat unhandled `ArgumentException` as a 400 error
* The logging of the 'ArgumentException' can also be disabled by setting the `Setup.SupressUnhandledArgumentExceptionLogging` flag to true
* Add lots of documentation
# 8.0.0
The last change was a breaking change.  Bump the major version number.
# 7.6.2
* Replace reference `Albatross.Serialization` with `Albatross.Serialization.Json`
* Update `Microsoft.AspNetCore.*` library to version 8.0.13
# 7.6.0
* update `Albatross.Authentication` to 7.6.0, it has a breaking change.
* Add `versions` endpoint in `AppInfoController`
