

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace m4k.Incremental {
public class AssetUI : Selectable {
    public TMP_Text descriptionLabel;
    public TMP_Text amountOwnedLabel;
    public TMP_InputField inputField;

    public AssetInstance currentAssetInstance { get; private set; }

    [System.NonSerialized]
    IncrementalUIBase ui;

    public void AssignAssetInstance(AssetInstance assetInstance, IncrementalUIBase ui) {
        this.ui = ui;
        currentAssetInstance = assetInstance;
        UpdateUI();

        // if(TryGetComponent<EventTrigger>(out var eventTrigger)) {
        //     var triggerEntry = new EventTrigger.Entry();
        //     triggerEntry.eventID = EventTriggerType.PointerClick;
        //     triggerEntry.callback.AddListener((eventData) => {
        //         IncrementalManager.I.TransactAmount(assetInstance, 1);
        //     });
        //     eventTrigger.triggers.Add(triggerEntry);
        // }
        if(!inputField) return;
        if(!assetInstance.asset.costCurrency) {
            inputField.gameObject.SetActive(false);
            return;
        }
        else
            inputField.gameObject.SetActive(true);
        inputField.onSubmit.AddListener((input) => {
            if(int.TryParse(input, out int amount))
                IncrementalManager.I.TransactAmount(assetInstance, amount);
        });
    }

    public void UpdateUI() {
        if(descriptionLabel)
            descriptionLabel.text = $"{currentAssetInstance.ToString()}({currentAssetInstance.asset.costAmount.Value.ToString(IncrementalManager.defaultNumberFormat)})";
        if(amountOwnedLabel)
            amountOwnedLabel.text = $"{currentAssetInstance.ownedAmount.ToString(IncrementalManager.defaultNumberFormat)}";
    }

    public override void OnSelect(BaseEventData eventData) {
        ui?.OnSelectionChange(gameObject);
    }
}}