using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Turkey
{
    public class DotNet
    {
        public List<Version> RuntimeVersions
        {
            get
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = "--list-runtimes",
                };
                Process p = Process.Start(startInfo);
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                var list = output
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => line.StartsWith("Microsoft.NETCore.App", StringComparison.Ordinal))
                    .Select(line => line.Split(" ")[1])
                    .Select(versionString => Version.Parse(versionString))
                    .OrderBy(x => x)
                    .ToList();
                return list;
            }
        }

        public Version LatestRuntimeVersion
        {
            get
            {
                return RuntimeVersions.Last();
            }
        }

        public List<Version> SdkVersions
        {
            get
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "dotnet",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Arguments = "--list-sdks",
                    };
                Process p = Process.Start(startInfo);
                p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                var list = output
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(" ")[0])
                    .Select(versionString => Version.Parse(versionString))
                    .OrderBy(x => x)
                    .ToList();
                return list;
            }
        }

        public Version LatestSdkVersion
        {
            get
            {
                return SdkVersions.LastOrDefault();
            }
        }

        public struct ProcessResult
        {
            public int ExitCode { get; }
            public string StandardOutput { get; }
            public string StandardError { get; }

            public ProcessResult(int exitCode, string stdout, string stderr)
            {
                ExitCode = exitCode;
                StandardOutput = stdout;
                StandardError = stderr;
            }
        }

        public static async Task<ProcessResult> BuildAsync(DirectoryInfo workingDirectory, CancellationToken token)
        {
            var arguments = new string[]
            {
                "build",
                "-p:UseRazorBuildServer=false",
                "-p:UseSharedCompilation=false",
                "-m:1",
            };
            var result = await RunDotNetCommandAsync(workingDirectory, arguments, token);
            return result;
        }

        public static async Task<ProcessResult> RunAsync(DirectoryInfo workingDirectory, CancellationToken token)
        {
            return await RunDotNetCommandAsync(workingDirectory, new string[] { "run", "--no-restore", "--no-build"} , token);
        }

        public static async Task<ProcessResult> TestAsync(DirectoryInfo workingDirectory, CancellationToken token)
        {
            return await RunDotNetCommandAsync(workingDirectory, new string[] { "test", "--no-restore", "--no-build"} , token);
        }

        private static async Task<ProcessResult> RunDotNetCommandAsync(DirectoryInfo workingDirectory, string[] commands, CancellationToken token)
        {
            if (workingDirectory == null)
            {
                throw new ArgumentNullException(nameof(workingDirectory));
            }

            var arguments = string.Join(" ", commands);
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = Process.Start(startInfo);
            using (StringWriter standardOutputWriter = new StringWriter())
            using (StringWriter standardErrorWriter = new StringWriter())
            {
                await process.WaitForExitAsync(standardOutputWriter, standardErrorWriter, token);
                int exitCode = exitCode = process.ExitCode;

                return new ProcessResult(exitCode, standardOutputWriter.ToString(), standardErrorWriter.ToString());
            }
        }
    }
}
