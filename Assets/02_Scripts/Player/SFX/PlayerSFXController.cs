using UnityEngine;

public class PlayerSFXController : MonoBehaviour
{
    [SerializeField] private AudioSource _as;

    [SerializeField] private AudioClip[] _slashSFX;

    public void PlayerOneShotSlashSFXRandom()
    {
        int randIndex = Random.Range(0, _slashSFX.Length);
        _as.PlayOneShot(_slashSFX[randIndex]);
    }
}   