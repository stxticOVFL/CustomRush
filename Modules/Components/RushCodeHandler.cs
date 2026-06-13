using TMPro;
using UnityEngine;

namespace CustomRush.Modules.Components
{
    internal class RushCodeHandler : MonoBehaviour
    {
        public TMP_InputField input;

        internal void SetupTextField(bool readOnly = false)
        {
            UI.AssignFonts(gameObject);
            transform.Find("Label").GetComponent<AxKLocalizedText>().SetKey("CustomRush/LABEL_RUSHCODE_T");
            input = GetComponentInChildren<TMP_InputField>();
            input.placeholder.GetComponent<AxKLocalizedText>().SetKey("CustomRush/LABEL_RUSHCODE");
            input.text = RushManager.lastCode.Value;
            input.textComponent.fontStyle = FontStyles.Normal; // lol
            input.readOnly = readOnly;

            if (!readOnly)
                input.onEndEdit.AddListener(RushManager.PopulateLevels);
        }
    }
}
