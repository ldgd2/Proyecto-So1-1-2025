using System.Collections;
using System.Threading;
using UnityEngine;

public class AgenteExplorador : MonoBehaviour
{
    public float tiempoEntrePasos = 0.5f;
    public GeneradorMapaLaberinto generadorMapa;
    public float anchoCelda = 1f;
    public float altoCelda = 1f;

    private LogicaExploracion logicaExploracion;
    private Thread hiloExploracion;

    public Vector2Int PosicionActual => logicaExploracion?.PosicionActual ?? Vector2Int.zero;

    void Start()
    {
        // Garantiza que ColaHiloPrincipal exista en la escena
        if (ColaHiloPrincipal.Instancia == null)
        {
            GameObject obj = new GameObject("ColaHiloPrincipal");
            obj.AddComponent<ColaHiloPrincipal>();
        }

        StartCoroutine(EsperarMapa());
    }

    IEnumerator EsperarMapa()
    {
        while (generadorMapa == null)
        {
            generadorMapa = FindFirstObjectByType<GeneradorMapaLaberinto>();
            yield return null;
        }

        while (generadorMapa.GetMapa() == null)
        {
            yield return null;
        }

        logicaExploracion = new LogicaExploracion(this, generadorMapa, tiempoEntrePasos, anchoCelda, altoCelda);
        logicaExploracion.Inicializar();

        hiloExploracion = new Thread(logicaExploracion.Explorar);
        hiloExploracion.Start();
    }

    void Update()
    {
        if (ColaHiloPrincipal.Instancia != null)
        {
            ColaHiloPrincipal.Instancia.ProcesarCola();
        }
    }

    public void ForzarRetroceso()
    {
        logicaExploracion?.ForzarRetroceso();
    }

    void OnDestroy()
    {
        if (hiloExploracion != null && hiloExploracion.IsAlive)
            hiloExploracion.Abort();
    }
}
