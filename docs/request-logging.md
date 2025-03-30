# Http Request Logging
By default `Albatross.Hosting` will log all requests with a source context of `usage`.  The content of the log is a property string that captures the properties of the incoming http requests. The property string has the format of `user_name` `remote-ip` `url` `method`
```
2025-03-30 08:50:09-04:00 [inf] Usage user_name 192.168.12.25 http://localhost:15000/api/app-info GET
```
To disable this feature, override the `Startup.LogUsage` property and set it to false.