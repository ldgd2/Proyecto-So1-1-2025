using System;
using System.Collections.Concurrent;
using UnityEngine;

public class ColaHiloPrincipal : MonoBehaviour
{
    public static ColaHiloPrincipal Instancia { get; private set; }

    private readonly ConcurrentQueue<Action> cola = new();

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Encolar(Action accion)
    {
        cola.Enqueue(accion);
    }

    public void ProcesarCola()
    {
        while (cola.TryDequeue(out var accion))
        {
            accion?.Invoke();
        }
    }
}
