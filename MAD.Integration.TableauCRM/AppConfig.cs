namespace MAD.Integration.TableauCRM
{
    public class AppConfig
    {
        public string ConnectionString { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string AuthEndpoint { get; set; }
        public string InstanceEndpoint { get; set; }
    }
}
