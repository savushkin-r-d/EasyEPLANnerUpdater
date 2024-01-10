using Octokit;
using Octokit.Internal;
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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace Updater;

public interface IMainWindowModel
{
    /// <summary>
    /// github-client
    /// </summary>
    GitHubClient GitHub { get; }

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
    /// Проверка наличия обновлений данного приложения.
    /// Если не установлен PAT, то осуществляется только раз в день, чтобы не
    /// забивать ограничение по числу запросов
    /// </summary>
    void CheckUpdatesApp();

    /// <summary>
    /// Режим кнопки запуска EPLAN:
    /// true: Запускать
    /// false: Пропустить (EPLAN уже запущен)
    /// </summary>
    bool StartButtonMode { get; set; }

    /// <summary>
    /// Видимость кнопки запуска
    /// </summary>
    bool StartButtonVisibility { get; }

    /// <summary>
    /// GitHub user
    /// </summary>
    User? User { get; set; }

    /// <summary>
    /// Инициализация релизов
    /// </summary>
    Task InitialyzeReleses();

    /// <summary>
    /// Инициализация PR's
    /// </summary>
    void InitializePullRequests();

    /// <summary>
    /// main window
    /// </summary>
    MainWindow MainWindow { get; }

    /// <summary>
    /// Проверить используемый PAT
    /// </summary>
    Task CheckPAT();

    /// <summary>
    /// Получить последние обновления
    /// </summary>
    /// <returns>Список последних обновлений</returns>
    List<IReleaseItem>? GetLatestsReleases();

    /// <summary>
    /// Скачать обновление релиза
    /// </summary>
    /// <param name="releaseItem">Релиз для установки обновлений</param>
    void InstallAsset(ReleaseItem? releaseItem);

    /// <summary>
    /// Скачать артифакт пулл-реквеста
    /// </summary>
    /// <param name="pullRequestItem"></param>
    void InstallPRArtifact(PullRequestItem? pullRequestItem);

    /// <summary>
    /// Установить обновления
    /// </summary>
    void UpdateAssemblies(object? sender, System.ComponentModel.AsyncCompletedEventArgs e);

    /// <summary>
    /// Делегат события после инициализации релизов
    /// </summary>
    public delegate void ReleasesInitializedHandler(int code);

    /// <summary>
    /// Событие вызываемое после инициализации релизов
    /// </summary>
    event ReleasesInitializedHandler? ReleasesInitialized;
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
    private static readonly string LOCATION = System.IO.Path.GetDirectoryName(
        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
    #pragma warning restore CS8601
    #pragma warning restore CS8602

    /// <summary> Octokit пользователь </summary>
    private User? user;

    Task? taskCheckPAT;

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

        taskCheckPAT = CheckPAT();
    }

    #region public properties
    public GitHubClient GitHub { get; } = new(new ProductHeaderValue("EasyEPLANnerUpdater"));

    public List<IReleaseItem> Releases { get; private set; } = [];

    public IReleaseItem? CurrentRelease { get; private set; } = null;

    public List<IPullRequsetItem> PullRequests { get; private set; } = [];

    public bool StartButtonMode
    {
        get => startButtonMode;
        set
        {
            startButtonMode = value;
            OnPropertyChanged(nameof(StartButtonMode));
        }
    }

    public bool StartButtonVisibility => (StartButtonMode is true && File.Exists(Settings.Default.EplanAppPath)) 
        || startButtonMode is false;

    public User? User
    {
        get => user;
        set
        {
            user = value;
            OnPropertyChanged(nameof(User));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event IMainWindowModel.ReleasesInitializedHandler? ReleasesInitialized;

    public MainWindow MainWindow => mainWindow;
    #endregion



    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    
    public void CheckUpdatesApp()
    {
        taskCheckPAT?.Wait();

        if (Settings.Default.UpdaterReleaseLastCheck == DateTime.Today && User is null)
            return;

        List<Release>? updaterReleases = null;

        try
        {
            updaterReleases = [.. GitHub.Repository.Release.GetAll(Settings.Default.GitOwner, Settings.Default.UpdaterGitRepo).Result];
            var latestAvailableRelease = updaterReleases.TakeWhile(r => r.TagName != Settings.Default.UpdaterReleaseTag).ToList().FirstOrDefault();

            if (latestAvailableRelease is null)
            {
                Settings.Default.UpdaterReleaseLastCheck = DateTime.Today;
                Settings.Default.Save();
                return;
            }

            #pragma warning disable SYSLIB0014 // Тип или член устарел
            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += (sender, e) => UpdateApp(latestAvailableRelease);
                webClient.DownloadProgressChanged += (sender, e) => MainWindow.Progress = e.ProgressPercentage / 1.25;

                webClient.DownloadFileAsync(new Uri(latestAvailableRelease.Assets[0].BrowserDownloadUrl), @$".\{TMP_ASSET_ARCHIVE}");
            }
            #pragma warning restore SYSLIB0014
        }
        catch
        {
            App.LoadingTokenSource.Cancel();
            App.UpdateCheckerError("Ошибка!");
            MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindow.State.Visibility = Visibility.Visible;
                MainWindow.State.Text = "Не удается получить сведения об обновлениях." +
                " Попробуйте позже.\n (Или попробуйте установить PAT в настройках)";
            });

            if (Settings.Default.ShowPullRequests is false)
                MainWindow.Dispatcher.Invoke(MainWindow.RefreshCancel);

            return;
        }
    }

    private async void UpdateApp(Release latestAvailableRelease)
    {
        await Task.Run(() =>
        {
            var updaterFile = (Process.GetCurrentProcess().MainModule?.FileName ?? "");
            var updaterName = updaterFile.Split('\\').Last().Replace(".exe", "");

            File.Delete(@$".\{updaterName}.exe.bak");
            File.Delete(@$".\{updaterName}.dll.bak");
            File.Delete(@$".\Octokit.dll.bak");
            MainWindow.Progress = 83;

            File.Move(@$".\{updaterName}.exe", @$".\{updaterName}.exe.bak");
            File.Move(@$".\{updaterName}.dll", @$".\{updaterName}.dll.bak");
            File.Move(@$".\Octokit.dll", @$".\Octokit.dll.bak");
            MainWindow.Progress = 85;

            try { Directory.Delete(@$"{TMP_ASSET_DIR}", true); }
            catch (Exception) { }
            MainWindow.Progress = 88;

            ZipFile.ExtractToDirectory(@$"{TMP_ASSET_ARCHIVE}", @$"{TMP_ASSET_DIR}");
            MainWindow.Progress = 93;

            CopyReleaseFiles(@$".\{TMP_ASSET_DIR}\Updater\", @".\");
            MainWindow.Progress = 97;

            File.Delete(@$"{TMP_ASSET_ARCHIVE}");
            Directory.Delete(@$"{TMP_ASSET_DIR}", true);
            MainWindow.Progress = 100;

            // Save info about check
            Settings.Default.UpdaterReleaseLastCheck = DateTime.Today;
            Settings.Default.UpdaterReleaseTag = latestAvailableRelease.TagName;
            Settings.Default.Save();

            // Restart programm
            Process.Start(updaterFile, string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
            App.Current.Dispatcher.Invoke(App.Current.Shutdown);
        });
    }

    public void InitializePullRequests()
    {
        List<PullRequest> pullRequestList;
        List<Artifact> artifactList;
        List<Issue> issueList;

        try
        {
            pullRequestList = [.. GitHub.Repository.PullRequest.GetAllForRepository(Settings.Default.GitOwner, Settings.Default.GitRepo).Result.Where(pr => pr.Draft is false)];
            artifactList = [.. GitHub.Actions.Artifacts.ListArtifacts(Settings.Default.GitOwner, Settings.Default.GitRepo).Result.Artifacts];
            issueList = [.. GitHub.Issue.GetAllForRepository(Settings.Default.GitOwner, Settings.Default.GitRepo, new RepositoryIssueRequest { State = ItemStateFilter.Open }).Result];
        }
        catch
        {
            App.UpdateCheckerError("Ошибка!");
            return;
        }

        PullRequests = artifactList
            .Where(art => pullRequestList.Any(pr => art.WorkflowRun.HeadSha == pr.Head.Sha))
            .DistinctBy(art => art.WorkflowRun.HeadSha)
            .ToDictionary(art => pullRequestList.First(pr => art.WorkflowRun.HeadSha == pr.Head.Sha), art => art)
            .ToDictionary(item => item.Key, item => (art: item.Value, issue: issueList.FirstOrDefault(issue => issue.Number == GetConnectedIssueFromPullRequest(item.Key))))
            .Select(item => new PullRequestItem(item.Key, item.Value.art, item.Value.issue) as IPullRequsetItem)
            .ToList();

        if (Settings.Default.UsePullRequestVersion)
        {
            var currentPR = PullRequests.Find(pr => pr.PullRequest.Number == Settings.Default.PullRequestNumber);
            if (currentPR is not null)
                currentPR.IsCurrentArtifact = true;
        }
    }

    /// <summary>
    /// Получить id issue из комментария PR
    /// </summary>
    private int GetConnectedIssueFromPullRequest(PullRequest pullRequest)
    {
        if (int.TryParse(IssueIDRegex().Match(pullRequest.Body ?? "").Groups["id"].Value, out var value))
            return value;
        else return -1;
    }

    public async Task CheckPAT()
    {
        try
        {
            User = await GitHub.User.Current();
        }
        catch
        {
            GitHub.Credentials = Credentials.Anonymous;
            User = null;
            Settings.Default.ShowPullRequests = false;
            Settings.Default.Save();
        }
    }

    public async Task InitialyzeReleses()
    {
        try
        {
            var taskReleases = GitHub.Repository.Release.GetAll(Settings.Default.GitOwner, Settings.Default.GitRepo);
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(taskReleases, Task.Delay(5000, timeoutCancellationTokenSource.Token));
                if (completedTask == taskReleases)
                {
                    timeoutCancellationTokenSource.Cancel();
                    Releases = (await taskReleases).ToList().OrderByDescending(r => r.CreatedAt)
                        .TakeWhile(r => r.TagName != Settings.Default.InitialReleaseAfter)
                        .Select(r => new ReleaseItem(r) as IReleaseItem)
                        .ToList();
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }

            var releaseTag = Settings.Default.ReleaseTag;

            if (string.IsNullOrEmpty(releaseTag) is false)
            {
                CurrentRelease = Releases.Find(r => r.Release.TagName == releaseTag);
                if (CurrentRelease is not null && Settings.Default.UsePullRequestVersion is false)
                    CurrentRelease.IsCurrentRelease = true;
                else CurrentRelease = null;
            }
        }
        catch
        {
            App.LoadingTokenSource.Cancel();
            App.UpdateCheckerError("Ошибка!");
            MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindow.State.Visibility = Visibility.Visible;
                MainWindow.State.Text = "Не удается получить сведения об обновлениях." +
                " Попробуйте позже.\n (Или попробуйте установить PAT в настройках)";
            });

            if (Settings.Default.ShowPullRequests is false)
                MainWindow.Dispatcher.Invoke(MainWindow.RefreshCancel);

            ReleasesInitialized?.Invoke(1);
            return;
        }

        if (CurrentRelease == Releases.FirstOrDefault())
        {
            MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindow.State.Visibility = Visibility.Visible;
                MainWindow.State.Text = "Последние обновления установлены.";
            });
            App.UpdateCheckerPass("Последние обновления уже установлены.");
        }
        if (Settings.Default.ShowPullRequests is false)
            MainWindow.Dispatcher.Invoke(MainWindow.RefreshCancel);
        App.LoadingTokenSource.Cancel();


        ReleasesInitialized?.Invoke(0);
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
            webClient.DownloadProgressChanged += (sender, e) => MainWindow.Progress = e.ProgressPercentage / 1.25;

            webClient.DownloadFileAsync(new Uri(releaseItem.Release.Assets[0].BrowserDownloadUrl), @$"{LOCATION}\{TMP_ASSET_ARCHIVE}");
        }
        #pragma warning restore SYSLIB0014

        Settings.Default.UsePullRequestVersion = false;
        Settings.Default.ReleaseTag = releaseItem.Release.TagName;
        Settings.Default.BTODescriptionUsedVersion = $"R: {releaseItem.Release.PublishedAt.GetValueOrDefault().Date.ToShortDateString()}";
        Settings.Default.Save();
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
        Settings.Default.BTODescriptionUsedVersion = $"#{pullRequestItem.PullRequest.Number}";
        Settings.Default.Save();
    }

    public async void UpdateAssemblies(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        await Task.Run(() =>
        {
            if (App.ParrentProcessID != -1)
            {
                var parentProc = Process.GetProcessById(App.ParrentProcessID);
                if (parentProc is not null)
                {
                    Settings.Default.EplanAppPath = parentProc.MainModule?.FileName;
                    if (App.SourceArg == RunSourceArg.FromMenu)
                        parentProc.CloseMainWindow();
                    else
                        parentProc.Kill();
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
            MainWindow.Dispatcher.Invoke(() =>
            {
                MainWindow.StartButton.Visibility = Visibility.Visible;
                MainWindow.RefreshButton_Click(this, new RoutedEventArgs());
            });
        });
    }

    public async void UpdateBTODescription()
    {
        RepositoryContent? description;
        DateTimeOffset? lastCommitForFileDate;

        try
        {
            description = (await GitHub.Repository.Content.GetAllContents(Settings.Default.GitOwner,
                Settings.Default.BTODescriptionRepo, Settings.Default.BTODescriptionPath))
            .FirstOrDefault();

            var request = new CommitRequest 
            { 
                Path = Settings.Default.BTODescriptionPath,
                Sha = "master" 
            };
            lastCommitForFileDate = 
                (await GitHub.Repository.Commit
                .GetAll(Settings.Default.GitOwner, Settings.Default.BTODescriptionRepo, request))
                .FirstOrDefault()?.Commit.Author.Date;
        }
        catch
        {
            return;
        }

        #pragma warning disable SYSLIB0014 // Тип или член устарел
        using (var webClient = new WebClient())
        {
            webClient.DownloadFileCompleted += async (sender, e) => { await Task.Delay(500); MainWindow.Progress = 0; };
            webClient.DownloadProgressChanged += (sender, e) => MainWindow.Progress = e.ProgressPercentage;

            webClient.DownloadFileAsync(new Uri(description?.DownloadUrl ?? ""), @$"{LOCATION}\{Settings.Default.BTODescriptionFilePath}");
        }
        #pragma warning restore SYSLIB0014

        if (lastCommitForFileDate is not null)
        {
            Settings.Default.BTODescriptionUsedVersion = lastCommitForFileDate.GetValueOrDefault().Date.ToShortDateString();
            Settings.Default.Save();
        }
    }

    /// <summary>
    /// Копировать файлы ассета из временной папки в рабочую
    /// </summary>
    /// <param name="path">Путь для копирования</param>
    /// <param name="copyPath">Путь для вставки</param>
    private static void CopyReleaseFiles(string path, string copyPath)
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
            Directory.CreateDirectory(copyPath + dir_info.Name);
            CopyReleaseFiles(dir, copyPath + dir_info.Name + "\\");
        }
    }
}
