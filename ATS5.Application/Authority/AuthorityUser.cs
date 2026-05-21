namespace ATS5.Application.Authority
{
    public sealed class AuthorityUser
    {
        public int ID { get; set; }

        public int RID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string PW { get; set; } = string.Empty;

        public AuthorityUser Clone()
        {
            return new AuthorityUser
            {
                ID = ID,
                RID = RID,
                UserName = UserName,
                PW = PW
            };
        }
    }
}
