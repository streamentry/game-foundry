using System;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Reads a development-only launch option. Desktop players receive
    /// <c>-name value</c> on the command line. Unity iOS players do not expose
    /// simctl launch arguments through GetCommandLineArgs, so the same option
    /// can also come from an environment variable named without the dash
    /// (for the iOS Simulator: <c>SIMCTL_CHILD_mosseloraQaTour=/path</c>).
    /// Callers stay responsible for their own development-build guards.
    /// </summary>
    internal static class DevArgs
    {
        public static string Read(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.Ordinal)) return args[i + 1];
            string value = Environment.GetEnvironmentVariable(name.TrimStart('-'));
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
