# BakingSheet Data Verification
This document explains about data verification process of BakingSheet.

## Overriding Verification Method
You can override `VerifyAssets` to verify sanity of `Sheet`, `SheetRow` and `SheetRowElem`.

```csharp
public class StatSheet : Sheet<StatSheet.Row>
{
    public class Row : SheetRow
    {
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        
        public override void VerifyAssets(SheetConvertingContext context)
        {
            base.VerifyAssets(context);
            
            if (MinValue > MaxValue)
                context.Logger.LogError("MinValue must be less or equal than MaxValue!");
        }
    }
}
```

Then, you can call `SheetContainerBase.Verify` to execute your verification logic.
It is recommended to call `Verify` when you are done importing from raw sources like `Excel` or `Google Sheet`,
as this feature is to prevent accidentally putting wrong value.
```csharp
sheetContainer.Verify();
```

## Defining Custom Verifier
You can define custom verifiers to verify specific type of column.
To make custom verifier, inherit from `SheetVerifier<T>`.
For example, BakingSheet's Unity package contains `ResourcePathVerifier`.

```csharp
/// <summary>
/// Verifies if asset at resource path exists.
/// </summary>
public class ResourcePathVerifier : SheetVerifier<ResourcePath>
{
    public override string Verify(PropertyInfo propertyInfo, ResourcePath assetPath)
    {
        if (!assetPath.IsValid())
            return null;

        var obj = assetPath.Load<UnityEngine.Object>();
        if (obj != null)
            return null;

        return $"Resource {assetPath.FullPath} not found!";
    }
}
```

`ResourcePathVerifier` can verify any `ResourcePath` column. Additionally you can specify more metadata with attributes and query from `PropertyInfo`.
```csharp
public class PrefabSheet : Sheet<PrefabSheet.Row>
{
    public class Row : SheetRow
    {
        public ResourcePath Path { get; private set; }
    }
}
```

Then, you can call `SheetContainerBase.Verify` after loading your sheet.
```csharp
sheetContainer.Verify(new ResourcePathVerifier() /*, new OtherVerifier()... */);
```
