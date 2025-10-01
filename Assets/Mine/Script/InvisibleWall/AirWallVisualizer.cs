using UnityEngine;

public class AirWallVisualizer : MonoBehaviour
{
    public Color tint = new Color(0.7f, 0.7f, 0.7f, 0.45f); 
    public float fadeIn = 0.15f;
    public float fadeOut = 0.3f;
    public float holdAfterPing = 0.6f;
    public Material overrideMaterial;

    Renderer _r;
    Material _matInst;
    float _lastPing = -999f;
    float _alpha;

    void Awake()
    {
        var bc = GetComponent<BoxCollider>();

        var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vis.name = "AirWallVisual";
        vis.transform.SetParent(transform, false);
        vis.transform.localPosition = bc.center;
        vis.transform.localRotation = Quaternion.identity;

        float z = Mathf.Max(0.02f, bc.size.z);
        vis.transform.localScale = new Vector3(bc.size.x, bc.size.y, z);

        var col = vis.GetComponent<Collider>();
#if UNITY_EDITOR
        DestroyImmediate(col);
#else
        Destroy(col);
#endif
        _r = vis.GetComponent<Renderer>();
        _r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _r.receiveShadows = false;

        _matInst = overrideMaterial ? new Material(overrideMaterial)
            : new Material(Shader.Find("Universal Render Pipeline/Unlit")) { renderQueue = 3000 };
        if (_matInst.shader.name == "Standard")
        {
            _matInst.SetFloat("_Mode", 3); // Transparent
            _matInst.EnableKeyword("_ALPHABLEND_ON");
            _matInst.renderQueue = 3000;
        }
        SetMatColor(new Color(tint.r, tint.g, tint.b, 0f));
        _r.sharedMaterial = _matInst;
    }

    void Update()
    {
        float target = (Time.time - _lastPing <= holdAfterPing) ? tint.a : 0f;
        float speed = (target > _alpha) ? (1f / Mathf.Max(0.01f, fadeIn)) : (1f / Mathf.Max(0.01f, fadeOut));
        _alpha = Mathf.MoveTowards(_alpha, target, Time.deltaTime * speed);
        SetMatColor(new Color(tint.r, tint.g, tint.b, _alpha));
    }

    void SetMatColor(Color c)
    {
        if (_matInst.HasProperty("_BaseColor")) _matInst.SetColor("_BaseColor", c); 
        else if (_matInst.HasProperty("_Color")) _matInst.SetColor("_Color", c);     
    }

    public void Ping() { _lastPing = Time.time; }

    void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("Player")) Ping();
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player")) Ping();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) Ping();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Ping();
    }
}

