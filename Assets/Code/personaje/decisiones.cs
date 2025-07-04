using System.Collections.Generic;
using UnityEngine;

public class Decisiones : MonoBehaviour
{
    public static Decisiones Instancia;

    private readonly object _lock = new();
    private readonly List<AgenteExplorador> agentesActivos = new();
    private readonly Dictionary<Vector2Int, AgenteExplorador> ocupadas = new();

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegistrarAgente(AgenteExplorador agente)
    {
        if (agente == null) return;
        lock (_lock)
        {
            if (!agentesActivos.Contains(agente))
            {
                agentesActivos.Add(agente);
                Debug.Log($"[Decisiones] Agente registrado: {agente.name}");
            }
        }
    }

    public bool HayAgenteEn(Vector2Int destino, out AgenteExplorador otro)
    {
        lock (_lock)
        {
            return ocupadas.TryGetValue(destino, out otro) && otro != null;
        }
    }

    public void ResolverConflicto(Vector2Int destino, AgenteExplorador solicitante, AgenteExplorador ocupante)
    {
        ColaHiloPrincipal.Instancia.Encolar(() =>
        {
            bool solicitanteAvanza = Random.value > 0.5f;
            AgenteExplorador quienAvanza = solicitanteAvanza ? solicitante : ocupante;
            AgenteExplorador quienRetrocede = solicitanteAvanza ? ocupante : solicitante;

            lock (_lock)
            {
                ocupadas[destino] = quienAvanza;
            }

            quienRetrocede.ForzarRetroceso();

            Debug.Log($"[Decisiones] Conflicto en {destino}: {quienRetrocede.name} retrocede, {quienAvanza.name} avanza.");
        });
    }

    public bool IntentarOcuparCelda(Vector2Int destino, AgenteExplorador solicitante)
    {
        lock (_lock)
        {
            if (ocupadas.TryGetValue(destino, out var ocupante))
            {
                if (ocupante == solicitante)
                {
                    return true; // ya es suya
                }

                // bloquea temporalmente
                ocupadas[destino] = null;

                ResolverConflicto(destino, solicitante, ocupante);
                return false; // espera a que se resuelva
            }
            else
            {
                ocupadas[destino] = solicitante;
                return true;
            }
        }
    }

    public void LiberarCelda(Vector2Int pos, AgenteExplorador agente)
    {
        lock (_lock)
        {
            if (ocupadas.TryGetValue(pos, out var actual) && actual == agente)
            {
                ocupadas.Remove(pos);
                ColaHiloPrincipal.Instancia.Encolar(() =>
                {
                    Debug.Log($"[Decisiones] Celda {pos} liberada por {agente.name}");
                });
            }
        }
    }
}
