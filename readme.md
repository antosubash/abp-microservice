# ABP Microservice Template

[![NuGet Badge](https://buildstats.info/nuget/Anto.Abp.Microservice.Template)](https://www.nuget.org/packages/Anto.Abp.Microservice.Template)

## Why this template?

This is a template for creating a microservice with ABP using dotnet. This template is created to make it easier to create a new microservice project with ABP. It is based on the blog post, [.NET Microservice with ABP](https://blog.antosubash.com/posts/abp-microservice-series) series I made earlier. It is created using the [dotnet new template engine](https://docs.microsoft.com/en-us/dotnet/core/tools/custom-templates) and it is available on [NuGet](https://www.nuget.org/packages/Anto.Abp.Microservice.Template/).

## Usage

### Install the template

```bash
dotnet new install Anto.Abp.Microservice.Template
```

### Create a new project

```bash
dotnet new abp-ms -n MyProjectName
```

### Quick Start (Development)

After cloning or creating a project from this template:

```bash
# First-time setup
make install          # Restore tools and install git hooks
make build            # Build the solution

# Run the application
make run              # Run with .NET Aspire (includes all services)

# Or use individual commands
dotnet tool restore
dotnet husky install
cd src && dotnet build Tasky.sln
cd src/apps/Tasky.AppHost && dotnet run
```

**Available Makefile commands:**
- `make help` - Show all available targets
- `make dev-help` - Show quick start guide
- `make build` - Build the solution
- `make test` - Run tests
- `make format` - Format code
- `make fix` - Auto-fix analyzer issues + format
- `make run` - Run with Aspire orchestration

See the [Makefile](Makefile) for all available commands.

## Documentation

You can find everything in this repo explained [here](https://blog.antosubash.com/posts/abp-microservice-series). There is also a [YouTube Video](https://www.youtube.com/watch?v=PFFNHQUn74A) for this project.

## Features

- [x] Docker support
- [x] API Gateway with YARP
- [x] Frontend with Angular
- [x] Swagger UI
- [x] Authentication and Authorization
- [x] ABP 7.0 support
- [x] Blazor UI
- [ ] Any suggestions? Create an issue or a pull request.

## License

This project is licensed under the [MIT License](LICENSE).

## Contributing

If you have any suggestions or improvements, please feel free to create an issue or a pull request.

## Acknowledgments

- [ABP Framework](https://abp.io/)
- [eShopOnAbp](https://github.com/abpframework/eShopOnAbp)

## Author

Anto Subash - [antosubash.com](https://antosubash.com)

## Donate

If you like this project, please consider supporting me on [PayPal](https://www.paypal.me/antosubash). Thank you!

## Disclaimer

This project is not affiliated with the ABP Framework project or the ABP Framework team. It is created by a community member. I am not responsible for any issues that may arise from using this template. Use at your own risk.

## Changelog

You can find the changelog [here](CHANGELOG.md).

## TODO

- [ ] Create a template for creating a new microservice.
- [ ] Getting Started guide.
- [ ] Add a new service guide.
- [ ] Any suggestions? Create an issue or a pull request.

## Resources

- If you are new to ABP, you can find the documentation [here](https://docs.abp.io/en/abp/latest/).
- If you are new to microservices, you can find the documentation [here](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/).
- If you are new to Docker, you can find the documentation [here](https://docs.docker.com/).
- If you are new to YARP, you can find the documentation [here](https://microsoft.github.io/reverse-proxy/articles/getting-started.html).
- If you are new to Angular, you can find the documentation [here](https://angular.io/docs).
- If you are new to Swagger, you can find the documentation [here](https://swagger.io/docs/).
- If you are new to Openiddict, you can find the documentation [here](https://documentation.openiddict.com/guides/getting-started.html/).
