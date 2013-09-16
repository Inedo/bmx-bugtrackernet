using System;
using System.Data;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.BugTrackerNet
{
    /// <summary>
    /// Represents an issue from BugTracker.NET
    /// </summary>
    [Serializable]
    internal sealed class BtnetIssue : IssueTrackerIssue
    {
        internal BtnetIssue(DataRow dr)
             : base(dr["bg_id"].ToString(), dr["status"].ToString(), dr["bg_short_desc"].ToString(), string.Empty, string.Empty)
        {
        }
    }
}
