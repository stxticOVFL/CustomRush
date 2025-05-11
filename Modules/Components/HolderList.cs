using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomRush.Modules.Components
{
    internal class HolderList : MonoBehaviour
    {
        public Transform content;
        HolderHandle ogH;
        readonly List<HolderHandle> handles = [];

        void Awake()
        {
            content = GetComponent<ScrollRect>().content;
            ogH = content.Find("Holder").GetOrAddComponent<HolderHandle>();
            ogH.gameObject.SetActive(false);
        }

        internal void Setup()
        {
            // first get rid of everything except for og
            foreach (var child in handles)
                Destroy(child.gameObject);

            handles.Clear();

            foreach (var d in RushManager.datas.Reverse())
            {
                var holder = Utils.InstantiateUI(ogH.gameObject, d.id, ogH.transform.parent).GetComponent<HolderHandle>();
                holder.Setup(d);
                handles.Add(holder);
            }

            if (handles.Count > 0)
            {
                handles.First().upButton.gameObject.SetActive(false);
                var pos = handles.First().downButton.transform.localPosition;
                pos.y = 0;
                handles.First().downButton.transform.localPosition = pos;

                handles.Last().downButton.gameObject.SetActive(false);
                pos = handles.Last().upButton.transform.localPosition;
                pos.y = 0;
                handles.Last().upButton.transform.localPosition = pos;
            }
        }

        internal class HolderHandle : MonoBehaviour
        {
            new TextMeshProUGUI name;
            internal Button upButton;
            internal Button downButton;

            internal Button unpackButton;
            internal Button removeButton;

            internal RushManager.DataHolder data;

            internal Image backdrop;

            internal RemovalHandle ogR;
            internal List<RemovalHandle> removes = [];
            void Awake()
            {
                name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
                unpackButton = name.transform.Find("Unpack").GetComponent<Button>();
                removeButton = name.transform.Find("Remove").GetComponent<Button>();
                upButton = name.transform.Find("Up").GetComponent<Button>();
                downButton = name.transform.Find("Down").GetComponent<Button>();

                backdrop = GetComponentInChildren<Image>();

                ogR = transform.Find("Removals").GetChild(0).GetOrAddComponent<RemovalHandle>();
                ogR.gameObject.SetActive(false);
            }

            void Update()
            {
                var bt = (backdrop.transform as RectTransform);
                bt.sizeDelta = new(bt.sizeDelta.x, (transform as RectTransform).sizeDelta.y);
            }

            internal void Setup(RushManager.DataHolder dh)
            {
                gameObject.SetActive(true); // calls awake

                name.text = "<noparse>";
                if (dh.data is CampaignData c)
                    name.text += LocalizationManager.GetTranslation(c.campaignDisplayName) ?? c.campaignDisplayName;
                else if (dh.data is MissionData m)
                    name.text += LocalizationManager.GetTranslation(m.missionDisplayName) ?? m.missionDisplayName;
                else if (dh.data is LevelData l)
                    name.text += LocalizationManager.GetTranslation(l.GetLevelDisplayName()) ?? l.levelDisplayName;
                name.text += $"</noparse><br><alpha=#80><size=50%>{dh.GetCode()}";

                data = dh;

                foreach (var r in dh.removals)
                {
                    var removal = Utils.InstantiateUI(ogR.gameObject, r.id, ogR.transform.parent).GetComponent<RemovalHandle>();
                    removal.Setup(dh, r);
                    removes.Add(removal);
                }

                upButton.onClick.AddListener(() =>
                {
                    var list = RushManager.datas.Reverse().ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == dh)
                        {
                            (list[i - 1], list[i]) = (list[i], list[i - 1]); // woah
                            break;
                        }
                    }

                    RushManager.datas = new(list);
                    RushManager.Sanitize();
                });

                downButton.onClick.AddListener(() =>
                {
                    var list = RushManager.datas.Reverse().ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == dh)
                        {
                            (list[i + 1], list[i]) = (list[i], list[i + 1]); // woah
                            break;
                        }
                    }

                    RushManager.datas = new(list);
                    RushManager.Sanitize();
                });

                unpackButton.onClick.AddListener(() =>
                {
                    RushManager.datas = new(RushManager.datas.SelectMany(x =>
                    {
                        if (x != dh)
                            return [x];
                        return x.children.Where(x => dh.Contains(x.data)).Reverse();
                    }).Reverse());

                    RushManager.Sanitize();
                });

                removeButton.onClick.AddListener(() =>
                {
                    RushManager.datas = new(RushManager.datas.Where(x => x != dh).Reverse());
                    RushManager.Sanitize();
                });

                if (dh.children.Count == 0)
                {
                    unpackButton.gameObject.SetActive(false);
                    var pos = removeButton.transform.localPosition;
                    pos.y = 0;
                    removeButton.transform.localPosition = pos;
                    removeButton.transform.localScale = Vector3.one * 1.5f;
                }

                var limage = dh.data;
                if (limage is CampaignData c2)
                    limage = c2.missionData[0];
                if (limage is MissionData m2)
                    limage = m2.levels[0];
                backdrop.sprite = (limage as LevelData).GetPreviewImage();

                Destroy(ogR.gameObject);
            }


            internal class RemovalHandle : MonoBehaviour
            {
                new AxKLocalizedText name;
                internal Button addButton;
                internal Button removeButton;

                internal Image backdrop;

                internal RushManager.DataHolder original;
                internal RushManager.DataHolder data;


                void Awake()
                {
                    name = transform.Find("Name").GetComponent<AxKLocalizedText>();
                    removeButton = name.transform.Find("Remove").GetComponent<Button>();

                    backdrop = GetComponentInChildren<Image>();
                }

                void Update()
                {
                    var bt = (backdrop.transform as RectTransform);
                    bt.sizeDelta = new(bt.sizeDelta.x, (transform as RectTransform).sizeDelta.y);
                }

                internal void Setup(RushManager.DataHolder host, RushManager.DataHolder r)
                {
                    gameObject.SetActive(true); // calls awake
                    string loc = "";
                    if (r.data is CampaignData c)
                        loc = LocalizationManager.GetTranslation(c.campaignDisplayName) ?? c.campaignDisplayName;
                    else if (r.data is MissionData m)
                        loc = LocalizationManager.GetTranslation(m.missionDisplayName) ?? m.missionDisplayName;
                    else if (r.data is LevelData l)
                        loc = LocalizationManager.GetTranslation(l.GetLevelDisplayName()) ?? l.levelDisplayName;
                    name.SetKey("CustomRush/LABEL_REMOVEITEM", [new("{0}", loc, false)]);

                    var limage = r.data;
                    if (r.data is CampaignData c2)
                        limage = c2.missionData[0];
                    if (r.data is MissionData m2)
                        limage = m2.levels[0];
                    backdrop.sprite = (limage as LevelData).GetPreviewImage();

                    removeButton.onClick.AddListener(() =>
                    {
                        host.removals.Remove(r);
                        RushManager.Sanitize();
                    });

                    original = host;
                    data = r;
                }
            }
        }
    }
}
