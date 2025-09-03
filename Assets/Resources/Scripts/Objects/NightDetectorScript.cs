using UnityEngine;
using System.Collections;

public class NightDetectorScript : MonoBehaviour
{
    [Header("Детали")]
    public GameObject Lights;

    private Coroutine _LightsCoroutine;

    private void Start()
    {
        GameScript._EnviromentScript.ChangeDayCycleEvent += TurnLights;
        if (GameScript._EnviromentScript.DayPhase == EnviromentScript.DayPhases.Night) Lights.SetActive(true);
    }

    private void TurnLights()
    {
        if (GameScript._EnviromentScript.DayPhase == EnviromentScript.DayPhases.Night)
        {
            if (_LightsCoroutine != null) StopCoroutine(_LightsCoroutine);
            _LightsCoroutine = StartCoroutine(StartTurnLights());
        }
        Lights.SetActive(false);
        IEnumerator StartTurnLights()
        {
            yield return new WaitForSeconds(4f + UnityEngine.Random.Range(0, 3f));
            Lights.SetActive(GameScript._EnviromentScript.DayPhase == EnviromentScript.DayPhases.Night);
        }
    }
}

