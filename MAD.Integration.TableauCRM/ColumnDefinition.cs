namespace MAD.Integration.TableauCRM
{
    public class ColumnDefinition
    {
        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public int? Scale { get; set; }

        public int? Precision { get; set; }
    }
}
