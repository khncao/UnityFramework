

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace m4k.Incremental {
public class AssetUI : MonoBehaviour {
    public TMP_Text descriptionLabel;
    public TMP_Text amountOwnedLabel;
    public TMP_InputField inputField;

    EventTrigger eventTrigger;

    [System.NonSerialized]
    AssetInstance currentAssetInstance;

    public void AssignAssetInstance(AssetInstance assetInstance) {
        currentAssetInstance = assetInstance;
        UpdateUI();

        // if(TryGetComponent<EventTrigger>(out eventTrigger)) {
        //     var triggerEntry = new EventTrigger.Entry();
        //     triggerEntry.eventID = EventTriggerType.PointerClick;
        //     triggerEntry.callback.AddListener((eventData) => {
        //         IncrementalManager.I.TransactAmount(assetInstance, 1);
        //     });
            
        //     eventTrigger.triggers.Add(triggerEntry);
        // }
        if(!inputField) return;
        inputField.onSubmit.AddListener((input) => {
            if(int.TryParse(input, out int amount))
                IncrementalManager.I.TransactAmount(assetInstance, amount);
        });
    }

    public void UpdateUI() {
        if(descriptionLabel)
            descriptionLabel.text = $"{currentAssetInstance.ToString()}";
        if(amountOwnedLabel)
            amountOwnedLabel.text = $"{currentAssetInstance.ownedAmount.ToString()}";
    }
}}