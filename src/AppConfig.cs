using Newtonsoft.Json;

namespace PackUpNtBack
{
    public class AppConfig
    {
        public string ApplicationName = "PackUpNt";
        public string GithubClient = "Github Client ID";
        public string GithubSecret = "Put your Github";
        public string SupabaseUrl = "";
        public string SupabaseKey = "";


        public void SaveAsJSON(StreamWriter sw)
        {
            sw.WriteLine(JsonConvert.SerializeObject(this));
            sw.Flush();
        }
    }
}