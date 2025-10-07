# S2HD-XNA - content folderless branch

A branch that tries to make the game NOT use the content folder at all!

A w.i.p. port of Sonic 2 HD to XNA/Monogame

You can use an original sonicorca.dat file or the [unpacked version](github.com/2HDModding/sonicorca_unpacked)

# BUGS:

SFX don't play
Textures are kinda weird with stuff around them

# Building

## Desktop

```
dotnet build S2HD.Desktop/S2HD.Desktop.csproj
```

## Android

```
dotnet workload install android
dotnet build S2HD.Android/S2HD.Android.csproj
```