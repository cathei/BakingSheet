# Reading from StreamingAssets
To use StreamingAssets folder of Unity.

## Installing BetterStreamingAssets
StreamingAssets is compressed `jar` file in Android builds, so default `FileSystem` cannot properly access to it.

BakingSheet includes `StreamingAssetsFileSystem` to support this. It is required to install [BetterStreamingAssets](https://github.com/gwiazdorrr/BetterStreamingAssets) package to use.

To install `BetterStreamingAssets`, install git package via `Add package from git url...`.
```
https://github.com/cathei/BetterStreamingAssets-Package.git
```

You can also use [OpenUPM](https://openupm.com/packages/com.cathei.betterstreamingassets/).
```
openupm add com.cathei.betterstreamingassets
```

Or install from original [BetterStreamingAssets](https://github.com/gwiazdorrr/BetterStreamingAssets) repository.

## Reading from StreamingAssets
Below is example of using `StreamingAssetsFileSystem` with `JsonSheetConverter` at runtime. Keep in mind that path is relative from `Assets/StreamingAssets`.
```csharp
// create json converter from path
var jsonConverter = new JsonSheetConverter("Relative/Json/Path", new StreamingAssetsFileSystem());

// load from json
await sheetContainer.Bake(jsonConverter);
```
