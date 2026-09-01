using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Splines.SplineInstantiate;

public class MeshTrailEffect : MonoBehaviour
{
    [SerializeField] private PlayerRotator _rotator;
    [SerializeField] private float _activeTime = 2f;
    [SerializeField] private float _meshRefreshRate = 0.1f;
    [SerializeField] private float _meshDestroyDelay = 3f;
    [SerializeField] private SkinnedMeshRenderer[] _skinnedMeshRenderers;

    [Header("Shader Related")]
    [SerializeField] private Material _mat;
    [SerializeField] private string _shaderVarRef;
    [SerializeField] private float _shaderVarRate = 0.1f;
    [SerializeField] private float _shaderVarRefreshRate = 0.05f;

    private bool _isTrailActive = false;

    public void ActiveDodgeEffect()
    {
        if (!_isTrailActive && _skinnedMeshRenderers != null)
        {
            _isTrailActive = true;
            StartCoroutine(ActiveTrail(_activeTime));
        }
    }

    private IEnumerator ActiveTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= _meshRefreshRate;

            for (int i = 0; i < _skinnedMeshRenderers.Length; i++) {

                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(transform.position, _rotator.FacingRotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                _skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.material = _mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, _shaderVarRate, _shaderVarRefreshRate));

                Destroy(gObj, _meshDestroyDelay);
            } 
            yield return new WaitForSeconds(_meshRefreshRate);
        }

        _isTrailActive = false;
    }

    private IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(_shaderVarRef);

        while(valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(_shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}