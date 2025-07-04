using System.Collections.Generic;
using UnityEngine;

public class Backtrack
{
    private Stack<Vector2Int> pilaPasos = new Stack<Vector2Int>();
    private bool enRetroceso = false;
    private Vector2Int ultimaPosicionPop = new Vector2Int(-1, -1);

    public void Inicializar(Vector2Int inicio)
    {
        pilaPasos.Clear();
        pilaPasos.Push(inicio);
        enRetroceso = false;
        ultimaPosicionPop = new Vector2Int(-1, -1);
    }

    public void Avanzar(Vector2Int nuevaPos)
    {
        if (enRetroceso)
        {
            Debug.Log("Deteniendo backtrack.");
            enRetroceso = false;
        }

        pilaPasos.Push(nuevaPos);
    }

    public Vector2Int Retroceder()
    {
        if (!enRetroceso)
        {
            Debug.Log("Empezando backtrack...");
            enRetroceso = true;
        }

        if (pilaPasos.Count > 0)
        {
            ultimaPosicionPop = pilaPasos.Pop();
            Debug.Log($"Backtrack: retrocedido desde {ultimaPosicionPop}");
        }

        return pilaPasos.Count > 0 ? pilaPasos.Peek() : new Vector2Int(-1, -1);
    }

    public bool HayPasos()
    {
        return pilaPasos.Count > 0;
    }

    public Vector2Int Actual()
    {
        return pilaPasos.Count > 0 ? pilaPasos.Peek() : new Vector2Int(-1, -1);
    }

    public Vector2Int UltimoPop()
    {
        return ultimaPosicionPop;
    }
}
