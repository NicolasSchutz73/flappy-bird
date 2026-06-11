using UnityEngine;
using UnityEngine.InputSystem;

// Oblige à avoir un Rigidbody2D sur cet objet (sinon le script ne fonctionne pas)
[RequireComponent(typeof(Rigidbody2D))]
public sealed class BirdController : MonoBehaviour
{
    // Vitesse du saut vers le haut
    [SerializeField] private float flapVelocity = 8f;
    
    // Angle de rotation quand l'oiseau monte
    [SerializeField] private float riseAngle = 25f;
    
    // Angle de rotation quand l'oiseau tombe
    [SerializeField] private float fallAngle = -70f;
    
    // Vitesse de rotation quand l'oiseau monte
    [SerializeField] private float riseRotationSpeed = 360f;
    
    // Vitesse de rotation quand l'oiseau tombe
    [SerializeField] private float fallRotationSpeed = 120f;

    // Référence au composant de physique 2D de l'oiseau
    private Rigidbody2D body;
    
    private bool flapRequested;

    // Au démarrage : récupère le Rigidbody2D attaché à cet objet
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // À chaque frame : écoute l'entrée utilisateur et gère la rotation
    private void Update()
    {
        // Vérifie si l'utilisateur appuie sur Espace
        if (Keyboard.current?.spaceKey.wasPressedThisFrame == true)
        {
            flapRequested = true;
        }

        // Met à jour l'angle de rotation de l'oiseau
        UpdateRotation();
    }

    // À intervalle fixe (pour la physique) : applique le saut si demandé
    private void FixedUpdate()
    {
        // Si aucun saut n'a été demandé → on ne fait rien
        if (!flapRequested)
        {
            return;
        }

        // Force la vitesse verticale de l'oiseau vers le haut
        body.linearVelocityY = flapVelocity;
        
        // Réinitialise le flag pour la prochaine frame
        flapRequested = false;
    }

    // Fait tourner l'oiseau selon qu'il monte ou tombe
    private void UpdateRotation()
    {
        // L'oiseau monte si sa vitesse verticale est positive (vers le haut)
        // OU si un saut a été demandé
        bool isRising = body.linearVelocityY > 0f || flapRequested;
        
        // Choisit l'angle cible selon la direction
        float targetAngle = isRising ? riseAngle : fallAngle;
        
        // Choisit la vitesse de rotation selon la direction
        float rotationSpeed = isRising ? riseRotationSpeed : fallRotationSpeed;
        
        // Crée la rotation cible
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        // Anime progressivement vers cet angle
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}
