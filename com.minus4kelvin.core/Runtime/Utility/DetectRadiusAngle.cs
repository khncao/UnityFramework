using System.Collections.Generic;
using UnityEngine;

namespace m4k {
/// <summary>
/// Check hits by checking distance and angle from transform and transform forward, respectively. Iterates through a list of externally managed type T to process hits into local recycle list named hits.
/// </summary>
public class DetectRadiusAngle {
    const float HitsStaleThreshold = 1f;

    public bool detectSelf { get; set; }

    public Transform self { get; set; }

    public IList<Transform> others { get; private set; }

    public IList<Transform> hits { get {
        CheckIfHitsStale();
        return _hits;
    }}

    public bool hasHit { get {
        CheckIfHitsStale();
        return _hits.Count > 0; 
    }}

    System.Predicate<Transform> query;
    IList<Transform> _hits;
    HashSet<Transform> _inRange;
    List<Transform> castedCache = new List<Transform>();
    
    float _lastCheckTime;
    float _viewAngles;
    float _maxSquaredRange;
    float _closestDistance;
    Transform _closest;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="self">Caller/reference point point transform to calculate distance and forward for angle</param>
    /// <param name="others">Should be reference to externally managed list</param>
    /// <param name="maxSquaredRange">Max squared radius from self</param>
    /// <param name="viewAngles">Angle from transform forward for hits; leave empty or 0f to only check radius</param>
    /// <param name="query"></param>
    public DetectRadiusAngle(Transform self, IList<Transform> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<Transform> query = null) {
        Init(self, castedCache, maxSquaredRange, viewAngles, query);
    }

    public DetectRadiusAngle(Transform self, IList<GameObject> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<Transform> query = null) {
        castedCache.Clear();
        foreach(var go in others) castedCache.Add(go.transform);
        Init(self, castedCache, maxSquaredRange, viewAngles, query);
    }

    public DetectRadiusAngle(Transform self, IList<Component> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<Transform> query = null) {
        castedCache.Clear();
        foreach(var c in others) castedCache.Add(c.transform);
        Init(self, castedCache, maxSquaredRange, viewAngles, query);
    }

    void Init(Transform self, IList<Transform> others, float maxSquaredRange, float viewAngles = 0f, System.Predicate<Transform> query = null) {
        this.self = self;
        this.others = others;
        this._viewAngles = viewAngles;
        this._maxSquaredRange = maxSquaredRange;
        this._hits = new List<Transform>();
        this._inRange = new HashSet<Transform>();
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

    public Transform GetCachedClosest() {
        CheckIfHitsStale();
        return _closest;
    }

    public bool CheckInRangeCached(Transform target) {
        CheckIfHitsStale();
        return _inRange.Contains(target);
    }

    public bool UpdateHits() 
    {
        _hits.Clear();
        _inRange.Clear();
        _closest = null;
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

    bool IsValid(Transform other) {
        Transform otherTransform = other;
        if(!detectSelf && otherTransform == self)
            return false;

        Vector3 direction = otherTransform.position - self.position;
        float sqrMagnitude = direction.sqrMagnitude;

        if(sqrMagnitude > _maxSquaredRange)
            return false;

        if(_viewAngles != 0f) {
            // direction -= self.up * Vector3.Dot(self.up, direction);
            direction.y = 0f;

            if(Vector3.Angle(self.forward, direction) > _viewAngles * 0.5f) 
                return false;

            // var dir = self.InverseTransformDirection(otherDirection);
            // var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // if(Mathf.Abs(angle) > _viewAngles * 0.5f)
            //     return false;
        }

        if(sqrMagnitude < _closestDistance) {
            _closestDistance = sqrMagnitude;
            _closest = other;
        }

        return true;
    }
}
}