using DotnetWebApiBench.DataAccess.Enums;

namespace DotnetWebApiBench.Models.Config
{
    public class BenchmarkSettings
    {
        internal const int DEFAULT_PHASE1_RECORDS = 10000;
        internal const int DEFAULT_PHASE2_SECONDS = 40;

        public int Phase1Rescords { get; set; } = DEFAULT_PHASE1_RECORDS;
        public int Phase2Seconds { get; set; } = DEFAULT_PHASE2_SECONDS;
        public int Phase2Users { get; set; }
        public bool UseMemoryDatabase { get; set; }
        public DbTypeEnum DatabaseType { get; set; } = DbTypeEnum.SQLite;
        public string DbServer { get; set; } = "localhost";
        public string DbName { get; set; } = "DWABench_Northwind";
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string DbConnectionString { get; set; }
    }
}
