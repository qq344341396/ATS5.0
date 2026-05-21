using System;
using ATS5.Application.Authority;
using Xunit;

namespace ATS5.Tests.Authority
{
    public sealed class AuthorityPermissionSerializerTests
    {
        [Fact]
        public void SerializeTags_UsesLegacyJsonStringArrayFormat()
        {
            var json = AuthorityPermissionSerializer.SerializeTags(new[] { "1001", "1005" });

            Assert.Equal("[\"1001\",\"1005\"]", json);
        }

        [Fact]
        public void ParseTags_ReadsLegacyPowersJson_AndReturnsEmptyForInvalidInput()
        {
            Assert.Equal(new[] { "1002", "1008" }, AuthorityPermissionSerializer.ParseTags("[\"1002\",\"1008\"]"));
            Assert.Empty(AuthorityPermissionSerializer.ParseTags(null));
            Assert.Empty(AuthorityPermissionSerializer.ParseTags(string.Empty));
            Assert.Empty(AuthorityPermissionSerializer.ParseTags("not json"));
        }
    }
}
