using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using GSheet = Google.Apis.Sheets.v4.Data.Sheet;

namespace Cathei.BakingSheet
{
    public class GoogleSheetImporter : ISheetImporter
    {
        private string _gsheetAddress;
        private ICredential _credential;
        private Spreadsheet _spreadsheet;

        public GoogleSheetImporter(string gsheetAddress, string credential)
        {
            _gsheetAddress = gsheetAddress;
            _credential = GoogleCredential.
                FromJson(credential).
                CreateScoped(new[] { DriveService.Scope.DriveReadonly });
        }

        public async Task<DateTime> FetchModifiedTime()
        {
            using (var service = new DriveService(new BaseClientService.Initializer() {
                HttpClientInitializer = _credential
            }))
            {
                var fileReq = service.Files.Get(_gsheetAddress);
                fileReq.SupportsTeamDrives = true;
                fileReq.Fields = "modifiedTime";

                var file = await fileReq.ExecuteAsync();
                return file.ModifiedTime.Value;
            }
        }

        public async Task<bool> Load()
        {
            using (var service = new SheetsService(new BaseClientService.Initializer() {
                HttpClientInitializer = _credential
            }))
            {
                var sheetReq = service.Spreadsheets.Get(_gsheetAddress);
                sheetReq.Fields = "properties,sheets(properties,data.rowData.values.formattedValue)";
                _spreadsheet = await sheetReq.ExecuteAsync();
                return true;
            }
        }

        private class Data : ISheetImporterData
        {
            private GridData _grid;

            public Data(GSheet gsheet)
            {
                _grid = gsheet.Data.First();
            }

            public string GetCell(int row, int col)
            {
                if (row >= _grid.RowData.Count ||
                    col >= _grid.RowData[row].Values?.Count)
                    return null;

                var value = _grid.RowData[row].Values?[col];
                return value?.FormattedValue;
            }
        }

        public ISheetImporterData GetData(string sheetName)
        {
            var gsheet = _spreadsheet.Sheets.FirstOrDefault(x => x.Properties.Title == sheetName);
            if (gsheet == null)
                return null;

            return new Data(gsheet);
        }
    }
}
