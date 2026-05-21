namespace ATS5.Application.Authority
{
    public sealed class AuthorityPermissionItem
    {
        public AuthorityPermissionItem(string tag, string name)
        {
            Tag = tag;
            Name = name;
        }

        public string Tag { get; }

        public string Name { get; }
    }
}
