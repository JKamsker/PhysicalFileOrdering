# Contributing

Install the .NET 10 SDK and run the repository checks before opening a pull
request:

```bash
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Keep native calls behind an operating-system guard, preserve deterministic
fallback behavior, and add tests for parser or ordering changes. Tests that
depend on a specific filesystem should skip or make conservative assertions on
other hosts.
