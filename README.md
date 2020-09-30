# BakingSheet
Easy datasheet management for C# and Unity

## Install
Download from NuGet or download unitypackage

## Concept
![Concept](Images/concept.png)

BakingSheet's core concept is controlling datasheet schema from C# code, make things flexible while supporting multiple sources like .xlsx or Google sheets.
Also, it helps to avoid having source datasheet files or parsing libraries for production applications. BakingSheet supports JSON serialization by default.

## Importers

Importers are simple implementation extracts RawSheet from datasheet sources. These come as separated library, as it's user's decision to select datasheet source.
User can have converting process, to convert datasheet to serialized files ahead of time and not include importers in production applications.

BakingSheet supports two basic importers
* `BakingSheet.Importers.Excel`
* `BakingSheet.Importers.Google`

User can create and customize their own importer by implementing `ISheetImporter`.
