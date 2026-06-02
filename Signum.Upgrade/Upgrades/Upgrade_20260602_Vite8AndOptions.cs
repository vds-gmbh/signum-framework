using Signum.Utilities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Signum.Upgrade.Upgrades;

class Upgrade_20260602_Vite8AndOptions : CodeUpgradeBase
{
    public override string Description => "Update to Vite 8 and replace direct properties with .Options pattern";

    public override void Execute(UpgradeContext uctx)
    {
        uctx.ChangeCodeFile(@"Southwind.Server/package.json", file =>
        {
            file.UpdateNpmPackage("sass", "1.100.0");
            file.UpdateNpmPackage("cross-env", "10.1.0");
            file.UpdateNpmPackage("vite", "8.0.15");
            file.UpdateNpmPackage("@vitejs/plugin-react", "6.0.2");
        });

        uctx.ChangeCodeFile(@"Southwind.Server/vite.config.js", file =>
        {
            file.ProcessLines(lines =>
            {
                var fromIdx = lines.FindIndex(a => a.Contains("manualChunks"));
                if (fromIdx == -1)
                    return false;

                var toIdx = lines.FindIndex(fromIdx, a => a.Trim() == "}");
                if (toIdx == -1)
                    return false;

                var arrayStart = lines.FindIndex(fromIdx, a => a.Contains("["));
                var arrayEnd = lines.FindIndex(arrayStart, a => a.Trim() == "]");

                var jsonArray = "[" + string.Join(",",
                    lines.GetRange(arrayStart + 1, arrayEnd - arrayStart - 1)
                         .Select(l => l.Trim())
                         .Where(l => !l.StartsWith("//") && l.Length > 0)
                         .Select(l => l.Replace("'", "\"").TrimEnd(','))) + "]";

                var packages = JsonSerializer.Deserialize<string[]>(jsonArray)!;

                var regexParts = string.Join("|", packages.Select(p => Regex.Escape(p).Replace("/", "[\\\\/]")));
                var indent = CodeFile.GetIndent(lines[fromIdx]);

                lines.RemoveRange(fromIdx, toIdx - fromIdx + 1);

                var newLines = new[]
                {
                    "codeSplitting: {",
                    "  groups: [",
                    "    {",
                    "      name: 'vendor',",
                    $"      test: /node_modules[\\\\/]({regexParts})/,",
                    "      priority: 10,",
                    "    },",
                    "  ],",
                    "},",
                }.Select(l => l.Indent(indent)).ToList();

                lines.InsertRange(fromIdx, newLines);
                return true;
            });
        });

        uctx.ForeachCodeFile(@"*.tsx", file =>
        {
            file.Replace("AppContext.Expander.onGetExpanded", "AppContext.Expander.Options.onGetExpanded");
            file.Replace("AppContext.Expander.onSetExpanded", "AppContext.Expander.Options.onSetExpanded");
            file.Replace("Services.NotifyPendingFilter.notifyPendingRequests", "Services.NotifyPendingFilter.Options.notifyPendingRequests");
            file.Replace("Notify.singleton", "Notify.getSingleton()");
            file.Replace("NumberFormatSettings.defaultNumberFormatLocale", "NumberFormatSettings.Options.defaultNumberFormatLocale");
            file.Replace("Services.VersionFilter.versionHasChanged", "Services.VersionFilter.Options.versionHasChanged");
            file.Replace("AuthClient.validatePassword", "AuthClient.Options.validatePassword");
        });

        SafeConsole.WriteLineColor(ConsoleColor.Magenta, "Remember to:");
        SafeConsole.WriteLineColor(ConsoleColor.DarkMagenta, "Run yarn install after this upgrade");
    }
}
