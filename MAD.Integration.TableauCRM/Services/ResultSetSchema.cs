using System.ComponentModel.DataAnnotations.Schema;

namespace MAD.Integration.TableauCRM.Services
{
    public class ResultSetSchema
    {
        public string Name { get; set; }        
        public string System_Type_Name { get; set; }          
        public int Precision { get; set; }
        public int Scale { get; set; }
    }
}
