﻿using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.ClaimQuest.CollectRewardCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<CollectRewardCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(CollectRewardCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            HtmlDocument html;
            Result result;
            do
            {
                if (cancellationToken.IsCancellationRequested) return Result.Fail(new Cancel());
                html = chromeBrowser.Html;
                var quest = _unitOfParser.QuestParser.GetQuestCollectButton(html);

                if (quest is null)
                {
                    result = await Handle(chromeBrowser);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                    result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                    continue;
                }

                result = await chromeBrowser.Click(By.XPath(quest.XPath));
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            while (_unitOfParser.QuestParser.IsQuestClaimable(html));
            return Result.Ok();
        }

        private async Task<Result> Handle(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var firstTab = _unitOfParser.NavigationTabParser.GetTab(html, 0);
            if (firstTab is null) return Result.Fail(Retry.NotFound("tasks", "tab"));

            var firstTabActive = _unitOfParser.NavigationTabParser.IsTabActive(firstTab);

            Result result;
            if (firstTabActive)
            {
                var secondTab = _unitOfParser.NavigationTabParser.GetTab(html, 1);
                result = await chromeBrowser.Click(By.XPath(secondTab.XPath));
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                result = await chromeBrowser.Click(By.XPath(firstTab.XPath));
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            return Result.Ok();
        }
    }
}