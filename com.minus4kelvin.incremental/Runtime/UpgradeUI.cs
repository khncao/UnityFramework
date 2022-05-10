
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace m4k.Incremental {
public class UpgradeUI : Selectable {
    public TMP_Text descriptionLabel;
    public TMP_Text amountOwnedLabel;
    public TMP_InputField inputField;

    public UpgradeInstance currentUpgradeInstance { get; private set; }

    [System.NonSerialized]
    IncrementalUIBase ui;

    public void AssignUpgradeInstance(UpgradeInstance upgradeInstance, IncrementalUIBase ui) {
        this.ui = ui;
        currentUpgradeInstance = upgradeInstance;
        UpdateUI();

        if(!inputField) return;
        inputField.onSubmit.AddListener((input) => {
            if(int.TryParse(input, out int amount))
                IncrementalManager.I.TransactAmount(upgradeInstance, amount);
        });
    }

    public void UpdateUI() {
        if(descriptionLabel)
            descriptionLabel.text = $"{currentUpgradeInstance.ToString()}({currentUpgradeInstance.upgrade.costAmount.Value})";
        if(amountOwnedLabel)
            amountOwnedLabel.text = $"{currentUpgradeInstance.ownedAmount.ToString()}";
    }

    public override void OnSelect(BaseEventData eventData) {
        ui?.OnSelectionChange(gameObject);
    }
}
}