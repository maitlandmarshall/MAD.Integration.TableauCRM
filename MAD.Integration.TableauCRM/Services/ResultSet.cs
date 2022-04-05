namespace MAD.Integration.TableauCRM.Services
{
    public class ResultSet
    {
        public IEnumerable<IDictionary<string, object>> Results { get; set; }
        public IEnumerable<ResultSetSchema> Schema { get; set; }
    }
}
