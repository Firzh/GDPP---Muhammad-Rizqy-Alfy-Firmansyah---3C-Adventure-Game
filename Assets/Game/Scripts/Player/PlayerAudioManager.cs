using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _footstepSfx;
    [SerializeField]
    private AudioSource _landingSfx;
    [SerializeField]
    private AudioSource _punchSfx;
    [SerializeField]
    private AudioSource _glideSfx;

    private void PlayFootstepSfx()
    {
        _footstepSfx.volume = Random.Range(0.7f, 1f);
        _footstepSfx.pitch = Random.Range(0.5f, 2.5f);
        _footstepSfx.Play();
    }

    private void LandingSFX()
    {
        _landingSfx.Play();
    }

    private void PunchSFX()
    {
        _punchSfx.volume = Random.Range(0.7f, 1f);
        _punchSfx.pitch = Random.Range(0.8f, 1.5f);
        _punchSfx.Play();
    }

    public void PlayGlideSfx()
    {
        _glideSfx.volume = Random.Range(0.7f, 1f);
        _glideSfx.pitch = Random.Range(0.8f, 1.5f);
        _glideSfx.Play();
    }

    public void StopGlideSfx()
    {
        _glideSfx.Stop();
    }
}