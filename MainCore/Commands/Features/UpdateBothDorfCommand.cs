﻿using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Features
{
    public class UpdateBothDorfCommand : ByAccountVillageIdBase, IRequest<Result>
    {
        public UpdateBothDorfCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class UpdateBothDorfCommandHandler : IRequestHandler<UpdateBothDorfCommand, Result>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfCommand _unitOfCommand;

        public UpdateBothDorfCommandHandler(IChromeManager chromeManager, UnitOfCommand unitOfCommand)
        {
            _chromeManager = chromeManager;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(UpdateBothDorfCommand request, CancellationToken cancellationToken)
        {
            return await Execute(request.AccountId, request.VillageId, cancellationToken);
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var url = chromeBrowser.CurrentUrl;
            Result result;
            if (url.Contains("dorf1"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 2), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 1), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 2), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 1), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, villageId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            return Result.Ok();
        }
    }
}