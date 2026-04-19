using UnityEngine;

/// <summary>
/// Comportement d'un "doigt" du Boss4 :
/// - Suit et vise le joueur,
/// - Avance jusqu'à une distance d'attaque puis tire périodiquement,
/// - Reçoit des dégâts et notifie son manager pour la vie globale,
/// - Gère sa propre mort (explosion, suppression de la boussole et notification au manager).
/// </summary>
public class Boss4_Doigt : MonoBehaviour
{
    [Header("Statistiques")]
    /// <summary>Points de vie de ce doigt.</summary>
    public float pointsDeVie = 50f;

    /// <summary>Référence au manager du boss (assignée par le manager lors de l'instanciation).</summary>
    [HideInInspector] public Boss4_Manager manager;

    [Header("Mouvement & Attaque")]
    /// <summary>Vitesse de vol en unités par seconde.</summary>
    public float vitesseVol = 20f;

    /// <summary>Vitesse de rotation pour s'orienter vers le joueur.</summary>
    public float vitesseRotation = 5f;

    /// <summary>Distance minimale pour arrêter d'avancer et commencer à tirer.</summary>
    public float distanceAttaque = 100f;

    /// <summary>Prefab du projectile tiré par ce doigt.</summary>
    public GameObject prefabTir;

    /// <summary>Transform du canon servant de point d'apparition des projectiles.</summary>
    public Transform canon;

    /// <summary>Effet d'explosion instancié à la mort.</summary>
    public GameObject effetExplosion;

    /// <summary>Timestamp du prochain tir.</summary>
    private float prochainTir;

    /// <summary>Transform du joueur (récupéré depuis le GameManager).</summary>
    private Transform joueur;

    /// <summary>Indique si ce doigt est déjà mort (évite traitements multiples).</summary>
    private bool estMort = false;

    /// <summary>Initialisation : récupère la référence du joueur et planifie un tir initial aléatoire.</summary>
    void Start()
    {
        if (GameManager.instance != null) joueur = GameManager.instance.joueur;
        prochainTir = Time.time + Random.Range(1f, 4f);
    }

    /// <summary>
    /// Logique par frame :
    /// - oriente le doigt vers le joueur,
    /// - avance s'il est trop loin,
    /// - tire lorsque la cadence le permet.
    /// </summary>
    void Update()
    {
        if (joueur == null) return;

        Vector3 direction = joueur.position - transform.position;
        Quaternion rotationVoulue = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationVoulue, vitesseRotation * Time.deltaTime);

        if (Vector3.Distance(transform.position, joueur.position) > distanceAttaque)
        {
            transform.Translate(Vector3.forward * vitesseVol * Time.deltaTime, Space.Self);
        }
        else if (Time.time >= prochainTir)
        {
            if (prefabTir != null && canon != null) Instantiate(prefabTir, canon.position, canon.rotation);
            prochainTir = Time.time + Random.Range(2f, 5f);
        }
    }

    /// <summary>
    /// Applique des dégâts à ce doigt.
    /// - Ignore les dégâts si le doigt est déjà mort.
    /// - Informe le manager pour la déduction de la vie globale.
    /// - Déclenche la mort si les PV atteignent zéro.
    /// </summary>
    /// <param name="degats">Montant de dégâts à appliquer.</param>
    public void PrendreDegats(int degats)
    {
        if (estMort) return;

        pointsDeVie -= degats;

        if (manager != null) manager.PrendreDegatsGlobal(degats);

        if (pointsDeVie <= 0)
        {
            estMort = true;
            Mourir();
        }
    }

    /// <summary>
    /// Gère la mort du doigt :
    /// - instancie l'effet d'explosion,
    /// - notifie le manager de la mort du doigt,
    /// - retire la cible de la boussole du GameManager,
    /// - détruit le GameObject.
    /// </summary>
    void Mourir()
    {
        if (effetExplosion != null) Instantiate(effetExplosion, transform.position, transform.rotation);
        if (manager != null) manager.UnDoigtEstMort();

        if (GameManager.instance != null)
        {
            GameManager.instance.ciblesBoss.Remove(transform);
        }

        Destroy(gameObject);
    }
}