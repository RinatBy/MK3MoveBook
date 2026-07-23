using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MK3MoveBook
{
    internal sealed class PatchInfo
    {
        public string TargetVersion { get; set; }
        public List<string> SupportedBaseVersions { get; set; }
    }

    internal static class DeltaUpdaterProgram
    {
        private const string MainExecutableName = "MK3 MoveBook.exe";
        private const string PatchInfoName = "patch-info.json";
        private const string TargetFilesName = "target-files.sha256";
        private const string DeletedFilesName = "deleted-files.txt";
        private const string SilentEnvironmentVariable =
            "MK3MOVEBOOK_UPDATE_SILENT";
        private const string LocalDataEnvironmentVariable =
            "MK3MOVEBOOK_UPDATE_LOCAL_DATA";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                string packageDirectory = AppDomain.CurrentDomain.BaseDirectory;
                PatchInfo patchInfo = ReadPatchInfo(packageDirectory);
                string applicationDirectory = FindApplicationDirectory(
                    packageDirectory,
                    args
                );
                if (string.IsNullOrEmpty(applicationDirectory))
                {
                    return;
                }

                InstallPatch(
                    packageDirectory,
                    applicationDirectory,
                    patchInfo
                );
                if (!IsSilent())
                {
                    MessageBox.Show(
                        "MK3 MoveBook обновлён до версии " +
                            patchInfo.TargetVersion +
                            ".\n\nТеперь книгу можно запускать как обычно.",
                        "MK3 MoveBook",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception exception)
            {
                Environment.ExitCode = 1;
                if (!IsSilent())
                {
                    MessageBox.Show(
                        "Не удалось установить лёгкое обновление.\n\n" +
                            exception.Message +
                            "\n\nЗакройте MK3 MoveBook и повторите попытку. " +
                            "Если ошибка сохранится, используйте полное обновление.",
                        "MK3 MoveBook",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private static bool IsSilent()
        {
            return string.Equals(
                Environment.GetEnvironmentVariable(
                    SilentEnvironmentVariable
                ),
                "1",
                StringComparison.Ordinal
            );
        }

        private static PatchInfo ReadPatchInfo(string packageDirectory)
        {
            string infoPath = Path.Combine(packageDirectory, PatchInfoName);
            if (!File.Exists(infoPath))
            {
                throw new InvalidDataException(
                    "В пакете не найден файл " + PatchInfoName + "."
                );
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            PatchInfo patchInfo = serializer.Deserialize<PatchInfo>(
                File.ReadAllText(infoPath, Encoding.UTF8)
            );
            Version parsedVersion;
            if (patchInfo == null ||
                !Version.TryParse(
                    patchInfo.TargetVersion,
                    out parsedVersion
                ) ||
                patchInfo.SupportedBaseVersions == null ||
                patchInfo.SupportedBaseVersions.Count == 0)
            {
                throw new InvalidDataException(
                    "Файл описания обновления повреждён."
                );
            }
            return patchInfo;
        }

        private static string FindApplicationDirectory(
            string packageDirectory,
            string[] args
        )
        {
            if (args != null && args.Length > 0 &&
                IsApplicationDirectory(args[0]))
            {
                return Path.GetFullPath(args[0]);
            }
            if (IsApplicationDirectory(packageDirectory))
            {
                return Path.GetFullPath(packageDirectory);
            }

            DirectoryInfo packageParent = Directory.GetParent(
                packageDirectory.TrimEnd(Path.DirectorySeparatorChar)
            );
            if (packageParent != null &&
                IsApplicationDirectory(packageParent.FullName))
            {
                return packageParent.FullName;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description =
                    "Выберите папку установленного MK3 MoveBook";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }
                if (!IsApplicationDirectory(dialog.SelectedPath))
                {
                    throw new DirectoryNotFoundException(
                        "В выбранной папке не найден MK3 MoveBook."
                    );
                }
                return Path.GetFullPath(dialog.SelectedPath);
            }
        }

        private static bool IsApplicationDirectory(string directory)
        {
            return !string.IsNullOrEmpty(directory) &&
                File.Exists(Path.Combine(directory, MainExecutableName)) &&
                File.Exists(Path.Combine(directory, "web", "version.json"));
        }

        private static void InstallPatch(
            string packageDirectory,
            string applicationDirectory,
            PatchInfo patchInfo
        )
        {
            string packageWebDirectory = Path.Combine(
                packageDirectory,
                "web"
            );
            string replacementExecutable = Path.Combine(
                packageDirectory,
                "payload",
                MainExecutableName
            );
            if (!Directory.Exists(packageWebDirectory) ||
                !File.Exists(replacementExecutable))
            {
                throw new InvalidDataException(
                    "Пакет обновления неполный."
                );
            }

            string localDataDirectory = Environment.GetEnvironmentVariable(
                LocalDataEnvironmentVariable
            );
            if (string.IsNullOrWhiteSpace(localDataDirectory))
            {
                localDataDirectory = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    ),
                    "MK3MoveBook"
                );
            }
            else
            {
                localDataDirectory = Path.GetFullPath(localDataDirectory);
            }
            string contentDirectory = Path.Combine(
                localDataDirectory,
                "Content"
            );
            string activeVersionPath = Path.Combine(
                localDataDirectory,
                "active-version.txt"
            );

            string bundledWebDirectory = Path.Combine(
                applicationDirectory,
                "web"
            );
            if (string.Equals(
                ReadContentVersion(bundledWebDirectory),
                patchInfo.TargetVersion,
                StringComparison.Ordinal
            ) && VerifyTargetFiles(
                bundledWebDirectory,
                packageDirectory,
                false
            ))
            {
                ReplaceMainExecutable(
                    replacementExecutable,
                    applicationDirectory
                );
                if (File.Exists(activeVersionPath))
                {
                    File.Delete(activeVersionPath);
                }
                return;
            }

            string baseWebDirectory = FindBaseWebDirectory(
                bundledWebDirectory,
                contentDirectory,
                activeVersionPath,
                patchInfo.SupportedBaseVersions
            );
            if (string.IsNullOrEmpty(baseWebDirectory))
            {
                throw new InvalidOperationException(
                    "Лёгкое обновление подходит только для версии " +
                    string.Join(" или ", patchInfo.SupportedBaseVersions) +
                    "."
                );
            }

            ReplaceMainExecutable(
                replacementExecutable,
                applicationDirectory
            );

            Directory.CreateDirectory(contentDirectory);
            string stagingDirectory = Path.Combine(
                contentDirectory,
                ".patch-" + Guid.NewGuid().ToString("N")
            );
            string stagedWebDirectory = Path.Combine(
                stagingDirectory,
                "web"
            );
            try
            {
                CopyDirectoryTree(baseWebDirectory, stagedWebDirectory);
                CopyDirectoryTree(
                    packageWebDirectory,
                    stagedWebDirectory
                );
                ApplyDeletedFiles(
                    stagedWebDirectory,
                    packageDirectory
                );

                if (!string.Equals(
                    ReadContentVersion(stagedWebDirectory),
                    patchInfo.TargetVersion,
                    StringComparison.Ordinal
                ) ||
                    !VerifyTargetFiles(
                        stagedWebDirectory,
                        packageDirectory,
                        true
                    ))
                {
                    throw new InvalidDataException(
                        "Проверка собранной версии не пройдена."
                    );
                }

                string versionDirectory = Path.Combine(
                    contentDirectory,
                    patchInfo.TargetVersion
                );
                if (Directory.Exists(versionDirectory))
                {
                    Directory.Delete(versionDirectory, true);
                }
                Directory.Move(stagingDirectory, versionDirectory);
                stagingDirectory = null;
                File.WriteAllText(
                    activeVersionPath,
                    patchInfo.TargetVersion,
                    new UTF8Encoding(false)
                );
            }
            finally
            {
                if (!string.IsNullOrEmpty(stagingDirectory) &&
                    Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, true);
                }
            }
        }

        private static string FindBaseWebDirectory(
            string bundledWebDirectory,
            string contentDirectory,
            string activeVersionPath,
            List<string> supportedVersions
        )
        {
            try
            {
                if (File.Exists(activeVersionPath))
                {
                    string activeVersion = File.ReadAllText(
                        activeVersionPath,
                        Encoding.UTF8
                    ).Trim();
                    string activeWebDirectory = Path.Combine(
                        contentDirectory,
                        activeVersion,
                        "web"
                    );
                    if (supportedVersions.Contains(activeVersion) &&
                        string.Equals(
                            ReadContentVersion(activeWebDirectory),
                            activeVersion,
                            StringComparison.Ordinal
                        ))
                    {
                        return activeWebDirectory;
                    }
                }
            }
            catch
            {
                // Try the bundled copy below.
            }

            string bundledVersion = ReadContentVersion(
                bundledWebDirectory
            );
            if (supportedVersions.Contains(bundledVersion))
            {
                return bundledWebDirectory;
            }
            return null;
        }

        private static void ReplaceMainExecutable(
            string replacementExecutable,
            string applicationDirectory
        )
        {
            string targetExecutable = Path.Combine(
                applicationDirectory,
                MainExecutableName
            );
            string temporaryExecutable = targetExecutable + ".update";
            File.Copy(replacementExecutable, temporaryExecutable, true);
            try
            {
                File.Copy(temporaryExecutable, targetExecutable, true);
            }
            finally
            {
                if (File.Exists(temporaryExecutable))
                {
                    File.Delete(temporaryExecutable);
                }
            }
        }

        private static string ReadContentVersion(string webDirectory)
        {
            try
            {
                string versionPath = Path.Combine(
                    webDirectory,
                    "version.json"
                );
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> values =
                    serializer.Deserialize<Dictionary<string, object>>(
                        File.ReadAllText(versionPath, Encoding.UTF8)
                    );
                object versionValue;
                if (values != null &&
                    values.TryGetValue("version", out versionValue))
                {
                    return Convert.ToString(versionValue).Trim();
                }
            }
            catch
            {
                // Invalid content is handled by the caller.
            }
            return "";
        }

        private static void ApplyDeletedFiles(
            string stagedWebDirectory,
            string packageDirectory
        )
        {
            string deletedFilesPath = Path.Combine(
                packageDirectory,
                DeletedFilesName
            );
            if (!File.Exists(deletedFilesPath))
            {
                return;
            }
            foreach (string rawPath in File.ReadAllLines(
                deletedFilesPath,
                Encoding.UTF8
            ))
            {
                string relativePath = rawPath.Trim();
                if (relativePath.Length == 0)
                {
                    continue;
                }
                string targetPath = ResolveSafeRelativePath(
                    stagedWebDirectory,
                    relativePath
                );
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
            }
        }

        private static bool VerifyTargetFiles(
            string webDirectory,
            string packageDirectory,
            bool throwOnFailure
        )
        {
            string manifestPath = Path.Combine(
                packageDirectory,
                TargetFilesName
            );
            if (!File.Exists(manifestPath))
            {
                if (throwOnFailure)
                {
                    throw new InvalidDataException(
                        "В пакете нет контрольного списка файлов."
                    );
                }
                return false;
            }

            foreach (string rawLine in File.ReadAllLines(
                manifestPath,
                Encoding.UTF8
            ))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }
                int separator = rawLine.IndexOf(' ');
                if (separator != 64)
                {
                    if (throwOnFailure)
                    {
                        throw new InvalidDataException(
                            "Контрольный список файлов повреждён."
                        );
                    }
                    return false;
                }
                string expectedHash = rawLine.Substring(0, 64);
                string relativePath = rawLine.Substring(separator + 1).Trim();
                string targetPath = ResolveSafeRelativePath(
                    webDirectory,
                    relativePath
                );
                if (!File.Exists(targetPath) ||
                    !string.Equals(
                        ComputeSha256(targetPath),
                        expectedHash,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    if (throwOnFailure)
                    {
                        throw new InvalidDataException(
                            "Не прошёл проверку файл: " + relativePath
                        );
                    }
                    return false;
                }
            }
            return true;
        }

        private static string ResolveSafeRelativePath(
            string rootDirectory,
            string relativePath
        )
        {
            string rootPath = Path.GetFullPath(rootDirectory) +
                Path.DirectorySeparatorChar;
            string targetPath = Path.GetFullPath(Path.Combine(
                rootDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar)
            ));
            if (!targetPath.StartsWith(
                rootPath,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                throw new InvalidDataException(
                    "Пакет содержит небезопасный путь."
                );
            }
            return targetPath;
        }

        private static string ComputeSha256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder result = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash)
                {
                    result.Append(value.ToString("X2"));
                }
                return result.ToString();
            }
        }

        private static void CopyDirectoryTree(
            string sourceDirectory,
            string destinationDirectory
        )
        {
            Directory.CreateDirectory(destinationDirectory);
            foreach (string sourceFile in Directory.GetFiles(sourceDirectory))
            {
                File.Copy(
                    sourceFile,
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceFile)
                    ),
                    true
                );
            }
            foreach (string sourceChild in Directory.GetDirectories(
                sourceDirectory
            ))
            {
                CopyDirectoryTree(
                    sourceChild,
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceChild)
                    )
                );
            }
        }
    }
}
