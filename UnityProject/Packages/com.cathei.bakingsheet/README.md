[![Nuget](https://img.shields.io/nuget/v/BakingSheet)](https://www.nuget.org/packages?q=BakingSheet) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/cathei/BakingSheet)](https://github.com/cathei/BakingSheet/releases) [![openupm](https://img.shields.io/npm/v/com.cathei.bakingsheet?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.cathei.bakingsheet/) [![GitHub](https://img.shields.io/github/license/cathei/BakingSheet)](https://github.com/cathei/BakingSheet/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/wXjxjfrDQa)

# BakingSheet 🍞
Easy datasheet management for C# and Unity. Supports Excel, Google Sheet, JSON and CSV format. It has been used for several mobile games that released on Google Play and AppStore.

## BakingSheet 4.0 is here!
With BakingSheet 4.0, you can [export and import with Unity's ScriptableObject](docs/scriptable-object.md).
Also, major support for [AssetPath](docs/asset-path.md) and customizable [ValueConverter](docs/value-converter.md) has been added!
Note that .unitypackage will not be provided anymore. Please [add git package from Package manager, or install via OpenUPM](#install).

## Concept
Throughout all stage of game development, you'll need to deal with various data. Characters, stats, stages, currencies and so on! If you're using Unity, scriptable object and inspector is not good enough for mass edition and lacks powerful features like functions or fill up. With BakingSheet your designers can use existing spreadsheet editor, while you, the programmer, can directly use C# object without messy parsing logics or code generations.

Let's say your team is making a RPG game. Your game has 100 characters and 10 stats for each character. If your team use Unity's scriptable object, designers will have to spend lots of time adding and editing from Unity inspector. And after setup what if you need mass edit, like to double ATK stat of all characters? Will you go through all characters with Unity inspector, or make Editor script for every time mass edit is required? With BakingSheet, designers can work easily with spreadsheet functions and fill ups without programmer help!

![Concept](.github/images/concept.png)

BakingSheet's core concept is controlling datasheet schema from C# code, make things flexible while supporting multiple sources like Excel files or Google sheets. You can think it as datasheet version of [ORM](https://en.wikipedia.org/wiki/Object%E2%80%93relational_mapping). Also, you won't have to include source Excel files or parsing libraries for production builds. BakingSheet supports JSON serialization by default, you can ship your build with JSON or your custom format.

BakingSheet's basic workflow is like this:
1. Programmers make C# schema that represents Datasheet. (They can provide sample Excel files or Google Sheet with headers.)
2. Designers fill up the Datasheet, using any powerful functions and features of spreadsheet.
3. Edit-time script converts Datasheet to JSON (or any custom format) with your C# schema and validates data.
4. Runtime script reads from JSON (or any custom format) with your C# schema.
5. Your business logic directly uses C# instance of your schema.
6. Profit!

Don't trust me that it's better than using ScriptableObject? You might change your mind if you see how famous SuperCell ships their games with CSV, like [Clash Royale](https://github.com/smlbiobot/cr-csv/tree/master/assets/csv_logic) or [Brawl Stars](https://github.com/weeco/brawlstars-assets/tree/master/7.278.1/csv_logic). Though of course, their games aren't made with Unity, still a very good example to show how you can utilize spreadsheet!

![Sample1](.github/images/sample_simple.jpg)
![Sample2](.github/images/sample_complex.jpg)

## Features
* Easy-to-use Datasheet management.
* Define schema as C# classes - no messy metadata on your datasheet.
* Supports importing from Excel, Google sheet, CSV and JSON.
* Supports exporting to CSV and JSON.
* Supports .NET platforms and all Unity platforms.
* Powerful Cross-sheet reference feature.
* Referencing Asset data with [AssetPath](docs/asset-path.md)
* [Customizable value converter](docs/value-converter.md)
* [Customizable data verification](docs/data-verification.md)

## Install
For C# projects or server, download with [NuGet](https://www.nuget.org/packages?q=BakingSheet).

For Unity projects, add git package from Package Manager.
```
https://github.com/cathei/BakingSheet.git?path=UnityProject/Packages/com.cathei.bakingsheet#v4.0.0
```

Or install it via [OpenUPM](https://openupm.com/packages/com.cathei.bakingsheet/).
```
openupm add com.cathei.bakingsheet
```

### Need help?
Before you start, we want to mention that if you have problem or need help, you can always ask directly on [Discord Channel](https://discord.gg/wXjxjfrDQa)!

## Contribution
We appreciate any contribution. Please create [issue](https://github.com/cathei/BakingSheet/issues) for bugs or feature requests. Any contribution to feature, test case, or documentation through [pull requests](https://github.com/cathei/BakingSheet/pulls) are welcome! Any blog posts, articles, shares about this project will be greatful!

## First Step
BakingSheet manages datasheet schema as C# code. `Sheet` class represents a table and `SheetRow` class represents a record. Below is example content of file `Items.xlsx`. Also, any column starts with `$` will be considered as comment and ignored.

![Plain Sample](.github/images/sample_plain.png)

<details>
<summary>Markdown version</summary>

| Id             | Name              | Price | $Comment   |
|----------------|-------------------|-------|------------|
| ITEM_LVUP001   | Warrior's Shield  | 10000 | Warrior Lv up material |
| ITEM_LVUP002   | Mage's Staff      | 10000 | Mage Lv up material |
| ITEM_LVUP003   | Assassin's Dagger | 10000 | Assassin Lv up material |
| ITEM_POTION001 | Health Potion     | 30    | Heal 20 Hp |
| ITEM_POTION002 | Mana Potion       | 50    | Heal 20 Mp |
</details>

Code below is corresponding BakingSheet class.
```csharp
public class ItemSheet : Sheet<ItemSheet.Row>
{
    public class Row : SheetRow
    {
        // use name of matching column
        public string Name { get; private set; }
        public int Price { get; private set; }
    }
}
```
You can see there are two classes, `ItemSheet` and `ItemSheet.Row`. Each represents a page of sheet and a single row. `ItemSheet` is surrounding `Row` class (It is not forced but recommended convention). Important part is they will inherit from `Sheet<TRow>` and `SheetRow`.

`Id` column is mandatory, so it is already defined in base `SheetRow` class. `Id` is `string` by default, but you can change type. See [this section](#using-non-string-column-as-id) to use non-string type for `Id`.

To represent collection of sheets, a document, let's create `SheetContainer` class inherits from `SheetContainerBase`.
```csharp
public class SheetContainer : SheetContainerBase
{
    public SheetContainer(Microsoft.Extensions.Logging.ILogger logger) : base(logger) {}

    // property name matches with corresponding sheet name
    // for .xlsx or google sheet, it is name of the sheet tab in the workbook
    // for .csv or .json, it is name of the file
    public ItemSheet Items { get; private set; }

    // add other sheets as you extend your project
    public CharacterSheet Characters { get; private set; }
}
```
You can add as many sheets you want as properties of your `SheetContainer`. This class is designed to be "fat", means single `SheetContainer` should contain all your sheets unless there is specific reason to partition your sheets. For example when you want to deploy some `Sheet` only exclusive to server program, you might want to partition `ServerSheetContainer` and `ClientSheetContainer`.

## Supported Column Type
* `string`
* Numeric primitive types (`int`, `long`, `float`, `double`, and so on)
* `bool` ("TRUE" or "FALSE")
* Custom `enum` types
* `DateTime` and `TimeSpan`
* Cross-sheet reference (`Sheet<>.Reference`)
* Nullable for any other supported value type (for example `int?`)
* `List<>` and `Dictionary<,>`
* Custom `struct` and `class` as [nested column](#using-nested-type-column)
* Custom type converted with [ValueConverter](docs/value-converter.md)

> **Note**  
> When using `JsonConverter`, `enum` is serialized as `string` by default so you won't have issue when reordering them.

## Converters
Converters are simple implementation import/export records from datasheet sources. These come as separated library, as it's user's decision to select datasheet source.
User can have converting process, to convert datasheet to other format ahead of time and not include heavy converters in production applications.

BakingSheet supports four basic converters. They're included in Unity package as well.

| Package Name                                                                                   | Format                       | Supports Import | Supports Export |
|------------------------------------------------------------------------------------------------|------------------------------|-----------------|-----------------|
| [BakingSheet.Converters.Excel](https://www.nuget.org/packages/BakingSheet.Converters.Excel/)   | Microsoft Excel              | O               | X               |
| [BakingSheet.Converters.Google](https://www.nuget.org/packages/BakingSheet.Converters.Google/) | Google Sheet                 | O               | X               |
| [BakingSheet.Converters.Csv](https://www.nuget.org/packages/BakingSheet.Converters.Csv/)       | Comma-Separated Values (CSV) | O               | O               |
| [BakingSheet.Converters.Json](https://www.nuget.org/packages/BakingSheet.Converters.Json/)     | JSON                         | O               | O               |
| [ScriptableObject Converter](docs/scriptable-object.md) (Unity only)                           | ScriptableObject             | O               | O (Read-only)   |

Below code shows how to convert `.xlsx` files from `Excel/Files/Path` directory.
```csharp
// any ILogger will work, there is built-in UnityLogger
var logger = new UnityLogger();

// pass logger to receive logs
var sheetContainer = new SheetContainer(logger);

// create excel converter from path
var excelConverter = new ExcelSheetConverter("Excel/Files/Path");

// bake sheets from excel converter
await sheetContainer.Bake(excelConverter);
```

For Google Sheet, first create your service account through Google API Console. Then add it to your sheet with `Viewer` permission. Use Google credential for that service account to create converter. For detailed information about how to create service account and link to your sheet, see [How to import from Google Sheet](./docs/google-sheet-import.md).

```csharp
// replace with your Google sheet identifier
// https://developers.google.com/sheets/api/guides/concepts
string googleSheetId = "1iWMZVI4FgtGbig4EgPIun_BRbzp4ulqRIzINZQl-AFI";

// service account credential than can read the sheet you're converting
// this starts with { "type": "service_account", "project_id": ...
string googleCredential = File.ReadAllText("Some/Path/Credential.json");

var googleConverter = new GoogleSheetConverter(googleSheetId, googleCredential);

// bake sheets from google converter
await sheetContainer.Bake(googleConverter);
```

## Save and Load Converted Datasheet
Below code shows how to load sheet from Excel and save as JSON. This typically happens through Unity Editor script or any pre-build time script.

```csharp
// create excel converter from path
var excelConverter = new ExcelSheetConverter("Excel/Files/Path");

// create json converter from path
var jsonConverter = new JsonSheetConverter("Json/Files/Path");

// convert from excel
await sheetContainer.Bake(excelConverter);

// save as json
await sheetContainer.Store(jsonConverter);
```

Then, for runtime you can load your data from JSON.

```csharp
// create json converter from path
var jsonConverter = new JsonSheetConverter("Json/Files/Path");

// load from json
await sheetContainer.Bake(jsonConverter);
```

You can extend `JsonSheetConverter` to customize serialization process. For example encrypting data or prettifying JSON.

> **Note**  
> For AOT platforms (iOS, Android), read about [AOT Code Stripping](#about-aot-code-stripping-unity).

> **Note**  
> If you are using `StreamingAssets` on Android, also see [Reading From StreamingAssets](docs/streaming-assets.md).

## Accessing Row
Now you have `SheetContainer` loaded from your data, accessing to the row is fairly simple. Below code shows how to access specific `ItemSheet.Row`.
```csharp
// same as sheetContainer.Items.Find("ITEM_LVUP003");
// returns null if no row found
var row = sheetContainer.Items["ITEM_LVUP003"];

// print "Assassin's dagger"
logger.LogInformation(row.Name);
```

`Sheet<T>` is `KeyedCollection`, you can loop through it and order is guaranteed to be as same as your spreadsheet. Plus of course you can use all benefits of `IEnumerable<T>`.
```csharp
// loop through all rows and print their names
foreach (var row in sheetContainer.Items)
    logger.LogInformation(row.Name);

// loop through item ids that price over 5000
foreach (var itemId in sheetContainer.Items.Where(row => row.Price > 5000).Select(row => row.Id))
    logger.LogInformation(itemId);
```

## Using List Column
List columns are used for simple array.

![List Sample](.github/images/sample_list.png)

<details>
<summary>Flat header</summary>

| Id         | Name          | Monsters:1 | Monsters:2 | Monsters:3 | Items:1        | Items:2      |
| ---------- | ------------- | ---------- | ---------- | ---------- | -------------- | ------------ |
| DUNGEON001 | Easy Field    | MONSTER001 |            |            | ITEM_POTION001 | ITEM_LVUP001 |
| DUNGEON002 | Expert Zone   | MONSTER001 | MONSTER002 |            | ITEM_POTION002 | ITEM_LVUP002 |
| DUNGEON003 | Dragon’s Nest | MONSTER003 | MONSTER004 | MONSTER005 | ITEM_LVUP003   |              |
</details>

<details>
<summary>Split header</summary>

| Id         | Name          | Monsters   |            |            | Items          |              |
|------------|---------------|------------|------------|------------|----------------|--------------|
|            |               | 1          | 2          | 3          | 1              | 2            |
| DUNGEON001 | Easy Field    | MONSTER001 |            |            | ITEM_POTION001 | ITEM_LVUP001 |
| DUNGEON002 | Expert Zone   | MONSTER001 | MONSTER002 |            | ITEM_POTION002 | ITEM_LVUP002 |
| DUNGEON003 | Dragon’s Nest | MONSTER003 | MONSTER004 | MONSTER005 | ITEM_LVUP003   |              |
</details>

```csharp
public class DungeonSheet : Sheet<DungeonSheet.Row>
{
    public class Row : SheetRow
    {
        public string Name { get; private set; }

        // you can use any supported type as list
        // to know more about sheet reference types, see cross-sheet reference section
        public List<MonsterSheet.Reference> Monsters { get; private set; }
        public List<ItemSheet.Reference> Items { get; private set; }
    }
}
```
Use it as simple as just including a column has type implmenting `IList<T>`. Since spreadsheet is designer's area, index on sheet is 1-based. So be aware when you access it from code.

Also you can pick between flat-header style(`Monsters:1`) and split-header style(`Monsters`, `1`) as the example shows. There is no problem to mix-and-match or nest them.

## Using Dictionary Column
Dictionary columns are used when key-based access of value is needed.

![Dictionary Sample](.github/images/sample_dict.png)

<details>
<summary>Flat header</summary>

| Id     | Name          | Texts:Greeting    | Texts:Purchasing | Texts:Leaving     |
| ------ | ------------- | ----------------- | ---------------- | ----------------- |
| NPC001 | Fat Baker     | Morning traveler! | Thank you!       | Come again!       |
| NPC002 | Blacksmith    | G'day!            | Good choice.     | Take care.        |
| NPC003 | Potion Master | What do you want? | Take it already. | Don't come again. |
</details>

<details>
<summary>Split header</summary>

| Id     | Name          | Texts             |                  |                   |
|--------|---------------|-------------------|------------------|-------------------|
|        |               | Greeting          | Purchasing       | Leaving           |
| NPC001 | Fat Baker     | Morning traveler! | Thank you!       | Come again!       |
| NPC002 | Blacksmith    | G'day!            | Good choice.     | Take care.        |
| NPC003 | Potion Master | What do you want? | Take it already. | Don't come again. |
</details>

```csharp
public enum Situation
{
    Greeting,
    Purchasing,
    Leaving
}

public class NpcSheet : Sheet<NpcSheet.Row>
{
    public class Row : SheetRow
    {
        public string Name { get; private set; }

        public Dictionary<Situation, string> Texts { get; private set; }
    }
}
```
Use it as simple as just including a column has type implmenting `IDictionary<TKey, TValue>`.

## Using Nested Type Column
Nested type columns are used for complex structure.

![Nested Type Sample](.github/images/sample_dict.png)

<details>
<summary>Flat header</summary>

| Id     | Name          | Texts:Greeting    | Texts:Purchasing | Texts:Leaving     |
| ------ | ------------- | ----------------- | ---------------- | ----------------- |
| NPC001 | Fat Baker     | Morning traveler! | Thank you!       | Come again!       |
| NPC002 | Blacksmith    | G'day!            | Good choice.     | Take care.        |
| NPC003 | Potion Master | What do you want? | Take it already. | Don't come again. |
</details>

<details>
<summary>Split header</summary>

| Id     | Name          | Texts             |                  |                   |
|--------|---------------|-------------------|------------------|-------------------|
|        |               | Greeting          | Purchasing       | Leaving           |
| NPC001 | Fat Baker     | Morning traveler! | Thank you!       | Come again!       |
| NPC002 | Blacksmith    | G'day!            | Good choice.     | Take care.        |
| NPC003 | Potion Master | What do you want? | Take it already. | Don't come again. |
</details>

```csharp
public struct SituationText
{
    public string Greeting { get; private set; }
    public string Purchasing { get; private set; }
    public string Leaving { get; private set; }
}

public class NpcSheet : Sheet<NpcSheet.Row>
{
    public class Row : SheetRow
    {
        public string Name { get; private set; }

        public SituationText Texts { get; private set; }
    }
}
```
As you see, content of the datasheet is just same as when using Dictionary column. The data type of column determines how BakingSheet reads the column.

## Using Row Array
Row arrays are used for 2-dimentional structure. Below is example content of file `Heroes.xlsx`.

![Row Array Sample](.github/images/sample_rowarray.png)

<details>
<summary>Markdown version</summary>

| Id      | Name     | Strength | Inteligence | Vitality | StatMultiplier | RequiredExp | RequiredItem |
|---------|----------|----------|-------------|----------|----------------|-------------|--------------|
| HERO001 | Warrior  | 100      | 80          | 140      | 1              | 0           |              |
|         |          |          |             |          | 1.2            | 10          |              |
|         |          |          |             |          | 1.4            | 20          |              |
|         |          |          |             |          | 1.6            | 40          |              |
|         |          |          |             |          | 2              | 100         | ITEM_LVUP001 |
| HERO002 | Mage     | 60       | 160         | 80       | 1              | 0           |              |
|         |          |          |             |          | 1.2            | 10          |              |
|         |          |          |             |          | 1.4            | 20          |              |
|         |          |          |             |          | 1.6            | 40          |              |
|         |          |          |             |          | 2              | 100         | ITEM_LVUP002 |
| HERO003 | Assassin | 140      | 100         | 80       | 1              | 0           |              |
|         |          |          |             |          | 1.2            | 10          |              |
|         |          |          |             |          | 1.4            | 20          |              |
|         |          |          |             |          | 1.6            | 40          |              |
|         |          |          |             |          | 2              | 100         | ITEM_LVUP003 |
</details>

Rows without `Id` is considered as part of previous row. You can merge the non-array cells to make it visually intuitive. Below corresponding code shows how to define row arrays.

```csharp
public class HeroSheet : Sheet<HeroSheet.Row>
{
    public class Row : SheetRowArray<Elem>
    {
        public string Name { get; private set; }

        public int Strength { get; private set; }
        public int Inteligence { get; private set; }
        public int Vitality { get; private set; }

        public Elem GetLevel(int level)
        {
            // Level 1 would be index 0
            return this[level - 1];
        }

        // Max level would be count of elements
        public int MaxLevel => Count;
    }

    public class Elem : SheetRowElem
    {
        public float StatMultiplier { get; private set; }
        public int RequiredExp { get; private set; }
        public string RequiredItem { get; private set; }
    }
}
```
`SheetRowArray<TElem>` implements `IEnumerable<TElem>`, indexer `this[int]` and `Count` property.

> **Note**  
> It is worth mention you can use `VerticalList<T>` to cover the case you want to vertically extend your `List<T>` without pairing them as `Elem`. Though we recommend to split the sheet in that case if possible.

## Using Cross-Sheet Reference
Below code shows how to replace `string RequiredItem` to `ItemSheet.Reference RequiredItem` to add extra reliablity. `Sheet<TKey, TRow>.Reference` type is serialized as `TKey`, and verifies that row with same id exists in the sheet.

```csharp
public class HeroSheet : Sheet<HeroSheet.Row>
{
    public class Row : SheetRowArray<Elem>
    {
        // ...
    }

    public class Elem : SheetRowElem
    {
        public float StatMultiplier { get; private set; }
        public int RequiredExp { get; private set; }
        public ItemSheet.Reference RequiredItem { get; private set; }
    }
}
```

```csharp
public class SheetContainer : SheetContainerBase
{
    // ...

    // use name of each matching sheet name from source
    public HeroSheet Heroes { get; private set; }
    public ItemSheet Items { get; private set; }
}
```
Both `ItemSheet` and `HeroSheet` must be the properties on same `SheetContainer` class to reference each other's row.

Now, not only error message will pop up when `RequiredItem` doesn't exist in `SheetContainer.Items`, you can access `ItemSheet.Row` directly through it.

```csharp
var heroRow = sheetContainer.Heroes["HERO001"];

// ITEM_LVUP001 from Items sheet
var itemRow = heroRow.GetLevel(5).RequiredItem.Ref;

// print "Warrior's Shield"
logger.LogInformation(itemRow.Name);
```

## Using Non-String Column as Id
Any type can be used value can be also used as `Id`. This is possible as passing type argument to generic class `SheetRow<TKey>` and `Sheet<TKey, TRow>`. Below is example content of file `Contstants.xlsx`.

![Const Sample](.github/images/sample_const.png)

<details>
<summary>Markdown version</summary>

| Id             | Value                                 |
|----------------|---------------------------------------|
| ServerAddress  | https://github.com/cathei/BakingSheet |
| InitialGold    | 1000                                  |
| CriticalChance | 0.1                                   |
</details>

Below code shows how to use enumeration type as Id.
```csharp
public enum GameConstant
{
    ServerAddress,
    InitialGold,
    CriticalChance,
}

public class ConstantSheet : Sheet<GameConstant, ConstantSheet.Row>
{
    public class Row : SheetRow<GameConstant>
    {
        public string Value { get; private set; }
    }
}
```

## Using Post Load Hook
You can override `PostLoad` method of `Sheet`, `SheetRow` or `SheetRowElem` to execute post load process.

Below code shows how to convert loaded sheet value dynamically.
```csharp
public class ConstantSheet : Sheet<GameConstant, ConstantSheet.Row>
{
    public class Row : SheetRow<GameConstant>
    {
        public string Value { get; private set; }

        private int valueInt;
        public int ValueInt => valueInt;

        private float valueFloat;
        public float ValueFloat => valueFloat;

        public override void PostLoad(SheetConvertingContext context)
        {
            base.PostLoad(context);

            int.TryParse(Value, out valueInt);
            float.TryParse(Value, out valueFloat);
        }
    }

    public string GetString(GameConstant key)
    {
        return Find(key).Value;
    }

    public int GetInt(GameConstant key)
    {
        return Find(key).ValueInt;
    }

    public float GetFloat(GameConstant key)
    {
        return Find(key).ValueFloat;
    }
}
```

> **Note**  
> Properties without setter are not serialized. Alternatively you can use `[NonSerialized]` attribute.

## Using AssetPostProcessor to Automate Converting
For Excel and CSV, you could set up `AssetPostProcessor` to automate converting process. Recommended practice is keeping both source `.xlsx` and `.csv` files alongside with destination `.json` files in your version control system. For Google Sheet, it is instead recommended to use custom `MenuItem` to convert into destination `.json` files that keeped in your version control.

The below is example source code that triggers when any `.xlsx` is changed, convert Excel sheet under `Assets/Excel` into `.json` under `Assets/StreamingAssets/Json`. You can customize this logic with your desired source and destination folder.
```csharp
public class ExcelPostprocessor : AssetPostprocessor
{
    static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // automatically run postprocessor if any excel file is imported
        string excelAsset = importedAssets.FirstOrDefault(x => x.EndsWith(".xlsx"));

        if (excelAsset != null)
        {
            // excel path as "Assets/Excel"
            var excelPath = Path.Combine(Application.dataPath, "Excel");

            // json path as "Assets/StreamingAssets/Json"
            var jsonPath = Path.Combine(Application.streamingAssetsPath, "Json");

            var logger = new UnityLogger();
            var sheetContainer = new SheetContainer(logger);

            // create excel converter from path
            var excelConverter = new ExcelSheetConverter(excelPath);

            // bake sheets from excel converter
            await sheetContainer.Bake(excelConverter);

            // create json converter to path
            var jsonConverter = new JsonSheetConverter(jsonPath);

            // save datasheet to streaming assets
            await sheetContainer.Store(jsonConverter);

            AssetDatabase.Refresh();

            Debug.Log("Excel sheet converted.");
        }
    }
}
```

## About AOT Code Stripping (Unity)
If you are working on AOT (IL2CPP) environment, you would have option called `Managed Stripping Level` in your Player Settings. Since BakingSheet uses reflection, if you set stripping level `Medium` or `High`, the stripper might remove the code piece that required. Especially some property setters.

You can prevent this by either using `Low` stripping level, or adding own `link.xml` to preserve your sheet classes. The below is simplest example of `link.xml`. If you want to know more about it, see [Unity's Documentation](https://docs.unity3d.com/Manual/ManagedCodeStripping.html#LinkXMLAnnotation).
```xml
<?xml version="1.0" encoding="utf-8" ?>
<linker>
  <!--
    Replace `MyCompany.MyGame.Sheet` to your assembly to prevent Unity code stripping
  -->
  <assembly fullname="MyCompany.MyGame.Sheet" preserve="all"/>
</linker>
```

## Optional Script Defining Symbols (Unity)
There is few optional symbols that can be defined for runtime usage. By default only JSON and ScriptableObject converters will be included to the build.

| Symbol                              | Effect                                                                                                                                                   |
|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| BAKINGSHEET_RUNTIME_GOOGLECONVERTER | Include Google Converter to your build.<br/>See also: [Google Sheet Converter](docs/google-sheet-import.md#how-to-use-google-sheet-converter-on-runtime) |
| BAKINGSHEET_RUNTIME_CSVCONVERTER    | Include CSV Converter to your build.                                                                                                                     |
