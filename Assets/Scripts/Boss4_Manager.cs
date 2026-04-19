using System.Drawing;
using UnityEngine;

/// <summary>
/// Manager du boss constitué de plusieurs "doigts".
/// - Calcule les points de vie globaux à partir du prefab d'un doigt.
/// - Instancie les doigts en cercle autour du manager.
/// - Agrège les cibles dans le système de boussole du <see cref="GameManager"/>.
/// - Fournit des méthodes pour appliquer des dégâts globaux et réagir à la mort d'un doigt.
/// </summary>
public class Boss4_Manager : MonoBehaviour
{
    /// <summary>Prefab du doigt (élément composant le boss).</summary>
    public GameObject prefabDoigt;

    /// <summary>Nombre de doigts à instancier (ex : 5).</summary>
    public int nombreDeDoigts = 5;

    /// <summary>Points de vie globaux maximums calculés (exposés en lecture dans l'Inspector).</summary>
    [HideInInspector] public float pointsDeVieMaxGlobaux;

    /// <summary>Points de vie globaux actuels (somme des PV de chaque doigt).</summary>
    private float pointsDeVieGlobaux;

    /// <summary>Compteur de doigts encore en vie.</summary>
    private int doigtsRestants;

    /// <summary>Indique si le boss a déjà été déclaré vaincu (évite les double-notifs).</summary>
    private bool bossDejaVaincu = false;

    /// <summary>
    /// Initialisation :
    /// - calcule les PV globaux avant que le GameManager ne les lise,
    /// - instancie les doigts en cercle autour de ce manager,
    /// - connecte chaque doigt à ce manager et à la liste de cibles du <see cref="GameManager"/>.
    /// </summary>
    void Awake()
    {
        doigtsRestants = nombreDeDoigts;

        float pvParDoigt = prefabDoigt.GetComponent<Boss4_Doigt>().pointsDeVie;
        pointsDeVieMaxGlobaux = pvParDoigt * nombreDeDoigts;
        pointsDeVieGlobaux = pointsDeVieMaxGlobaux;

        for (int i = 0; i < nombreDeDoigts; i++)
        {
            float angle = i * Mathf.PI * 2 / nombreDeDoigts;
            Vector3 positionRelative = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 60f;
            Vector3 spawnPos = transform.position + positionRelative;

            GameObject doigt = Instantiate(prefabDoigt, spawnPos, transform.rotation);
            doigt.GetComponent<Boss4_Doigt>().manager = this;

            if (GameManager.instance != null)
            {
                GameManager.instance.ciblesBoss.Add(doigt.transform);
            }
        }
    }

    /// <summary>
    /// Applique des dégâts à la vie globale du boss (appelé par un doigt lorsqu'il subit des dégâts).
    /// Met à jour la barre de vie unique gérée par le <see cref="GameManager"/>.
    /// </summary>
    /// <param name="degats">Quantité de dégâts à soustraire.</param>
    public void PrendreDegatsGlobal(int degats)
    {
        pointsDeVieGlobaux -= degats;
        if (GameManager.instance != null)
        {
            GameManager.instance.MettreAJourSanteBoss(pointsDeVieGlobaux);
        }
    }

    /// <summary>
    /// Appelée par un doigt lorsqu'il meurt :
    /// - décrémente le compteur de doigts restants,
    /// - notifie le <see cref="GameManager"/> de la victoire lorsque tous les doigts sont morts,
    /// - détruit ce manager après notification.
    /// </summary>
    public void UnDoigtEstMort()
    {
        doigtsRestants--;

        if (doigtsRestants <= 0 && !bossDejaVaincu)
        {
            bossDejaVaincu = true;
            if (GameManager.instance != null) GameManager.instance.BossVaincu();
            Destroy(gameObject);
        }
    }
}