using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
namespace LinbusCompanyLocalizeCustomer
{

    public static class Logger
    {
        public enum LogLevel { Trace = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Fatal = 5 }

        private static readonly object _sync = new object();
        private static string _logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.log");
        private static LogLevel _minLevel = LogLevel.Info;

        public static void Configure(string logFilePath = null, LogLevel minLevel = LogLevel.Info)
        {
            if (!string.IsNullOrWhiteSpace(logFilePath))
            {
                _logFile = logFilePath;
            }
            _minLevel = minLevel;
            try
            {
                var dir = System.IO.Path.GetDirectoryName(_logFile);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
            }
            catch { }
        }

        public static void Info(string message) => Log(LogLevel.Info, message);
        public static void Debug(string message) => Log(LogLevel.Debug, message);
        public static void Warn(string message) => Log(LogLevel.Warn, message);
        public static void Error(string message) => Log(LogLevel.Error, message);
        public static void Fatal(string message) => Log(LogLevel.Fatal, message);

        private static void Log(LogLevel level, string message)
        {
            try
            {
                if (level < _minLevel) return;
                var line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                lock (_sync)
                {
                    File.AppendAllText(_logFile, line + System.Environment.NewLine);
                }
            }
            catch
            {
                // 捕获内部异常
            }
        }
    }
    public static class SteamGameLocator
    {
        /// <summary>
        /// 根据 Steam AppID 查找游戏安装目录，未找到则返回 null。
        /// </summary>
        public static string? FindGamePath(int appId)
        {
            string? steamPath = GetSteamPath();
            if (steamPath == null || !Directory.Exists(steamPath))
                return null;

            // 读取所有库文件夹
            var libraryPaths = GetLibraryFolders(steamPath);
            // 加入 Steam 安装目录下的默认库（steamapps 本身就是一个库）
            libraryPaths.Insert(0, steamPath);

            foreach (var libPath in libraryPaths)
            {
                string manifestFile = Path.Combine(libPath, "steamapps", $"appmanifest_{appId}.acf");
                if (!File.Exists(manifestFile))
                    continue;

                string? installDir = ParseAppManifestForInstallDir(manifestFile);
                if (string.IsNullOrEmpty(installDir))
                    continue;

                string fullPath = Path.Combine(libPath, "steamapps", "common", installDir);
                if (Directory.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }

        /// <summary>
        /// 查找 Steam 安装目录（跨平台）。
        /// </summary>
        private static string? GetSteamPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 尝试从注册表读取
                foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
                {
                    using var key = root.OpenSubKey(@"Software\Valve\Steam");
                    var steamPath = key?.GetValue("SteamPath") as string;
                    if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
                        return steamPath;
                }
                // 默认路径（32位程序在64位系统上可能被重定向，这里直接拼接常见位置）
                var defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Steam");
                if (Directory.Exists(defaultPath))
                    return defaultPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var linuxPath = Path.Combine(home, ".steam", "steam");
                if (Directory.Exists(linuxPath))
                    return linuxPath;
                // 某些发行版可能使用 ~/.local/share/Steam
                linuxPath = Path.Combine(home, ".local", "share", "Steam");
                if (Directory.Exists(linuxPath))
                    return linuxPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var macPath = Path.Combine(home, "Library", "Application Support", "Steam");
                if (Directory.Exists(macPath))
                    return macPath;
            }
            return null;
        }

        /// <summary>
        /// 解析 libraryfolders.vdf，返回所有额外库的绝对路径。
        /// </summary>
        private static List<string> GetLibraryFolders(string steamPath)
        {
            var paths = new List<string>();
            string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdfPath))
                return paths;

            string content = File.ReadAllText(vdfPath);
            // VDF 中库路径以 "path" "xxx" 形式出现
            var matches = Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\"");
            foreach (Match match in matches)
            {
                string libPath = match.Groups[1].Value;
                // 路径中可能使用双反斜杠，转换为正常路径
                libPath = libPath.Replace("\\\\", "\\");
                if (Directory.Exists(libPath))
                    paths.Add(libPath);
            }
            return paths;
        }

        /// <summary>
        /// 解析 appmanifest_xxx.acf，提取 installdir 的值。
        /// </summary>
        private static string? ParseAppManifestForInstallDir(string manifestPath)
        {
            string[] lines = File.ReadAllLines(manifestPath);
            foreach (string line in lines)
            {
                // 查找类似 "installdir" "Some Game Folder"
                var match = Regex.Match(line, "\"installdir\"\\s+\"([^\"]+)\"");
                if (match.Success)
                    return match.Groups[1].Value;
            }
            return null;
        }
    }
}
