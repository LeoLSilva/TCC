using System.Collections;
using UnityEngine;

public class HeadAlgorithmGuide : MonoBehaviour
{
    [SerializeField] private MissionManager _missionManager;
    [SerializeField] private AlgoritmCreater _algoritmCreater;

    [Header("Sprites das Pecas")]
    [SerializeField] private Sprite _headSprite;
    [SerializeField] private Sprite _antennaSprite;
    [SerializeField] private Sprite _eyeSprite;
    [SerializeField] private Sprite _mouthSprite;

    [SerializeField] private float _delayBetweenSteps = 0.4f;

    private void OnEnable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnStepChanged += HandleStepChanged;
        }
    }

    private void OnDisable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnStepChanged -= HandleStepChanged;
        }
    }

    private void HandleStepChanged(MissionState mission, int step)
    {
        if (mission == MissionState.Mission2 && step == 2)
        {
            _missionManager.ActiveAlgoritm(true);
            StartCoroutine(GenerateHeadGuideRoutine());
        }
        else if (mission == MissionState.Mission3 && step == 0)
        {
            if (_algoritmCreater != null)
            {
                _algoritmCreater.ClearAlgorithm();
            }
        }
    }

    private IEnumerator GenerateHeadGuideRoutine()
    {
        if (_algoritmCreater == null) yield break;

        _algoritmCreater.ClearAlgorithm();

        yield return new WaitForSeconds(1f);

        _algoritmCreater.RegisterPrint(_headSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterPrint(_antennaSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterConnection(_antennaSprite, _headSprite, null);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterPrint(_antennaSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterConnection(_antennaSprite, _headSprite, null);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterPrint(_eyeSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterConnection(_eyeSprite, _headSprite, null);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterPrint(_eyeSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterConnection(_eyeSprite, _headSprite, null);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterPrint(_mouthSprite);
        yield return new WaitForSeconds(_delayBetweenSteps);

        _algoritmCreater.RegisterConnection(_mouthSprite, _headSprite, null);
    }
}