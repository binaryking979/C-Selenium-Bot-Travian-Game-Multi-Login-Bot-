﻿using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using Serilog;

namespace MainCore.Commands.General
{
    public class ChooseAccessCommand : ByAccountIdBase, ICommand<AccessDto>
    {
        public bool IgnoreSleepTime { get; }

        public ChooseAccessCommand(AccountId accountId, bool ignoreSleepTime) : base(accountId)
        {
            IgnoreSleepTime = ignoreSleepTime;
        }
    }

    [RegisterAsTransient]
    public class ChooseAccessCommandHandler : ICommandHandler<ChooseAccessCommand, AccessDto>
    {
        public AccessDto Value { get; private set; }

        private readonly UnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;
        private readonly ILogService _logService;

        public ChooseAccessCommandHandler(UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand, ILogService logService)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _logService = logService;
        }

        public async Task<Result> Handle(ChooseAccessCommand command, CancellationToken cancellationToken)
        {
            var logger = _logService.GetLogger(command.AccountId);
            var accesses = _unitOfRepository.AccountRepository.GetAccesses(command.AccountId);

            var access = await GetValidAccess(accesses, logger, cancellationToken);
            if (access is null) return Result.Fail(NoAccessAvailable.AllAccessNotWorking);

            _unitOfRepository.AccountRepository.UpdateAccessLastUsed(access.Id);

            if (accesses.Count == 1)
            {
                Value = access;
                return Result.Ok();
            }
            if (command.IgnoreSleepTime)
            {
                Value = access;
                return Result.Ok();
            }

            var minSleep = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.SleepTimeMin);

            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Result.Fail(NoAccessAvailable.LackOfAccess);

            Value = access;
            return Result.Ok();
        }

        private async Task<AccessDto> GetValidAccess(List<AccessDto> accesses, ILogger logger, CancellationToken cancellationToken)
        {
            foreach (var access in accesses)
            {
                logger.Information("Check connection {proxy}", access.Proxy);
                var result = await _unitOfCommand.ValidateProxyCommand.Handle(new(access), cancellationToken);
                if (result.IsFailed) return null;
                if (!_unitOfCommand.ValidateProxyCommand.Value)
                {
                    logger.Warning("Connection {proxy} cannot connect to travian.com", access.Proxy);
                    continue;
                }
                logger.Information("Connection {proxy} is working", access.Proxy);
                return access;
            }
            return null;
        }
    }
}