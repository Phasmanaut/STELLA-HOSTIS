using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

// After every successful Web build, this window asks what's new in it and can publish it to gabrielwlogue.com:
// the text goes under the STELLA HOSTIS logo on the game page, the build files replace the ones in the
// website's site/stella-hostis/Build/, and the website's own deploy.ps1 uploads it all to the Pi.
// "Don't publish" leaves the website alone, so test builds never have to go live.
// Editor-only (it lives in an Editor folder), so none of this ends up in the game.
public class PublishWebBuildWindow : EditorWindow
{
    private const string WebsiteFolder = @"D:\Personal Projects\personal-website";
    private static string GamePage => Path.Combine(WebsiteFolder, "site", "stella-hostis", "index.html");
    private static string SiteBuildFolder => Path.Combine(WebsiteFolder, "site", "stella-hostis", "Build");

    //The paragraph under the logo on the game page
    private static readonly Regex WhatsNew = new Regex(@"(<p\b[^>]*\bid=""whats-new""[^>]*>)([\s\S]*?)(</p>)");

    //The four files a Web build is made of, matched the same way deploy.ps1 matches them
    private static readonly string[] BuildFilePatterns = { "*.loader.js", "*.data*", "*.framework.js*", "*.wasm*" };

    //Kept serialized so the window survives scripts recompiling while it's open
    [SerializeField] private string[] buildFiles;
    [SerializeField] private string currentText;
    [SerializeField] private string newText = "";
    [SerializeField] private string[] recentCommits;
    private Vector2 commitScroll;
    private GUIStyle textBoxStyle;

    //Hooks into every build. Only Web builds that finished get the window
    public class BuildHook : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) return;

            string buildFolder = Path.Combine(report.summary.outputPath, "Build");
            EditorApplication.delayCall += () => //wait until the build has fully finished before opening a window
            {
                BuildResult result = report != null ? report.summary.result : BuildResult.Unknown;
                if (result == BuildResult.Failed || result == BuildResult.Cancelled) return;
                Open(buildFolder);
            };
        }
    }

    static void Open(string buildFolder)
    {
        string[] files = FindBuildFiles(buildFolder);
        if (files == null)
        {
            Debug.LogWarning($"[Publish] Didn't find all four build files in {buildFolder}, so there's nothing to publish.");
            return;
        }

        string current = ReadWhatsNew();
        if (current == null)
        {
            EditorUtility.DisplayDialog("Publish to website",
                $"Couldn't find the paragraph under the logo (the one with id=\"whats-new\") in:\n{GamePage}\n\nThe build was not published.", "OK");
            return;
        }

        PublishWebBuildWindow window = GetWindow<PublishWebBuildWindow>(true, "Publish to website", true);
        window.buildFiles = files;
        window.currentText = current;
        window.newText = "";
        window.recentCommits = CommitsSince(LastPublishedTime());
        window.minSize = new Vector2(480f, 380f);
    }

    void OnGUI()
    {
        if (buildFiles == null) { Close(); return; }
        if (textBoxStyle == null) textBoxStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

        EditorGUILayout.LabelField("What's new in this build?", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Shown under the STELLA HOSTIS logo on gabrielwlogue.com/stella-hostis. Leave it empty to keep the current text.",
            EditorStyles.wordWrappedMiniLabel);
        newText = EditorGUILayout.TextArea(newText, textBoxStyle, GUILayout.MinHeight(70f));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("On the site now", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(currentText, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Commits since the last published build", EditorStyles.boldLabel);
        commitScroll = EditorGUILayout.BeginScrollView(commitScroll);
        if (recentCommits.Length == 0) EditorGUILayout.LabelField("None found.", EditorStyles.wordWrappedMiniLabel);
        foreach (string commit in recentCommits) EditorGUILayout.LabelField("• " + commit, EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField("Changes that aren't committed yet won't be listed here.", EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Don't publish", GUILayout.Height(28f))) Close();
            GUILayout.FlexibleSpace();
            string publishLabel = string.IsNullOrWhiteSpace(newText) ? "Publish (keep current text)" : "Publish";
            if (GUILayout.Button(publishLabel, GUILayout.Width(200f), GUILayout.Height(28f))) Publish();
        }
    }

    void Publish()
    {
        try
        {
            ReplaceSiteBuild();
            if (!string.IsNullOrWhiteSpace(newText)) WriteWhatsNew(newText.Trim());
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Publish to website", "Couldn't update the website folder:\n" + e.Message, "OK");
            return;
        }

        try
        {
            RunDeploy();
            Debug.Log("[Publish] Website folder updated. deploy.ps1 is uploading it in the PowerShell window.");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Publish to website",
                "The website folder is updated, but deploy.ps1 couldn't be started:\n" + e.Message + "\n\nRun it by hand from " + WebsiteFolder, "OK");
        }
        Close();
    }

    //The newest file of each kind, so leftovers from an older build in the same folder are never picked up. Null if any is missing
    static string[] FindBuildFiles(string buildFolder)
    {
        if (!Directory.Exists(buildFolder)) return null;

        string[] files = BuildFilePatterns
            .Select(pattern => new DirectoryInfo(buildFolder).GetFiles(pattern).OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault())
            .Select(file => file?.FullName)
            .ToArray();
        return files.Contains(null) ? null : files;
    }

    //Empties the website's Build folder and copies the new build in. Copies keep their timestamps,
    //which is how the next publish knows when this one was built
    void ReplaceSiteBuild()
    {
        Directory.CreateDirectory(SiteBuildFolder);
        foreach (string old in Directory.GetFiles(SiteBuildFolder)) File.Delete(old);
        foreach (string file in buildFiles) File.Copy(file, Path.Combine(SiteBuildFolder, Path.GetFileName(file)));
    }

    static DateTime? LastPublishedTime()
    {
        if (!Directory.Exists(SiteBuildFolder)) return null;
        FileInfo[] published = new DirectoryInfo(SiteBuildFolder).GetFiles();
        return published.Length == 0 ? (DateTime?)null : published.Max(f => f.LastWriteTimeUtc);
    }

    //Commit subjects from the game's git history, newest first. With no earlier publish, the last 10
    static string[] CommitsSince(DateTime? since)
    {
        string range = since.HasValue ? $"--since=\"{since.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "-n 10";
        try
        {
            using (Process git = Process.Start(new ProcessStartInfo("git", $"log {range} --format=%s")
            {
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            }))
            {
                string output = git.StandardOutput.ReadToEnd();
                git.WaitForExit(5000);
                return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        catch (Exception)
        {
            return new string[0]; //no git on this machine: the list is only a reminder, so do without it
        }
    }

    //The paragraph's text as plain text, or null if the page or the paragraph can't be found
    static string ReadWhatsNew()
    {
        if (!File.Exists(GamePage)) return null;
        Match match = WhatsNew.Match(File.ReadAllText(GamePage));
        if (!match.Success) return null;

        string text = Regex.Replace(match.Groups[2].Value, @"<br\s*/?>", "\n");
        text = Regex.Replace(text, "<[^>]+>", "");
        return WebUtility.HtmlDecode(text).Trim();
    }

    static void WriteWhatsNew(string text)
    {
        byte[] bytes = File.ReadAllBytes(GamePage);
        bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        string html = File.ReadAllText(GamePage);

        //Escaped, since it goes straight into the page. Line breaks in the box become line breaks on the site
        string markup = WebUtility.HtmlEncode(text).Replace("\r\n", "\n").Replace("\n", "<br>");
        html = WhatsNew.Replace(html, match => match.Groups[1].Value + markup + match.Groups[3].Value, 1);

        File.WriteAllText(GamePage, html, new UTF8Encoding(bom));
    }

    //A visible window, so the upload can be watched and any SSH error read. It waits for Enter before closing
    static void RunDeploy()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"try { & '.\\deploy.ps1' } catch { Write-Host $_ -ForegroundColor Red }; Read-Host 'Press Enter to close'\"",
            WorkingDirectory = WebsiteFolder,
            UseShellExecute = true,
        });
    }
}
