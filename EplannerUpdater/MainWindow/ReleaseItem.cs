using Octokit;
using System.Text.RegularExpressions;

namespace Updater;

public interface IReleaseItem
{
    Release Release { get; }

    bool IsCurrentRelease { get; set; }
}

public partial class ReleaseItem(Release release) : IReleaseItem
{
    public Release Release => release;

    public bool IsCurrentRelease { get; set; } = false;

    public string ChangeLog
    {
        get
        {
            var regex = ChangeLogRegex().Match(release.Body);
            if (regex.Success)
                return regex.Groups["change_log"].Value;
            else
                return release.Body;
        }
    }

    [GeneratedRegex(@"```ChangeLog(?:\r\n|\n)(?<change_log>[\S\s]*?)(?:\r\n|\n)```", RegexOptions.Multiline)]
    private static partial Regex ChangeLogRegex();
}
