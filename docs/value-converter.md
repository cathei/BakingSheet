# BakingSheet Value Converter
BakingSheet supports `ValueConverter` to support custom data type.

Note that `ValueConverter` will be only used when you importing or exporting with cell-based converters (Excel, Google Sheet, CSV).

You can extend `JsonSheetConverter` instead, if you want to specify converting logic and configuration when you converting to JSON.

## Defining Sheet Value Converter
To support custom data type, user can implement own `SheetValueConverter` for the type.

For example, this is built-in `DateTime` converter.
```csharp
public class DateTimeValueConverter : SheetValueConverter<DateTime>
{
    protected override DateTime StringToValue(Type type, string value, SheetValueConvertingContext context)
    {
        var local = DateTime.Parse(value, context.FormatProvider);
        return TimeZoneInfo.ConvertTimeToUtc(local, context.TimeZoneInfo);
    }

    protected override string ValueToString(Type type, DateTime value, SheetValueConvertingContext context)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(value, context.TimeZoneInfo);
        return local.ToString(context.FormatProvider);
    }
}
```

## Specifying Converter with Attribute
You can specify your custom `SheetValueConverter` with `SheetValueConverterAttribute`.

You can specify on specific Type,
```csharp
[SheetValueConverter(typeof(MyTypeConverter)]
public class MyType
{
    // ...
}
```

Or specify on specific column.
```csharp
public class MySheet : Sheet<MySheet.Row>
{
    public class Row : SheetRow
    {
        [SheetValueConverter(typeof(MyColumnConverter))]
        public int MyColumn { get; set; }
    }
}
```

## Specifying Converter with ContractResolver
And you can specify your custom `ValueConverter` by overriding `SheetContainerBase.ContractResolver`.

```csharp
public class MySheetContainer : SheetContainerBase
{
    // sheet definitions
    public ItemSheet Items { get; set; }
    public CharacterSheet Characters { get; set; }
    
    // constructor
    public MySheetContainer(ILogger logger) : base(logger) { }
    
    // overrides
    public readonly static SheetContractResolver ContractResolverInstace = new SheetContractResolver(new MyValueConverter());
    
    public override SheetContractResolver ContractResolver => ContractResolverInstace;
}
```
It is advised to reuse `SheetContractResolver` instance, since it will contain reflection information for value converters.
