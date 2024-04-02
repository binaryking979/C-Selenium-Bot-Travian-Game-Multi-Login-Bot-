﻿using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using System.Net;
using System.Text.Json;

namespace MainCore.Commands.UI.MainLayout
{
    public class LoginAccountCommand : ByListBoxItemBase, IRequest
    {
        public LoginAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;
        private readonly IDialogService _dialogService;

        private readonly UnitOfRepository _unitOfRepository;

        private readonly ICommandHandler<ChooseAccessCommand, AccessDto> _chooseAccessCommand;
        private readonly ICommandHandler<OpenBrowserCommand> _openBrowserCommand;
        private readonly ICommandHandler<CloseBrowserCommand> _closeBrowserCommand;
        private readonly ILogService _logService;
        private readonly IMediator _mediator;

        public LoginAccountCommandHandler(ITaskManager taskManager, ITimerManager timerManager, IDialogService dialogService, UnitOfRepository unitOfRepository, ICommandHandler<ChooseAccessCommand, AccessDto> chooseAccessCommand, ICommandHandler<OpenBrowserCommand> openBrowserCommand, ILogService logService, IMediator mediator, ICommandHandler<CloseBrowserCommand> closeBrowserCommand)
        {
            _taskManager = taskManager;
            _timerManager = timerManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
            _chooseAccessCommand = chooseAccessCommand;
            _openBrowserCommand = openBrowserCommand;
            _logService = logService;
            _mediator = mediator;
            _closeBrowserCommand = closeBrowserCommand;
        }

        public async Task Handle(LoginAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(accounts.SelectedItemId);
            var account = _unitOfRepository.AccountRepository.Get(accountId, true);
            //================
            //var account = _unitOfRepository.AccountRepository.Get(accountId, true);
            //  _dialogService.ShowMessageBox("user name", account.ProfileID);

            var tribe = (TribeEnums)_unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                _dialogService.ShowMessageBox("Warning", "Choose tribe first");
                return;
            }

            if (_taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account's browser is already opened");
                return;
            }

            await _taskManager.SetStatus(accountId, StatusEnums.Starting);
            var logger = _logService.GetLogger(accountId);

            Result result;
            result = await _chooseAccessCommand.Handle(new(accountId, false), cancellationToken);

            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{errors}", string.Join(Environment.NewLine, errors));

                await _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }

            //===========================
            // API endpoint URL
            /*
            string url = "http://localhost:36912/browser/start-profile";
            string profile_id;
            string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjA1ZjYyNDliNWVjMTA0YWQ3MTM3NDQiLCJ0eXBlIjoiZGV2Iiwiand0aWQiOiI2NjA1ZjY1MDY5Y2IwZGZlMWM3YjJlYWUifQ.RGL8NFxQHwoXbw8aKo0SZDmPswScFNvyf-wj5uaK9g4";
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url);
            request1.Method = "POST";
            request1.ContentType = "application/json";
            string requestBody = "{\"profileId\":\"" + account.ProfileID + "\",\"sync\":\"true\"}";
            byte[] requestBodyBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);
            request1.ContentLength = requestBodyBytes.Length;

            // Write the request body
            using (Stream requestStream = request1.GetRequestStream())
            {
                requestStream.Write(requestBodyBytes, 0, requestBodyBytes.Length);
            }

            // Get the response
            using (HttpWebResponse response = (HttpWebResponse)request1.GetResponse())
            {
                // Read the response body
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string responseJson = reader.ReadToEnd();

                        using (JsonDocument document = JsonDocument.Parse(responseJson))
                        {
                            // Access specific fields from the JSON data
                            JsonElement root = document.RootElement;

                            //profile_id = root.GetProperty("id").GetString();


                        }
                    }
                }
            }
            */
            //============================
            var access = _chooseAccessCommand.Value;
            logger.Information("Using connection {proxy} to start chrome", access.Proxy);
            result = await _openBrowserCommand.Handle(new(accountId, access), cancellationToken);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                var errors = result.Errors.Select(x => x.Message).ToList();
                logger.Error("{errors}", string.Join(Environment.NewLine, errors));
                await _taskManager.SetStatus(accountId, StatusEnums.Offline);

                await _closeBrowserCommand.Handle(new(accountId), cancellationToken);

                return;
            }
            await _mediator.Publish(new AccountInit(accountId), cancellationToken);

            _timerManager.Start(accountId);
            await _taskManager.SetStatus(accountId, StatusEnums.Online);
            //_dialogService.ShowMessageBox("Error",
        }
    }
}