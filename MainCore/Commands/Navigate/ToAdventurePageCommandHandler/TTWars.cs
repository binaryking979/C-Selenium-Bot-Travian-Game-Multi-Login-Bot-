﻿using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate.ToAdventurePageCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<ToAdventurePageCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TTWars(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToAdventurePageCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = _unitOfParser.HeroParser.GetHeroAdventure(html);
            if (adventure is null) return Result.Fail(Retry.ButtonNotFound("hero adventure"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("adventures", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}