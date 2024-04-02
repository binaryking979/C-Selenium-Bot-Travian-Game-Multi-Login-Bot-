﻿using FluentResults;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
        protected VillageTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        public VillageId VillageId { get; protected set; }

        public void Setup(AccountId accountId, VillageId villageId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            VillageId = villageId;
            CancellationToken = cancellationToken;
        }

        protected override async Task<Result> PreExecute()
        {
            Result result;

            result = await base.PreExecute();
            if (result.IsFailed) return result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchVillageCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}