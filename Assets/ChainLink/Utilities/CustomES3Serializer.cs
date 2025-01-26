namespace ChainLink.Serializer
{
    public static class ChainLinkSerializer
    {
        public static string ToJson<T>(T obj)
        {
            return System.Text.Encoding.UTF8.GetString(ES3.Serialize<T>(obj));
        }

        public static T FromJson<T>(string json)
        {
            return ES3.Deserialize<T>(System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}