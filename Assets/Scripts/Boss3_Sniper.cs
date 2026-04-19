using UnityEngine;
using System.Collections;

/// <summary>
/// Comportement du boss "Sniper".
/// - Se tourne vers le joueur, recule si celui-ci est trop proche.
/// - Charge un viseur laser puis tire un projectile rapide.
/// - Gère les points de vie et la destruction.
/// </summary>
public class Boss3_Sniper : MonoBehaviour
{
    [Header("Statistiques")]
    /// <summary>Points de vie maximum du boss.</summary>
    public float pointsDeVieMax = 120f;

    /// <summary>Points de vie actuels du boss.</summary>
    public float pointsDeVie;

    [Header("Mouvements")]
    /// <summary>Vitesse de recul en unités par seconde.</summary>
    public float vitesseRecul = 60f;

    /// <summary>Distance minimale avant que le boss ne recule.</summary>
    public float distanceFuite = 250f;

    /// <summary>Vitesse d'interpolation de la rotation vers le joueur.</summary>
    public float vitesseRotation = 8f;

    [Header("Attaque Sniper")]
    /// <summary>Prefab du projectile sniper (doit se déplacer très vite).</summary>
    public GameObject prefabTirSniper;

    /// <summary>Transform du canon utilisé pour instancier le projectile.</summary>
    public Transform canon;

    /// <summary>Temps entre deux tirs successifs (secondes).</summary>
    public float tempsEntreTirs = 4f;

    /// <summary>Temps de charge pendant lequel le viseur laser est visible (secondes).</summary>
    public float tempsDeCharge = 2f;

    [Header("Effets Visuels")]
    /// <summary>Effet d'explosion instancié à la mort.</summary>
    public GameObject effetExplosion;

    /// <summary>LineRenderer utilisé comme viseur laser lors de la charge.</summary>
    public LineRenderer viseurLaser;

    /// <summary>Référence au transform du joueur (récupéré depuis GameManager).</summary>
    private Transform joueur;

    /// <summary>Indique si le boss est en train de charger son tir.</summary>
    private bool enTrainDeCharger = false;

    /// <summary>Chronomètre utilisé pour cadencer les tirs.</summary>
    private float chronoTir = 0f;

    /// <summary>
    /// Initialise les PV et récupère la référence du joueur.
    /// </summary>
    void Start()
    {
        pointsDeVie = pointsDeVieMax;
        if (GameManager.instance != null) joueur = GameManager.instance.joueur;
        if (viseurLaser != null) viseurLaser.enabled = false;
    }

    /// <summary>
    /// Logique par frame : rotation vers le joueur, gestion de la distance et du tir.
    /// </summary>
    void Update()
    {
        if (joueur == null) return;

        Vector3 directionJoueur = joueur.position - transform.position;
        Quaternion rotationVoulue = Quaternion.LookRotation(directionJoueur);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationVoulue, vitesseRotation * Time.deltaTime);

        if (!enTrainDeCharger)
        {
            float distance = Vector3.Distance(transform.position, joueur.position);

            if (distance < distanceFuite)
            {
                transform.Translate(Vector3.back * vitesseRecul * Time.deltaTime, Space.Self);
            }

            chronoTir += Time.deltaTime;
            if (chronoTir >= tempsEntreTirs)
            {
                StartCoroutine(SequenceDeTirSniper());
            }
        }
    }

    /// <summary>
    /// Coroutine gérant la séquence de tir : affichage du viseur, attente de charge, instanciation du projectile et réinitialisation.
    /// </summary>
    IEnumerator SequenceDeTirSniper()
    {
        enTrainDeCharger = true;

        if (viseurLaser != null)
        {
            viseurLaser.enabled = true;
            viseurLaser.SetPosition(0, canon.position);
            viseurLaser.SetPosition(1, joueur.position);
        }

        yield return new WaitForSeconds(tempsDeCharge);

        if (viseurLaser != null) viseurLaser.enabled = false;

        if (prefabTirSniper != null && canon != null)
        {
            canon.LookAt(joueur.position);
            Instantiate(prefabTirSniper, canon.position, canon.rotation);
        }

        chronoTir = 0f;
        enTrainDeCharger = false;
    }

    /// <summary>
    /// Applique des dégâts au boss et met à jour l'UI de santé via le GameManager.
    /// </summary>
    /// <param name="degats">Montant de dégâts appliqué.</param>
    public void PrendreDegats(int degats)
    {
        pointsDeVie -= degats;
        if (GameManager.instance != null) GameManager.instance.MettreAJourSanteBoss(pointsDeVie);

        if (pointsDeVie <= 0)
        {
            pointsDeVie = 0;
            Mourir();
        }
    }

    /// <summary>
    /// Gère la mort du boss : instancie l'explosion, notifie le GameManager et détruit le GameObject.
    /// </summary>
    void Mourir()
    {
        if (effetExplosion != null) Instantiate(effetExplosion, transform.position, transform.rotation);
        if (GameManager.instance != null) GameManager.instance.BossVaincu();
        Destroy(gameObject);
    }
}