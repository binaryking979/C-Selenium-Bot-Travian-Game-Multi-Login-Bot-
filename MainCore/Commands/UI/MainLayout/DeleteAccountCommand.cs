using MainCore.Common.Enums;
using MainCore.Common.MediatR;
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
    public class DeleteAccountCommand : ByListBoxItemBase, IRequest
    {
        public DeleteAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
    {
        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        public DeleteAccountCommandHandler(IMediator mediator, UnitOfRepository unitOfRepository, IDialogService dialogService, ITaskManager taskManager)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _taskManager = taskManager;
        }

        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(accounts.SelectedItemId);
            var account = _unitOfRepository.AccountRepository.Get(accountId, true);


            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account should be offline");
                return;
            }
            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {accounts.SelectedItem.Content}");
            if (!result) return;

            await Task.Run(() => _unitOfRepository.AccountRepository.Delete(accountId), cancellationToken);
            //=======================
            // API endpoint URL
            string url = "https://api.gologin.com/browser";
            string profile_id;
            string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjA1ZjYyNDliNWVjMTA0YWQ3MTM3NDQiLCJ0eXBlIjoiZGV2Iiwiand0aWQiOiI2NjA1ZjY1MDY5Y2IwZGZlMWM3YjJlYWUifQ.RGL8NFxQHwoXbw8aKo0SZDmPswScFNvyf-wj5uaK9g4";
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url + "/" + account.ProfileID);
            request1.Method = "DELETE";
            //request.Method = "GET";

            // Set the content type
            request1.ContentType = "application/json";
            request1.Headers.Add("Authorization", "Bearer " + bearerToken);
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request1.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string responseJson = reader.ReadToEnd();
                            _dialogService.ShowMessageBox("Result", "Delete");
                        }
                    }
                }
            }
            catch (WebException ex)
            {

            }
            //=======================
            await _mediator.Publish(new AccountUpdated(), cancellationToken);
        }
    }
}