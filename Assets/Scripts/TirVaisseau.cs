using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Gère les tirs du vaisseau : lasers (clic gauche) et missiles (clic droit).
/// - Instancie les projectiles aux points de tir définis.
/// - Gère le cooldown des missiles et met à jour un slider optionnel.
/// - Applique une limite d'angle et une distance de tir par défaut.
/// </summary>
public class TirVaisseau : MonoBehaviour
{
    /// <summary>Préfab du laser instancié au clic gauche.</summary>
    public GameObject prefabLaser;

    /// <summary>Préfab du missile instancié au clic droit.</summary>
    public GameObject prefabMissile;

    /// <summary>Point de tir gauche (Transform utilisé pour orienter/instancier).</summary>
    public Transform pointDeTirGauche;

    /// <summary>Point de tir droit (Transform utilisé pour orienter/instancier).</summary>
    public Transform pointDeTirDroit;

    [Header("Réglages Cooldown")]
    /// <summary>Temps (en secondes) entre deux tirs de missile.</summary>
    public float tempsEntreMissiles = 2.0f;

    /// <summary>Timestamp indiquant le prochain tir de missile autorisé.</summary>
    private float prochainTirMissile = 0f;

    /// <summary>Slider UI optionnel affichant la progression du cooldown missile.</summary>
    public Slider sliderMissile;

    [Header("Visée")]
    /// <summary>Distance utilisée lorsque le rayon souris ne touche rien.</summary>
    public float distanceTirDefaut = 300f;

    /// <summary>Angle maximal (en degrés) autorisé entre l'avant du vaisseau et la direction de tir.</summary>
    public float angleMaxTir = 180f;

    /// <summary>Référence au script de mouvement du vaisseau, pour vérifier l'état de boost.</summary>
    private MouvementVaisseau scriptMouvement;

    /// <summary>Référence à la caméra principale utilisée pour générer les rayons souris.</summary>
    private Camera cam;

    /// <summary>
    /// Initialisation : récupère la référence du script de mouvement et la caméra principale.
    /// </summary>
    void Start()
    {
        scriptMouvement = GetComponent<MouvementVaisseau>();
        cam = Camera.main;
    }

    /// <summary>
    /// Boucle principale : mise à jour du slider, prévention des tirs pendant le boost,
    /// lecture des entrées souris, calcul de la cible et instanciation des projectiles.
    /// </summary>
    void Update()
    {
        // Mise à jour du slider de cooldown missile (si présent)
        if (sliderMissile != null)
        {
            if (Time.time < prochainTirMissile)
            {
                float tempsRestant = prochainTirMissile - Time.time;
                sliderMissile.value = 1f - (tempsRestant / tempsEntreMissiles);
            }
            else
            {
                sliderMissile.value = 1f;
            }
        }

        // Interdire le tir pendant le boost si le script de mouvement le signale
        if (scriptMouvement != null && scriptMouvement.estEnBoost) return;

        // Lecture des clics souris
        bool clicGauche = Input.GetMouseButtonDown(0);
        bool clicDroit = Input.GetMouseButtonDown(1);

        if (clicGauche || clicDroit)
        {
            Ray rayonSouris = cam.ScreenPointToRay(Input.mousePosition);
            Vector3 pointCible;

            // Ignorer le joueur et ses projectiles lors du raycast (adapté aux noms de layers du projet)
            int masqueIgnore = ~((1 << LayerMask.NameToLayer("Joueur")) | (1 << LayerMask.NameToLayer("ProjectilesJoueur")));

            if (Physics.Raycast(rayonSouris, out RaycastHit hit, 1500f, masqueIgnore))
            {
                pointCible = hit.point;
                if (Vector3.Distance(transform.position, pointCible) < 15f)
                {
                    pointCible = rayonSouris.GetPoint(distanceTirDefaut);
                }
            }
            else
            {
                pointCible = rayonSouris.GetPoint(distanceTirDefaut);
            }

            // Limitation d'angle de tir
            Vector3 directionCible = pointCible - transform.position;
            if (Vector3.Angle(transform.forward, directionCible) > angleMaxTir)
            {
                pointCible = transform.position + (transform.forward * distanceTirDefaut);
            }

            // Instanciation du laser (clic gauche)
            if (clicGauche && prefabLaser != null)
            {
                pointDeTirDroit.LookAt(pointCible);
                Instantiate(prefabLaser, pointDeTirDroit.position, pointDeTirDroit.rotation);
            }

            // Instanciation du missile (clic droit) avec gestion du cooldown
            if (clicDroit && Time.time >= prochainTirMissile && prefabMissile != null)
            {
                pointDeTirGauche.LookAt(pointCible);
                Instantiate(prefabMissile, pointDeTirGauche.position, pointDeTirGauche.rotation);
                prochainTirMissile = Time.time + tempsEntreMissiles;
                if (sliderMissile != null) sliderMissile.value = 0f;
            }
        }
    }
}