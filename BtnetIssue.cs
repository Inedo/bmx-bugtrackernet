using System;
using System.Data;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.BugTrackerNet
{
    /// <summary>
    /// Represents an issue from BugTracker.NET
    /// </summary>
    [Serializable]
    internal sealed class BtnetIssue : Issue
    {
        internal BtnetIssue(DataRow dr)
        {
            IssueId = dr["bg_id"].ToString();
            IssueStatus = dr["status"].ToString();
            IssueTitle = dr["bg_short_desc"].ToString();
        }
    }
}
