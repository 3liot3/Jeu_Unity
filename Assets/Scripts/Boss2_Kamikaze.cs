using UnityEngine;

/// <summary>
/// Comportement du boss "Kamikaze" :
/// - Poursuit et suit le joueur de façon agressive,
/// - Déclenche une explosion de contact lorsqu'il est suffisamment proche,
/// - Reçoit des dégâts et notifie le GameManager de sa mort.
/// </summary>
public class Boss2_Kamikaze : MonoBehaviour
{
    [Header("Statistiques")]
    /// <summary>Points de vie maximum du boss.</summary>
    public float pointsDeVieMax = 80f;

    /// <summary>Points de vie actuels du boss.</summary>
    public float pointsDeVie;

    [Header("Mouvements Kamikaze")]
    /// <summary>Vitesse de poursuite (unités par seconde).</summary>
    public float vitessePoursuite = 60f;

    /// <summary>Vitesse d'interpolation de la rotation pour suivre le joueur.</summary>
    public float vitesseRotation = 10f;

    [Header("Dégâts de collision")]
    /// <summary>Distance minimale pour déclencher l'explosion de contact.</summary>
    public float distanceExplosion = 10f;

    [Header("Effets Visuels")]
    /// <summary>Prefab de l'effet d'explosion instancié lors de la mort.</summary>
    public GameObject effetExplosion;

    /// <summary>Transform du joueur (récupéré depuis GameManager).</summary>
    private Transform joueur;

    /// <summary>Initialise les points de vie et récupère la référence du joueur.</summary>
    void Start()
    {
        pointsDeVie = pointsDeVieMax;
        if (GameManager.instance != null)
        {
            joueur = GameManager.instance.joueur;
        }
    }

    /// <summary>
    /// Logique par frame :
    /// - oriente le boss vers le joueur,
    /// - avance en avant selon <see cref="vitessePoursuite"/>,
    /// - vérifie la distance et déclenche l'attaque kamikaze si nécessaire.
    /// </summary>
    void Update()
    {
        if (joueur == null) return;

        Vector3 directionJoueur = joueur.position - transform.position;
        Quaternion rotationVoulue = Quaternion.LookRotation(directionJoueur);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationVoulue, vitesseRotation * Time.deltaTime);

        transform.Translate(Vector3.forward * vitessePoursuite * Time.deltaTime, Space.Self);

        float distance = Vector3.Distance(transform.position, joueur.position);
        if (distance < distanceExplosion)
        {
            AttaqueKamikaze();
        }
    }

    /// <summary>
    /// Exécute l'attaque de contact : inflige une perte de vie au joueur via le <see cref="GameManager"/> puis déclenche la mort du kamikaze.
    /// </summary>
    void AttaqueKamikaze()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.PerdreUneVie("Percuté par le Kamikaze !");
        }

        Mourir();
    }

    /// <summary>
    /// Applique des dégâts au boss, met à jour l'UI de santé via le <see cref="GameManager"/> et déclenche la mort si les PV atteignent zéro.
    /// </summary>
    /// <param name="degats">Quantité de dégâts à appliquer.</param>
    public void PrendreDegats(int degats)
    {
        pointsDeVie -= degats;

        if (GameManager.instance != null)
        {
            GameManager.instance.MettreAJourSanteBoss(pointsDeVie);
        }

        if (pointsDeVie <= 0)
        {
            pointsDeVie = 0;
            Mourir();
        }
    }

    /// <summary>
    /// Gère la mort du boss : instancie l'effet d'explosion, notifie le <see cref="GameManager"/> et détruit le GameObject.
    /// </summary>
    void Mourir()
    {
        if (effetExplosion != null)
        {
            Instantiate(effetExplosion, transform.position, transform.rotation);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.BossVaincu();
        }

        Destroy(gameObject);
    }
}