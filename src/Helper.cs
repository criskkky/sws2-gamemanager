using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace GameManager;

public static class Helper
{
  public static string[] RadioArray = [
      "coverme",
        "takepoint",
        "holdpos",
        "regroup",
        "followme",
        "takingfire",
        "go",
        "fallback",
        "sticktog",
        "getinpos",
        "stormfront",
        "report",
        "roger",
        "enemyspot",
        "needbackup",
        "sectorclear",
        "inposition",
        "reportingin",
        "getout",
        "negative",
        "enemydown",
        "sorry",
        "cheer",
        "compliment",
        "thanks",
        "go_a",
        "go_b",
        "needrop",
        "deathcry"
  ];
  public static string[] MoneyMessageArray = [
      "#Player_Cash_Award_Kill_Teammate",
        "#Player_Cash_Award_Killed_VIP",
        "#Player_Cash_Award_Killed_Enemy_Generic",
        "#Player_Cash_Award_Killed_Enemy",
        "#Player_Cash_Award_Bomb_Planted",
        "#Player_Cash_Award_Bomb_Defused",
        "#Player_Cash_Award_Rescued_Hostage",
        "#Player_Cash_Award_Interact_Hostage",
        "#Player_Cash_Award_Respawn",
        "#Player_Cash_Award_Get_Killed",
        "#Player_Cash_Award_Damage_Hostage",
        "#Player_Cash_Award_Kill_Hostage",
        "#Player_Point_Award_Killed_Enemy",
        "#Player_Point_Award_Killed_Enemy_Plural",
        "#Player_Point_Award_Killed_Enemy_NoWeapon",
        "#Player_Point_Award_Killed_Enemy_NoWeapon_Plural",
        "#Player_Point_Award_Assist_Enemy",
        "#Player_Point_Award_Assist_Enemy_Plural",
        "#Player_Point_Award_Picked_Up_Dogtag",
        "#Player_Point_Award_Picked_Up_Dogtag_Plural",
        "#Player_Team_Award_Killed_Enemy",
        "#Player_Team_Award_Killed_Enemy_Plural",
        "#Player_Team_Award_Bonus_Weapon",
        "#Player_Team_Award_Bonus_Weapon_Plural",
        "#Player_Team_Award_Picked_Up_Dogtag",
        "#Player_Team_Award_Picked_Up_Dogtag_Plural",
        "#Player_Team_Award_Picked_Up_Dogtag_Friendly",
        "#Player_Cash_Award_ExplainSuicide_YouGotCash",
        "#Player_Cash_Award_ExplainSuicide_TeammateGotCash",
        "#Player_Cash_Award_ExplainSuicide_EnemyGotCash",
        "#Player_Cash_Award_ExplainSuicide_Spectators",
        "#Team_Cash_Award_T_Win_Bomb",
        "#Team_Cash_Award_Elim_Hostage",
        "#Team_Cash_Award_Elim_Bomb",
        "#Team_Cash_Award_Win_Time",
        "#Team_Cash_Award_Win_Defuse_Bomb",
        "#Team_Cash_Award_Win_Hostages_Rescue",
        "#Team_Cash_Award_Win_Hostage_Rescue",
        "#Team_Cash_Award_Loser_Bonus",
        "#Team_Cash_Award_Bonus_Shorthanded",
        "#Notice_Bonus_Enemy_Team",
        "#Notice_Bonus_Shorthanded_Eligibility",
        "#Notice_Bonus_Shorthanded_Eligibility_Single",
        "#Team_Cash_Award_Loser_Bonus_Neg",
        "#Team_Cash_Award_Loser_Zero",
        "#Team_Cash_Award_Rescued_Hostage",
        "#Team_Cash_Award_Hostage_Interaction",
        "#Team_Cash_Award_Hostage_Alive",
        "#Team_Cash_Award_Planted_Bomb_But_Defused",
        "#Team_Cash_Award_Survive_GuardianMode_Wave",
        "#Team_Cash_Award_CT_VIP_Escaped",
        "#Team_Cash_Award_T_VIP_Killed",
        "#Team_Cash_Award_no_income",
        "#Team_Cash_Award_no_income_suicide",
        "#Team_Cash_Award_Generic",
        "#Team_Cash_Award_Custom"
  ];
  public static string[] SavedbyArray = [
      "#Chat_SavePlayer_Savior",
        "#Chat_SavePlayer_Spectator",
        "#Chat_SavePlayer_Saved"
  ];
  public static string[] TeamWarningArray = [
      "#Cstrike_TitlesTXT_Game_teammate_attack",
        "#Cstrike_TitlesTXT_Game_teammate_kills",
        "#Cstrike_TitlesTXT_Hint_careful_around_teammates",
        "#Cstrike_TitlesTXT_Hint_try_not_to_injure_teammates",
        "#Cstrike_TitlesTXT_Killed_Teammate",
        "#SFUI_Notice_Game_teammate_kills",
        "#SFUI_Notice_Hint_careful_around_teammates",
        "#SFUI_Notice_Killed_Teammate"
  ];
  public static string[] ChickenMessageArray = [
      "#Pet_Killed"
  ];
  public static string[] DefusingBombMessageArray = [
      "#Cstrike_TitlesTXT_Defusing_Bomb"
  ];

  public static string[] PlantingBombMessageArray = [
      "#Cstrike_TitlesTXT_Planting_Bomb"
  ];

  public static readonly Dictionary<string, string[]> WeaponCategories;

  // Constructor Estático
  static Helper()
  {
    WeaponCategories = new Dictionary<string, string[]>
        {
            {"A", ["weapon_awp", "weapon_g3sg1", "weapon_scar20", "weapon_ssg08"]},
            {"B", ["weapon_ak47", "weapon_aug", "weapon_famas", "weapon_galilar", "weapon_m4a1_silencer", "weapon_m4a1", "weapon_sg556"]},
            {"C", ["weapon_m249", "weapon_negev"]},
            {"D", ["weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014"]},
            {"E", ["weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9", "weapon_p90", "weapon_ump45"]},
            {"F", ["weapon_cz75a", "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock", "weapon_hkp2000", "weapon_p250", "weapon_revolver", "weapon_tec9", "weapon_usp_silencer"]},
            {"G", ["weapon_smokegrenade", "weapon_hegrenade", "weapon_flashbang", "weapon_decoy", "weapon_molotov", "weapon_incgrenade"]},
            {"H", ["item_defuser", "item_cutters"]},
            {"I", ["weapon_taser"]},
            {"J", ["weapon_healthshot"]},
            {"K", ["weapon_knife", "weapon_knife_t"]}
        };
    WeaponCategories["ANY"] = WeaponCategories.Values.SelectMany(x => x).ToArray();
  }

  public static HookResult FilterMessageByParams(CUserMessageTextMsg msg, IEnumerable<string> filterStrings)
  {
    foreach (var param in msg.Param)
    {
      foreach (var filter in filterStrings)
      {
        if (param.Contains(filter))
        {
          return HookResult.Stop;
        }
      }
    }
    return HookResult.Continue;
  }
}