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



            //supabase testing
            var sbUsers = await supabase.Rpc("get_users_names", null);
            GetUsersNamesEntry[] sbUserEntries = JsonConvert.DeserializeObject<GetUsersNamesEntry[]>(sbUsers.Content);

            foreach (var entry in sbUserEntries)
            {
                var user = await github.User.Get(entry.username);
                var repos = await github.Repository.GetAllForUser(entry.username);
                Console.WriteLine($"Checking {user.Name} for valid repos...");
                foreach (var repo in repos)
                {
                    System.Threading.Thread.Sleep(100);
                    var contents = await github.Repository.Content.GetAllContents(repo.Id);
                    foreach (var content in contents)
                    {
                        if (content.Name == null) continue;
                        var match = Regex.Match(content.Name, @"[a-z0-9]*.csproj", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            Console.WriteLine($"{repo.Name} is a csproj repo");
                        }
                        match = Regex.Match(content.Name, @"package.json", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            Console.WriteLine($"{repo.Name} contains a package.json. Likely an NPM project");
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