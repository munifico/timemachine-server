using Newtonsoft.Json.Converters;

namespace TimeMachineServer.Helper
{
    class JsonDateConverter : IsoDateTimeConverter
    {
        public JsonDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}