# BakingSheet AssetPath
BakingSheet support variants of AssetPath type, so you can easily specify a reference to an asset.

## ResourcePath
`ResourcePath` is path that indicating asset under Unity's `Resources` folder.
The path is relative to `Resources` folder, and extension must not be used.
Sub-assets can be referenced with `[]`, as `My/Asset/Path[SubAssetName]`.

## DirectAssetPath
`DirectAssetPath` is path that indicating any asset under Unity's `Assets` folder.
The result of `Get<T>` is only valid when using ScriptableObject reference or importer.
See [Converting with ScriptableObject](scriptable-object.md) for details.

## AddressablePath
`AddressablePath` is path that indicating address of Addressable Assets.
`LoadAsync<T>` or `Get<T>` is only accessible when Addressable Assets package is installed.
