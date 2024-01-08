using Octokit;

namespace Updater;

public interface IPullRequsetItem
{
    PullRequest PullRequest { get; }

    Artifact Artifact { get; }

    bool IsCurrentArtifact { get; set; }
}

public class PullRequestItem(PullRequest pullRequest, Artifact artifact, Issue? issue = null) : IPullRequsetItem
{
    public PullRequest PullRequest => pullRequest;

    public Artifact Artifact => artifact;

    public bool HasIssue => issue is not null;

    public Issue? Issue => issue;

    public string PullRequestToolTip => $"{pullRequest.Title}\n\n{pullRequest.Body}";

    public string? IssueToolTip => HasIssue? $"{issue?.Title}\n\n{issue?.Body}" : null;

    public bool IsCurrentArtifact { get; set; } = false;

    public bool ReviewRequested => pullRequest.RequestedReviewers
        .Select(user => user.Login)
        .Contains(App.MainWindow?.Model?.User?.Login);

    public string GroupName => ReviewRequested ? "Ожидают обзора" : "Все запросы";
}
    
