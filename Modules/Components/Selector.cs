using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomRush.Modules.Components
{
    internal class Selector : MonoBehaviour
    {
        public Transform content;
        Campaign ogC;
        readonly List<Campaign> cs = [];

        void Awake()
        {
            content = GetComponent<ScrollRect>().content;
            ogC = content.Find("Campaign").GetOrAddComponent<Campaign>();
            ogC.gameObject.SetActive(false);
        }

        internal void Setup()
        {
            // first get rid of everything except for og
            foreach (var child in cs)
                Destroy(child.gameObject);

            cs.Clear();

            // now we spawn them all
            var gd = Singleton<Game>.Instance.GetGameData();

            foreach (var c in gd.campaigns)
            {
                var campaign = Utils.InstantiateUI(ogC.gameObject, c.campaignID, ogC.transform.parent).GetComponent<Campaign>();
                campaign.Setup(c);
                cs.Add(campaign);
            }

            Refresh();
        }

        internal void Refresh()
        {
            foreach (var child in cs)
            {
                child.removeButton.gameObject.SetActive(RushManager.datas.Any(x => x.Contains(child.data)));

                foreach (var m in child.ms)
                {
                    m.removeButton.gameObject.SetActive(RushManager.datas.Any(x => x.Contains(m.data)));

                    foreach (var l in m.ls)
                        l.removeButton.gameObject.SetActive(RushManager.datas.Any(x => x.Contains(l.data)));
                }
            }
        }

        internal class Campaign : MonoBehaviour
        {
            new TextMeshProUGUI name;
            internal Button addButton;
            internal Button removeButton;

            internal CampaignData data;

            internal Mission ogM;
            internal List<Mission> ms = [];
            void Awake()
            {
                name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
                addButton = name.transform.Find("Add").GetComponent<Button>();
                removeButton = name.transform.Find("Remove").GetComponent<Button>();
                ogM = transform.Find("Mission").GetOrAddComponent<Mission>();
                ogM.gameObject.SetActive(false);
            }

            internal void Setup(CampaignData c)
            {
                gameObject.SetActive(true); // calls awake
                name.text = LocalizationManager.GetTranslation(c.campaignDisplayName) ?? c.campaignDisplayName;

                data = c;

                foreach (var m in c.missionData)
                {
                    var mission = Utils.InstantiateUI(ogM.gameObject, m.missionID, ogM.transform.parent).GetComponent<Mission>();
                    mission.Setup(m);
                    ms.Add(mission);
                }

                addButton.onClick.AddListener(() =>
                {
                    RushManager.datas.Push(RushManager.DataHolder.Create(data));
                    RushManager.Sanitize();
                });

                removeButton.onClick.AddListener(() => RushManager.Remove(data, data.campaignID));

                Destroy(ogM.gameObject);
            }

            internal class Mission : MonoBehaviour
            {
                new TextMeshProUGUI name;
                internal Button addButton;
                internal Button removeButton;

                internal MissionData data;

                internal Level ogL;
                internal List<Level> ls = [];

                void Awake()
                {
                    name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
                    addButton = name.transform.Find("Add").GetComponent<Button>();
                    removeButton = name.transform.Find("Remove").GetComponent<Button>();
                    ogL = transform.Find("Levels").GetChild(0).GetOrAddComponent<Level>();
                    ogL.gameObject.SetActive(false);
                }

                internal void Setup(MissionData m)
                {
                    gameObject.SetActive(true); // calls awake
                    name.text = LocalizationManager.GetTranslation(m.missionDisplayName) ?? m.missionDisplayName;

                    data = m;

                    foreach (var l in m.levels)
                    {
                        var level = Utils.InstantiateUI(ogL.gameObject, l.levelID, ogL.transform.parent).GetComponent<Level>();
                        level.Setup(l);
                        ls.Add(level);
                    }

                    addButton.onClick.AddListener(() =>
                    {
                        RushManager.datas.Push(RushManager.DataHolder.Create(data));
                        RushManager.Sanitize();
                    });

                    removeButton.onClick.AddListener(() => RushManager.Remove(data, data.missionID));

                    Destroy(ogL.gameObject);
                }

                internal class Level : MonoBehaviour
                {
                    new TextMeshProUGUI name;
                    internal Button addButton;
                    internal Button removeButton;

                    internal LevelData data;

                    void Awake()
                    {
                        name = GetComponent<TextMeshProUGUI>();
                        addButton = transform.Find("Add").GetComponent<Button>();
                        removeButton = transform.Find("Remove").GetComponent<Button>();
                    }

                    internal void Setup(LevelData l)
                    {
                        gameObject.SetActive(true); // calls awake
                        name.text = LocalizationManager.GetTranslation(l.GetLevelDisplayName()) ?? l.levelDisplayName;
                        addButton.onClick.AddListener(() =>
                        {
                            RushManager.datas.Push(RushManager.DataHolder.Create(data));
                            RushManager.Sanitize();
                        });

                        removeButton.onClick.AddListener(() => RushManager.Remove(data, data.levelID));

                        data = l;
                    }
                }
            }
        }
    }
}
