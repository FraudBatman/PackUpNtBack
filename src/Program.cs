﻿using Newtonsoft.Json;
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
            github.Credentials = new Credentials(_config.GithubSecret);

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
                //github testing
                var user = await github.User.Get(entry.username);
                Console.WriteLine("{0} has {1} public repositories - go check out their profile at {2}",
                    user.Name,
                    user.PublicRepos,
                    user.Url);
            }
            // var repo = await github.Repository.Content.GetAllContents("FraudBatman", "PackUpNtBack");

            foreach (var entry in sbUserEntries)
            {
                var user = await github.User.Get(entry.username);
                var repos = await github.Repository.GetAllForUser(entry.username);
                foreach (var repo in repos)
                {
                    System.Threading.Thread.Sleep(100);
                    var contents = await github.Repository.Content.GetAllContents(repo.Id);
                    foreach (var content in contents)
                    {
                        if (content.Content == null) continue;
                        var match = Regex.Match(content.Content, @"[a-z0-9]*.csproj", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            Console.WriteLine($"{repo} is a csproj repo");
                        }
                    }
                }
            }

        }
    }
}