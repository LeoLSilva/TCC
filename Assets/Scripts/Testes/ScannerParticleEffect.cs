using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ScannerParticleEffect : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emissionModule;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _emissionModule = _particleSystem.emission;
        _emissionModule.enabled = false;
    }

    public void UpdateScanEffect(float progress)
    {
        if (progress > 0f && progress < 1f)
        {
            if (!_particleSystem.isPlaying)
            {
                _particleSystem.Play();
            }
            _emissionModule.enabled = true;
        }
        else
        {
            _emissionModule.enabled = false;
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void ForceStop()
    {
        if (_particleSystem != null)
        {
            _emissionModule.enabled = false;
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}