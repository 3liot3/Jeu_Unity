using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Comporte le comportement du boss "Putrefaction" :
/// - Intro cinématique d'entrée,
/// - Gestion des phases (bouclier, laser dévastateur, déplacements & tirs, invocations),
/// - Système de bouclier avec régénération,
/// - Attaques : gros laser canalise, tirs standards et invocations de minions,
/// - Réception des dégâts, transitions de phase et destruction finale.
/// </summary>
public class Boss5_Putrefaction : MonoBehaviour
{
    /// <summary>Verrouillage empêchant la rotation du boss (utilisé lors du tir du laser).</summary>
    private bool verrouillageLaser = false;

    [Header("Statistiques Globales")]
    /// <summary>Points de vie maximum du boss.</summary>
    public float pointsDeVieMax = 1500f;

    /// <summary>Points de vie actuellement restants.</summary>
    public float pointsDeVie;

    /// <summary>Phase courante du boss (1, 2 ou 3).</summary>
    private int phaseActuelle = 1;

    [Header("Intro Cinématique")]
    /// <summary>Vitesse d'entrée du boss lors de l'intro (units/sec).</summary>
    public float vitesseEntree = 800f;

    /// <summary>Distance à laquelle le boss stoppe son dash d'entrée et commence le combat.</summary>
    public float distanceCombat = 1000f;

    /// <summary>Indique si le boss est toujours en phase d'introduction.</summary>
    private bool enIntro = true;

    [Header("Phase 1 : Le Bouclier")]
    /// <summary>Points de vie maximum du bouclier (absorbe les dégâts tant qu'il est actif).</summary>
    public float hpBouclierMax = 150f;

    /// <summary>Points de vie actuels du bouclier.</summary>
    private float hpBouclier;

    /// <summary>Indique si le bouclier est actif.</summary>
    private bool bouclierActif = false;

    /// <summary>Objet visuel du bouclier 3D (bulle) à activer/désactiver.</summary>
    public GameObject visuelBouclier3D;

    /// <summary>Temps (en secondes) avant régénération du bouclier après brisure.</summary>
    public float tempsRegenBouclier = 10f;

    [Header("Phase 1 : Gros Laser")]
    /// <summary>LineRenderer utilisé comme viseur visuel avant le tir du gros laser.</summary>
    public LineRenderer viseurLaser;

    /// <summary>Prefab du gros laser physique instancié lorsque le tir est exécuté.</summary>
    public GameObject prefabGrosLaser;

    /// <summary>Transform du canon central d'où sort le gros laser.</summary>
    public Transform canonCentral;

    [Header("Phase 2 : Déplacements & Tirs")]
    /// <summary>Vitesse de déplacement en phase 2.</summary>
    public float vitessePhase2 = 20f;

    /// <summary>Prefab d'un projectile de type "capsule".</summary>
    public GameObject prefabCapsule;

    /// <summary>Prefab d'un missile standard.</summary>
    public GameObject prefabMissile;

    /// <summary>Transform du canon gauche utilisé pour tirer en phase 2.</summary>
    public Transform canonGauche;

    /// <summary>Transform du canon droit utilisé pour tirer en phase 2.</summary>
    public Transform canonDroit;

    [Header("Phase 3 : Invocations")]
    /// <summary>Prefabs des minions invoqués (anciens boss en version fantôme).</summary>
    public GameObject[] prefabsMinions;

    /// <summary>Liste des minions invoqués actuellement.</summary>
    private List<GameObject> minionsInvoques = new List<GameObject>();

    /// <summary>Référence au transform du joueur (récupéré depuis GameManager).</summary>
    private Transform joueur;

    /// <summary>Indique si le gros laser est en cours de canalisation/tir.</summary>
    private bool estEnTrainDeTirerGrosLaser = false;

    /// <summary>Chronomètre pour cadencement des tirs en phase 2.</summary>
    private float chronoTirPhase2 = 0f;

    /// <summary>Chronomètre pour cadence d'invocation en phase 3.</summary>
    private float chronoInvocation = 0f;

    /// <summary>Initialisation des PV, référence joueur et activation initiale du bouclier.</summary>
    void Start()
    {
        pointsDeVie = pointsDeVieMax;
        if (GameManager.instance != null) joueur = GameManager.instance.joueur;

        if (GameManager.instance != null)
            GameManager.instance.limiteZoneActive = false;

        ActiverBouclier();
    }

    /// <summary>Logique par frame : intro, rotation vers le joueur, comportement par phase.</summary>
    void Update()
    {
        if (joueur == null) return;

        if (enIntro)
        {
            transform.LookAt(joueur);
            transform.Translate(Vector3.forward * vitesseEntree * Time.deltaTime, Space.Self);

            if (Vector3.Distance(transform.position, joueur.position) <= distanceCombat)
            {
                enIntro = false;
                if (GameManager.instance != null) GameManager.instance.limiteZoneActive = true;
            }
            return;
        }

        if (!enIntro && !verrouillageLaser)
        {
            Vector3 direction = joueur.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
        }

        if (phaseActuelle == 1)
        {
            if (!estEnTrainDeTirerGrosLaser) StartCoroutine(AttaqueGrosLaser());
        }
        else
        {
            MouvementIntelligent();
            AttaquesStandard();

            if (phaseActuelle == 3)
            {
                chronoInvocation += Time.deltaTime;
                if (chronoInvocation >= 15f)
                {
                    InvoquerMinion();
                    chronoInvocation = 0f;
                }
            }
        }
    }

    /// <summary>Active le bouclier, initialise ses PV et active les visuels associés.</summary>
    void ActiverBouclier()
    {
        hpBouclier = hpBouclierMax;
        bouclierActif = true;
        if (visuelBouclier3D != null) visuelBouclier3D.SetActive(true);
        if (GameManager.instance != null && GameManager.instance.contourBleuBouclier != null)
            GameManager.instance.contourBleuBouclier.SetActive(true);
    }

    /// <summary>Brise le bouclier, désactive les visuels et déclenche la régénération différée.</summary>
    void BriserBouclier()
    {
        bouclierActif = false;
        if (visuelBouclier3D != null) visuelBouclier3D.SetActive(false);
        if (GameManager.instance != null && GameManager.instance.contourBleuBouclier != null)
            GameManager.instance.contourBleuBouclier.SetActive(false);

        StartCoroutine(RegenererBouclier());
    }

    /// <summary>Coroutine de temporisation avant réactivation du bouclier (si toujours en phase 1).</summary>
    IEnumerator RegenererBouclier()
    {
        yield return new WaitForSeconds(tempsRegenBouclier);
        if (phaseActuelle == 1) ActiverBouclier();
    }

    /// <summary>
    /// Coroutine gérant la canalisation et le tir du gros laser :
    /// 1) Viseur visuel suivant le joueur,
    /// 2) verrouillage de la direction de tir et clignotement,
    /// 3) instanciation du prefab du gros laser.
    /// </summary>
    IEnumerator AttaqueGrosLaser()
    {
        estEnTrainDeTirerGrosLaser = true;

        if (viseurLaser != null) viseurLaser.enabled = true;

        float chrono = 0f;
        while (chrono < 3f)
        {
            viseurLaser.SetPosition(0, canonCentral.position);
            viseurLaser.SetPosition(1, joueur.position);
            chrono += Time.deltaTime;
            yield return null;
        }

        verrouillageLaser = true;

        Vector3 directionVerrouillee = (joueur.position - canonCentral.position).normalized;

        viseurLaser.SetPosition(1, canonCentral.position + directionVerrouillee * 2000f);

        float t = 0f;
        while (t < 0.6f)
        {
            viseurLaser.enabled = !viseurLaser.enabled;
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        viseurLaser.enabled = false;

        if (prefabGrosLaser != null)
        {
            Instantiate(prefabGrosLaser, canonCentral.position, Quaternion.LookRotation(directionVerrouillee));
        }

        yield return new WaitForSeconds(2f);
        verrouillageLaser = false;
        estEnTrainDeTirerGrosLaser = false;
    }

    /// <summary>Déplacement simple et oscillant utilisé en phase 2 pour maintenir une distance et strafes.</summary>
    void MouvementIntelligent()
    {
        float distance = Vector3.Distance(transform.position, joueur.position);

        if (distance > 1200f) transform.Translate(Vector3.forward * vitessePhase2 * Time.deltaTime, Space.Self);
        else if (distance < 700f) transform.Translate(Vector3.back * vitessePhase2 * Time.deltaTime, Space.Self);

        transform.Translate(Vector3.right * (vitessePhase2 * 0.5f) * Mathf.Sin(Time.time) * Time.deltaTime, Space.Self);
    }

    /// <summary>Cadencement des tirs standards en phase 2 (alterne entre capsule et missile).</summary>
    void AttaquesStandard()
    {
        chronoTirPhase2 += Time.deltaTime;
        if (chronoTirPhase2 >= 1.5f)
        {
            GameObject projectile = (Random.value > 0.5f) ? prefabCapsule : prefabMissile;
            Instantiate(projectile, canonGauche.position, canonGauche.rotation);
            Instantiate(projectile, canonDroit.position, canonDroit.rotation);
            chronoTirPhase2 = 0f;
        }
    }

    /// <summary>Invoque un minion aléatoire autour du boss et l'ajoute à la liste des minions invoqués.</summary>
    void InvoquerMinion()
    {
        if (prefabsMinions.Length == 0) return;

        int index = Random.Range(0, prefabsMinions.Length);
        Vector3 spawnMinion = transform.position + (Random.insideUnitSphere * 80f);
        GameObject nouveauMinion = Instantiate(prefabsMinions[index], spawnMinion, Quaternion.identity);
        minionsInvoques.Add(nouveauMinion);
    }

    /// <summary>
    /// Applique des dégâts au boss.
    /// - Pendant l'intro le boss est invincible.
    /// - Si le bouclier est actif, il absorbe les dégâts et peut être brisé.
    /// - Sinon les PV du boss diminuent et l'UI est mise à jour.
    /// </summary>
    /// <param name="degats">Quantité de dégâts à appliquer.</param>
    public void PrendreDegats(int degats)
    {
        if (enIntro) return;

        if (bouclierActif)
        {
            hpBouclier -= degats;
            if (hpBouclier <= 0) BriserBouclier();
            return;
        }

        pointsDeVie -= degats;
        if (GameManager.instance != null) GameManager.instance.MettreAJourSanteBoss(pointsDeVie);

        VerifierChangementDePhase();

        if (pointsDeVie <= 0) Mourir();
    }

    /// <summary>Vérifie et applique les transitions de phase selon les PV restants.</summary>
    void VerifierChangementDePhase()
    {
        float tiers = pointsDeVieMax / 3f;

        if (phaseActuelle == 1 && pointsDeVie <= tiers * 2)
        {
            phaseActuelle = 2;
            if (bouclierActif) BriserBouclier();
            estEnTrainDeTirerGrosLaser = false;
            if (viseurLaser != null) viseurLaser.enabled = false;
        }
        else if (phaseActuelle == 2 && pointsDeVie <= tiers)
        {
            phaseActuelle = 3;
        }
    }

    /// <summary>Nettoie les minions restants, notifie le GameManager et détruit l'objet boss.</summary>
    void Mourir()
    {
        foreach (GameObject minion in minionsInvoques)
        {
            if (minion != null) Destroy(minion);
        }

        if (GameManager.instance != null) GameManager.instance.BossVaincu();
        Destroy(gameObject);
    }
}