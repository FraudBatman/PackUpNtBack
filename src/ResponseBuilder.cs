using System.Text.RegularExpressions;
using PackUpNtBack.Models;
using System.Diagnostics;

namespace PackUpNtBack
{
    public class ResponseBuilder
    {
        ulong repoId;
        public string repoName;
        public PackageUpdateResponse? response;
        public Package[] packages;
        public bool valid = false;
        public static ulong? lastResponseId;

        public ResponseBuilder(long repoID)
        {
            repoId = UInt64.Parse(repoID.ToString());
        }

        public async Task<PackageUpdateResponse> generateResponse()
        {
            response = new PackageUpdateResponse();
            response.RepoId = repoId;

            var repo = await Program.github.Repository.Get(Int64.Parse(repoId.ToString()));
            repoName = repo.Name;

            var contents = await Program.github.Repository.Content.GetAllContents(repo.Id);
            foreach (var content in contents)
            {
                if (content.Name == null) continue;
                var match = Regex.Match(content.Name, @"[a-z0-9]*.csproj", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine($"{repo.Name} is a csproj repo");
                    response.RepoType = ".NET";
                    var projectFile = await Program.github.Repository.Content.GetAllContents(repo.Id, content.Name);
                    await checkDotnet(projectFile[0]);
                }
                match = Regex.Match(content.Name, @"package.json", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine($"{repo.Name} contains a package.json. Likely an NPM project");
                    response.RepoType = "NPM";
                    var projectFile = await Program.github.Repository.Content.GetAllContents(repo.Id, content.Name);
                    var lockFile = await Program.github.Repository.Content.GetAllContents(repo.Id, "package-lock.json");
                    await checkNPM(projectFile[0], lockFile[0]);
                }
            }

            return response;
        }

        private async Task checkDotnet(Octokit.RepositoryContent projectFile)
        {
            //create temp directory
            Directory.CreateDirectory("temp");
            File.WriteAllText("temp/" + projectFile.Name, projectFile.Content);

            //dotnet restore for updates
            var restorePsi = new ProcessStartInfo();
            restorePsi.FileName = "dotnet";
            restorePsi.Arguments = "restore";
            restorePsi.WorkingDirectory = "temp";
            var restoreProc = Process.Start(restorePsi);
            restoreProc.WaitForExit();

            //get the update file
            var psi = new ProcessStartInfo();
            psi.FileName = "dotnet";
            psi.Arguments = $"list {projectFile.Name} package --outdated";
            psi.UseShellExecute = false;
            psi.WorkingDirectory = "temp";
            psi.RedirectStandardOutput = true;
            var proc = Process.Start(psi);
            var reader = proc.StandardOutput;
            string result = reader.ReadToEnd();
            //File.WriteAllText(projectFile.Name + ".txt", result); //for checking output

            //delete temp
            recursiveDelete(new DirectoryInfo("temp"));

            packages = getAllMatches(result, @"^\s*>\s*(\S+)\s+(\d+\.\d+\.\d+)\s+(\d+\.\d+\.\d+)\s+(\d+\.\d+\.\d+)$", 1, 3, 4);
            if (packages.Count() > 0)
            {
                valid = true;
            }
        }

        private async Task checkNPM(Octokit.RepositoryContent packageFile, Octokit.RepositoryContent lockFile)
        {
            //create temp directory
            Directory.CreateDirectory("temp");
            File.WriteAllText("temp/" + "package.json", packageFile.Content);
            File.WriteAllText("temp/" + "package-lock.json", lockFile.Content);

            //install the packages
            var instPsi = new ProcessStartInfo();
            instPsi.FileName = "npm";
            instPsi.Arguments = "install";
            instPsi.UseShellExecute = false;
            instPsi.WorkingDirectory = "temp";
            instPsi.RedirectStandardOutput = true;
            var instProc = Process.Start(instPsi);
            instProc.WaitForExit();

            //get the update file
            var psi = new ProcessStartInfo();
            psi.FileName = "npm";
            psi.Arguments = "outdated";
            psi.UseShellExecute = false;
            psi.WorkingDirectory = "temp";
            psi.RedirectStandardOutput = true;
            var proc = Process.Start(psi);
            var reader = proc.StandardOutput;
            string result = reader.ReadToEnd();

            //delete temp
            recursiveDelete(new DirectoryInfo("temp"));

            if (repoName == "PackUpNtFront")
            {
                Console.WriteLine(result + '\n');
            }

            packages = getAllMatches(result, @"^(\S+)\s+(\S+)\s+(\d+\.\d+\.\d+)\s+(\d+\.\d+\.\d+)\s+(\S+)\s+(\S+)$", 1, 2, 4);
            if (packages.Count() > 0)
            {
                valid = true;
            }
        }

        private static void recursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                recursiveDelete(dir);
            }
            baseDir.Delete(true);
        }

        public static Package[] getAllMatches(string input, string regexMatch, int nameGr, int repoGr, int currGr)
        {
            var lines = input.Split('\n');
            var returnValue = new List<Package>();

            foreach (var line in lines)
            {
                var match = Regex.Match(line, regexMatch);
                if (match.Success)
                {
                    returnValue.Add(new Package(match.Groups[nameGr].Value,
                                                match.Groups[repoGr].Value,
                                                match.Groups[currGr].Value));
                }
            }

            return returnValue.ToArray();
        }
    }
}