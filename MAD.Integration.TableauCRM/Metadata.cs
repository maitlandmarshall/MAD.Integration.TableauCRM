using Newtonsoft.Json;

namespace MAD.Integration.TableauCRM
{
    public class Metadata
    {
        [JsonProperty("fileFormat")]
        public FileFormat FileFormat { get; set; }

        [JsonProperty("objects")]
        public List<ObjectInfo> Objects { get; set; }
    }

    public class FileFormat
    {
        [JsonProperty("charsetName")]
        public string CharsetName { get; set; } = "UTF-8";

        [JsonProperty("fieldsDelimitedBy")]
        public string FieldsDelimitedBy { get; set; } = ",";

        [JsonProperty("fieldsEnclosedBy")]
        public string FieldsEnclosedBy { get; set; } = "\"";

        [JsonProperty("fieldsEscapedBy")]
        public string FieldsEscapedBy { get; set; } = "\"";

        [JsonProperty("numberOfLinesToIgnore")]
        public int NumberOfLinesToIgnore { get; set; }
    }

    public class ObjectInfo
    {
        [JsonProperty("connector")]
        public string Connector { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fields")]
        public List<FieldInfo> Fields { get; set; }

        [JsonProperty("fullyQualifiedName")]
        public string FullyQualifiedName { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rowLevelSecurityFilter")]
        public string RowLevelSecurityFilter { get; set; }
    }

    public class FieldInfo
    {
        [JsonProperty("canTruncate")]
        public bool CanTruncate { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("decimalSeparator")]
        public string DecimalSeparator { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty("firstDayOfWeek")]
        public int FirstDayOfWeek { get; set; }

        [JsonProperty("fiscalMonthOffset")]
        public int FiscalMonthOffset { get; set; }

        [JsonProperty("fullyQualifiedName")]
        public string FullyQualifiedName { get; set; }        

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("groupSeparator")]
        public string GroupSeparator { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }        

        [JsonProperty("isMultiValue")]
        public bool IsMultiValue { get; set; }

        [JsonProperty("isSkipped")]
        public bool IsSkipped { get; set; }

        [JsonProperty("isSystemField")]
        public bool IsSystemField { get; set; }

        [JsonProperty("isUniqueId")]
        public bool IsUniqueId { get; set; }

        [JsonProperty("isYearEndFiscalYear")]
        public bool IsYearEndFiscalYear { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("multiValueSeparator")]
        public string MultiValueSeparator { get; set; }

        [JsonProperty("precision")]
        public int Precision { get; set; }

        [JsonProperty("scale")]
        public int Scale { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
