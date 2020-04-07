using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PopulationControl
{
  public class PopulationControlSubModule : MBSubModuleBase
  {
    public const string MODULE_ID = "ipherian_population_control";

    public override void OnCampaignStart(Game game, object starterObject)
    {
      AddBehaviors(game, (CampaignGameStarter) starterObject);
    }

    public override void OnGameLoaded(Game game, object initializerObject)
    {
      AddBehaviors(game, (CampaignGameStarter) initializerObject);
    }

    private void AddBehaviors(Game game, CampaignGameStarter gameInitializer)
    {
      if (!(game.GameType is Campaign))
      {
        return;
      }
      gameInitializer.AddBehavior(new PopulationControlCampaignBehavior());
    }
  }
}
