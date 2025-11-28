# S2HD-XNA
A w.i.p. port of Sonic 2 HD to XNA/Monogame


# Building

## Desktop

```
dotnet build S2HD.Desktop/S2HD.Desktop.csproj
```

## Android

```
dotnet workload install android
dotnet build S2HD.Android/S2HD.Android.csproj
cd S2HD.Android; dotnet publish -c Release -f net9.0-android35.0
```