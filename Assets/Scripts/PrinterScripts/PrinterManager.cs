using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private GameObject _currentPartObject;
    [SerializeField] private DiagramScreen _diagramScreen;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _isEmpty = true;
    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _wallPrinter;
    [Header("Collider")]
    [SerializeField] private bool _objectInPrinter;

    public event Action<bool> OnPrinterStateChanged;

    private void Start()
    {
        _wallPrinter.SetActive(false);
        _animator = GetComponent<Animator>();
        _diagramScreen = FindAnyObjectByType<DiagramScreen>();
    }

    public void Printer(GameObject obj)
    {
        if (_isEmpty)
        {
            // O SEGREDO: Se a peça tiver um pai (o DiagramContainer), nós selecionamos o pai inteiro para clonar.
            // Se ela não tiver pai (for uma peça que o jogador quebrou e ficou solta), clona só ela.
            if (obj.transform.parent != null)
            {
                _currentPartObject = obj.transform.parent.gameObject;
            }
            else
            {
                _currentPartObject = obj;
            }

            StartCoroutine(SpawnPointAnim());
        }
    }

    IEnumerator SpawnPointAnim()
    {
        _wallPrinter.SetActive(true);
        _isEmpty = false;
        _animator.SetInteger("anim", 1);
        yield return new WaitForSeconds(1f);

        // 1. Clona o objeto (que agora é a pasta inteira com todas as peças)
        _currentPartObject = Instantiate(_currentPartObject);
        _currentPartObject.transform.position = _spawnPoint.transform.position;
        _currentPartObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // 2. Pega TODOS os PartsScripts que estão dentro dessa pasta clonada
        PartsScript[] allSpawnedParts = _currentPartObject.GetComponentsInChildren<PartsScript>();
        int maskLayer = LayerMask.NameToLayer("Mask");

        // 3. Aplica a layer e a física em todas as peças do diagrama clonado
        foreach (var part in allSpawnedParts)
        {
            part.gameObject.layer = maskLayer;
            part.ChangeRigid(false);
        }

        yield return new WaitForSeconds(.5f);
        _animator.SetInteger("anim", 2);
        yield return new WaitForSeconds(2.15f);
        _animator.SetInteger("anim", 0);

        // 4. Adiciona o script temporário da impressora em todas as peças também
        foreach (var part in allSpawnedParts)
        {
            part.gameObject.AddComponent<TempObjPrinter>();
        }

        _wallPrinter.SetActive(false);
    }

    public void SetObjectsInPrinter(bool currentState)
    {
        _objectInPrinter = currentState;
        _isEmpty = !_objectInPrinter;
        OnPrinterStateChanged?.Invoke(_objectInPrinter);
    }
}