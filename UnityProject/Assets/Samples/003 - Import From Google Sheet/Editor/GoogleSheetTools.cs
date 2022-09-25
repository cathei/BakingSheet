using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Examples
{
    public static class GoogleSheetTools
    {
        // unit test google account credential
        private static readonly string GoogleCredential = @"{
  ""type"": ""service_account"",
  ""project_id"": ""bakingsheet"",
  ""private_key_id"": ""ac015398cb66da5dc4164c86fe6757739a8c2742"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCuLeCNJEJN5dRz\nvNyMLyMlGu81iqCnSfpYAgv6BccdxBrvK3nmBtvQO15YI4+sl4NAYl16XCv+FxNl\nN+NJa76EmwV7ikyNKxNhUmVnoMiv7I7JxGVdh57ZueJiISyLsOqP2xU/sUqx6ETL\nfywCKq4CYcsEZtFW8aLgVmOsnD+CUIlEYZ1sZPq557DkIZfOQ0qoUToZRzc69RNj\nl+JGuvzEPSHV74j9inruj/BovRIz7x1R9fhOmWHhiMA9bt2+bgNPZd3r7Wit29iW\nX4d11H2JjDVCczHMOvgRyn7vlSUgos95IVIHNDTB86r6MDFcBKHAKvhSxq3CL9CI\noW/yPs6DAgMBAAECggEAE5p2O2xpSfsj/iKzb9OeUP7HDW+cxTc+Rcl1L56W2nLH\n1UM/ZPRAqR8Fb0YRVYt36jcn7i2Rhfdy52Fy40CNIL+RzmWyV/ERLVLottERxTsK\nlwrTncM1zatuHz08ljxDKg2uRozdq0hI9d5KRbQoAXygLM6loPNxMW6HqVWskC3w\nb1//sRCKQ4UoFLQ9rg7DAwelPdtaXwFKJieYTMmQsxl0qq5tMd71v0BJ7jB7B8au\nTEqAi8PCWPyWxBT404n98R7N0k9txlQ0iLnmwSGPPor4YCckU1ESYd/lQhoiGd69\n0SrrpSjbAJvxYneNJvypF7Id7ZNEklqDC/MgeBYhgQKBgQDsXUWti2vo4U8PMZ1z\n7qVXWflVtPlWuLif77mf5d7vIhPD454CgQAawoQVNn9e1zJYOOFtg1HHD0sxsXDF\nRYZ8gtiNxj80a2QHBDBPKY/5H1rtDIiHZiokgwl/CtqYFwIVt3hxokMvcTXZp/au\nYMMKEDFknJwNEMcJ9aLNlJPWVwKBgQC8ph9NIn3zt9pPuU6/SCO+fSzYiguCTROY\nZnrpYD1SDYpHCPqfdx7vgVrykoEhByjYbksSoO0nbcPzlz8suTkbPNamvNJL8DZ4\nRZr4DcirSITk+oPG6XzqOSgKYAeYAUTU+FUieVT4xR3RYFpJ1bWmh5psS4EuqI5Z\ngh73fsH1tQKBgQChBd3dH7lQ+oVonW1duHutaZ9B8ztWCBvG4YK82tYoZTe/4MYZ\ngJZ4pIFlDi5xhvtGPOHeQHMqwFGCthZUXkwDcQRkeWU/qdWILiNXGsJ5fhHsofg0\n/bXCD/8dLGDE8g+6ibYk+9z3ahG8iP+1xje5GFT97O8mu4JNV6kkko6wmwJ/HACM\npOZ5y7N+tFbNWZdbturdvUbAbpcCUZzkob+nWoan/+NVUFZeQS7yUQ9uG3j0lyvH\n+PvqN/ATJhVNS1YzI9fkBNwCW0NM0o2Cc0+YedRJ5bNJ3DzMTfgt+VxHxEhr7zDt\ncjCQbFzWED49KLiQPifixDBw/HwIpUCrWBF4uQKBgQC6fxzIsehKAJf1aaMCbyVB\nPvv4Xl8IbSf8ua3nIYN5mOONDS5mZRMxhwnGiWpYl9G1jZ8Y80TjlK139+rJygZJ\nu0+zDyfUWzAPNZKZMkaHvv9nEdP/G0Rt6CmvxI6SMn5eJ7nbsdv0q6COdut4qy3W\n1COC09QO7YzIG8cGykwhLQ==\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""unittest@bakingsheet.iam.gserviceaccount.com"",
  ""client_id"": ""110268003030165501214"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/unittest%40bakingsheet.iam.gserviceaccount.com""
}";

        public class PrettyJsonConverter : JsonSheetConverter
        {
            public PrettyJsonConverter(string path, IFileSystem fileSystem = null) : base(path, fileSystem)
            { }

            public override JsonSerializerSettings GetSettings(Microsoft.Extensions.Logging.ILogger logError)
            {
                var settings = base.GetSettings(logError);

                settings.Formatting = Formatting.Indented;

                return settings;
            }
        }

        [MenuItem("BakingSheet/Sample/Import From Google")]
        public static async void ConvertFromGoogle()
        {
            var jsonPath = Path.Combine(Application.streamingAssetsPath, "Google");

            var googleConverter = new GoogleSheetConverter("1iWMZVI4FgtGbig4EgPIun_BRbzp4ulqRIzINZQl-AFI", GoogleCredential, TimeZoneInfo.Utc);

            var sheetContainer = new Google.SheetContainer(UnityLogger.Default);

            await sheetContainer.Bake(googleConverter);

            // create json converter to path
            var jsonConverter = new PrettyJsonConverter(jsonPath);

            // save datasheet to streaming assets
            await sheetContainer.Store(jsonConverter);

            AssetDatabase.Refresh();

            Debug.Log("Google sheet converted.");
        }
    }
}