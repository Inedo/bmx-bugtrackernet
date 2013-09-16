using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Diagnostics;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.BugTrackerNet
{
    [ProviderProperties(
        "BugTracker.NET",
        "Supports BugTracker.NET 2.0 and later; requires that a custom field be added to bugs so that they can be associated with releases.")]
    [CustomEditor(typeof(BugTrackerNetProviderEditor))]
    public sealed class BugTrackerNetProvider : IssueTrackingProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BugTrackerNetProvider"/> class.
        /// </summary>
        public BugTrackerNetProvider()
        {
            this.ClosedStatusName = "closed";
        }

        [Persistent]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the status that indicates an issue is closed
        /// </summary>
        [Persistent]
        public string ClosedStatusName { get; set; }

        /// <summary>
        /// Gets or sets the name fo the custom field on the bug used to indicate
        /// whether an issue is tied to a particular release. If null or empty,
        /// issues are not tied to a release
        /// </summary>
        [Persistent]
        public string ReleaseNumberCustomField { get; set; }

        public override Issue[] GetIssues(string releaseNumber)
        {
            var issues = new List<Issue>();
            foreach (DataRow dr in ExecuteDataTable(BuildGetIssuesSql(releaseNumber)).Rows)
                issues.Add(new BtnetIssue(dr));

            return issues.ToArray();
        }

        public override bool IsIssueClosed(Issue issue)
        {
            return string.Equals(issue.IssueStatus, ClosedStatusName, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return "Provides access to a BugTracker.NET 2.8.7 installation.";
        }

        public override bool IsAvailable()
        {
            return true;
        }

        public override void ValidateConnection()
        {
            try
            {
                this.GetIssues("0");
            }
            catch (Exception ex)
            {
                throw new NotAvailableException(ex.Message, ex);
            }
        }

        private SqlConnection CreateConnection()
        {
            var conStr = new SqlConnectionStringBuilder(ConnectionString) { Pooling = false };

            var con = new SqlConnection(conStr.ToString());
            con.InfoMessage += (s, e) => Tracer.Information(e.Message);

            return con;
        }

        private SqlCommand CreateCommand(string cmdText)
        {
            return new SqlCommand
            {
                CommandText = cmdText,
                Connection = this.CreateConnection()
            };
        }

        private void ExecuteNonQuery(string cmdText)
        {
            using (var cmd = CreateCommand(cmdText))
            {
                try
                {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        private DataTable ExecuteDataTable(string cmdText)
        {
            return this.ExecuteDataTable(cmdText, new SqlParameter[0]);
        }

        private DataTable ExecuteDataTable(string cmdText, params SqlParameter[] sqlParams)
        {
            var dt = new DataTable();
            using (var cmd = this.CreateCommand(cmdText))
            {
                cmd.Parameters.AddRange(sqlParams);
                try
                {
                    cmd.Connection.Open();
                    dt.Load(cmd.ExecuteReader());
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }

            return dt;
        }

        private string BuildGetIssuesSql(string releaseNumber)
        {
            return "SELECT B.[bg_id]" +
                    "      ,B.[bg_short_desc]" +
                    "      ,COALESCE(" +
                    "          S.[st_name]," +
                    "          CAST(B.[bg_status] AS VARCHAR(8))) [status]" +
                    (string.IsNullOrEmpty(ReleaseNumberCustomField)
                        ? string.Empty
                        : string.Format("      ,[{0}]", ReleaseNumberCustomField)
                        ) +
                    "  FROM [bugs] B" +
                    "       LEFT JOIN [statuses] S ON B.[bg_status] = s.[st_id] " +
                    (string.IsNullOrEmpty(ReleaseNumberCustomField) || releaseNumber == null
                        ? string.Empty
                        : string.Format(" WHERE [{0}] = '{1}'",
                            ReleaseNumberCustomField,
                            releaseNumber.Replace("'", "''"))
                        );
        }
    }
}
