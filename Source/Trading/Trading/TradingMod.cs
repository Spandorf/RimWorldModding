using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Trading
{


    [StaticConstructorOnStartup]
    public static class TradeUI
    {
        private const float CountColumnWidth = 75f;

        private const float PriceColumnWidth = 100f;

        private const float AdjustColumnWidth = 240f;

        private const float DragSliderWidth = 90f;

        private const float DragSliderHeight = 25f;

        public const float TotalNumbersColumnsWidths = 590f;

        private const float AdjustArrowWidth = 30f;

        public const float ResourceIconSize = 27f;

        private static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow", true);

        private static readonly Texture2D TradeAlternativeBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.04f));

        private static readonly Texture2D SilverFlashTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.4f));

        public static readonly Color NoTradeColor = new Color(0.5f, 0.5f, 0.5f);

        public static void DrawTradeableRow(Rect rect, Tradeable trad, int index)
        {
            if (index % 2 == 1)
            {
                GUI.DrawTexture(rect, TradeUI.TradeAlternativeBGTex);
            }
            Text.Font = GameFont.Small;
            GUI.BeginGroup(rect);
            float num = rect.width;
            int num2 = trad.CountHeldBy(Transactor.Trader);
            if (num2 != 0)
            {
                Rect rect2 = new Rect(num - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                Text.Anchor = TextAnchor.MiddleRight;
                Rect rect3 = rect2;
                rect3.xMin += 5f;
                rect3.xMax -= 5f;
                Widgets.Label(rect3, num2.ToStringCached());
                TooltipHandler.TipRegion(rect2, "TraderCount".Translate());
                Rect rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleRight;
                TradeUI.DrawPrice(rect4, trad, TradeAction.PlayerBuys);
            }
            num -= 175f;
            Rect rect5 = new Rect(num - 240f, 0f, 240f, rect.height);
            TradeUI.DrawCountAdjustInterface(rect5, trad);
            num -= 240f;
            int num3 = trad.CountHeldBy(Transactor.Colony);
            if (num3 != 0)
            {
                Rect rect6 = new Rect(num - 100f, 0f, 100f, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                TradeUI.DrawPrice(rect6, trad, TradeAction.PlayerSells);
                Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
                if (Mouse.IsOver(rect7))
                {
                    Widgets.DrawHighlight(rect7);
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect rect8 = rect7;
                rect8.xMin += 5f;
                rect8.xMax -= 5f;
                Widgets.Label(rect8, num3.ToStringCached());
                TooltipHandler.TipRegion(rect7, "ColonyCount".Translate());
            }
            num -= 175f;
            Rect rect9 = new Rect(0f, 0f, num, rect.height);
            if (Mouse.IsOver(rect9))
            {
                Widgets.DrawHighlight(rect9);
            }
            Rect rect10 = new Rect(0f, 0f, 27f, 27f);
            Widgets.ThingIcon(rect10, trad.AnyThing);
            Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect11 = new Rect(80f, 0f, rect9.width - 80f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect11, trad.Label);
            Text.WordWrap = true;
            Tradeable localTrad = trad;
            TooltipHandler.TipRegion(rect9, new TipSignal(delegate
            {
                if (!localTrad.HasAnyThing)
                {
                    return string.Empty;
                }
                return localTrad.Label + ": " + localTrad.TipDescription;
            }, localTrad.GetHashCode()));
            GenUI.ResetLabelAlign();
            GUI.EndGroup();
        }

        public static void DrawIcon(float x, float y, ThingDef thingDef)
        {
            Rect rect = new Rect(x, y, 27f, 27f);
            Color color = GUI.color;
            GUI.color = thingDef.graphicData.color;
            GUI.DrawTexture(rect, thingDef.uiIcon);
            GUI.color = color;
            TooltipHandler.TipRegion(rect, new TipSignal(() => thingDef.LabelCap + ": " + thingDef.description, thingDef.GetHashCode()));
        }

        private static void DrawPrice(Rect rect, Tradeable trad, TradeAction action)
        {
            if (trad.IsCurrency)
            {
                return;
            }
            if (!trad.TraderWillTrade)
            {
                return;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            float num = trad.PriceFor(action);
            PriceType pType = PriceTypeUtlity.ClosestPriceType(num / trad.BaseMarketValue);
            switch (pType)
            {
                case PriceType.VeryCheap:
                    GUI.color = new Color(0f, 1f, 0f);
                    break;
                case PriceType.Cheap:
                    GUI.color = new Color(0.5f, 1f, 0.5f);
                    break;
                case PriceType.Normal:
                    GUI.color = Color.white;
                    break;
                case PriceType.Expensive:
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                    break;
                case PriceType.Exorbitant:
                    GUI.color = new Color(1f, 0f, 0f);
                    break;
            }
            string label = "$" + num.ToString("F2");
            Func<string> textGetter = delegate
            {
                if (!trad.HasAnyThing)
                {
                    return string.Empty;
                }
                if (!trad.TraderWillTrade)
                {
                    return "TraderWillNotTrade".Translate();
                }
                return ((action != TradeAction.PlayerBuys) ? "SellPriceDesc".Translate() : "BuyPriceDesc".Translate()) + "\n\n" + "PriceTypeDesc".Translate(new object[]
                {
                    ("PriceType" + pType).Translate()
                });
            };
            TooltipHandler.TipRegion(rect, new TipSignal(textGetter, trad.GetHashCode() * 297));
            Rect rect2 = new Rect(rect);
            rect2.xMax -= 5f;
            rect2.xMin += 5f;
            if (Text.Anchor == TextAnchor.MiddleLeft)
            {
                rect2.xMax += 300f;
            }
            if (Text.Anchor == TextAnchor.MiddleRight)
            {
                rect2.xMin -= 300f;
            }
            Widgets.Label(rect2, label);
            GUI.color = Color.white;
        }

        private static void DrawCountAdjustInterface(Rect rect, Tradeable trad)
        {
            Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f);
            if (Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f && trad.IsCurrency)
            {
                GUI.DrawTexture(rect2, TradeUI.SilverFlashTex);
            }
            if (!trad.IsCurrency)
            {
                int num = trad.CountHeldBy(Transactor.Colony) + trad.CountHeldBy(Transactor.Trader);
                float num2 = (float)num / 400f;
                if (num2 < 1f)
                {
                    num2 = 1f;
                }
                if (DragSliderManager.DragSlider(rect2, num2, new DragSliderCallback(TradeSliders.TradeSliderDraggingStarted), new DragSliderCallback(TradeSliders.TradeSliderDraggingUpdate), new DragSliderCallback(TradeSliders.TradeSliderDraggingCompleted)))
                {
                    TradeSliders.dragTrad = trad;
                    TradeSliders.dragBaseAmount = trad.countToDrop;
                    TradeSliders.dragLimitWarningGiven = false;
                }
            }
            int countToDrop = trad.countToDrop;
            string label;
            if (countToDrop > 0)
            {
                label = countToDrop.ToString();
            }
            else if (countToDrop < 0)
            {
                label = (-countToDrop).ToString();
            }
            else
            {
                GUI.color = TradeUI.NoTradeColor;
                label = "0";
            }
            if (Mouse.IsOver(rect2))
            {
                GUI.color = Color.yellow;
            }
            if (trad.IsCurrency)
            {
                GUI.color = Color.white;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect2, label);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            if (!trad.IsCurrency)
            {
                if (trad.CanSetToDropOneMore().Accepted)
                {
                    Rect rect3 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
                    if (Widgets.TextButton(rect3, "<", true, false))
                    {
                        AcceptanceReport acceptanceReport = trad.TrySetToDropOneMore();
                        if (!acceptanceReport.Accepted)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
                        }
                        else
                        {
                            SoundDefOf.TickHigh.PlayOneShotOnCamera();
                        }
                    }
                    rect3.x -= rect3.width;
                    if (Widgets.TextButton(rect3, "<<", true, false))
                    {
                        trad.SetToDropMax();
                        SoundDefOf.TickHigh.PlayOneShotOnCamera();
                    }
                }
                if (trad.CanSetToLaunchOneMore().Accepted)
                {
                    Rect rect4 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
                    if (Widgets.TextButton(rect4, ">", true, false))
                    {
                        AcceptanceReport acceptanceReport2 = trad.TrySetToLaunchOneMore();
                        if (!acceptanceReport2.Accepted)
                        {
                            Messages.Message(acceptanceReport2.Reason, MessageSound.RejectInput);
                        }
                        else
                        {
                            SoundDefOf.TickLow.PlayOneShotOnCamera();
                        }
                    }
                    rect4.x += rect4.width;
                    if (Widgets.TextButton(rect4, ">>", true, false))
                    {
                        trad.SetToLaunchMax();
                        SoundDefOf.TickLow.PlayOneShotOnCamera();
                    }
                }
            }
            if (trad.countToDrop != 0)
            {
                Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TradeUI.TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TradeUI.TradeArrow.height / 2), (float)TradeUI.TradeArrow.width, (float)TradeUI.TradeArrow.height);
                if (trad.countToDrop > 0)
                {
                    position.x += position.width;
                    position.width *= -1f;
                }
                GUI.DrawTexture(position, TradeUI.TradeArrow);
            }
            if (!trad.TraderWillTrade)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                Widgets.DrawLineHorizontal(rect2.x + rect2.width / 3f, rect2.y + rect2.height / 2f, rect2.width / 3f);
                GUI.color = Color.white;
            }
        }
    }

    public static class TradeUtility
    {
        public static bool TradeAsOne(Thing a, Thing b)
        {
            if (a.def.tradeNeverStack || b.def.tradeNeverStack)
            {
                return false;
            }
            if (a.def.category == ThingCategory.Pawn)
            {
                if (b.def != a.def)
                {
                    return false;
                }
                if (a.def.race.Humanlike)
                {
                    return false;
                }
                Pawn pawn = (Pawn)a;
                Pawn pawn2 = (Pawn)b;
                return pawn.kindDef == pawn2.kindDef && pawn.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn2.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn.gender == pawn2.gender && (pawn.Name == null || pawn.Name.Numerical) && (pawn2.Name == null || pawn2.Name.Numerical) && pawn.ageTracker.CurLifeStageIndex == pawn2.ageTracker.CurLifeStageIndex && Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - pawn2.ageTracker.AgeBiologicalYearsFloat) <= 1f;
            }
            else
            {
                if (a.def.useHitPoints && Mathf.Abs(a.HitPoints - b.HitPoints) >= 10)
                {
                    return false;
                }
                QualityCategory qualityCategory;
                QualityCategory qualityCategory2;
                if (a.TryGetQuality(out qualityCategory) && b.TryGetQuality(out qualityCategory2) && qualityCategory != qualityCategory2)
                {
                    return false;
                }
                if (a.def.category == ThingCategory.Item)
                {
                    return a.CanStackWith(b);
                }
                Log.Error(string.Concat(new object[]
                {
                    "Unknown TradeAsOne pair: ",
                    a,
                    ", ",
                    b
                }));
                return false;
            }
        }

        public static bool EverTradeable(ThingDef def)
        {
            return def.tradeability != Tradeability.Never && ((def.category == ThingCategory.Item || def.category == ThingCategory.Pawn) && def.GetStatValueAbstract(StatDefOf.MarketValue, null) > 0f);
        }

        public static float RandomPriceFactorFor(ITrader trader, Tradeable tradeable)
        {
            int num = DefDatabase<ThingDef>.AllDefsListForReading.IndexOf(tradeable.ThingDef);
            Rand.PushSeed();
            Rand.Seed = trader.RandomPriceFactorSeed * num;
            float result = Rand.Range(0.9f, 1.1f);
            Rand.PopSeed();
            return result;
        }

        public static Thing ThingFromStockMatching(ITrader trader, Thing thing)
        {
            if (thing is Pawn)
            {
                return null;
            }
            foreach (Thing current in trader.Goods)
            {
                if (TradeUtility.TradeAsOne(current, thing))
                {
                    return current;
                }
            }
            return null;
        }

        public static bool TradeableNow(Thing t)
        {
            CompRottable compRottable = t.TryGetComp<CompRottable>();
            return compRottable == null || compRottable.Stage < RotStage.Rotting;
        }

        public static void LaunchThingsOfType(ThingDef resDef, int debt, TradeShip trader)
        {
            while (debt > 0)
            {
                Thing thing = null;
                foreach (Building_OrbitalTradeBeacon current in Building_OrbitalTradeBeacon.AllPowered())
                {
                    foreach (IntVec3 current2 in current.TradeableCells)
                    {
                        foreach (Thing current3 in Find.ThingGrid.ThingsAt(current2))
                        {
                            if (current3.def == resDef)
                            {
                                thing = current3;
                                goto IL_C4;
                            }
                        }
                    }
                }
                IL_C4:
                if (thing == null)
                {
                    Log.Error("Could not find any " + resDef + " to transfer to trader.");
                    break;
                }
                int num = Math.Min(debt, thing.stackCount);
                Thing thing2 = thing.SplitOff(num);
                if (trader != null)
                {
                    trader.AddToStock(thing2);
                }
                debt -= num;
            }
        }

        public static void MakePrisonerOfColony(Pawn pawn)
        {
            if (pawn.Faction != null)
            {
                pawn.SetFaction(null, null);
            }
            pawn.guest.SetGuestStatus(Faction.OfColony, true);
            pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.Anesthetic, pawn, null), null, null);
        }

        public static void CheckInteractWithTradersTeachOpportunity(Pawn pawn)
        {
            if (pawn.Dead)
            {
                return;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && lord.CurLordToil is LordToil_DefendTraderCaravan && lord.ticksInToil > 3600)
            {
                ConceptDecider.TeachOpportunity(ConceptDefOf.InteractingWithTraders, pawn, OpportunityType.Important);
            }
        }
    }
}
