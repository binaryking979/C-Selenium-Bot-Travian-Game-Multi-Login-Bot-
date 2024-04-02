using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountDetailDto
    {
        public string Username { get; set; }
        public string Server { get; set; }
        public string ProfileID { get; set; }
        public string Password { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }

        public static AccountDetailDto Create(string username, string server, string password, string profileID)
        {
            return new AccountDetailDto()
            {
                Username = username,
                Server = server,
                Password = password,
                ProfileID = profileID,
            };
        }

        public static AccountDetailDto Create(string username, string server, string password, string proxyHost, int proxyPort,string profileID)
        {
            var dto = Create(username, server, password, profileID);
            dto.ProxyHost = proxyHost;
            dto.ProxyPort = proxyPort;
            return dto;
        }

        public static AccountDetailDto Create(string username, string server, string password, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, string profileID)
        {
            var dto = Create(username, server, password, proxyHost, proxyPort, profileID);
            dto.ProxyUsername = proxyUsername;
            dto.ProxyPassword = proxyPassword;
            return dto;
        }
    }

    [Mapper]
    public static partial class AccountDetailMapper
    {
        public static Account ToEnitty(this AccountDetailDto dto)
        {
            var account = dto.ToAccount();
            var access = dto.ToAccess();
            account.Accesses = new List<Access>() { access };
            return account;
        }

        private static partial Account ToAccount(this AccountDetailDto dto);

        private static partial Access ToAccess(this AccountDetailDto dto);
    }
}