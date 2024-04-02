﻿using MainCore.Common.Enums;

namespace MainCore.Common.Extensions
{
    public static class TribeExtension
    {
        public static TroopEnums GetSettle(this TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => TroopEnums.RomanSettler,
                TribeEnums.Teutons => TroopEnums.TeutonSettler,
                TribeEnums.Gauls => TroopEnums.GaulSettler,
                TribeEnums.Egyptians => TroopEnums.EgyptianSettler,
                TribeEnums.Huns => TroopEnums.HunSettler,
                _ => TroopEnums.None,
            };
        }

        public static List<HeroItemEnums> GetWeapons(this TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => new List<HeroItemEnums>() {
                        HeroItemEnums.WeaponLegionnaire1,
                        HeroItemEnums.WeaponLegionnaire2,
                        HeroItemEnums.WeaponLegionnaire3,
                        HeroItemEnums.WeaponPraetorian1,
                        HeroItemEnums.WeaponPraetorian2,
                        HeroItemEnums.WeaponPraetorian3,
                        HeroItemEnums.WeaponImperian1,
                        HeroItemEnums.WeaponImperian2,
                        HeroItemEnums.WeaponImperian3,
                        HeroItemEnums.WeaponEquitesImperatoris1,
                        HeroItemEnums.WeaponEquitesImperatoris2,
                        HeroItemEnums.WeaponEquitesImperatoris3,
                        HeroItemEnums.WeaponEquitesCaesaris1,
                        HeroItemEnums.WeaponEquitesCaesaris2,
                        HeroItemEnums.WeaponEquitesCaesaris3,
                    },
                TribeEnums.Teutons => new List<HeroItemEnums>() {
                        HeroItemEnums.WeaponClubswinger1,
                        HeroItemEnums.WeaponClubswinger2,
                        HeroItemEnums.WeaponClubswinger3,
                        HeroItemEnums.WeaponSpearman1,
                        HeroItemEnums.WeaponSpearman2,
                        HeroItemEnums.WeaponSpearman3,
                        HeroItemEnums.WeaponAxeman1,
                        HeroItemEnums.WeaponAxeman2,
                        HeroItemEnums.WeaponAxeman3,
                        HeroItemEnums.WeaponPaladin1,
                        HeroItemEnums.WeaponPaladin2,
                        HeroItemEnums.WeaponPaladin3,
                        HeroItemEnums.WeaponTeutonicKnight1,
                        HeroItemEnums.WeaponTeutonicKnight2,
                        HeroItemEnums.WeaponTeutonicKnight3,
                    },
                TribeEnums.Gauls => new List<HeroItemEnums>() {
                        HeroItemEnums.WeaponPhalanx1,
                        HeroItemEnums.WeaponPhalanx2,
                        HeroItemEnums.WeaponPhalanx3,
                        HeroItemEnums.WeaponSwordsman1,
                        HeroItemEnums.WeaponSwordsman2,
                        HeroItemEnums.WeaponSwordsman3,
                        HeroItemEnums.WeaponTheutatesThunder1,
                        HeroItemEnums.WeaponTheutatesThunder2,
                        HeroItemEnums.WeaponTheutatesThunder3,
                        HeroItemEnums.WeaponDruidrider1,
                        HeroItemEnums.WeaponDruidrider2,
                        HeroItemEnums.WeaponDruidrider3,
                        HeroItemEnums.WeaponHaeduan1,
                        HeroItemEnums.WeaponHaeduan2,
                        HeroItemEnums.WeaponHaeduan3,
                    },
                TribeEnums.Egyptians => new List<HeroItemEnums>() {
                        HeroItemEnums.WeaponSlaveMilitia1,
                        HeroItemEnums.WeaponSlaveMilitia2,
                        HeroItemEnums.WeaponSlaveMilitia3,
                        HeroItemEnums.WeaponAshWarden1,
                        HeroItemEnums.WeaponAshWarden2,
                        HeroItemEnums.WeaponAshWarden3,
                        HeroItemEnums.WeaponKhopeshWarrior1,
                        HeroItemEnums.WeaponKhopeshWarrior2,
                        HeroItemEnums.WeaponKhopeshWarrior3,
                        HeroItemEnums.WeaponAnhurGuard1,
                        HeroItemEnums.WeaponAnhurGuard2,
                        HeroItemEnums.WeaponAnhurGuard3,
                        HeroItemEnums.WeaponReshephChariot1,
                        HeroItemEnums.WeaponReshephChariot2,
                        HeroItemEnums.WeaponReshephChariot3,
                    },
                TribeEnums.Huns => new List<HeroItemEnums>() {
                        HeroItemEnums.WeaponMercenary1,
                        HeroItemEnums.WeaponMercenary2,
                        HeroItemEnums.WeaponMercenary3,
                        HeroItemEnums.WeaponBowman1,
                        HeroItemEnums.WeaponBowman2,
                        HeroItemEnums.WeaponBowman3,
                        HeroItemEnums.WeaponSteppeRider1,
                        HeroItemEnums.WeaponSteppeRider2,
                        HeroItemEnums.WeaponSteppeRider3,
                        HeroItemEnums.WeaponMarksman1,
                        HeroItemEnums.WeaponMarksman2,
                        HeroItemEnums.WeaponMarksman3,
                        HeroItemEnums.WeaponMarauder1,
                        HeroItemEnums.WeaponMarauder2,
                        HeroItemEnums.WeaponMarauder3,
                    },
                _ => new List<HeroItemEnums>(),
            };
        }
    }
}