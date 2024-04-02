﻿using MainCore.Commands.UI.Farming;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using System.Drawing;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class FarmingViewModel : AccountTabViewModelBase
    {
        public FarmListSettingInput FarmListSettingInput { get; } = new();
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;

        public ReactiveCommand<Unit, Unit> UpdateFarmList { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> ActiveFarmList { get; }
        public ReactiveCommand<Unit, Unit> Start { get; }
        public ReactiveCommand<Unit, Unit> Stop { get; }
        public ReactiveCommand<AccountId, List<ListBoxItem>> LoadFarmList { get; }
        public ReactiveCommand<AccountId, Dictionary<AccountSettingEnums, int>> LoadSetting { get; }

        private static readonly Dictionary<Color, string> _activeTexts = new()
        {
            { Color.Green , "Deactive" },
            { Color.Red , "Active" },
            { Color.Black , "No farmlist selected" },
        };

        public FarmingViewModel(IMediator mediator, UnitOfRepository unitOfRepository)

        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            UpdateFarmList = ReactiveCommand.CreateFromTask(UpdateFarmListHandler);
            Start = ReactiveCommand.CreateFromTask(StartHandler);
            Stop = ReactiveCommand.CreateFromTask(StopHandler);

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            ActiveFarmList = ReactiveCommand.CreateFromTask(ActiveFarmListHandler);

            LoadFarmList = ReactiveCommand.Create<AccountId, List<ListBoxItem>>(LoadFarmListHandler);
            LoadSetting = ReactiveCommand.Create<AccountId, Dictionary<AccountSettingEnums, int>>(LoadSettingHandler);

            LoadFarmList.Subscribe(items =>
            {
                FarmLists.Load(items);
                if (items.Count > 0)
                {
                    var color = FarmLists.SelectedItem?.Color ?? Color.Black;
                    ActiveText = _activeTexts[color];
                }
            });
            LoadSetting.Subscribe(items => FarmListSettingInput.Set(items));
            ActiveFarmList.Subscribe(x =>
            {
                var color = FarmLists.SelectedItem?.Color ?? Color.Black;
                ActiveText = _activeTexts[color];
            });
        }

        public async Task FarmListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadFarmList.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmList.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
            await LoadSetting.Execute(accountId).SubscribeOn(RxApp.TaskpoolScheduler);
        }

        private async Task UpdateFarmListHandler()
        {
            await _mediator.Send(new UpdateFarmListCommand(AccountId));
        }

        private async Task StartHandler()
        {
            await _mediator.Send(new StartFarmListCommand(AccountId));
        }

        private async Task StopHandler()
        {
            await _mediator.Send(new StopFarmListCommand(AccountId));
        }

        private async Task SaveHandler()
        {
            await _mediator.Send(new SaveFarmListSettingsCommand(AccountId, FarmListSettingInput));
        }

        private async Task ActiveFarmListHandler()
        {
            await _mediator.Send(new ActiveFarmListCommand(AccountId, FarmLists.SelectedItem));
        }

        private Dictionary<AccountSettingEnums, int> LoadSettingHandler(AccountId accountId)
        {
            var items = _unitOfRepository.AccountSettingRepository.Get(accountId);
            return items;
        }

        private List<ListBoxItem> LoadFarmListHandler(AccountId accountId)
        {
            var items = _unitOfRepository.FarmRepository.GetItems(accountId);
            return items;
        }

        private string _activeText = "No farmlist selected";

        public string ActiveText
        {
            get => _activeText;
            set => this.RaiseAndSetIfChanged(ref _activeText, value);
        }
    }
}