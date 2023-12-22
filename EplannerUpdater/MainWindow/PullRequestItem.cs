using Octokit;

namespace Updater;

public interface IPullRequsetItem
{
    PullRequest PullRequest { get; }

    Artifact Artifact { get; }
}

public class PullRequestItem(PullRequest pullRequest, Artifact artifact, Issue? issue = null) : IPullRequsetItem
{
    public PullRequest PullRequest => pullRequest;

    public Artifact Artifact => artifact;

    public bool HasIssue => issue is not null;

    public Issue? Issue => issue;

    public string PullRequestToolTip => $"{pullRequest.Title}\n\n{pullRequest.Body}";

    public string? IssueToolTip => HasIssue? $"{issue?.Title}\n\n{issue?.Body}" : null;
}
