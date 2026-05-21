using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Application.Authority
{
    public sealed class AuthorityRole
    {
        public int ID { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string Powers { get; set; } = "[]";

        public string Remark { get; set; } = string.Empty;

        public IReadOnlyList<string> SelectedPermissionTags { get; set; } = Array.Empty<string>();

        public AuthorityRole Clone()
        {
            return new AuthorityRole
            {
                ID = ID,
                RoleName = RoleName,
                Powers = Powers,
                Remark = Remark,
                SelectedPermissionTags = SelectedPermissionTags.ToList()
            };
        }
    }
}
