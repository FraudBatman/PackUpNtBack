namespace PackUpNtBack.Models
{
    //"[{\"user_id\":\"2841b1ea-df25-4cc0-8f0d-233bbefdba1c\",\"email\":\"eriksmith98@gmail.com\",\"username\":\"E-Smitty\"}, \n {\"user_id\":\"7c5e2ddc-9f00-4935-abfe-486e88c0b8eb\",\"email\":\"nicsonfire4@gmail.com\",\"username\":\"FraudBatman\"}, \n {\"user_id\":\"63ce1273-b518-4e5f-9c76-09139a9e4bbc\",\"email\":\"adventuredonut343@gmail.com\",\"username\":\"JPaek2000\"}]"
    public class GetUsersNamesEntry
    {
        public string user_id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
    }
}