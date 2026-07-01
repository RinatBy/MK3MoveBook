using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace MK3MoveBook
{
    internal static class Program
    {
        private const string BootstrapperFileName =
            "MicrosoftEdgeWebview2Setup.exe";

        [STAThread]
        private static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!EnsureWebView2Runtime())
            {
                return;
            }
            Application.Run(new MoveBookWindow());
        }

        private static bool EnsureWebView2Runtime()
        {
            if (WebView2RuntimeIsAvailable())
            {
                return true;
            }

            DialogResult answer = MessageBox.Show(
                "Для работы MK3 MoveBook нужен системный компонент " +
                "Microsoft Edge WebView2 Runtime.\n\n" +
                "На этом компьютере компонент не найден. Установить " +
                "официальный компонент Microsoft сейчас?\n\n" +
                "Для установки требуется подключение к интернету.",
                "Требуется WebView2 Runtime",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );
            if (answer != DialogResult.Yes)
            {
                return false;
            }

            string bootstrapperPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                BootstrapperFileName
            );
            if (!File.Exists(bootstrapperPath))
            {
                MessageBox.Show(
                    "Не найден установщик " + BootstrapperFileName + ".\n\n" +
                    "Скачайте полный архив MK3 MoveBook заново и не " +
                    "переносите отдельный EXE из папки программы.",
                    "Не найден установщик WebView2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = bootstrapperPath;
                startInfo.WorkingDirectory =
                    AppDomain.CurrentDomain.BaseDirectory;
                startInfo.UseShellExecute = true;

                using (Process installer = Process.Start(startInfo))
                {
                    if (installer == null)
                    {
                        throw new InvalidOperationException(
                            "Не удалось запустить установщик."
                        );
                    }
                    installer.WaitForExit();
                }

                for (int attempt = 0; attempt < 10; attempt++)
                {
                    if (WebView2RuntimeIsAvailable())
                    {
                        return true;
                    }
                    Thread.Sleep(500);
                }

                MessageBox.Show(
                    "WebView2 Runtime не был установлен.\n\n" +
                    "Запустите " + BootstrapperFileName +
                    " вручную из папки программы и затем снова откройте книгу.",
                    "Установка WebView2 не завершена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось установить WebView2 Runtime.\n\n" +
                    exception.Message,
                    "Ошибка установки WebView2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        private static bool WebView2RuntimeIsAvailable()
        {
            try
            {
                string version =
                    CoreWebView2Environment.GetAvailableBrowserVersionString();
                return !string.IsNullOrWhiteSpace(version);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    internal sealed class UpdateManifest
    {
        public string Version { get; set; }
        public string PackageUrl { get; set; }
        public string Sha256 { get; set; }
    }

    internal sealed class MoveBookWindow : Form
    {
        private const string AppHost = "appassets.example";
        private const string LatestManifestUrl =
            "https://github.com/RinatBy/MK3MoveBook/releases/latest/download/" +
            "update-manifest.json";

        private readonly WebView2 browser;
        private readonly Panel loadingPanel;
        private readonly Label loadingLabel;
        private readonly JavaScriptSerializer jsonSerializer;
        private readonly string localDataDirectory;
        private readonly string bundledWebDirectory;
        private readonly string contentDirectory;
        private readonly string activeVersionPath;

        private string currentWebDirectory;
        private string currentContentVersion;
        private bool usingDownloadedContent;
        private bool updateBusy;
        private UpdateManifest availableUpdate;

        public MoveBookWindow()
        {
            Text = "MK3 MoveBook — Книга приёмов";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(20, 12, 29);
            ClientSize = new Size(1420, 880);
            MinimumSize = new Size(1080, 700);

            jsonSerializer = new JavaScriptSerializer();
            localDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MK3MoveBook"
            );
            bundledWebDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "web"
            );
            contentDirectory = Path.Combine(localDataDirectory, "Content");
            activeVersionPath = Path.Combine(localDataDirectory, "active-version.txt");
            currentWebDirectory = SelectWebDirectory();

            string iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "web",
                "assets",
                "icon.ico"
            );
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }

            browser = new WebView2();
            browser.CreationProperties = new CoreWebView2CreationProperties();
            browser.CreationProperties.UserDataFolder = Path.Combine(
                localDataDirectory,
                "DesktopClient"
            );
            browser.Dock = DockStyle.Fill;
            browser.BackColor = BackColor;
            Controls.Add(browser);

            loadingLabel = new Label();
            loadingLabel.AutoSize = true;
            loadingLabel.Font = new Font("Segoe UI", 13.0F, FontStyle.Regular);
            loadingLabel.ForeColor = Color.FromArgb(237, 226, 211);
            loadingLabel.BackColor = Color.Transparent;
            loadingLabel.Text = "Открываем книгу приёмов…";

            loadingPanel = new Panel();
            loadingPanel.Dock = DockStyle.Fill;
            loadingPanel.BackColor = BackColor;
            loadingPanel.Controls.Add(loadingLabel);
            loadingPanel.Resize += CenterLoadingLabel;
            Controls.Add(loadingPanel);
            loadingPanel.BringToFront();

            Shown += OnWindowShown;
        }

        private string SelectWebDirectory()
        {
            currentContentVersion = ReadContentVersion(bundledWebDirectory);
            usingDownloadedContent = false;

            try
            {
                if (!File.Exists(activeVersionPath))
                {
                    return bundledWebDirectory;
                }

                string activeVersion = File.ReadAllText(
                    activeVersionPath,
                    Encoding.UTF8
                ).Trim();
                Version parsedVersion;
                if (!Version.TryParse(activeVersion, out parsedVersion))
                {
                    return bundledWebDirectory;
                }

                string candidate = Path.Combine(
                    contentDirectory,
                    activeVersion,
                    "web"
                );
                string candidateVersion = ReadContentVersion(candidate);
                if (!File.Exists(Path.Combine(candidate, "index.html")) ||
                    !string.Equals(
                        candidateVersion,
                        activeVersion,
                        StringComparison.Ordinal
                    ) ||
                    CompareVersions(candidateVersion, currentContentVersion) < 0)
                {
                    return bundledWebDirectory;
                }

                currentContentVersion = candidateVersion;
                usingDownloadedContent = true;
                return candidate;
            }
            catch
            {
                return bundledWebDirectory;
            }
        }

        private string ReadContentVersion(string webDirectory)
        {
            try
            {
                string versionPath = Path.Combine(webDirectory, "version.json");
                Dictionary<string, object> values =
                    jsonSerializer.Deserialize<Dictionary<string, object>>(
                        File.ReadAllText(versionPath, Encoding.UTF8)
                    );
                object versionValue;
                if (values != null &&
                    values.TryGetValue("version", out versionValue))
                {
                    string version = Convert.ToString(versionValue).Trim();
                    Version parsedVersion;
                    if (Version.TryParse(version, out parsedVersion))
                    {
                        return version;
                    }
                }
            }
            catch
            {
                // The bundled fallback below keeps older packages usable.
            }
            return "1.0.0";
        }

        private static int CompareVersions(string left, string right)
        {
            Version leftVersion;
            Version rightVersion;
            if (!Version.TryParse(left, out leftVersion))
            {
                leftVersion = new Version(0, 0, 0);
            }
            if (!Version.TryParse(right, out rightVersion))
            {
                rightVersion = new Version(0, 0, 0);
            }
            return leftVersion.CompareTo(rightVersion);
        }

        private void CenterLoadingLabel(object sender, EventArgs eventArgs)
        {
            loadingLabel.Left = Math.Max(
                16,
                (loadingPanel.ClientSize.Width - loadingLabel.Width) / 2
            );
            loadingLabel.Top = Math.Max(
                16,
                (loadingPanel.ClientSize.Height - loadingLabel.Height) / 2
            );
        }

        private async void OnWindowShown(object sender, EventArgs eventArgs)
        {
            Shown -= OnWindowShown;
            CenterLoadingLabel(this, EventArgs.Empty);
            string initializationStage = "проверка файлов";

            try
            {
                string indexPath = Path.Combine(currentWebDirectory, "index.html");
                if (!File.Exists(indexPath))
                {
                    throw new FileNotFoundException(
                        "Не найдена папка интерфейса. Положите папку web рядом с программой.",
                        indexPath
                    );
                }

                initializationStage = "создание окна WebView2";
                await browser.EnsureCoreWebView2Async(null);

                initializationStage = "подключение локальных страниц";
                MapCurrentWebDirectory();

                initializationStage = "настройка окна";
                CoreWebView2Settings settings = browser.CoreWebView2.Settings;
                settings.AreDefaultContextMenusEnabled = false;
                settings.AreDevToolsEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsZoomControlEnabled = false;
                settings.AreBrowserAcceleratorKeysEnabled = false;

                browser.CoreWebView2.NavigationStarting += KeepNavigationInsideBook;
                browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
                browser.CoreWebView2.ProcessFailed += OnBrowserProcessFailed;
                browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                initializationStage = "открытие книги";
                loadingPanel.Visible = false;
                browser.Visible = true;
                browser.BringToFront();
                NavigateToBook();
            }
            catch (Exception exception)
            {
                loadingPanel.Visible = false;
                MessageBox.Show(
                    this,
                    "Не удалось открыть MK3 MoveBook.\n\n" +
                    "Этап: " + initializationStage + ".\n" +
                    exception.Message +
                    "\n\nУбедитесь, что Microsoft Edge WebView2 Runtime установлен в Windows.",
                    "MK3 MoveBook",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Close();
            }
        }

        private void MapCurrentWebDirectory()
        {
            browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                AppHost,
                currentWebDirectory,
                CoreWebView2HostResourceAccessKind.Allow
            );
        }

        private void NavigateToBook()
        {
            browser.CoreWebView2.Navigate("https://" + AppHost + "/index.html");
        }

        private void KeepNavigationInsideBook(
            object sender,
            CoreWebView2NavigationStartingEventArgs eventArgs
        )
        {
            Uri target;
            if (!Uri.TryCreate(eventArgs.Uri, UriKind.Absolute, out target))
            {
                eventArgs.Cancel = true;
                return;
            }

            if (!string.Equals(
                target.Host,
                AppHost,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                eventArgs.Cancel = true;
            }
        }

        private void OnNavigationCompleted(
            object sender,
            CoreWebView2NavigationCompletedEventArgs eventArgs
        )
        {
            if (eventArgs.IsSuccess)
            {
                loadingPanel.Visible = false;
                browser.Focus();
                return;
            }

            if (usingDownloadedContent)
            {
                try
                {
                    if (File.Exists(activeVersionPath))
                    {
                        File.Delete(activeVersionPath);
                    }
                }
                catch
                {
                    // The bundled content is still used for this launch.
                }

                usingDownloadedContent = false;
                currentWebDirectory = bundledWebDirectory;
                currentContentVersion = ReadContentVersion(bundledWebDirectory);
                loadingLabel.Text =
                    "Обновление не загрузилось. Возвращаем встроенную версию…";
                loadingPanel.Visible = true;
                loadingPanel.BringToFront();
                CenterLoadingLabel(this, EventArgs.Empty);
                MapCurrentWebDirectory();
                NavigateToBook();
                return;
            }

            loadingLabel.Text =
                "Не удалось загрузить страницы книги. Код: " +
                eventArgs.WebErrorStatus + ".";
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
            CenterLoadingLabel(this, EventArgs.Empty);
        }

        private void OnBrowserProcessFailed(
            object sender,
            CoreWebView2ProcessFailedEventArgs eventArgs
        )
        {
            loadingLabel.Text =
                "Веб-движок завершил работу. Перезапустите MK3 MoveBook.";
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
            CenterLoadingLabel(this, EventArgs.Empty);
        }

        private async void OnWebMessageReceived(
            object sender,
            CoreWebView2WebMessageReceivedEventArgs eventArgs
        )
        {
            string message;
            try
            {
                message = eventArgs.TryGetWebMessageAsString();
            }
            catch
            {
                return;
            }

            if (string.Equals(
                message,
                "check-updates",
                StringComparison.Ordinal
            ))
            {
                await CheckForUpdatesAsync();
            }
            else if (string.Equals(
                message,
                "install-update",
                StringComparison.Ordinal
            ))
            {
                await InstallAvailableUpdateAsync();
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            if (updateBusy)
            {
                return;
            }

            updateBusy = true;
            SendUpdateState(
                "checking",
                "Проверка…",
                "Проверяем обновления на GitHub…",
                ""
            );

            try
            {
                string manifestJson;
                using (WebClient client = CreateWebClient())
                {
                    manifestJson = await client.DownloadStringTaskAsync(
                        new Uri(LatestManifestUrl)
                    );
                }

                UpdateManifest manifest = ParseManifest(manifestJson);
                if (CompareVersions(
                    manifest.Version,
                    currentContentVersion
                ) > 0)
                {
                    availableUpdate = manifest;
                    SendUpdateState(
                        "available",
                        "Обновить",
                        "Доступна версия " + manifest.Version +
                        ". Нажмите, чтобы установить.",
                        "install"
                    );
                }
                else
                {
                    availableUpdate = null;
                    SendUpdateState(
                        "current",
                        "Актуально",
                        "Установлена актуальная версия " +
                        currentContentVersion + ".",
                        "check"
                    );
                }
            }
            catch
            {
                availableUpdate = null;
                SendUpdateState(
                    "error",
                    "Повторить",
                    "Не удалось связаться с GitHub. Текущая версия продолжит работать.",
                    "check"
                );
            }
            finally
            {
                updateBusy = false;
            }
        }

        private UpdateManifest ParseManifest(string manifestJson)
        {
            Dictionary<string, object> values =
                jsonSerializer.Deserialize<Dictionary<string, object>>(
                    manifestJson
                );
            if (values == null)
            {
                throw new InvalidDataException("Пустой манифест обновления.");
            }

            object versionValue;
            object packageValue;
            object shaValue;
            if (!values.TryGetValue("version", out versionValue) ||
                !values.TryGetValue("packageUrl", out packageValue) ||
                !values.TryGetValue("sha256", out shaValue))
            {
                throw new InvalidDataException(
                    "В манифесте обновления не хватает обязательных полей."
                );
            }

            string version = Convert.ToString(versionValue).Trim();
            string packageUrl = Convert.ToString(packageValue).Trim();
            string sha256 = Convert.ToString(shaValue).Trim().ToUpperInvariant();

            Version parsedVersion;
            Uri packageUri;
            if (!Version.TryParse(version, out parsedVersion))
            {
                throw new InvalidDataException("Некорректная версия обновления.");
            }
            if (!Uri.TryCreate(packageUrl, UriKind.Absolute, out packageUri) ||
                !string.Equals(
                    packageUri.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                !string.Equals(
                    packageUri.Host,
                    "github.com",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                throw new InvalidDataException(
                    "Пакет обновления должен загружаться с GitHub по HTTPS."
                );
            }
            if (!IsSha256(sha256))
            {
                throw new InvalidDataException(
                    "Некорректная контрольная сумма обновления."
                );
            }

            return new UpdateManifest
            {
                Version = version,
                PackageUrl = packageUrl,
                Sha256 = sha256
            };
        }

        private static bool IsSha256(string value)
        {
            if (value == null || value.Length != 64)
            {
                return false;
            }
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                bool isDigit = character >= '0' && character <= '9';
                bool isHexLetter = character >= 'A' && character <= 'F';
                if (!isDigit && !isHexLetter)
                {
                    return false;
                }
            }
            return true;
        }

        private async Task InstallAvailableUpdateAsync()
        {
            if (updateBusy)
            {
                return;
            }
            if (availableUpdate == null)
            {
                await CheckForUpdatesAsync();
                return;
            }

            UpdateManifest manifest = availableUpdate;
            updateBusy = true;
            string archivePath = null;
            string stagingDirectory = null;

            try
            {
                string downloadDirectory = Path.Combine(
                    localDataDirectory,
                    "Downloads"
                );
                Directory.CreateDirectory(downloadDirectory);
                Directory.CreateDirectory(contentDirectory);
                archivePath = Path.Combine(
                    downloadDirectory,
                    "MK3MoveBook-web-" + manifest.Version + ".zip"
                );

                SendUpdateState(
                    "downloading",
                    "Загрузка…",
                    "Загружаем версию " + manifest.Version + "…",
                    ""
                );
                using (WebClient client = CreateWebClient())
                {
                    await client.DownloadFileTaskAsync(
                        new Uri(manifest.PackageUrl),
                        archivePath
                    );
                }

                string actualHash = await Task.Run(
                    () => ComputeSha256(archivePath)
                );
                if (!string.Equals(
                    actualHash,
                    manifest.Sha256,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    throw new InvalidDataException(
                        "Контрольная сумма загруженного пакета не совпала."
                    );
                }

                SendUpdateState(
                    "installing",
                    "Установка…",
                    "Проверяем и устанавливаем обновление…",
                    ""
                );
                stagingDirectory = Path.Combine(
                    contentDirectory,
                    ".staging-" + Guid.NewGuid().ToString("N")
                );
                string extractTarget = stagingDirectory;
                await Task.Run(
                    () => ExtractZipSafely(archivePath, extractTarget)
                );

                string stagedWebDirectory = Path.Combine(
                    stagingDirectory,
                    "web"
                );
                if (!File.Exists(
                    Path.Combine(stagedWebDirectory, "index.html")
                ))
                {
                    throw new InvalidDataException(
                        "В пакете обновления не найдена папка web."
                    );
                }
                string stagedVersion = ReadContentVersion(
                    stagedWebDirectory
                );
                if (!string.Equals(
                    stagedVersion,
                    manifest.Version,
                    StringComparison.Ordinal
                ))
                {
                    throw new InvalidDataException(
                        "Версия пакета не совпадает с манифестом."
                    );
                }

                string versionDirectory = Path.Combine(
                    contentDirectory,
                    manifest.Version
                );
                if (Directory.Exists(versionDirectory))
                {
                    Directory.Delete(versionDirectory, true);
                }
                Directory.Move(stagingDirectory, versionDirectory);
                stagingDirectory = null;
                File.WriteAllText(
                    activeVersionPath,
                    manifest.Version,
                    new UTF8Encoding(false)
                );

                availableUpdate = null;
                SendUpdateState(
                    "restarting",
                    "Перезапуск…",
                    "Обновление установлено. Перезапускаем книгу…",
                    ""
                );
                await Task.Delay(700);
                Application.Restart();
            }
            catch
            {
                SendUpdateState(
                    "error",
                    "Повторить",
                    "Обновление не установлено. Текущая версия не изменена.",
                    "check"
                );
            }
            finally
            {
                updateBusy = false;
                TryDeleteFile(archivePath);
                TryDeleteDirectory(stagingDirectory);
            }
        }

        private static WebClient CreateWebClient()
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] =
                "MK3MoveBook-Desktop";
            return client;
        }

        private static string ComputeSha256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder result = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    result.Append(hash[index].ToString("X2"));
                }
                return result.ToString();
            }
        }

        private static void ExtractZipSafely(
            string archivePath,
            string destinationDirectory
        )
        {
            Directory.CreateDirectory(destinationDirectory);
            string rootPath =
                Path.GetFullPath(destinationDirectory) +
                Path.DirectorySeparatorChar;

            using (FileStream stream = File.OpenRead(archivePath))
            using (ZipArchive archive = new ZipArchive(
                stream,
                ZipArchiveMode.Read
            ))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName))
                    {
                        continue;
                    }

                    string relativePath = entry.FullName.Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );
                    string targetPath = Path.GetFullPath(
                        Path.Combine(destinationDirectory, relativePath)
                    );
                    if (!targetPath.StartsWith(
                        rootPath,
                        StringComparison.OrdinalIgnoreCase
                    ))
                    {
                        throw new InvalidDataException(
                            "Пакет содержит небезопасный путь."
                        );
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(targetPath);
                        continue;
                    }

                    string parentDirectory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(parentDirectory))
                    {
                        Directory.CreateDirectory(parentDirectory);
                    }
                    using (Stream input = entry.Open())
                    using (FileStream output = new FileStream(
                        targetPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None
                    ))
                    {
                        input.CopyTo(output);
                    }
                }
            }
        }

        private static void TryDeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Cleanup is best-effort only.
            }
        }

        private static void TryDeleteDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return;
            }
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch
            {
                // Cleanup is best-effort only.
            }
        }

        private void SendUpdateState(
            string state,
            string label,
            string details,
            string action
        )
        {
            if (browser.CoreWebView2 == null)
            {
                return;
            }

            Dictionary<string, object> payload =
                new Dictionary<string, object>();
            payload["type"] = "movebook-update-status";
            payload["state"] = state;
            payload["label"] = label;
            payload["details"] = details;
            payload["action"] = action;
            payload["currentVersion"] = currentContentVersion;
            browser.CoreWebView2.PostWebMessageAsJson(
                jsonSerializer.Serialize(payload)
            );
        }
    }
}
