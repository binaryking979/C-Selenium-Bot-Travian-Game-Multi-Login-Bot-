﻿using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Parsers;
using MainCore.Services;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Commands.Features
{
    public class CompleteImmediatelyCommand : ByAccountIdBase, IRequest<Result>
    {
        public CompleteImmediatelyCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class CompleteImmediatelyCommandHandler : IRequestHandler<CompleteImmediatelyCommand, Result>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public CompleteImmediatelyCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(CompleteImmediatelyCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;
            var chromeBrowser = _chromeManager.Get(accountId);

            var html = chromeBrowser.Html;

            var completeNowButton = _unitOfParser.CompleteImmediatelyParser.GetCompleteButton(html);
            if (completeNowButton is null) return Result.Fail(Retry.ButtonNotFound("complete now"));

            Result result;

            result = await chromeBrowser.Click(By.XPath(completeNowButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool confirmShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var confirmButton = _unitOfParser.CompleteImmediatelyParser.GetConfirmButton(doc);
                return confirmButton is not null;
            };

            result = await chromeBrowser.Wait(confirmShown, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var confirmButton = _unitOfParser.CompleteImmediatelyParser.GetConfirmButton(html);
            if (confirmButton is null) return Result.Fail(Retry.ButtonNotFound("complete now"));

            var oldQueueCount = _unitOfParser.QueueBuildingParser.Get(html)
                .Where(x => x.Level != -1)
                .Count();

            result = await chromeBrowser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool queueDifferent(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var newQueueCount = _unitOfParser.QueueBuildingParser.Get(doc)
                    .Where(x => x.Level != -1)
                    .Count();
                return oldQueueCount != newQueueCount;
            };
            result = await chromeBrowser.Wait(queueDifferent, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}