using UnityEngine;

[ExecuteAlways]
public class MoonSync : MonoBehaviour
{
    [SerializeField]Light moonLight;

    static readonly int MoonDirId = Shader.PropertyToID("_MoonDirection");

    private void Reset()
    {
        moonLight = GetComponent<Light>();
    }

    private void LateUpdate()
    {
        if (moonLight == null) return;

        // NOTA: La luz va para forward parqa que sea en dirección contraria de la luna en el cielo
        Vector3 toMoon = -moonLight.transform.forward;
        Shader.SetGlobalVector(MoonDirId, new Vector4(toMoon.x, toMoon.y, toMoon.z, 0f));
    }
}
