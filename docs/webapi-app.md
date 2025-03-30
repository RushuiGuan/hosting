# New WebApi Application
* Please find the sample project [here](../Sample.WebApi/).
* Create a new console application that targets net8.0
* Open the project file and change the Project Sdk to `Microsoft.NET.Sdk.Web`
* Add a reference to the Albatross.Hosting assembly
* Create an `appsettings.json`, `hostsettings.json` and `serilog.json` file in the project root
* Set all three files to `Copy to Output Directory` = `Copy if newer`
* Edit the `hostsettings.json` file and add the following code
	```json
	{
		"urls": "http://localhost:5000"
	}
	```
* Create a start up file `MyStartup.cs` that extends `Albatross.Hosting.Startup` class
	* The startup is used to register dependencies, initializaiton and config.
	* Reference the base class [Startup](../Albatross.Hosting/Startup.cs) to see the options.
* Update the Main method of the`Program.cs` file with code below.
	```csharp
	public static Task Main(string[] args) {
		return new Setup(args)
			.ConfigureWebHost<MyStartup>()
			.RunAsync();
	}
	```
* These steps will create a new webapp that listens on `http://localhost:5000`