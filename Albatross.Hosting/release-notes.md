# 8.0.2
* Add the functionality to treat unhandled `ArgumentException` as a 400 error.  The feature is enabled by default.  Set the `TreatArgumentExceptionAsBadRequest` property of the `Startup` base class to false to disable.  The logging of the 'ArgumentException' can also be disabled by setting the `Setup.SupressUnhandledArgumentExceptionLogging` flag to true
# 8.0.0
The last change was a breaking change.  Bump the major version number.
# 7.6.2
* Replace reference `Albatross.Serialization` with `Albatross.Serialization.Json`
* Update `Microsoft.AspNetCore.*` library to version 8.0.13
# 7.6.0
* update `Albatross.Authentication` to 7.6.0, it has a breaking change.
* Add `versions` endpoint in `AppInfoController`
