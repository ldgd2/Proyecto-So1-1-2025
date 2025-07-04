using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LogicaExploracion
{
    private AgenteExplorador agente;
    private GeneradorMapaLaberinto generadorMapa;

    private float tiempoEntrePasos;
    private float anchoCelda, altoCelda;

    private int[,] mapa;
    private Vector2Int posicionActual;
    private Backtrack backtrack = new();
    private bool[,] visitado;
    private bool metaEncontrada = false;
    private bool forzarRetroceso = false;

    public Vector2Int PosicionActual => posicionActual;

    public LogicaExploracion(AgenteExplorador agente, GeneradorMapaLaberinto mapa, float paso, float ancho, float alto)
    {
        this.agente = agente;
        this.generadorMapa = mapa;
        this.tiempoEntrePasos = paso;
        this.anchoCelda = ancho;
        this.altoCelda = alto;
    }

    public void Inicializar()
    {
        mapa = generadorMapa.GetMapa();
        posicionActual = ConvertirMundoAGrilla(agente.transform.position);
        visitado = new bool[mapa.GetLength(0), mapa.GetLength(1)];
        backtrack.Inicializar(posicionActual);

        Decisiones.Instancia?.RegistrarAgente(agente);
        Decisiones.Instancia?.IntentarOcuparCelda(posicionActual, agente);
    }

    public void Explorar()
    {
        visitado[posicionActual.y, posicionActual.x] = true;

        while (!metaEncontrada)
        {
            if (forzarRetroceso)
            {
                RealizarRetroceso();
                continue;
            }

            if (!IntentarMover())
            {
                RetrocederOSeguir();
            }

            Thread.Sleep((int)(tiempoEntrePasos * 1000));
        }
    }

    public void ForzarRetroceso()
    {
        forzarRetroceso = true;
    }

    bool IntentarMover()
    {
        List<Vector2Int> vecinos = ObtenerVecinosLibres(posicionActual);

        foreach (var vecino in vecinos)
        {
            if (!visitado[vecino.y, vecino.x])
            {
                bool puedeAvanzar = Decisiones.Instancia.IntentarOcuparCelda(vecino, agente);
                if (!puedeAvanzar)
                {
                    Encolar(() => Debug.Log($"[Agente:{agente.name}] Esperando resolución de conflicto en {vecino}..."));
                    return false; // espera a que se resuelva el conflicto
                }

                MoverA(vecino);
                return true;
            }
        }

        return false;
    }

    void MoverA(Vector2Int nuevaPos)
    {
        Decisiones.Instancia?.LiberarCelda(posicionActual, agente);

        posicionActual = nuevaPos;
        backtrack.Avanzar(posicionActual);
        visitado[nuevaPos.y, nuevaPos.x] = true;

        Encolar(() => agente.transform.position = ConvertirGrillaAMundo(posicionActual));

        if (mapa[nuevaPos.y, nuevaPos.x] == 3)
        {
            Encolar(() =>
            {
                Debug.Log($"[Agente:{agente.name}] ¡Meta encontrada!");
                metaEncontrada = true;
            });
        }
    }

    void RetrocederOSeguir()
    {
        Encolar(() => Debug.Log($"[Agente:{agente.name}] Retrocediendo (sin caminos libres)"));
        var pos = backtrack.Retroceder();

        if (pos.x != -1)
        {
            Decisiones.Instancia?.LiberarCelda(posicionActual, agente);
            posicionActual = pos;
            Encolar(() => agente.transform.position = ConvertirGrillaAMundo(posicionActual));
        }
        else
        {
            Encolar(() => Debug.Log($"[Agente:{agente.name}] Retrocedió todo... reintentando desde último punto válido."));
            Vector2Int retryPos = backtrack.UltimoPop();
            backtrack.Avanzar(retryPos);

            Decisiones.Instancia?.LiberarCelda(posicionActual, agente);
            posicionActual = retryPos;

            Encolar(() => agente.transform.position = ConvertirGrillaAMundo(posicionActual));
        }
    }

    void RealizarRetroceso()
    {
        var pos = backtrack.Retroceder();
        if (pos.x != -1)
        {
            Decisiones.Instancia?.LiberarCelda(posicionActual, agente);
            posicionActual = pos;
            Encolar(() => agente.transform.position = ConvertirGrillaAMundo(posicionActual));
        }
        forzarRetroceso = false;
    }

    List<Vector2Int> ObtenerVecinosLibres(Vector2Int pos)
    {
        List<Vector2Int> vecinos = new();
        Vector2Int[] direcciones = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var dir in direcciones)
        {
            Vector2Int nuevo = pos + dir;
            if (nuevo.y >= 0 && nuevo.y < mapa.GetLength(0) &&
                nuevo.x >= 0 && nuevo.x < mapa.GetLength(1))
            {
                int val = mapa[nuevo.y, nuevo.x];
                if (val == 0 || val == 2 || val == 3)
                    vecinos.Add(nuevo);
            }
        }

        return vecinos;
    }

    Vector2Int ConvertirMundoAGrilla(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / anchoCelda);
        int y = Mathf.RoundToInt(-pos.y / altoCelda);
        return new Vector2Int(x, y);
    }

    Vector3 ConvertirGrillaAMundo(Vector2Int grid)
    {
        float x = grid.x * anchoCelda;
        float y = -grid.y * altoCelda;
        return new Vector3(x, y, 0f);
    }

    void Encolar(System.Action accion)
    {
        ColaHiloPrincipal.Instancia.Encolar(accion);
    }
}
