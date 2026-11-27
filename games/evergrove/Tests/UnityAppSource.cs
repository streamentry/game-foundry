using System;
using System.IO;
using System.Linq;
using System.Text;

// The Unity client's EvergroveApp is one partial class spread over EvergroveApp*.cs files
// (checklist 20). Source-audit tests read every partial, so a symbol may live in any of them.
internal static class UnityAppSource
{
    public static string ReadAll()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string unity = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", "Unity");
                if (!Directory.Exists(unity)) continue;
                string[] files = Directory.GetFiles(unity, "EvergroveApp*.cs")
                    .OrderBy(f => Path.GetFileName(f) == "EvergroveApp.cs" ? 0 : 1)
                    .ThenBy(f => f, StringComparer.Ordinal).ToArray();
                var text = new StringBuilder();
                foreach (string file in files) text.Append(File.ReadAllText(file)).Append('\n');
                return text.ToString();
            }
        }
        throw new Exception("Could not find the Unity EvergroveApp sources from " + Directory.GetCurrentDirectory());
    }
}
