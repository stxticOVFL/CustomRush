using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NeonLite;
using UnityEngine;
using static CustomRush.Modules.Components.Selector;
using static CustomRush.Modules.Components.Selector.Campaign;
using static LevelRush;

namespace CustomRush.Modules
{
    internal class RushManager : NeonLite.Modules.IModule
    {
#pragma warning disable CS0414
#pragma warning disable CS0660
#pragma warning disable CS0661
        const bool priority = true;
        const bool active = true;

        internal class DataHolder(UnityEngine.Object d, string id = "")
        {
            internal UnityEngine.Object data = d;
            internal string id = id;
            internal List<DataHolder> children = [];
            internal readonly HashSet<DataHolder> removals = [];

            internal static DataHolder Create(CampaignData c)
            {
                DataHolder res = new(c, c.campaignID)
                {
                    children = [.. c.missionData.Select(Create)]
                };
                return res;
            }
            internal static DataHolder Create(MissionData m)
            {
                DataHolder res = new(m, m.missionID)
                {
                    children = [.. m.levels.Select(Create)]
                };
                return res;
            }
            internal static DataHolder Create(LevelData l) => new(l, l.levelID);

            public static bool operator ==(DataHolder x, UnityEngine.Object y) => x.data == y;
            public static bool operator !=(DataHolder x, UnityEngine.Object y) => x.data != y;

            public override string ToString() => data.ToString();

            internal bool Remove(UnityEngine.Object data, string id)
            {
                if (!Contains(data))
                    return false;
                removals.Add(new(data, id));
                CustomRush.Logger.DebugMsg($"remove deep {data} from {this}");

                if (this.data == data)
                    return true;
                if (EffectiveCount == 0)
                    return true;

                foreach (var child in children)
                {
                    if (child.Remove(data, id))
                    {
                        child.removals.Clear(); // clear its removals list so the toplevel removewhere works
                        removals.RemoveWhere(x => child.Contains(x.data));
                        removals.Add(child);
                    }
                }
                return false;
            }

            bool Add(UnityEngine.Object data) => removals.RemoveWhere(x => x.data == data) != 0;

            internal bool Contains(UnityEngine.Object data)
            {
                if (removals.Any(x => x.data == data))
                    return false;
                if (this.data == data)
                    return true;
                foreach (var child in children)
                {
                    if (!removals.Contains(child) && child.Contains(data))
                        return true;
                }
                return false;
            }

            internal int EffectiveCount
            {
                get
                {
                    if (!Contains(data)) // if we don't contain ourselves we don't exist
                        return 0;
                    if (children.Count == 0)
                        return 1;
                    int res = 0;
                    foreach (var child in children)
                    {
                        if (!removals.Contains(child))
                            res += child.EffectiveCount;
                    }
                    return res;
                }
            }

            internal string GetCode()
            {
                StringBuilder sb = new(id);
                sb.Append("; ");
                foreach (var removal in removals)
                {
                    sb.Append("~");
                    sb.Append(removal.id);
                    sb.Append("; ");
                }
                return sb.ToString();
            }

            internal List<LevelData> GetLevels()
            {
                List<LevelData> res = new(EffectiveCount);
                if (res.Capacity == 0)
                    return res;
                if (children.Count == 0)
                    res.Add((LevelData)data);
                else
                {
                    foreach (var child in children)
                    {
                        if (!removals.Contains(child))
                            res.AddRange(child.GetLevels());
                    }
                }
                return res;
            }
        }

        internal static Stack<DataHolder> datas = [];
        static List<LevelData> levels;

        static void Activate(bool _)
        {
            Patching.AddPatch(typeof(Leaderboards), "SetModeLevelRush", HideIfCustom, Patching.PatchTarget.Prefix);
            Patching.AddPatch(typeof(LevelRush), "IsHellRushUnlocked", IsHellRushUnlocked, Patching.PatchTarget.Prefix);
            Patching.AddPatch(typeof(LevelRush), "GetNumLevelsInRush", GetNumLevelsInRush, Patching.PatchTarget.Prefix);
            Patching.AddPatch(typeof(LevelRush), "GetCurrentLevelRushLevelData", GetCurrentLevelRushLevelData, Patching.PatchTarget.Prefix);
            Patching.AddPatch(typeof(LevelRush), "GetLevelRushDataByType", GetLevelRushDataByType, Patching.PatchTarget.Prefix);
        }

        static bool HideIfCustom(Leaderboards __instance, LevelRush.LevelRushType levelRushType, bool justRequestingScores)
        {
            var isCustom = levelRushType == Constants.CUSTOM_RUSHTYPE;
            __instance.gameObject.SetActive(!isCustom);
            return !isCustom;
        }

        static bool IsHellRushUnlocked(LevelRush.LevelRushType levelRush, ref bool __result)
        {
            var isCustom = levelRush == Constants.CUSTOM_RUSHTYPE;
            __result = isCustom;
            return !isCustom;
        }

        static bool GetNumLevelsInRush(LevelRush.LevelRushType levelRushType, ref int __result)
        {
            var isCustom = levelRushType == Constants.CUSTOM_RUSHTYPE;
            if (isCustom)
            {
                foreach (var data in datas)
                    __result += data.EffectiveCount;
            }
            return !isCustom;
        }

        static bool GetCurrentLevelRushLevelData(LevelRushStats ___m_currentLevelRush, ref LevelData __result)
        {
            if (___m_currentLevelRush.levelRushType != Constants.CUSTOM_RUSHTYPE)
                return true;

            if (___m_currentLevelRush.currentLevelIndex == 0)
                SetLevelList();

            int num = ___m_currentLevelRush.randomizedIndex[___m_currentLevelRush.currentLevelIndex];
            __result = levels[num];
            return false;
        }

        static bool GetLevelRushDataByType(LevelRush.LevelRushType levelRushType, ref LevelRushData __result)
        {
            var isCustom = levelRushType == Constants.CUSTOM_RUSHTYPE;
            if (isCustom)
                __result = new LevelRushData(Constants.CUSTOM_RUSHTYPE);
            return !isCustom;
        }

        internal static void SetLevelList() => levels = datas.Reverse().SelectMany(x => x.GetLevels()).ToList();

        internal static string GetCode()
        {
            StringBuilder sb = new();
            foreach (var data in datas.Reverse())
                sb.Append(data.GetCode());
            return sb.ToString().Trim();
        }

        internal static void Remove(UnityEngine.Object data, string id, bool checkinter = true)
        {
            CustomRush.Logger.DebugMsg($"remove {data}");
            var d = datas.First(x => x.Contains(data));
            d.Remove(data, id);
            Sanitize(checkinter);
        }

        internal static void Sanitize(bool checkinter = true, bool err = false)
        {
            if (datas.Any(x => x.EffectiveCount == 0))
                datas = new(datas.Where(x => x.EffectiveCount != 0).Reverse()); // get rid of everything that doesn't mean anything

            if (checkinter)
            {
                if (!err)
                {
                    string code = GetCode();
                    UI.codeHandler.input.SetTextWithoutNotify(code);
                    UI.holderList.Setup();
                }
                UI.selector.Refresh(); // refresh the selector

                MainMenu.Instance()._screenLevelRush.startRushButton.interactable = datas.Count != 0;
                UI.selector.content.GetComponent<CanvasGroup>().interactable = !err;
                UI.holderList.content.GetComponent<CanvasGroup>().interactable = !err;  
            }
        }

        internal static void PopulateLevels(string code)
        {
            datas.Clear();

            var gd = Singleton<Game>.Instance.GetGameData();

            var split = code.Split(';');
            CustomRush.Logger.DebugMsg($"{split.Length} split");

            bool error = false;

            foreach (var ID in split) // semis to seperate
            {
                var tID = ID.Trim();
                if (string.IsNullOrWhiteSpace(tID))
                    continue;
                bool remove = tID.StartsWith("-") || tID.StartsWith("~");
                if (remove)
                    tID = tID.Substring(1);
                var levelData = gd.GetLevelData(tID);
                if (levelData == null)
                {
                    var mission = gd.GetMission(tID);
                    if (mission == null)
                    {
                        var campaign = gd.GetCampaign(tID);
                        if (campaign == null)
                        {
                            error = true;
                            datas.Clear();
                            break;
                        }
                        else if (!remove)
                            datas.Push(DataHolder.Create(campaign));
                        else
                            Remove(campaign, tID, false);
                    }
                    else if (!remove)
                        datas.Push(DataHolder.Create(mission));
                    else
                        Remove(mission, tID, false);
                }
                else if (!remove)
                    datas.Push(DataHolder.Create(levelData));
                else
                    Remove(levelData, tID, false);
            }

            Sanitize(err: error);
        }
    }
}
