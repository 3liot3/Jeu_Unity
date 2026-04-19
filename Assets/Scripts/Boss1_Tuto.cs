using UnityEngine;

/// <summary>
/// Comportement du boss tutoriel :
/// - se tourne vers le joueur,
/// - avance jusqu'à une distance d'arrêt puis ouvre le feu depuis plusieurs canons,
/// - gère les points de vie, la mise à jour de l'UI de santé et la mort (explosion + notification au GameManager).
/// </summary>
public class Boss1_Tuto : MonoBehaviour
{
    [Header("Statistiques")]
    /// <summary>Points de vie maximum du boss.</summary>
    public float pointsDeVieMax = 100f;

    /// <summary>Points de vie actuels du boss.</summary>
    public float pointsDeVie;

    [Header("Mouvements")]
    /// <summary>Vitesse de déplacement en unités par seconde.</summary>
    public float vitesseDeplacement = 15f;

    /// <summary>Distance à partir de laquelle le boss s'arrête pour tirer.</summary>
    public float distanceDarret = 150f;

    /// <summary>Vitesse d'interpolation de la rotation vers le joueur.</summary>
    public float vitesseRotation = 5f;

    [Header("Attaque")]
    /// <summary>Prefab du projectile ennemi tiré par les canons.</summary>
    public GameObject prefabProjectileEnnemi;

    /// <summary>Transforms des canons utilisés pour instancier les projectiles.</summary>
    public Transform[] canons;

    /// <summary>Temps entre deux salves (secondes).</summary>
    public float cadenceDeTir = 3f;

    /// <summary>Timestamp du prochain tir autorisé.</summary>
    private float prochainTir = 0f;

    [Header("Effets Visuels")]
    /// <summary>Prefab d'explosion instancié lors de la mort.</summary>
    public GameObject effetExplosion;

    /// <summary>Référence au transform du joueur (récupérée depuis GameManager).</summary>
    private Transform joueur;

    /// <summary>Initialise les PV et récupère la référence du joueur.</summary>
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
    /// - avance si la distance dépasse <see cref="distanceDarret"/>,
    /// - déclenche le tir selon la cadence.
    /// </summary>
    void Update()
    {
        if (joueur == null) return;

        Vector3 directionJoueur = joueur.position - transform.position;
        Quaternion rotationVoulue = Quaternion.LookRotation(directionJoueur);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationVoulue, vitesseRotation * Time.deltaTime);

        float distance = Vector3.Distance(transform.position, joueur.position);
        if (distance > distanceDarret)
        {
            transform.Translate(Vector3.forward * vitesseDeplacement * Time.deltaTime, Space.Self);
        }

        if (Time.time >= prochainTir)
        {
            Tirer();
            prochainTir = Time.time + cadenceDeTir;
        }
    }

    /// <summary>
    /// Instancie les projectiles depuis chaque canon configuré.
    /// </summary>
    void Tirer()
    {
        if (prefabProjectileEnnemi == null) return;

        foreach (Transform canon in canons)
        {
            Instantiate(prefabProjectileEnnemi, canon.position, canon.rotation);
        }
    }

    /// <summary>
    /// Applique des dégâts au boss, met à jour l'UI via <see cref="GameManager"/> et déclenche la mort si les PV atteignent zéro.
    /// </summary>
    /// <param name="degats">Montant de dégâts à appliquer.</param>
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
    /// Gère la mort du boss : explosion visuelle, notification du GameManager et destruction du GameObject.
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