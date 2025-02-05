﻿using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateAdventureCommand : ByAccountIdBase, ICommand
    {
        public UpdateAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateAdventureCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateAdventureCommand>
    {
        public UpdateAdventureCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateAdventureCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.HeroParser.GetAdventures(html);
            _unitOfRepository.AdventureRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new AdventureUpdated(command.AccountId), cancellationToken);
            return Result.Ok();
        }
    }
}