using CustomRush.Modules.Components;
using NeonLite;
using TMPro;
using UnityEngine;

namespace CustomRush.Modules
{
    internal class UI : NeonLite.Modules.IModule
    {
#pragma warning disable CS0414
        const bool priority = true;
        const bool active = true;

        static GameObject codePrefab;
        static GameObject selectorPrefab;
        static GameObject listPrefab;

        static void Activate(bool activate)
        {
            Patching.AddPatch(typeof(MenuScreenLevelRush), "Awake", Initialize, Patching.PatchTarget.Postfix);
            Patching.AddPatch(typeof(MenuScreenLevelRush), "InitLevelRushImages", ShowButton, Patching.PatchTarget.Postfix);
            Patching.AddPatch(typeof(MenuScreenLevelRush), "OnSelectRush", HideButton, Patching.PatchTarget.Postfix);
            Patching.AddPatch(typeof(MenuScreenLevelRush), "SetInfoText", SetupCustomScreen, Patching.PatchTarget.Prefix);
            Patching.AddPatch(typeof(MenuScreenLevelRushComplete), "OnSetVisible", OnComplete, Patching.PatchTarget.Postfix);

            CustomRush.OnBundleLoad += bundle =>
            {
                codePrefab = bundle.LoadAsset<GameObject>("Assets/Prefabs/Code Input.prefab");
                selectorPrefab = bundle.LoadAsset<GameObject>("Assets/Prefabs/Selector.prefab");
                listPrefab = bundle.LoadAsset<GameObject>("Assets/Prefabs/List.prefab");
            };
        }

        static MenuButtonHolder customButton;
        public static RushCodeHandler codeHandler;
        public static RushCodeHandler codeHandlerEND;
        static GameObject selectorBG;
        public static Selector selector;
        public static HolderList holderList;

        static void Initialize(MenuScreenLevelRush __instance)
        {
            // setup custombutton
            customButton = Utils.InstantiateUI(__instance.heavenButton.gameObject, "Custom Rush Button", __instance.heavenButton.transform.parent).GetComponent<MenuButtonHolder>();
            customButton.onClickEvent.RemoveAllListeners();
            customButton.onClickEvent.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off); // weird and different but ok
            customButton.onClickEvent.AddListener(() => __instance.OnSelectRush(Constants.CUSTOM_RUSHTYPE));
            customButton.localizedText.SetKey("CustomRush/BUTTON_CUSTOMRUSH");

            var pos = customButton.transform.localPosition;
            pos.x = __instance.yellowButton.transform.localPosition.x;
            pos.y -= 200;
            customButton.transform.localPosition = pos;

            // setup the typerrrr

            void SetupCode()
            {
                var codeObj = Utils.InstantiateUI(codePrefab, "Rush Code Input", __instance.infoText.transform.parent);
                codeObj.transform.position = __instance.infoText.transform.position;
                (codeObj.transform as RectTransform).sizeDelta = (__instance.infoText.transform as RectTransform).sizeDelta;
                codeHandler = codeObj.AddComponent<RushCodeHandler>();

                //BEFORE WE SET IT UP lets setup the end screen one
                codeObj = Utils.InstantiateUI(codeObj, "Rush Code Display", MainMenu.Instance()._screenLevelRushComplete.transform);
                var pos = MainMenu.Instance()._screenLevelRushComplete._leaderboardsRef.transform.localPosition;
                pos.y = -230.5f; //bleh const set but it's fine
                codeObj.transform.localPosition = pos;
                codeHandlerEND = codeObj.GetComponent<RushCodeHandler>();

                codeHandler.SetupTextField();
                codeHandlerEND.SetupTextField(true);
            }

            if (codePrefab != null)
                SetupCode();
            else
                CustomRush.OnBundleLoad += _ => SetupCode(); // ordered delegates!!

            void SetupSelector()
            {
                selectorBG = Utils.InstantiateUI(__instance.infoHolder.Find("Description BG (1)").gameObject, "Selector BG", __instance.infoHolder);
                pos = selectorBG.transform.localPosition;
                pos.y = 375;
                selectorBG.transform.localPosition = pos;
                pos = selectorBG.transform.localScale;
                pos.y = 7;
                selectorBG.transform.localScale = pos;

                var holder = new GameObject("Holder").GetComponent<Transform>();
                holder.parent = selectorBG.transform;
                holder.localScale = new Vector3(1, 1.0f / 7, 1) * 2;
                holder.localPosition = new Vector2(-375, 30);
                var selObj = Utils.InstantiateUI(selectorPrefab, "Selector", holder);   
                AssignFonts(selObj);
                selector = selObj.AddComponent<Selector>();
            }

            if (selectorPrefab != null)
                SetupSelector();
            else
                CustomRush.OnBundleLoad += _ => SetupSelector(); // ordered delegates!!

            void SetupList()
            {
                var listObj = Utils.InstantiateUI(listPrefab, "Holder List", __instance.leaderboardsRef.transform.parent);
                var lbT = __instance.leaderboardsRef.leaderboardsRef.transform as RectTransform;
                var listT = listObj.transform as RectTransform;
                listT.sizeDelta = lbT.sizeDelta;
                // first, get the unpivoted position
                var pos = new Vector2(lbT.localPosition.x, lbT.localPosition.y) - (lbT.sizeDelta * lbT.pivot);
                // then, retarget based on list's pivot
                pos += listT.sizeDelta * listT.pivot;
                listT.sizeDelta = new(listT.sizeDelta.x, 805); // sweetspot

                // ok now we uh
                listT.localScale *= 2.1f;
                listT.sizeDelta /= 2.1f;

                listObj.transform.localPosition = new Vector3(pos.x, pos.y, lbT.localPosition.z);
                AssignFonts(listObj);
                holderList = listObj.AddComponent<HolderList>();
            }

            if (listPrefab != null)
                SetupList();
            else
                CustomRush.OnBundleLoad += _ => SetupList(); // ordered delegates!!
        }

        static void ShowButton()
        {
            if (!customButton)
                Initialize(MainMenu.Instance()._screenLevelRush); // game start too fast
            customButton.gameObject.SetActive(true);
            customButton.LoadButton(0.3f);
        }

        static void HideButton(LevelRush.LevelRushType levelRushType)
        {
            customButton.UnloadButton();
            customButton.gameObject.SetActive(false);
            var isCustom = levelRushType == Constants.CUSTOM_RUSHTYPE;
            selectorBG.SetActive(isCustom);
            holderList.gameObject.SetActive(isCustom);
            if (!isCustom)
                return;

            codeHandler.input.textComponent.GetComponent<AxKLocalizedText>().ChangeToLocalizedFont();
            selector.Setup();
            RushManager.PopulateLevels(codeHandler.input.text);
        }

        static bool SetupCustomScreen(MenuScreenLevelRush __instance, LevelRush.LevelRushType ___m_levelRushType, bool ___m_heavenMode)
        {
            var isCustom = ___m_levelRushType == Constants.CUSTOM_RUSHTYPE;
            __instance.infoText.gameObject.SetActive(!isCustom);
            codeHandler.gameObject.SetActive(isCustom);
            if (!isCustom)
                return true;

            __instance.titleText.SetKey(___m_heavenMode ? "CustomRush/TITLE_HEAVEN" : "CustomRush/TITLE_HELL");
            return false;
        }

        static void OnComplete(MenuScreenLevelRushComplete __instance)
        {
            var isCustom = LevelRush.GetCurrentLevelRushType() == Constants.CUSTOM_RUSHTYPE;
            codeHandlerEND.gameObject.SetActive(isCustom);
            if (!isCustom) 
                return;

            codeHandlerEND.input.text = RushManager.GetCode();
            __instance._rushName.SetKey(LevelRush.IsHellRush() ? "CustomRush/TITLE_HELL" : "CustomRush/TITLE_HEAVEN");
            __instance.bestTimeText.gameObject.SetActive(false);
        }

        internal static void AssignFonts(GameObject obj)
        {
            foreach (TMP_Text tmp in obj.GetComponentsInChildren<TMP_Text>(true))
            {
                CustomRush.Logger.DebugMsg(tmp);
                var fontName = tmp.text.Replace("\u200b", "").Trim();
                int i = 0;
                bool found = false;
                foreach (var set in AxKLocalizedTextLord.GetInstance().fontLib.textMeshProFontSets)
                {
                    if (set.english.name == fontName)
                    {
                        CustomRush.Logger.DebugMsg(set);
                        found = true;
                        var style = tmp.fontStyle;
                        NeonLite.Modules.Localization.SetupUI(tmp, i).ChangeToLocalizedFont();
                        tmp.fontStyle = style;
                        break;
                    }
                    ++i;
                }
                if (!found)
                    CustomRush.Logger.Error($"Failed to find font for {tmp} '{fontName}'");
            }
        }
    }
}
