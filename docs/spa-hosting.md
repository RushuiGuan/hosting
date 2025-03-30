# SPA (Single Page Application) Hosting
Albatross.Hosting can host SPAs such as angular projects within itself.  To enable SPA Hosting, following the steps below.

# Features
1. baseHref support - SPA projects can have different baseHref path based on deployment location.  `Albatross.Hosting` will adjust the baseHref value using configuration so that it can be adjusted automatically by environment.
1. config file support - `Albatross.Hosting` supports transformation of config file by environments similar to transformation of `appsettings.json` file in aspnetcore.

# Quick Start
1. override the `Startup.Spa` property to return true
2. Copy the output of the SPA project to the `wwwroot` folder.
2. create the following entries in the `appsettings.json` config file
	```json
	{
		"angular": {
			"baseHrefFile": [ "wwwroot", "index.html" ],
			"configFile": [
				"wwwroot",
				"assets",
				"config.json"
			],
			"baseHref": "/"
		}
	}
	```
4. `angular.baseHrefFile` and `angular.baseHref` property
	* the `baseHrefFile` property is the file path of the entry point file for SPA in array format.
	* the `baseHref` property is the base url of the deployed SPA application.  This could be diff based on the environments.
5. `angular.configFile` property
	* This property specify the json config file of the SPA application.
	* The SPA application can have a config file for different environments similar to the `appsettings.json` file in aspnetcore.
		1. config.json
		1. config.staging.json
		1. config.production.json
	* Upon start up, the system will apply the configuration of the current environment into the root `config.json` file.