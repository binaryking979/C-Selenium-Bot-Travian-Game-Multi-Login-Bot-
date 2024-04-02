using FluentValidation;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace MainCore.Commands.UI.Account
{
    public class AddAccountCommand : IRequest
    {
        public AddAccountCommand(AccountInput accountInput)
        {
            AccountInput = accountInput;
           
        }

        public AccountInput AccountInput { get; }
        public AccountInput Input { get; }
        public AccountInput Target { get; }
    }

    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand>
    {
        private readonly IValidator<AccountInput> _accountInputValidator;
        private readonly IDialogService _dialogService;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public AddAccountCommandHandler(IValidator<AccountInput> accountInputValidator, IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _accountInputValidator = accountInputValidator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {
            var accountInput = request.AccountInput;

            var results = _accountInputValidator.Validate(accountInput);
            
            if (!results.IsValid)
            {
                _dialogService.ShowMessageBox("Error", results.ToString());
                return;
            }
            //====================================================
            // API endpoint URL
            // API endpoint URL
            string url = "https://api.gologin.com/browser";
            string profile_id;

            string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjA1ZjYyNDliNWVjMTA0YWQ3MTM3NDQiLCJ0eXBlIjoiZGV2Iiwiand0aWQiOiI2NjA1ZjY1MDY5Y2IwZGZlMWM3YjJlYWUifQ.RGL8NFxQHwoXbw8aKo0SZDmPswScFNvyf-wj5uaK9g4";


            // Create a request object
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url);
            request1.Method = "POST";
            //request.Method = "GET";

            // Set the content type
            request1.ContentType = "application/json";
            request1.Headers.Add("Authorization", "Bearer " + bearerToken);


            // Set the request body
            //string requestBody = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            string requestBody = "{\"name\":\""+accountInput.Username+"\",\"notes\":\"\",\"role\":\"owner\",\"browserType\":\"chrome\",\"os\":\"win\"" +
                ",\"startUrl\":\"string\",\"googleServicesEnabled\":false,\"lockEnabled\":false,\"debugMode\":false" +
                ",\"navigator\":{\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.6099.110 Safari/537.36\"" +
                ",\"resolution\":\"1600x900\",\"language\":\"en-US,en;q=0.9,en-AU;q=0.8,de;q=0.7\",\"platform\":\" \",\"doNotTrack\":false,\"hardwareConcurrency\":0" +
                ",\"deviceMemory\":1,\"maxTouchPoints\":0},\"geolocation\":{\"mode\":\"prompt\",\"enabled\":true,\"customize\":true,\"fillBasedOnIp\":true,\"latitude\":0" +
                ",\"longitude\":0,\"accuracy\":10},\"canBeRunning\":true,\"runDisabledReason\":null,\"geoProxyInfo\":{},\"storage\":{\"local\":true,\"extensions\":true" +
                ",\"bookmarks\":true,\"history\":true,\"passwords\":true,\"session\":true},\"proxyRegion\":\"\",\"folders\":[],\"sharedEmails\":[]" +
                ",\"createdAt\":\"2024-03-23T04:01:07.903Z\",\"updatedAt\":\"2024-03-24T10:03:50.869Z\",\"lastActivity\":\"2024-03-23T04:01:15.194Z\",\"proxyEnabled\": false" +
                ",\"proxy\":{\"mode\":\"none\",\"host\": \"\",\"port\": 80,\"username\": \"\",\"password\":\"\",\"autoProxyRegion\":\"us\",\"torProxyRegion\":\"us\"}" +
                ",\"dns\":\"string\",\"plugins\": {\"enableVulnerable\": true,\"enableFlash\": true},\"timezone\":{\"enabled\": true,\"fillBasedOnIp\": true" +
                ",\"timezone\": \"\"},\"audioContext\": {\"mode\":\"off\",\"noise\": 0},\"canvas\":{\"mode\":\"off\",\"noise\":0},\"fonts\":{\"families\": [\"string\"]" +
                ",\"enableMasking\": true,\"enableDomRect\":true},\"mediaDevices\":{\"videoInputs\":0,\"audioInputs\": 0,\"audioOutputs\":0,\"enableMasking\":false}" +
                ",\"webRTC\": {\"mode\":\"alerted\",\"enabled\":true,\"customize\":true,\"localIpMasking\":false,\"fillBasedOnIp\": true,\"publicIp\": \"string\"" +
                ",\"localIps\": [\"string\"]},\"webGL\": {\"mode\": \"noise\",\"getClientRectsNoise\":0,\"noise\":0},\"clientRects\":{\"mode\":\"noise\",\"noise\":0}" +
                ",\"webGLMetadata\":{\"mode\":\"mask\",\"vendor\":\"string\",\"renderer\":\"string\"},\"webglParams\":[],\"profile\":\"string\",\"googleClientId\":\"string\"" +
                ",\"updateExtensions\":true,\"chromeExtensions\":[],\"userChromeExtensions\":[],\"tags\":[],\"permissions\":{\"transferProfile\":false" +
                ",\"transferToMyWorkspace\":false,\"shareProfile\":true,\"manageFolders\":true,\"editProfile\":true,\"deleteProfile\":true,\"cloneProfile\":true" +
                ",\"exportProfile\":false,\"updateUA\":true,\"addVpnUfoProxy\":true,\"runProfile\":true,\"viewProfile\":true,\"addProfileTag\":true,\"removeProfileTag\": true" +
                ",\"viewShareLinks\": true,\"createShareLinks\": true,\"updateShareLinks\": true,\"deleteShareLinks\":true},\"isBookmarksSynced\": true,\"autoLang\": true" +
                ",\"facebookAccountData\":{\"notParsedData\":{}}}";

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

                            profile_id = root.GetProperty("id").GetString();


                        }
                    }
                }
            }


            
           
            //===================================================
            //accountInput.ProfileId = profile_id;

            await _waitingOverlayViewModel.Show("adding account");
            
            var dto = accountInput.ToDto();
            //ring str1 = dto.ProfileID;
            dto.ProfileID  = profile_id;
            //_dialogService.ShowMessageBox("Error", str1);
            var success = await Task.Run(() => _unitOfRepository.AccountRepository.Add(dto));
            if (success) await _mediator.Publish(new AccountUpdated(), cancellationToken);

            await _waitingOverlayViewModel.Hide();
            _dialogService.ShowMessageBox("Information", success ? "Added account" : "Account is duplicated");
        }
    }
}