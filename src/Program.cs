using Newtonsoft.Json;
using Octokit;
using PackUpNtBack.Models;
using System.Text.RegularExpressions;

namespace PackUpNtBack
{
    public class Program
    {
        public static AppConfig _config = new AppConfig();
        public static GitHubClient? github;
        public static Supabase.Client supabase;
        public static Task Main(string[] args)
            => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            Console.Clear();
            // /*bypassing a lot of stuff for testing

            var sr = new StreamReader("PackUpNtFront.txt");
            var result = sr.ReadToEnd();
            var packageStrings = ResponseBuilder.getAllMatches(result, @"^(\S+)\s+(\S+)\s+(\d+\.\d+\.\d+)\s+(\d+\.\d+\.\d+)\s+(\S+)\s+(\S+)$", 1, 3, 4);

            return;
            //end of bypass
            // */

            //FILE CREATION

            //check that the data directory exists at all before starting to make the mandatory files
            if (!Directory.Exists("data"))
            {
                Console.WriteLine("Creating data directory...");
                Directory.CreateDirectory("data");
            }

            //check for any required files that may be missing
            if (!File.Exists("data/config.json"))
            {
                Console.WriteLine("config.json not found. Create at /data/config.json.\n"
                                + "Please update the config.json file before opening again.");
                AppConfig newConfig = new AppConfig();
                newConfig.SaveAsJSON(new StreamWriter(new FileStream("data/config.json", System.IO.FileMode.CreateNew)));
                return;
            }
            //Grab the config, since it's used in initialization
            _config = JsonConvert.DeserializeObject<AppConfig>(await File.ReadAllTextAsync("data/config.json"))!;

            github = new GitHubClient(new ProductHeaderValue("PackUpNt"));
            Console.WriteLine(GetOauthLoginUrl());
            Console.Write("Go to the link and copy the code: ");
            var code = Console.ReadLine();
            Console.Clear();

            var request = new OauthTokenRequest(_config.GithubClient, _config.GithubSecret, code);
            var token = await github.Oauth.CreateAccessToken(request);
            github.Credentials = new Credentials(token.AccessToken);



            //initialize the Supabase Client
            var sbUrl = _config.SupabaseUrl;
            var sbKey = _config.SupabaseKey;
            supabase = new Supabase.Client(sbUrl, sbKey, new Supabase.SupabaseOptions { AutoConnectRealtime = true, AutoRefreshToken = true });
            await supabase.InitializeAsync();



            // //supabase testing
            var sbUsers = await supabase.Rpc("get_users_names", null);
            GetUsersNamesEntry[] sbUserEntries = JsonConvert.DeserializeObject<GetUsersNamesEntry[]>(sbUsers.Content);
            var sbResponses = supabase.From<PackageUpdateResponse>();
            var sbPackages = supabase.From<PackUpNtBack.Models.Package>();
            var responsesResponse = await sbResponses.Get(); //easily top 5 worst variable names i've made
            ResponseBuilder.lastResponseId = responsesResponse.Models.Last().ID++;

            if (ResponseBuilder.lastResponseId == null)
            {
                throw new Exception("Last Response ID not found");
            }

            foreach (var entry in sbUserEntries)
            {
                var user = await github.User.Get(entry.username);
                var repos = await github.Repository.GetAllForUser(entry.username);
                Console.WriteLine($"Checking {user.Name} for valid repos...");
                foreach (var repo in repos)
                {
                    var builder = new ResponseBuilder(repo.Id);
                    await builder.generateResponse();
                    if (builder.valid)
                    {
                        await sbResponses.Insert(builder.response);
                        builder.response.ID = ++ResponseBuilder.lastResponseId;
                        Console.WriteLine($"{builder.repoName} | {builder.response.RepoId} | {builder.response.ID}");
                        foreach (var package in builder.packages)
                        {
                            package.ResponseId = builder.response.ID;
                            Console.WriteLine($"{package.Name} | {package.RepoVersion} < {package.CurrentVersion} | Response: {package.ResponseId}");
                            await sbPackages.Insert(package);
                        }
                    }
                }
            }
        }

        private string GetOauthLoginUrl()
        {
            // 1. Redirect users to request GitHub access
            var request = new OauthLoginRequest(_config.GithubClient)
            {
                Scopes = { "user", "notifications" },
            };
            var oauthLoginUrl = github.Oauth.GetGitHubLoginUrl(request);
            return oauthLoginUrl.ToString();
        }
    }
}