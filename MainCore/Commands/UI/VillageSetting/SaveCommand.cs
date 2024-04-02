﻿using FluentValidation;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.VillageSetting
{
    public class SaveCommand : ByAccountVillageIdBase, IRequest
    {
        public SaveCommand(AccountId accountId, VillageId villageId, VillageSettingInput villageSettingInput) : base(accountId, villageId)
        {
            VillageSettingInput = villageSettingInput;
        }

        public VillageSettingInput VillageSettingInput { get; }
    }

    public class SaveCommandHandler : IRequestHandler<SaveCommand>
    {
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public SaveCommandHandler(IValidator<VillageSettingInput> villageSettingInputValidator, UnitOfRepository unitOfRepository, IDialogService dialogService, IMediator mediator)
        {
            _villageSettingInputValidator = villageSettingInputValidator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _mediator = mediator;
        }

        public async Task Handle(SaveCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageSettingInput = request.VillageSettingInput;
            var result = _villageSettingInputValidator.Validate(villageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var villageId = request.VillageId;
            var settings = villageSettingInput.Get();
            await Task.Run(() => _unitOfRepository.VillageSettingRepository.Update(villageId, settings), cancellationToken);
            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings saved");
        }
    }
}