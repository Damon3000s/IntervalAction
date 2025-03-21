# ktsu.IntervalAction

**IntervalAction** is a .NET library that provides a simple way to execute an action at a specified interval.

[![License](https://img.shields.io/github/license/ktsu-dev/IntervalAction.svg?label=&logo=nuget)](LICENSE.md)

[![NuGet Version](https://img.shields.io/nuget/v/ktsu.IntervalAction?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.IntervalAction)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.IntervalAction?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.IntervalAction)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.IntervalAction?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.IntervalAction)

[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/IntervalAction?label=&logo=github)](https://github.com/ktsu-dev/IntervalAction/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/IntervalAction?label=&logo=github)](https://github.com/ktsu-dev/IntervalAction/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/IntervalAction/dotnet.yml?label=&logo=github)](https://github.com/ktsu-dev/IntervalAction/actions)

## Supported Targets

- .NET 8.0 or .NET 9.0

## Installation

To install IntervalAction, you can use the .NET CLI:

```bash
dotnet add package ktsu.IntervalAction
```

Or by adding the package reference to your project file:

```xml
<PackageReference Include="ktsu.IntervalAction" Version="X.X.X" />
```

Or you can use the NuGet Package Manager in Visual Studio to search for and install the `ktsu.IntervalAction` package.

## Usage

To use IntervalAction, use the `IntervalAction.Start()` static method with the options you want and keep a reference to the returned `IntervalAction` object. You can then call `Stop()` to stop the action from executing and `Restart()` to start it again.

```csharp
using ktsu.IntervalAction;

var intervalAction = IntervalAction.Start(new()
{
    Action = () => Console.WriteLine("Hello, World!"),
    ActionInterval = TimeSpan.FromSeconds(1),
    PollingInterval = TimeSpan.FromMilliseconds(100), // Optional, default is 1 second
    IntervalType = IntervalType.FromLastStart // Optional, default is FromLastCompletion
});

// an action will execute immediately and then every second
// outputting "Hello, World!" to the console until stopped

intervalAction.Stop();

// the action will no longer execute until you call Restart()

intervalAction.Restart();
```

Here's an additional usage example that demonstrates using IntervalType.FromLastCompletion:

```csharp
using ktsu.IntervalAction;

var intervalAction = IntervalAction.Start(new()
{
    Action = () =>
    {
        Console.WriteLine($"Action started at: {DateTimeOffset.Now}");
        // Simulate a task that takes some time to complete
        Task.Delay(500).Wait();
        Console.WriteLine($"Action completed at: {DateTimeOffset.Now}");
    },
    ActionInterval = TimeSpan.FromSeconds(2),
    PollingInterval = TimeSpan.FromMilliseconds(100),
    IntervalType = IntervalType.FromLastCompletion // Waits until the action completes before starting the interval timer
});

// The action will execute immediately and then every 2 seconds after the previous execution completes
```

## Options

- `Action` - The action to execute at the specified interval.
- `ActionInterval` - The interval at which to execute the action.
- `PollingInterval` - The interval at which to check if the action should be executed. (Optional, default is 1 second)
- `IntervalType` - The type of interval to use. (Optional, default is FromLastCompletion)

## Testing

The project contains comprehensive unit tests for the IntervalAction library.

To run the tests for IntervalAction, you can use the .NET CLI:

```bash
dotnet test
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## Versioning

Check the [CHANGELOG.md](CHANGELOG.md) for detailed release notes and version changes.

## Acknowledgements

Special thanks to all contributors and the .NET community for their support.
