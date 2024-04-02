﻿using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.StartAdventure.ExploreAdventureCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<ExploreAdventureCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ExploreAdventureCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = _unitOfParser.HeroParser.GetAdventure(html);
            if (adventure is null) return Result.Fail(Retry.ButtonNotFound("adventure place"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(adventure.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool continueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = _unitOfParser.HeroParser.GetContinueButton(doc);
                return continueButton is not null;
            };

            result = await chromeBrowser.Wait(continueShow, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}