﻿using MainCore.Common.Enums;
using MainCore.Entities;
using Microsoft.EntityFrameworkCore;
using StronglyTypedIds;

[assembly: StronglyTypedIdDefaults(
    backingType: StronglyTypedIdBackingType.Int,
    converters: StronglyTypedIdConverter.SystemTextJson |
                StronglyTypedIdConverter.EfCoreValueConverter)]

namespace MainCore.Infrasturecture.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region table

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountInfo> AccountsInfo { get; set; }
        public DbSet<Access> Accesses { get; set; }
        public DbSet<AccountSetting> AccountsSetting { get; set; }
        public DbSet<HeroItem> HeroItems { get; set; }
        public DbSet<Village> Villages { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<QueueBuilding> QueueBuildings { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<VillageSetting> VillagesSetting { get; set; }
        public DbSet<Farm> FarmLists { get; set; }
        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Adventure> Adventures { get; set; }
        public DbSet<Troop> Troops { get; set; }
        public DbSet<ExpansionSlot> ExpansionSlots { get; set; }

        #endregion table

        #region account setting

        public static readonly Dictionary<AccountSettingEnums, int> AccountDefaultSettings = new()
        {
            {AccountSettingEnums.ClickDelayMin, 500 },
            {AccountSettingEnums.ClickDelayMax, 900 },
            {AccountSettingEnums.TaskDelayMin, 1000 },
            {AccountSettingEnums.TaskDelayMax, 1500 },
            {AccountSettingEnums.EnableAutoLoadVillageBuilding, 1 },
            {AccountSettingEnums.UseStartAllButton, 0 },
            {AccountSettingEnums.FarmIntervalMin, 540 },
            {AccountSettingEnums.FarmIntervalMax, 660 },
            {AccountSettingEnums.Tribe, 0 },
            {AccountSettingEnums.SleepTimeMin, 480 },
            {AccountSettingEnums.SleepTimeMax, 600 },
            {AccountSettingEnums.WorkTimeMin, 600 },
            {AccountSettingEnums.WorkTimeMax, 720 },
            {AccountSettingEnums.HeadlessChrome, 0 },
            {AccountSettingEnums.EnableAutoStartAdventure, 0 },
            {AccountSettingEnums.EnableAutoSetHeroPoint, 0 },
            {AccountSettingEnums.HeroFightingPoint, 1 },
            {AccountSettingEnums.HeroOffPoint, 0},
            {AccountSettingEnums.HeroDefPoint, 0 },
            {AccountSettingEnums.HeroResourcePoint, 3 },
            {AccountSettingEnums.EnableAutoReviveHero, 0 },
            {AccountSettingEnums.HeroRespawnVillage, 0 },
            {AccountSettingEnums.UseHeroResourceToRevive, 0 },
            {AccountSettingEnums.EquipGearBeforeStartAdventure, 0 },
            {AccountSettingEnums.HealingBeforeStartAdventure, 0 },
            {AccountSettingEnums.HealthBeforeStartAdventure, 40 },
        };

        public void FillHero()
        {
            if (Heroes.Any()) return;
            if (!Accounts.Any()) return;

            var heros = Accounts.Select(x => new Hero()
            {
                AccountId = x.Id,
                Health = 0,
                Status = 0,
            });

            Heroes.AddRange(heros);
            SaveChanges();
        }

        private List<AccountSettingEnums> GetMissingAccountSettings()
        {
            var defaultSettings = AccountDefaultSettings.Keys
                .AsEnumerable();
            var dbSettings = AccountsSetting
                .Select(x => x.Setting)
                .Distinct()
                .AsEnumerable();
            return defaultSettings.Except(dbSettings).ToList();
        }

        public void FillAccountSettings()
        {
            var missingSettings = GetMissingAccountSettings();
            if (missingSettings.Count == 0) return;

            var missingSettingsDict = missingSettings
                .Select(x => new
                {
                    Setting = x,
                    Value = AccountDefaultSettings[x],
                })
                .ToDictionary(x => x.Setting, x => x.Value);

            var ids = Accounts
                .Select(x => x.Id)
                .AsEnumerable();

            var settings = new List<AccountSetting>();

            foreach (var id in ids)
            {
                foreach (var (setting, value) in missingSettingsDict)
                {
                    settings.Add(new AccountSetting
                    {
                        AccountId = id,
                        Setting = setting,
                        Value = value,
                    });
                }
            }

            AddRange(settings);
            SaveChanges();
        }

        public void FillAccountSettings(AccountId accountId)
        {
            var settings = new List<AccountSetting>();

            foreach (var (setting, value) in AccountDefaultSettings)
            {
                settings.Add(new AccountSetting
                {
                    AccountId = accountId.Value,
                    Setting = setting,
                    Value = value,
                });
            }

            AddRange(settings);
            SaveChanges();
        }

        #endregion account setting

        #region village setting

        public static readonly Dictionary<VillageSettingEnums, int> VillageDefaultSettings = new()
        {
            {VillageSettingEnums.UseHeroResourceForBuilding, 0 },
            {VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding, 0 },
            {VillageSettingEnums.UseSpecialUpgrade, 0 },
            {VillageSettingEnums.CompleteImmediately, 0 },
            {VillageSettingEnums.Tribe, 0 },
            {VillageSettingEnums.TrainTroopEnable, 0 },
            {VillageSettingEnums.TrainTroopRepeatTimeMin, 120 },
            {VillageSettingEnums.TrainTroopRepeatTimeMax, 180 },
            {VillageSettingEnums.TrainWhenLowResource, 0 },
            {VillageSettingEnums.BarrackTroop, 0 },
            {VillageSettingEnums.BarrackAmountMin, 1 },
            {VillageSettingEnums.BarrackAmountMax, 10 },
            {VillageSettingEnums.StableTroop, 0 },
            {VillageSettingEnums.StableAmountMin, 1 },
            {VillageSettingEnums.StableAmountMax, 10 },
            {VillageSettingEnums.WorkshopTroop, 0 },
            {VillageSettingEnums.WorkshopAmountMin, 1 },
            {VillageSettingEnums.WorkshopAmountMax, 10 },

            {VillageSettingEnums.AutoNPCEnable, 0 },
            {VillageSettingEnums.AutoNPCOverflow, 0 },
            {VillageSettingEnums.AutoNPCGranaryPercent, 95 },
            {VillageSettingEnums.AutoNPCWood, 1 },
            {VillageSettingEnums.AutoNPCClay, 1 },
            {VillageSettingEnums.AutoNPCIron, 1 },
            {VillageSettingEnums.AutoNPCCrop, 0 },

            {VillageSettingEnums.AutoRefreshEnable, 0 },
            {VillageSettingEnums.AutoRefreshMin, 45 },
            {VillageSettingEnums.AutoRefreshMax, 75 },

            {VillageSettingEnums.AutoClaimQuestEnable, 0 },
            {VillageSettingEnums.CompleteImmediatelyTime, 180 },
            {VillageSettingEnums.TrainTroopBatch, 0 },
            {VillageSettingEnums.TrainTroopBatchSize, 0 },
            {VillageSettingEnums.TrainTroopWaitBuilding, 0 },
            {VillageSettingEnums.ResearchTroopWaitBuilding, 0 },
            {VillageSettingEnums.CelebrationWaitBuilding, 0 },

            {VillageSettingEnums.AutoTrainSettle, 0 },
            {VillageSettingEnums.AutoSendSettle, 0 },
        };

        private List<VillageSettingEnums> GetMissingVillageSettings()
        {
            var defaultSettings = VillageDefaultSettings.Keys
                .AsEnumerable();
            var dbSettings = VillagesSetting
                .Select(x => x.Setting)
                .Distinct()
                .AsEnumerable();
            return defaultSettings.Except(dbSettings).ToList();
        }

        public void FillVillageSettings()
        {
            var missingSettings = GetMissingVillageSettings();
            if (missingSettings.Count == 0) return;

            var missingSettingsDict = missingSettings
                .Select(x => new
                {
                    Setting = x,
                    Value = VillageDefaultSettings[x],
                })
                .ToDictionary(x => x.Setting, x => x.Value);

            var ids = Villages
                .Select(x => x.Id)
                .AsEnumerable();

            var settings = new List<VillageSetting>();

            foreach (var id in ids)
            {
                foreach (var (setting, value) in missingSettingsDict)
                {
                    settings.Add(new VillageSetting
                    {
                        VillageId = id,
                        Setting = setting,
                        Value = value,
                    });
                }
            }

            AddRange(settings);
            SaveChanges();
        }

        public void FillVillageSettings(AccountId accountId, VillageId villageId)
        {
            var settings = new List<VillageSetting>();

            foreach (var (setting, value) in VillageDefaultSettings)
            {
                if (setting == VillageSettingEnums.Tribe)
                {
                    var tribe = AccountsSetting
                        .Where(x => x.AccountId == accountId.Value)
                        .Where(x => x.Setting == AccountSettingEnums.Tribe)
                        .Select(x => x.Value)
                        .FirstOrDefault();

                    settings.Add(new VillageSetting
                    {
                        VillageId = villageId.Value,
                        Setting = setting,
                        Value = tribe,
                    });
                    continue;
                }
                settings.Add(new VillageSetting
                {
                    VillageId = villageId.Value,
                    Setting = setting,
                    Value = value,
                });
            }

            AddRange(settings);
            SaveChanges();
        }

        #endregion village setting

        public void AddVersionInfo()
        {
            Database.ExecuteSqlRaw("CREATE TABLE \"VersionInfo\" (\"Version\" INTEGER NOT NULL, \"AppliedOn\" DATETIME, \"Description\" TEXT)");

            var migrations = new List<KeyValuePair<long, string>>()
            {
                KeyValuePair.Create(202303071943,"AddHeroTable"),
                KeyValuePair.Create(202303072023,"AddAdventureTable"),
                KeyValuePair.Create(202403151103,"AddTroopTable"),
                KeyValuePair.Create(202403221509,"AddExpansionSlotTable"),
                KeyValuePair.Create(202422032353,"AddSettlerToVillageTabel"),
                KeyValuePair.Create(202403232304,"AddMaximumVillageToAccountInfoTable"),
            };
            foreach (var migration in migrations)
            {
                Database.ExecuteSqlRaw($"INSERT INTO VersionInfo (Version, Description) VALUES ('{migration.Key}','{migration.Value}')");
            }
        }
    }
}