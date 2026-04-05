# Http Request Logging

`Albatross.Hosting` does not include request logging by default. Use `Serilog.AspNetCore`'s `UseSerilogRequestLogging()` middleware to opt in.

Override `Configure` in your `Startup` class and call `UseSerilogRequestLogging()` before `base.Configure(...)`:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    public override void Configure(IApplicationBuilder app, ProgramSetting programSetting, EnvironmentSetting environmentSetting, ILogger<MyStartup> logger) {
        app.UseSerilogRequestLogging();
        base.Configure(app, programSetting, environmentSetting, logger);
    }
}
```

Calling it before `base.Configure(...)` ensures it wraps the full request pipeline. Serilog writes one structured log entry per request, including the HTTP method, path, status code, and elapsed time.
