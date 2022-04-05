
using UnityEngine;
using UnityEngine.UI;

namespace m4k.SaveLoad {
public class SaveDataSlot : MonoBehaviour {
#if TMPRO
    public TMPro.TMP_Text slotId;
    public TMPro.TMP_Text sceneName;
    public TMPro.TMP_Text playTime;
    public TMPro.TMP_Text money;
#else
    public Text slotId;
    public Text sceneName;
    public Text playTime;
    public Text money;
#endif
}
}