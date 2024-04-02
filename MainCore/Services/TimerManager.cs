﻿using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using Polly;
using Timer = System.Timers.Timer;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public sealed class TimerManager : ITimerManager
    {
        private readonly Dictionary<AccountId, Timer> _timers = new();

        private bool _isShutdown = false;

        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly ILogService _logService;
        private readonly ICommandHandler<DelayTaskCommand> _delayTaskCommand;

        public TimerManager(ITaskManager taskManager, IChromeManager chromeManager, ILogService logService, ICommandHandler<DelayTaskCommand> delayTaskCommand)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _logService = logService;
            _delayTaskCommand = delayTaskCommand;
        }

        public async Task Execute(AccountId accountId)
        {
            var status = _taskManager.GetStatus(accountId);
            if (status != StatusEnums.Online) return;
            var tasks = _taskManager.GetTaskList(accountId);
            if (tasks.Count == 0) return;
            var task = tasks.First();

            if (task.ExecuteAt > DateTime.Now) return;

            var logger = _logService.GetLogger(accountId);

            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<Result>(x => x.HasError<Retry>())
                .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(5), onRetryAsync: async (error, _, retryCount, _) =>
                {
                    logger.Warning("There is something wrong.");
                    if (error.Exception is null)
                    {
                        var errors = error.Result.Reasons.Select(x => x.Message).ToList();
                        logger.Error("{errors}", string.Join(Environment.NewLine, errors));
                    }
                    else
                    {
                        var exception = error.Exception;
                        logger.Error(exception, "{message}", exception.Message);
                    }
                    logger.Warning("Retry {retryCount} for {taskName}", retryCount, task.GetName());

                    var chromeBrowser = _chromeManager.Get(accountId);
                    await chromeBrowser.Refresh(task.CancellationToken);
                });

            var taskInfo = _taskManager.GetTaskInfo(accountId);
            taskInfo.IsExecuting = true;
            task.Stage = StageEnums.Executing;
            var cts = new CancellationTokenSource();
            taskInfo.CancellationTokenSource = cts;
            task.CancellationToken = cts.Token;

            var cacheExecuteTime = task.ExecuteAt;

            logger.Information("{taskName} is started", task.GetName());
            ///===========================================================///
            var poliResult = await retryPolicy.ExecuteAndCaptureAsync(task.Handle);
            ///===========================================================///
            logger.Information("{taskName} is finished", task.GetName());

            task.Stage = StageEnums.Waiting;
            taskInfo.IsExecuting = false;
            cts.Dispose();
            taskInfo.CancellationTokenSource = null;

            if (poliResult.FinalException is not null)
            {
                logger.Warning("There is something wrong. Bot is pausing. Last exception is");
                var ex = poliResult.FinalException;
                logger.Error(ex, "{message}", ex.Message);
                await _taskManager.SetStatus(accountId, StatusEnums.Paused);
            }
            else
            {
                var result = poliResult.Result ?? poliResult.FinalHandledResult;
                if (result.IsFailed)
                {
                    var errors = result.Reasons.Select(x => x.Message).ToList();
                    logger.Warning(string.Join(Environment.NewLine, errors));

                    if (result.HasError<Stop>())
                    {
                        await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                    else if (result.HasError<Skip>())
                    {
                        if (task.ExecuteAt == cacheExecuteTime)
                        {
                            await _taskManager.Remove(accountId, task);
                        }
                    }
                    else if (result.HasError<Cancel>())
                    {
                        await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                    else if (result.HasError<Retry>())
                    {
                        await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                }
                else
                {
                    if (task.ExecuteAt == cacheExecuteTime)
                    {
                        await _taskManager.Remove(accountId, task);
                    }
                    else
                    {
                        await _taskManager.ReOrder(accountId);
                    }
                }
            }

            await _delayTaskCommand.Handle(new DelayTaskCommand(accountId), CancellationToken.None);
        }

        public void Shutdown()
        {
            _isShutdown = true;
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
        }

        public void Start(AccountId accountId)
        {
            if (!_timers.ContainsKey(accountId))
            {
                var timer = new Timer(100) { AutoReset = false };
                timer.Elapsed += async (sender, e) =>
                {
                    if (_isShutdown) return;
                    await Execute(accountId);
                    timer.Start();
                };

                _timers.Add(accountId, timer);
                timer.Start();
            }
        }
    }
}