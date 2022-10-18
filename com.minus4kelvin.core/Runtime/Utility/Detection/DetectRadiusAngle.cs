using System.Collections.Generic;
using UnityEngine;

namespace m4k {
/// <summary>
/// Check hits by checking distance and angle from transform and transform forward, respectively. Iterates through a list of externally managed type T to process hits into local recycle list named hits.
/// </summary>
public abstract class DetectRadiusAngle<T> {
    protected const float HitsStaleThreshold = 1f;

    public bool detectSelf { get; set; }

    public T self { get; set; }

    public IList<T> others { get; private set; }

    public IList<T> hits { get {
        CheckIfHitsStale();
        return _hits;
    }}

    public bool hasHit { get {
        CheckIfHitsStale();
        return _hits.Count > 0; 
    }}

    protected System.Predicate<T> query;
    protected IList<T> _hits;
    protected HashSet<T> _inRange;
    
    protected float _lastCheckTime;
    protected float _viewAngles;
    protected float _maxSquaredRange;
    protected float _closestDistance;
    protected T _closest;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="self">Caller/reference point point transform to calculate distance and forward for angle</param>
    /// <param name="others">Should be reference to externally managed list</param>
    /// <param name="maxSquaredRange">Max squared radius from self</param>
    /// <param name="viewAngles">Angle from transform forward for hits; leave empty or 0f to only check radius</param>
    /// <param name="query"></param>
    public DetectRadiusAngle(T self, IList<T> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<T> query = null) {
        this.self = self;
        this.others = others;
        this._viewAngles = viewAngles;
        this._maxSquaredRange = maxSquaredRange;
        this._hits = new List<T>();
        this._inRange = new HashSet<T>();
        this.query = query;
        this.detectSelf = false;
    }

    protected void Init(T self, IList<T> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<T> query = null) {
        this.self = self;
        this.others = others;
        this._viewAngles = viewAngles;
        this._maxSquaredRange = maxSquaredRange;
        this._hits = new List<T>();
        this._inRange = new HashSet<T>();
        this.query = query;
        this.detectSelf = false;
    }

    public bool CheckIfHitsStale() {
        bool stale = (Time.time - _lastCheckTime) > HitsStaleThreshold;
        if(stale) {
            Debug.LogWarning("Stale hit cache");
        }
        return stale;
    }

    public T GetCachedClosest() {
        CheckIfHitsStale();
        return _closest;
    }

    public bool CheckInRangeCached(T target) {
        CheckIfHitsStale();
        return _inRange.Contains(target);
    }

    public bool UpdateHits() 
    {
        _hits.Clear();
        _inRange.Clear();
        _closest = default(T);
        _closestDistance = Mathf.Infinity;

        foreach(var t in others) {
            if(( (query != null && query.Invoke(t)) || query == null)
            && t != null && IsValid(t)
            ) {
                _hits.Add(t);
                _inRange.Add(t);
            }
        }

        _lastCheckTime = Time.time;
        return hasHit;
    }

    protected abstract bool IsValid(T other);
}
}