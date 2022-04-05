namespace MAD.Integration.TableauCRM.Data
{
    public class Configuration
    {
        public int Id { get; set; }

        public string DatabaseName { get; set; }

        public string TableName { get; set; }

        public string DestinationTableName { get; set; }    
        
        public bool IsActive { get; set; }
    }
}
