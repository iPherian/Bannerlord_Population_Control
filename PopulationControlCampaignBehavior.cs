using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace PopulationControl
{
  class PopulationControlCampaignBehavior : CampaignBehaviorBase
  {
    private const string PROSPERITY_CHEAT_MENU_TITLE =
        "{=!}Change Prosperity (Cheat)";
    private const string MANAGE_POPULATION_MENU_TITLE = "{=!}Manage Population";
    private const float MIN_PROSPERITY = 1.1f;
    private const float MIN_LOYALTY = 0.0f;
    private const int MENU_TOWN_INSERT_INDEX = 5;
    private const int MENU_CASTLE_INSERT_INDEX = 3;
    private const string MODULE_ID = PopulationControlSubModule.MODULE_ID;

    public override void RegisterEvents()
    {
      CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
          this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
      CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
          this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
    }

    public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
    {
      AddGameMenus(campaignGameStarter);
    }

    private void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
    {
      try
      {
        AddManagePopulationToMenu(
            new List<Tuple<string, int>>{
                new Tuple<string, int>("town", MENU_TOWN_INSERT_INDEX),
                new Tuple<string, int>("castle", MENU_CASTLE_INSERT_INDEX)},
            campaignGameSystemStarter);
        AddProsperityCheatToMenu(
            new List<Tuple<string, int>>{
                new Tuple<string, int>("town", MENU_TOWN_INSERT_INDEX + 1),
                new Tuple<string, int>("castle", MENU_CASTLE_INSERT_INDEX + 1)},
            campaignGameSystemStarter);
      }
      catch (KeyNotFoundException e)
      {
        DisplayInfoMsg(
            "Population Control mod: Couldn't add menus. This is harmless, but you'll need to reload your save before the population management menus appear.");
      }
    }

    private void
    AddManagePopulationToMenu(IEnumerable<Tuple<string, int>> parentMenus,
                              CampaignGameStarter campaignGameSystemStarter)
    {
      foreach (Tuple<string, int> parentMenu in parentMenus)
      {
        AddManagePopulationToMenu(parentMenu, campaignGameSystemStarter);
      }
    }

    private void
    AddManagePopulationToMenu(Tuple<string, int> parentMenu,
                              CampaignGameStarter campaignGameSystemStarter)
    {
      string parentMenuId = parentMenu.Item1;
      int insertIntoParentAt = parentMenu.Item2;

      string menuId = MODULE_ID + "_" + parentMenuId + "_manage_population";
      campaignGameSystemStarter.AddGameMenuOption(
          parentMenuId, menuId, MANAGE_POPULATION_MENU_TITLE,
          new GameMenuOption.OnConditionDelegate(
              game_menu_manage_population_on_condition),
          x => GameMenu.SwitchToMenu(menuId), false, insertIntoParentAt);
      campaignGameSystemStarter.AddGameMenu(
          menuId,
          "{=!}Manage Population.\n\nPopulation is considered to be equivalent to prosperity and reducing it reduces loyalty proportionally. E.g. if you remove 500 people and this is 10% the city, 10% of loyalty will also be reduced.",
          new OnInitDelegate(game_menu_manage_population_on_init),
          GameOverlays.MenuOverlayType.SettlementWithCharacters);
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_decrease_10_percent",
          "{=!}Kick 10% of people out (decreases loyalty)", x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_manage_population_decrease_pop_10_percent_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_decrease_30_percent",
          "{=!}Kick 30% of people out (decreases loyalty)", x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_manage_population_decrease_pop_30_percent_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_decrease_200",
          "{=!}Kick 200 people out (decreases loyalty)", x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_manage_population_decrease_pop_200_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_decrease_500",
          "{=!}Kick 500 people out (decreases loyalty)", x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_manage_population_decrease_pop_500_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_change_back", "{=!}Done",
          new GameMenuOption.OnConditionDelegate(back_on_condition),
          x => GameMenu.SwitchToMenu(parentMenuId), true);
    }

    private void
    AddProsperityCheatToMenu(IEnumerable<Tuple<string, int>> parentMenus,
                             CampaignGameStarter campaignGameSystemStarter)
    {
      foreach (Tuple<string, int> parentMenu in parentMenus)
      {
        AddProsperityCheatToMenu(parentMenu, campaignGameSystemStarter);
      }
    }

    private void
    AddProsperityCheatToMenu(Tuple<string, int> parentMenu,
                             CampaignGameStarter campaignGameSystemStarter)
    {
      string parentMenuId = parentMenu.Item1;
      int insertIntoParentAt = parentMenu.Item2;

      string menuId = MODULE_ID + "_" + parentMenuId + "_prosperity_cheat";
      campaignGameSystemStarter.AddGameMenuOption(
          parentMenuId, menuId, PROSPERITY_CHEAT_MENU_TITLE,
          new GameMenuOption.OnConditionDelegate(
              game_menu_prosperity_cheat_on_condition),
          x => GameMenu.SwitchToMenu(menuId), false, insertIntoParentAt);
      campaignGameSystemStarter.AddGameMenu(
          menuId, PROSPERITY_CHEAT_MENU_TITLE,
          new OnInitDelegate(game_menu_prosperity_cheat_on_init),
          GameOverlays.MenuOverlayType.SettlementWithCharacters);
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_increase_500", "{=!}Increase by 500 (no penalty)",
          x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_prosperity_cheat_increase_prosperity_by_500_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_decrease_500", "{=!}Decrease by 500 (no penalty)",
          x => true,
          new GameMenuOption.OnConsequenceDelegate(
              game_menu_prosperity_cheat_decrease_prosperity_by_500_on_consequence));
      campaignGameSystemStarter.AddGameMenuOption(
          menuId, menuId + "_change_back", "{=!}Done",
          new GameMenuOption.OnConditionDelegate(back_on_condition),
          x => GameMenu.SwitchToMenu(parentMenuId), true);
    }

    private static bool
    game_menu_manage_population_on_condition(MenuCallbackArgs args)
    {
      return Settlement.CurrentSettlement.OwnerClan.Leader == Hero.MainHero;
    }

    private static void
    game_menu_manage_population_on_init(MenuCallbackArgs args)
    {
      Campaign.Current.GameMenuManager.MenuLocations.Clear();
    }

    private static void
    game_menu_manage_population_decrease_pop_10_percent_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperityPercent(-0.1f);
    }

    private static void
    game_menu_manage_population_decrease_pop_30_percent_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperityPercent(-0.3f);
    }

    private static void
    game_menu_manage_population_decrease_pop_200_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperity(-200.0f);
    }

    private static void
    game_menu_manage_population_decrease_pop_500_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperity(-500.0f);
    }

    private static bool
    game_menu_prosperity_cheat_on_condition(MenuCallbackArgs args)
    {
      return Game.Current.CheatMode;
    }

    private static void
    game_menu_prosperity_cheat_on_init(MenuCallbackArgs args)
    {
      Campaign.Current.GameMenuManager.MenuLocations.Clear();
    }

    private static void
    game_menu_prosperity_cheat_increase_prosperity_by_500_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperityRaw(500.0f);
    }

    private static void
    game_menu_prosperity_cheat_decrease_prosperity_by_500_on_consequence(
        MenuCallbackArgs args)
    {
      ChangeProsperityRaw(-500.0f);
    }

    private static bool back_on_condition(MenuCallbackArgs args)
    {
      args.optionLeaveType = GameMenuOption.LeaveType.Leave;
      return true;
    }

    private static void ChangeProsperityPercent(float deltaPercent)
    {
      ChangeProsperity(Settlement.CurrentSettlement.Prosperity * deltaPercent);
    }

    private static void ChangeProsperityRaw(float delta)
    {
      Settlement.CurrentSettlement.Prosperity = Math.Max(
          MIN_PROSPERITY, Settlement.CurrentSettlement.Prosperity + delta);
      DisplayInfoMsg("Prosperity has been changed to  " +
                     Settlement.CurrentSettlement.Prosperity.ToString() +
                     " in " + Settlement.CurrentSettlement.Name.ToString());
    }

    private static void ChangeProsperity(float delta)
    {
      float oldProsperity = Settlement.CurrentSettlement.Prosperity;
      float newProsperity = Math.Max(MIN_PROSPERITY, oldProsperity + delta);
      float realProsperityDelta = newProsperity - oldProsperity;

      ChangeProsperityRaw(realProsperityDelta);
      if (Settlement.CurrentSettlement.IsTown ||
          Settlement.CurrentSettlement.IsCastle)
      {
        // no loyalty change on increase in population
        float loyaltyDeltaRatio =
            Math.Min(0.0f, realProsperityDelta / oldProsperity);
        float oldLoyalty = Settlement.CurrentSettlement.Town.Loyalty;
        Settlement.CurrentSettlement.Town.Loyalty =
            Math.Max(MIN_LOYALTY, oldLoyalty + oldLoyalty * loyaltyDeltaRatio);
        DisplayInfoMsg("Loyalty has been changed to " +
                       Settlement.CurrentSettlement.Town.Loyalty + " in " +
                       Settlement.CurrentSettlement.Name.ToString());
      }
    }

    private static void DisplayInfoMsg(string msg)
    {
      InformationManager.DisplayMessage(new InformationMessage(msg));
    }

    public override void SyncData(IDataStore dataStore) {}
  }
}
