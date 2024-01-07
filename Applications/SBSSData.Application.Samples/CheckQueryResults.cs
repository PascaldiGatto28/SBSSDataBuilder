using SBSSData.Application.Support;
using SBSSData.Softball.Common;

namespace SBSSData.Application.Samples
{

    public class CheckQueryResults<T>
    {
        public static readonly string TestOutput = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\SBSSData-Application-Samples\TestOutput\");

        private CheckQueryResults()
        {
            QueryJson = string.Empty;
            DeserializedJson = string.Empty;
            Check = false;
        }

        public CheckQueryResults(T queryResults)
        {
            if (!Directory.Exists(TestOutput))
            {
                Directory.CreateDirectory(TestOutput);
            }

            QueryJson ??= queryResults == null ? string.Empty : queryResults.ToJsonString();
            DeserializedJson = string.Empty;
            Check = false;
        }

        public string QueryJson
        {
            get;
            set;
        }

        public string DeserializedJson
        {
            get;
            set;
        }

        public bool Check
        {
            get;
            set;
        }

        public int QueryJsonLength => QueryJson.Length;

        public int DeserializedJsonLength => DeserializedJson.Length;

        public string Name => $"CheckQuyeryResults<{Utilities.TypeToString(typeof(T))}>";

        public override string ToString()
        {

            return $"{Name}: QueryJson={QueryJsonLength:#,##0} bytes; DesJson={DeserializedJson.Length:#,##0} bytes; Are equal is {Check}";
        }

        public static CheckQueryResults<T> CheckResults(T queryResults, string name = "")
        {
            bool check = false;
            string fileName = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
            string outputPath = $"{TestOutput}{fileName}.json";

            CheckQueryResults<T> checkResults = new();

            if (queryResults != null)
            {
                string json = queryResults.ToJsonString();
                string desJson = string.Empty;
                queryResults.Serialize(outputPath);
                T? persistedResults = outputPath.Deserialize<T>();
                if (persistedResults != null)
                {
                    desJson = persistedResults.ToJsonString();
                    check = json == desJson;
                }

                checkResults = new CheckQueryResults<T>()
                {
                    QueryJson = json,
                    DeserializedJson = check ? "The same as QueryJson" : desJson,
                    Check = check
                };
            }

            return checkResults;
        }

    }
}
