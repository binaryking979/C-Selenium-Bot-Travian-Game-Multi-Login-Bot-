﻿using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class LogoutAccountCommand : ByListBoxItemBase, IRequest
    {
        public LogoutAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class LogoutAccountCommandHandler : IRequestHandler<LogoutAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly ICommandHandler<CloseBrowserCommand> _closeBrowserCommand;

        public LogoutAccountCommandHandler(ITaskManager taskManager, IDialogService dialogService, ICommandHandler<CloseBrowserCommand> closeBrowserCommand)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _closeBrowserCommand = closeBrowserCommand;
        }

        public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(accounts.SelectedItemId);
            var status = _taskManager.GetStatus(accountId);
            switch (status)
            {
                case StatusEnums.Offline:
                    _dialogService.ShowMessageBox("Warning", "Account's browser is already closed");
                    return;

                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    _dialogService.ShowMessageBox("Warning", $"TBS is {status}. Please waiting");
                    return;

                case StatusEnums.Online:
                case StatusEnums.Paused:
                    break;

                default:
                    break;
            }

            await _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            await _closeBrowserCommand.Handle(new(accountId), cancellationToken);

            await _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}