namespace MicroCommerce.Shared.Options
{
    public class IdentityOptions
    {
        public UriOptions Uri { get; set; }

        public string Audience { get; set; }

        public string[] Scopes { get; set; }

        public class UriOptions
        {
            public string Internal { get; set; }

            public string External { get; set; }
        }
    }
}
