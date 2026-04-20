using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestion centrale du jeu :
/// - progression (vies, niveaux, lecture de la difficulté),
/// - spawn et gestion des boss,
/// - gestion de la zone de combat (alerte visuelle et perte de vie hors-zone),
/// - boussole UI pour indiquer les cibles ennemies,
/// - interface UI (vies, annonces, UI boss),
/// - séquence de mort / respawn et retour au menu.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Instance singleton du GameManager.</summary>
    public static GameManager instance;

    /// <summary>Contour bleu du bouclier (référence visuelle).</summary>
    public GameObject contourBleuBouclier;

    /// <summary>Bouton d'accès pour retourner au menu principal (activé à la mort définitive).</summary>
    public GameObject boutonRetourMenu;

    /// <summary>Indique si la limite de la zone de combat est active.</summary>
    [HideInInspector] public bool limiteZoneActive = true;

    [Header("Progression du Joueur")]
    /// <summary>Nombre de vies du joueur.</summary>
    public int vies = 3;

    /// <summary>Indice du niveau / boss courant.</summary>
    public int niveauActuel = 0;

    /// <summary>Transform du joueur (utilisé pour positionnement et calculs).</summary>
    public Transform joueur;

    [Header("Ennemis")]
    /// <summary>Tableau des prefabs de boss dans l'ordre de progression.</summary>
    public GameObject[] prefabsBoss;

    /// <summary>Noms affichés des boss correspondant aux prefabs.</summary>
    public string[] nomsDesBoss;

    /// <summary>Référence au boss actuellement actif (lecture publique, écriture privée).</summary>
    public GameObject bossActuel { get; private set; }

    [Header("Zone de Combat (Écran Rouge)")]
    /// <summary>Image plein écran utilisée pour l'alerte de sortie de zone.</summary>
    public Image ecranRouge;

    /// <summary>Distance à partir de laquelle l'alerte commence à s'afficher.</summary>
    public float distanceAlerte = 400f;

    /// <summary>Distance maximale tolérée avant perte de vie.</summary>
    public float distanceMax = 800f;

    [Header("Interface (Boussole)")]
    /// <summary>Tableau des flèches UI utilisées pour indiquer les cibles.</summary>
    public RectTransform[] flechesUI;

    /// <summary>Liste dynamique des transforms des cibles affichées par la boussole.</summary>
    [HideInInspector] public List<Transform> ciblesBoss = new List<Transform>();

    [Header("UI Boss (Style Elden Ring)")]
    /// <summary>Conteneur UI affiché quand un boss est actif.</summary>
    public GameObject conteneurUIBoss;

    /// <summary>Slider représentant la barre de vie du boss.</summary>
    public Slider sliderBarreVie;

    /// <summary>Texte affichant le nom du boss.</summary>
    public TextMeshProUGUI texteNomBoss;

    [Header("Interface Joueur")]
    /// <summary>Texte affichant le nombre de vies restantes.</summary>
    public TextMeshProUGUI texteVies;

    /// <summary>Texte d'annonce utilisé pour les séquences (ex : décompte, messages).</summary>
    public TextMeshProUGUI texteAnnonce;

    [Header("Séquence de Mort")]
    /// <summary>Effet d'explosion instancié lors de la mort du joueur.</summary>
    public GameObject effetExplosionGrosse;

    /// <summary>Image utilisée pour le fondu au noir à la mort.</summary>
    public Image ecranNoir;

    /// <summary>Durée du fondu au noir (en secondes).</summary>
    public float tempsFondu = 1.5f;

    /// <summary>Vitesse de recul de la caméra pendant l'animation de mort.</summary>
    public float reculCameraVitesse = 30f;

    /// <summary>Indique si la séquence de mort est en cours pour éviter les doublons.</summary>
    private bool estEnTrainDeMourir = false;

    /// <summary>Verrou utilisé pour éviter de déclencher plusieurs transitions de boss simultanément.</summary>
    private bool enTransitionDeBoss = false;

    /// <summary>Initialisation du singleton.</summary>
    void Awake()
    {
        if (instance == null) instance = this;
    }

    /// <summary>
    /// Initialisation : lecture de la difficulté, configuration initiale des UI et lancement de la séquence du premier boss.
    /// </summary>
    void Start()
    {
        int difficulte = PlayerPrefs.GetInt("DifficulteJeu", 1);

        if (difficulte == 0)
        {
            vies = 5;
        }
        else if (difficulte == 1)
        {
            vies = 3;
        }
        else if (difficulte == 2)
        {
            vies = 1;
        }

        if (conteneurUIBoss != null) conteneurUIBoss.SetActive(false);
        if (ecranNoir != null) ecranNoir.gameObject.SetActive(false);
        if (texteAnnonce != null) texteAnnonce.gameObject.SetActive(false);

        MettreAJourInterfaceVies();
        StartCoroutine(SequenceNouveauBoss());

        if (boutonRetourMenu != null) boutonRetourMenu.SetActive(false);

        MettreAJourInterfaceVies();
        StartCoroutine(SequenceNouveauBoss());
    }

    /// <summary>Appelé chaque frame : gère la zone de combat et la boussole.</summary>
    void Update()
    {
        GererLaZoneDeCombat();
        GererLaFleche();
    }

    /// <summary>
    /// Coroutine d'annonce et de countdown avant l'apparition d'un nouveau boss.
    /// Si tous les boss sont vaincus, affiche le texte de victoire et se termine.
    /// </summary>
    IEnumerator SequenceNouveauBoss()
    {
        if (niveauActuel >= prefabsBoss.Length)
        {
            if (texteAnnonce != null)
            {
                texteAnnonce.gameObject.SetActive(true);
                texteAnnonce.text = "VICTOIRE TOTALE !";
            }
            yield break;
        }

        if (texteAnnonce != null)
        {
            texteAnnonce.gameObject.SetActive(true);
            string nomFuturBoss = nomsDesBoss[niveauActuel];

            texteAnnonce.text = "MENACE DÉTECTÉE : \n" + nomFuturBoss;
            yield return new WaitForSeconds(3f);

            for (int i = 3; i > 0; i--)
            {
                texteAnnonce.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            texteAnnonce.text = "ENGAGEMENT !";
            yield return new WaitForSeconds(1f);
            texteAnnonce.gameObject.SetActive(false);
        }

        SpawnBossPhysique();
    }

    /// <summary>
    /// Instancie physiquement le boss courant selon l'indice <see cref="niveauActuel"/>.
    /// Configure les distances de la zone de combat et initialise l'UI boss.
    /// Remarque : le Boss4 (manager de doigts) gère l'ajout de ses sous-cibles lui-même.
    /// </summary>
    void SpawnBossPhysique()
    {
        if (niveauActuel >= prefabsBoss.Length) return;

        ciblesBoss.Clear();
        limiteZoneActive = (niveauActuel != 4);

        float distanceSpawn = (niveauActuel == 4) ? 30000f : 400f;
        Vector3 positionSpawn = joueur.position + (joueur.forward * distanceSpawn) + (Random.insideUnitSphere * 50f);
        bossActuel = Instantiate(prefabsBoss[niveauActuel], positionSpawn, Quaternion.identity);

        enTransitionDeBoss = false;

        if (niveauActuel == 4)
        {
            distanceAlerte = 10000f;
            distanceMax = 12000f;
        }
        else
        {
            distanceAlerte = 1000f;
            distanceMax = 2000f;
        }

        if (conteneurUIBoss != null && sliderBarreVie != null)
        {
            conteneurUIBoss.SetActive(true);
            if (niveauActuel < nomsDesBoss.Length) texteNomBoss.text = nomsDesBoss[niveauActuel];

            Boss1_Tuto b1 = bossActuel.GetComponent<Boss1_Tuto>();
            Boss2_Kamikaze b2 = bossActuel.GetComponent<Boss2_Kamikaze>();
            Boss3_Sniper b3 = bossActuel.GetComponent<Boss3_Sniper>();
            Boss4_Manager b4 = bossActuel.GetComponent<Boss4_Manager>();
            Boss5_Putrefaction b5 = bossActuel.GetComponent<Boss5_Putrefaction>();

            if (b1 != null)
            {
                sliderBarreVie.maxValue = b1.pointsDeVieMax; sliderBarreVie.value = b1.pointsDeVieMax;
                ciblesBoss.Add(bossActuel.transform);
            }
            else if (b2 != null)
            {
                sliderBarreVie.maxValue = b2.pointsDeVieMax; sliderBarreVie.value = b2.pointsDeVieMax;
                ciblesBoss.Add(bossActuel.transform);
            }
            else if (b3 != null)
            {
                sliderBarreVie.maxValue = b3.pointsDeVieMax; sliderBarreVie.value = b3.pointsDeVieMax;
                ciblesBoss.Add(bossActuel.transform);
            }
            else if (b4 != null)
            {
                sliderBarreVie.maxValue = b4.pointsDeVieMaxGlobaux; sliderBarreVie.value = b4.pointsDeVieMaxGlobaux;
            }
            else if (b5 != null)
            {
                sliderBarreVie.maxValue = b5.pointsDeVieMax;
                sliderBarreVie.value = b5.pointsDeVieMax;
                ciblesBoss.Add(bossActuel.transform);
            }
        }
    }

    /// <summary>Met à jour la valeur de la barre de vie du boss depuis les scripts de boss.</summary>
    /// <param name="santeActuelle">Valeur actuelle de santé du boss.</param>
    public void MettreAJourSanteBoss(float santeActuelle)
    {
        if (sliderBarreVie != null) sliderBarreVie.value = santeActuelle;
    }

    /// <summary>
    /// Doit être appelé par un boss lorsqu'il est vaincu.
    /// Empêche les transitions multiples, masque l'UI boss et lance la séquence du boss suivant.
    /// </summary>
    public void BossVaincu()
    {
        if (enTransitionDeBoss) return;
        enTransitionDeBoss = true;

        if (conteneurUIBoss != null) conteneurUIBoss.SetActive(false);
        niveauActuel++;

        StartCoroutine(SequenceNouveauBoss());
    }

    /// <summary>
    /// Gère l'affichage de l'écran rouge selon la distance au boss et déclenche la perte de vie si la distance dépasse la limite.
    /// </summary>
    void GererLaZoneDeCombat()
    {
        if (!limiteZoneActive || bossActuel == null || joueur == null || ecranRouge == null)
        {
            if (ecranRouge != null) ecranRouge.color = new Color(1f, 0f, 0f, 0f);
            return;
        }

        float distance = Vector3.Distance(joueur.position, bossActuel.transform.position);

        if (distance > distanceAlerte)
        {
            float opaciteRouge = (distance - distanceAlerte) / (distanceMax - distanceAlerte);
            ecranRouge.color = new Color(1f, 0f, 0f, Mathf.Clamp01(opaciteRouge));

            if (distance > distanceMax && !estEnTrainDeMourir)
            {
                PerdreUneVie("Sortie de la zone de combat !");
            }
        }
        else
        {
            ecranRouge.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Met à jour les flèches de la boussole UI pour pointer vers les cibles dans <see cref="ciblesBoss"/>.
    /// Positionne et oriente les flèches selon que la cible soit dans l'écran ou hors-écran.
    /// </summary>
    void GererLaFleche()
    {
        if (Camera.main == null) return;
        Camera cam = Camera.main;

        for (int i = 0; i < flechesUI.Length; i++)
        {
            if (i < ciblesBoss.Count && ciblesBoss[i] != null)
            {
                flechesUI[i].gameObject.SetActive(true);
                Transform cibleActuelle = ciblesBoss[i];

                Vector3 positionVue = cam.WorldToViewportPoint(cibleActuelle.position);
                bool estSurEcran = positionVue.z > 0 && positionVue.x > 0 && positionVue.x < 1 && positionVue.y > 0 && positionVue.y < 1;

                if (estSurEcran)
                {
                    Vector3 positionEcranBoss = cam.WorldToScreenPoint(cibleActuelle.position);
                    positionEcranBoss.y += 80f;
                    flechesUI[i].position = positionEcranBoss;
                    flechesUI[i].localEulerAngles = new Vector3(0, 0, 180f);
                }
                else
                {
                    Vector3 directionVersBoss = cibleActuelle.position - cam.transform.position;
                    Vector3 directionRelative = cam.transform.InverseTransformDirection(directionVersBoss);
                    directionRelative.z = 0;
                    directionRelative.Normalize();

                    float angle = Mathf.Atan2(directionRelative.y, directionRelative.x) * Mathf.Rad2Deg;
                    flechesUI[i].localEulerAngles = new Vector3(0, 0, angle - 90f);

                    float marge = 50f;
                    float rayonX = (Screen.width / 2f) - marge;
                    float rayonY = (Screen.height / 2f) - marge;
                    Vector2 centreEcran = new Vector2(Screen.width / 2f, Screen.height / 2f);

                    flechesUI[i].position = centreEcran + new Vector2(directionRelative.x * rayonX, directionRelative.y * rayonY);
                }

                float distance = Vector3.Distance(joueur.position, cibleActuelle.position);
                float pourcentageDistance = Mathf.InverseLerp(distanceAlerte, 50f, distance);
                float taille = Mathf.Lerp(0.2f, 0.6f, pourcentageDistance);
                flechesUI[i].localScale = new Vector3(taille, taille, taille);
            }
            else
            {
                if (flechesUI[i] != null) flechesUI[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Met à jour le texte affichant le nombre de vies du joueur.</summary>
    public void MettreAJourInterfaceVies()
    {
        if (texteVies != null) texteVies.text = "VIES : " + vies;
    }

    /// <summary>
    /// Décrémente les vies du joueur et déclenche la séquence de mort si nécessaire.
    /// </summary>
    /// <param name="raison">Texte décrivant la raison de la perte de vie (affiché dans le log ou UI selon besoin).</param>
    public void PerdreUneVie(string raison)
    {
        if (estEnTrainDeMourir) return;
        estEnTrainDeMourir = true;

        vies--;
        MettreAJourInterfaceVies();

        StartCoroutine(MortJoueurSequence());
    }

    /// <summary>
    /// Coroutine gérant la séquence de mort : désactivation des contrôles, explosion, fondu, respawn ou fin de partie.
    /// </summary>
    IEnumerator MortJoueurSequence()
    {
        MouvementVaisseau scriptMouvement = joueur.GetComponent<MouvementVaisseau>();
        TirVaisseau scriptTir = joueur.GetComponent<TirVaisseau>();
        if (scriptMouvement != null) scriptMouvement.enabled = false;
        if (scriptTir != null) scriptTir.enabled = false;

        Camera cam = Camera.main;
        SuiviCamera scriptCam = cam.GetComponent<SuiviCamera>();
        if (scriptCam != null) scriptCam.enabled = false;

        if (effetExplosionGrosse != null) Instantiate(effetExplosionGrosse, joueur.position, joueur.rotation);

        MeshRenderer[] renderers = joueur.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderers) r.enabled = false;

        float chrono = 0f;
        Color couleurNoir = Color.black;
        couleurNoir.a = 0f;

        if (ecranNoir != null)
        {
            ecranNoir.gameObject.SetActive(true);
            ecranNoir.color = couleurNoir;
        }

        while (chrono < tempsFondu)
        {
            chrono += Time.deltaTime;
            cam.transform.Translate(Vector3.back * reculCameraVitesse * Time.deltaTime, Space.Self);

            if (ecranNoir != null)
            {
                couleurNoir.a = Mathf.Lerp(0f, 1f, chrono / tempsFondu);
                ecranNoir.color = couleurNoir;
            }

            yield return null;
        }

        yield return new WaitForSeconds(2f);

        if (vies > 0)
        {
            if (bossActuel != null)
            {
                float distanceRespawn = (niveauActuel == 4) ? 2500f : 200f;
                joueur.position = bossActuel.transform.position + (bossActuel.transform.forward * distanceRespawn);
                joueur.LookAt(bossActuel.transform.position);
            }

            foreach (MeshRenderer r in renderers) r.enabled = true;
            if (scriptMouvement != null) scriptMouvement.enabled = true;
            if (scriptTir != null) scriptTir.enabled = true;
            if (scriptCam != null) scriptCam.enabled = true;

            if (ecranNoir != null)
            {
                couleurNoir.a = 0f;
                ecranNoir.color = couleurNoir;
                ecranNoir.gameObject.SetActive(false);
            }

            estEnTrainDeMourir = false;
        }
        else
        {
            if (texteAnnonce != null)
            {
                texteAnnonce.gameObject.SetActive(true);
                texteAnnonce.text = "VOUS ÊTES MORT.";
            }
            if (boutonRetourMenu != null)
            {
                boutonRetourMenu.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    /// <summary>Charge la scène du menu principal (index 0).</summary>
    public void RetourMenuPrincipal()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}