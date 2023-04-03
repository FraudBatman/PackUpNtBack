using System.Text.RegularExpressions;
using PackUpNtBack.Models;
using System.Diagnostics;

namespace PackUpNtBack
{
    public class ResponseBuilder
    {
        ulong repoId;
        PackageUpdateResponse? response;

        public ResponseBuilder(long repoID)
        {
            repoId = UInt64.Parse(repoID.ToString());
        }

        public async Task<PackageUpdateResponse> generateResponse()
        {
            response = new PackageUpdateResponse();
            response.RepoId = repoId;

            var repo = await Program.github.Repository.Get(Int64.Parse(repoId.ToString()));
            response.RepoName = repo.Name;

            var contents = await Program.github.Repository.Content.GetAllContents(repo.Id);
            foreach (var content in contents)
            {
                if (content.Name == null) continue;
                var match = Regex.Match(content.Name, @"[a-z0-9]*.csproj", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine($"{repo.Name} is a csproj repo");
                    var projectFile = await Program.github.Repository.Content.GetAllContents(repo.Id, content.Name);
                    await checkDotnet(projectFile[0]);
                }
                match = Regex.Match(content.Name, @"package.json", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine($"{repo.Name} contains a package.json. Likely an NPM project");
                    var projectFile = await Program.github.Repository.Content.GetAllContents(repo.Id, content.Name);
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
    }
}