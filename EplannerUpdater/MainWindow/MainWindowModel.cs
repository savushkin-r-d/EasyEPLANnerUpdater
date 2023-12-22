using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Updater;

public interface IMainWindowModel
{
    /// <summary>
    /// Все релизы
    /// </summary>
    List<IReleaseItem>? Releases { get; }

    /// <summary>
    /// Текущий релиз
    /// </summary>
    IReleaseItem? CurrentRelease { get; }

    /// <summary>
    /// Инициализированные пулл-реквесты
    /// </summary>
    List<IPullRequsetItem>? PullRequests { get; }

    /// <summary>
    /// Инициализация релизов
    /// </summary>
    void InitialyzeReleses();

    /// <summary>
    /// Инициализация PR's
    /// </summary>
    void InitializePullRequests();
}

public partial class MainWindowModel : IMainWindowModel, INotifyPropertyChanged
{
    #region private fields

    /// <summary> Название устанавливаемого архива </summary>
    private static readonly string TMP_ASSET_ARCHIVE = "TMP.zip";
    /// <summary> Название временной папки, в котороую извлекаются файлы архива </summary>
    private static readonly string TMP_ASSET_DIR = "TMP";

    #pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
    #pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
    /// <summary> Путь к папке в которой находится приложение (Eplanner 2.9) </summary>
    private static readonly string LOCATION = Path.GetDirectoryName(
        Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
    #pragma warning restore CS8601
    #pragma warning restore CS8602

    /// <summary> Octokit пользователь </summary>
    private User? user;

    /// <summary>
    /// Режим кнопки запуска:
    ///     true - Пропустить
    ///     false - Запустить
    /// </summary>
    public bool startButtonMode = App.ParrentProcessID == -1;

    private MainWindow mainWindow;

    /// <summary>
    /// Шаблон для поиска ID issue в PR
    /// </summary>
    [GeneratedRegex(@"#(?<id>\d+)")]
    private static partial Regex IssueIDRegex();
    #endregion




    



    public MainWindowModel(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;

        try
        {
            GitHub.Credentials = new Credentials(Settings.Default.PAT);
        }
        catch { }

        CheckPAT();
    }


    #region public properties
    public GitHubClient GitHub { get; } = new(new ProductHeaderValue("EasyEPLANnerUpdater"));

    public List<IReleaseItem> Releases { get; private set; } = [];

    public IReleaseItem? CurrentRelease { get; private set; } = null;

    public List<IPullRequsetItem> PullRequests { get; private set; } = [];

    public bool ParentProcessKilled { get; private set; }

    public bool Status { get; private set; } = false;

    public bool StartButtonMode
    {
        get => startButtonMode;
        set
        {
            startButtonMode = value;
            OnPropertyChanged(nameof(StartButtonMode));
        }
    }

    public User? User
    {
        get => user;
        set
        {
            user = value;
            OnPropertyChanged(nameof(User));
        }
    }

    public MainWindow MainWindow => mainWindow;
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public void InitializePullRequests()
    {
        List<PullRequest> pullRequestList;
        List<Artifact> artifactList;
        List<Issue> issueList;

        try
        {
            var source = new CancellationTokenSource();
            TimeOut(source.Token);

            pullRequestList = [.. GitHub.Repository.PullRequest.GetAllForRepository(Settings.Default.GitOwner, Settings.Default.GitRepo).Result];
            artifactList = [.. GitHub.Actions.Artifacts.ListArtifacts(Settings.Default.GitOwner, Settings.Default.GitRepo).Result.Artifacts];
            issueList = [.. GitHub.Issue.GetAllForRepository(Settings.Default.GitOwner, Settings.Default.GitRepo, new RepositoryIssueRequest { State = ItemStateFilter.Open }).Result];


            source.Cancel();
        }
        catch
        {
            App.UpdateCheckerError("Ошибка!");
            Status = false;
            return;
        }


        //var issue = issueList[0];
        PullRequests = artifactList
            .Where(art => pullRequestList.Any(pr => art.WorkflowRun.HeadSha == pr.Head.Sha))
            .ToDictionary(art => pullRequestList.First(pr => art.WorkflowRun.HeadSha == pr.Head.Sha), art => art)
            .ToDictionary(item => item.Key, item => (art: item.Value, issue: issueList.FirstOrDefault(issue => issue.Number == GetPullRequestConnectedIssue(item.Key))))
            .Select(item => new PullRequestItem(item.Key, item.Value.art, item.Value.issue) as IPullRequsetItem)
            .ToList();

        Status = true;
    }

    private int GetPullRequestConnectedIssue(PullRequest pullRequest)
    {
        if (int.TryParse(IssueIDRegex().Match(pullRequest.Body ?? "").Groups["id"].Value, out var value))
            return value;
        else return -1;
    }

    public void CheckPAT()
    {
        try
        {
            User = GitHub.User.Current().Result;
        }
        catch
        {
            GitHub.Credentials = Credentials.Anonymous;
            User = null;
            Settings.Default.ShowPullRequests = false;
        }
    }

    private static async void TimeOut(CancellationToken token)
    {
        await Task.Run(async () =>
        {
            await Task.Delay(5000);
            if (!token.IsCancellationRequested)
            {
                App.UpdateCheckerError("Время проверки вышло");
            }
        }, token);
    }


    public void InitialyzeReleses()
    {
        try
        {
            var source = new CancellationTokenSource();
            TimeOut(source.Token);

            Releases = GitHub.Repository.Release.GetAll(Settings.Default.GitOwner, Settings.Default.GitRepo).Result
                .ToList().OrderByDescending(r => r.CreatedAt).Select(r => new ReleaseItem(r) as IReleaseItem).ToList();

            source.Cancel();

            var releaseTag = Settings.Default.ReleaseTag;

            if (string.IsNullOrEmpty(releaseTag) is false)
            {
                CurrentRelease = Releases.Find(r => r.Release.TagName == releaseTag);
                if (CurrentRelease is not null)
                    CurrentRelease.IsCurrentRelease = true;
            }
        }
        catch
        {
            MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindow.State.Visibility = Visibility.Visible;
                MainWindow.State.Text = "Не удается получить сведения об обновлениях. Попробуйте позже. (Попробуйте установить PAT в настройках)";
            });

            App.UpdateCheckerError("Ошибка!");

            Status = false;
            return;
        }

        if (CurrentRelease == Releases.FirstOrDefault())
        {
            App.UpdateCheckerError("Последние обновления уже установлены.");
        }

        Status = true;
    }

    public List<IReleaseItem>? GetLatestsReleases()
    {
        if (CurrentRelease is null || Releases is null)
            return null;

        var index = Releases.IndexOf(CurrentRelease);

        if (index == -1)
            return null;

        return Releases.Take(index).ToList();
    }

    public void InstallAsset(ReleaseItem? releaseItem)
    {
        if (releaseItem is null)
            return;


        #pragma warning disable SYSLIB0014 // Тип или член устарел
        using (var webClient = new WebClient())
        {
            webClient.DownloadFileCompleted += UpdateAssemblies;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            webClient.DownloadFileAsync(new Uri(releaseItem.Release.Assets[0].BrowserDownloadUrl), @$"{LOCATION}\{TMP_ASSET_ARCHIVE}");
        }
        #pragma warning restore SYSLIB0014

        Settings.Default.ReleaseTag = releaseItem.Release.TagName;
    }

    public async void InstallPRArtifact(PullRequestItem? pullRequestItem)
    {
        if (pullRequestItem is null)
            return;

        var stream = await GitHub.Actions.Artifacts.DownloadArtifact(Settings.Default.GitOwner, Settings.Default.GitRepo, pullRequestItem.Artifact.Id, "zip");
        MainWindow.Progress = 20;
        var archive = File.OpenWrite(@$"{LOCATION}\{TMP_ASSET_ARCHIVE}");
        MainWindow.Progress = 30;
        await stream.CopyToAsync(archive);
        MainWindow.Progress = 60;
        archive.Close();
        MainWindow.Progress = 75;

        UpdateAssemblies(null, new (null, false, null));

        Settings.Default.UsePullRequestVersion = true;
        Settings.Default.PullRequestNumber = pullRequestItem.PullRequest.Number;
    }

    private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        => MainWindow.Progress = e.ProgressPercentage / 1.25;

    public async void UpdateAssemblies(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        await Task.Run(() =>
        {
            if (App.ParrentProcessID != -1)
            {
                var parentProc = Process.GetProcessById(App.ParrentProcessID);
                if (parentProc is not null)
                {
                    Settings.Default.EplanAppPath = parentProc?.MainModule?.FileName;
                    parentProc?.Kill();
                    ParentProcessKilled = true;
                }
            }

            try { Directory.Delete(@$"{LOCATION}\{TMP_ASSET_DIR}", true); }
            catch (Exception) { }

            MainWindow.Progress = 85;

            System.IO.Compression.ZipFile.ExtractToDirectory(@$"{LOCATION}\{TMP_ASSET_ARCHIVE}", @$"{LOCATION}\{TMP_ASSET_DIR}");

            MainWindow.Progress = 90;

            CopyReleaseFiles(@$"{LOCATION}\{TMP_ASSET_DIR}", @$"{LOCATION}\");

            MainWindow.Progress = 93;


            File.Delete(@$"{LOCATION}\{TMP_ASSET_ARCHIVE}");

            MainWindow.Progress = 95;

            Directory.Delete(@$"{LOCATION}\{TMP_ASSET_DIR}", true);

            MainWindow.Progress = 100;
            MainWindow.ResetProgressBarWithDelay();

            StartButtonMode = true;
        });
    }

    public static void CopyReleaseFiles(string path, string copyPath)
    {
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            if (file is null)
                continue;
            FileInfo file_info = new(file);
            File.Copy(file, copyPath + file_info.Name, true);
        }

        string[] dirs = Directory.GetDirectories(path);
        foreach (string dir in dirs)
        {
            if (dir is null)
                continue;
            var dir_info = new DirectoryInfo(dir);
            CopyReleaseFiles(dir, copyPath + dir_info.Name + "\\");
        }
    }


}
