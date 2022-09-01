using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Raw;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using GSheet = Google.Apis.Sheets.v4.Data.Sheet;

namespace Cathei.BakingSheet
{
    public class GoogleSheetConverter : RawSheetImporter
    {
        private string _gsheetAddress;
        private ICredential _credential;
        private Spreadsheet _spreadsheet;

        public GoogleSheetConverter(string gsheetAddress, string credential, TimeZoneInfo timeZoneInfo = null, IFormatProvider formatProvider = null)
            : base(timeZoneInfo, formatProvider)
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

        protected override async Task<bool> LoadData()
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

        private class Page : IRawSheetImporterPage
        {
            private GridData _grid;

            public Page(GSheet gsheet)
            {
                _grid = gsheet.Data.First();
            }

            public string GetCell(int col, int row)
            {
                if (row >= _grid.RowData.Count ||
                    col >= _grid.RowData[row].Values?.Count)
                    return null;

                var value = _grid.RowData[row].Values?[col];
                return value?.FormattedValue;
            }
        }

        protected override IRawSheetImporterPage GetPage(string sheetName)
        {
            var gsheet = _spreadsheet.Sheets.FirstOrDefault(x => x.Properties.Title == sheetName);
            if (gsheet == null)
                return null;

            return new Page(gsheet);
        }
    }
}
