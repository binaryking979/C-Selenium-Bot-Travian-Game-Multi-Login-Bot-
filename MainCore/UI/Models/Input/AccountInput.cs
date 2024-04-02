using DynamicData;
using MainCore.DTO;
using MainCore.Entities;
using OpenQA.Selenium.DevTools;
using ReactiveUI;
using Riok.Mapperly.Abstractions;
using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    public class AccountInput : ReactiveObject
    {
        public AccountId Id { get; set; }
        private string _username;
        private string _server;
        private string _profileId;
        public ObservableCollection<AccessInput> Accesses = new();

        public void SetAccesses(IEnumerable<AccessInput> accesses)
        {
            Accesses.Clear();
            Accesses.AddRange(accesses);
        }

        public void Clear()
        {
            Server = "";
            Username = "";
            ProfileId = "";
            Accesses.Clear();
        }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Server
        {
            get => _server;
            set => this.RaiseAndSetIfChanged(ref _server, value);
        }
        public string ProfileId
        {
            get => this._profileId;
            set => this.RaiseAndSetIfChanged(ref _profileId,value);
        }
        public void CopyTo(AccountInput target)
        {
            target.ProfileId = ProfileId;
        }
    }

    [Mapper]
    public static partial class AccountInputMapper
    {
        public static AccountDto ToDto(this AccountInput input)
        {
            var dto = input.MapToDto();

            Uri.TryCreate(input.Server, UriKind.Absolute, out var url);
            dto.Server = url.AbsoluteUri;
            //dto.ProfileID = input.ProfileId;
            return dto;
        }

        [MapperIgnoreTarget(nameof(AccountInput.Server))]
        private static partial AccountDto MapToDto(this AccountInput input);

        public static void To(this AccountDto account, AccountInput input)
        {
            account.MapTo(input);
            input.Accesses.Clear();
            input.Accesses.AddRange(account.Accesses.Select(x => x.ToInput()));
        }

        [MapperIgnoreTarget(nameof(AccountInput.Accesses))]
        private static partial void MapTo(this AccountDto account, AccountInput input);
    }
}