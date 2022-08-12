[![Nuget](https://img.shields.io/nuget/v/BakingSheet)](https://www.nuget.org/packages?q=BakingSheet) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/cathei/BakingSheet)](https://github.com/cathei/BakingSheet/releases) [![GitHub](https://img.shields.io/github/license/cathei/BakingSheet)](https://github.com/cathei/BakingSheet/blob/master/LICENSE) [![Discord](https://img.shields.io/discord/942240862354702376?color=%235865F2&label=discord&logo=discord&logoColor=%23FFFFFF)](https://discord.gg/wXjxjfrDQa)

# BakingSheet
Easy datasheet management for C# and Unity. Supports Excel, Google Sheet, JSON and CSV format. It has been used for several mobile games that released on Google Play and AppStore.

## Install
Download with [NuGet](https://www.nuget.org/packages?q=BakingSheet) or download [.unitypackage release](https://github.com/cathei/BakingSheet/releases)

### Need help?
Before you start, I want to mention that if you have problem or need help, you can always ask me on [Discord Channel](https://discord.gg/wXjxjfrDQa)!

## Concept
![Concept](.github/images/concept.png)

BakingSheet's core concept is controlling datasheet schema from C# code, make things flexible while supporting multiple sources like .xlsx or Google sheets.
Also, it helps to avoid having source datasheet files or parsing libraries for production applications. BakingSheet supports JSON serialization by default.

![Sample1](.github/images/sample_simple.jpg)
![Sample2](.github/images/sample_complex.jpg)

## First Step
BakingSheet manages datasheet schema as C# code. `Sheet` class represents a table and `SheetRow` class represents a record. Below is example content of file `Items.xlsx`. Also, any column starts with $ will be considered as comment and ignored.

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
Note that `Id` column is already defined in base `SheetRow` class.

To represent collection of sheets, implement `SheetContainerBase` class.
```csharp
public class SheetContainer : SheetContainerBase
{
    public SheetContainer(Microsoft.Extensions.Logging.ILogger logger) : base(logger) {}

    // use name of each matching sheet name from source
    public ItemSheet Items { get; private set; }
}
```

## Converters
Converters are simple implementation import/export records from datasheet sources. These come as separated library, as it's user's decision to select datasheet source.
User can have converting process, to convert datasheet to other format ahead of time and not include heavy converters in production applications.

BakingSheet supports four basic converters. They're included in .unitypackage as well.
| Package Name                  | Format                       | Supports Import | Supports Export |
|-------------------------------|------------------------------|-----------------|-----------------|
| [BakingSheet.Converters.Excel](https://www.nuget.org/packages/BakingSheet.Converters.Excel/)  | Microsoft Excel              | O               | X               |
| [BakingSheet.Converters.Google](https://www.nuget.org/packages/BakingSheet.Converters.Google/) | Google Sheet                 | O               | X               |
| [BakingSheet.Converters.Csv](https://www.nuget.org/packages/BakingSheet.Converters.Csv/)    | Comma-Separated Values (CSV) | O               | O               |
| [BakingSheet.Converters.Json](https://www.nuget.org/packages/BakingSheet.Converters.Json/)   | JSON                         | O               | O               |

Below code shows how to convert .xlsx files from `Excel/Files/Path` directory.
```csharp
// pass logger to receive logs
var sheetContainer = new SheetContainer(logger);

// create excel converter from path
var excelConverter = new ExcelSheetConverter("Excel/Files/Path");

// bake sheets from excel converter
await sheetContainer.Bake(excelConverter);
```

## Save and Load Converted Datasheet
Below code shows how to save and load converted json.

```csharp
// create json converter from path
var jsonConverter = new JsonSheetConverter("Saved/Files/Path");

// save as json
await sheetContainer.Store(jsonConverter);

// later, load from json
await sheetContainer.Bake(jsonConverter);
```
You can extend `JsonSheetConverter` to customize serialization process.

## Accessing Row
Below code shows how to access specific `ItemSheet.Row`.
```csharp
var row = sheetContainer.Items["ITEM_LVUP003"];

// Assassin's dagger
logger.LogInformation(row.Name);

// loop through all rows
foreach (var value in sheetContainer.Items.Values)
    logger.LogInformation(value.Name);
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
Note that properties without setter are not serialized. Alternatively you can use `[NonSerialized]` attribute.

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
Note that the content of datasheet is just same as when using Dictionary column. The data type of column determines how BakingSheet reads the column.

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
            return this[level - 1];
        }
    }

    public class Elem : SheetRowElem
    {
        public float StatMultiplier { get; private set; }
        public int RequiredExp { get; private set; }
        public string RequiredItem { get; private set; }
    }
}
```
Note that `SheetRowArray<TElem>` is implementing `IEnumerable<TElem>` and indexer.

It is worth mention you can use `VerticalList<T>` to cover the case you need multiple list of different length in one column. Though I recommend to split the sheet in that case if possible.

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
Note that both `ItemSheet` and `HeroSheet` have to be one of the properties on same `SheetContainer` class.

## Custom Converters
User can create and customize their own converter by implementing `ISheetImporter` and `ISheetExporter`.

## Custom Verifiers
You can verify datasheet sanity with custom verifiers. For example, you can define `ResourceAttribute` to mark columns that should reference path inside of Unity's Resources folder.
