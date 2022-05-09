
using System.Numerics;
using UnityEngine;

namespace m4k.Incremental {
[System.Serializable]
public class CurrencyInstance : ISerializationCallbackReceiver {
    public string name;
    
    [SerializeField]
    string _ownedAmount;
    public BigInteger ownedAmount;

    [System.NonSerialized]
    Currency _currency;
    public Currency currency { get {
        if(_currency == null)
            _currency = IncrementalManager.I.GetCurrency(name);
        return _currency;
    }}
    
    public CurrencyInstance(Currency currency) {
        this._currency = currency;
        this.name = currency.name;
    }
    public override string ToString() {
        return $"{currency.displayName}";
    }

    public void OnBeforeSerialize() {
        _ownedAmount = ownedAmount.ToString();
    }
    public void OnAfterDeserialize() {
        BigInteger.TryParse(_ownedAmount, out ownedAmount);
    }
}

[CreateAssetMenu(fileName = "Currency", menuName = "Data/Incremental/Currency", order = 0)]
public class Currency : ScriptableObject {
    public string displayName;
    public string description;
    public int startAmount;
}
}