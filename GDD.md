# Game Design Document - Flappy Bird: Gravity Shift

## 1. Concept

Flappy Bird: Gravity Shift est un jeu d'arcade 2D vertical inspire de Flappy Bird.

Le joueur controle un oiseau avec une seule action : cliquer pour lui donner une impulsion. Le but est de passer entre des paires de tuyaux sans toucher les obstacles, le sol ou le plafond.

La particularite du projet est l'alternance jour/nuit : tous les 20 points, le decor change et la gravite s'inverse. Le joueur doit alors adapter son reflexe, car le clic ne sert plus a monter mais a descendre.

## 2. Mecaniques de jeu

- Le jeu se controle au clic gauche ou au trackpad.
- En mode jour, la gravite tire l'oiseau vers le bas et le clic le fait monter.
- En mode nuit, la gravite tire l'oiseau vers le haut et le clic le fait descendre.
- Les tuyaux apparaissent a droite de l'ecran et defilent vers la gauche a vitesse constante.
- Chaque paire de tuyaux contient un espace praticable place a une hauteur aleatoire.
- Un trigger entre les tuyaux ajoute 1 point quand l'oiseau passe la paire.
- Une meme paire de tuyaux ne peut rapporter qu'un seul point.
- Toucher un tuyau, la base ou le plafond declenche le Game Over.
- Le jeu ne recharge pas la scene : il passe par les etats attente, partie, Game Over, puis retour au menu.

## 3. Objectif et fin de partie

L'objectif est de faire le meilleur score possible.

Il n'y a pas de condition de victoire finale : la partie continue tant que le joueur evite les obstacles. Le score augmente de 1 a chaque paire de tuyaux franchie.

La partie se termine si l'oiseau entre en collision avec :

- un tuyau ;
- la base au sol ;
- le plafond, surtout utile en gravite inversee.

Le meilleur score est conserve localement et affiche sur l'ecran de fin.

## 4. Progression

La progression repose sur le score et sur l'alternance de mode :

- de 0 a 19 points : jour, gravite normale, clic pour monter ;
- de 20 a 39 points : nuit, gravite inversee, clic pour descendre ;
- de 40 a 59 points : retour au jour et a la gravite normale ;
- puis alternance tous les 20 points.

Lors d'un changement de mode, la vitesse verticale de l'oiseau est remise a zero et un court temps d'adaptation est applique. Cela evite que le joueur soit projete immediatement dans un obstacle au moment du switch.

La difficulte augmente naturellement avec la duree de la partie : le joueur doit garder le rythme, anticiper les tuyaux et s'adapter aux inversions de gravite.

## 5. Perimetre du projet

### MVP

- Une scene principale au format mobile vertical 9:16.
- Un oiseau jouable avec physique, animation et rotation.
- Des paires de tuyaux generees regulierement.
- Des colliders sur les tuyaux, la base et le plafond.
- Un systeme de score en sprites.
- Un meilleur score local.
- Une boucle complete : menu de depart, partie, mort, ecran Game Over, retour au menu.
- Les sons principaux : saut, point, collision et mort.
- Le changement jour/nuit avec inversion de gravite tous les 20 points.
- Une version WebGL publiable sur itch.io.

### Hors scope / options

- Classement en ligne.
- Skins d'oiseau ou de tuyaux selectionnables.
- Sauvegarde cloud.
- Menu de pause complet.
- Parametres audio avances.
- Tutoriel interactif.
- Medailles animees ou recompenses de fin de partie plus detaillees.

## 6. Direction artistique

Le jeu utilise une direction artistique pixel art proche de Flappy Bird.

Le cadrage est pense pour un ecran mobile vertical, avec une resolution native de reference en 288 x 512. Les sprites sont affiches en pixel perfect pour conserver un rendu net.

Les elements principaux sont :

- un decor de jour lumineux avec ciel bleu et ville en fond ;
- un decor de nuit plus sombre pour signaler le changement de mode ;
- des tuyaux verts ;
- une base au sol ;
- un oiseau anime avec trois sprites de battement d'aile ;
- une interface pixel art avec score, message de depart, Game Over, panneau de score et bouton start.

Le changement visuel jour/nuit doit etre immediatement comprehensible par le joueur, car il annonce aussi le changement de controle.

## 7. Son et musique

Le jeu utilise des effets sonores courts et reactifs :

- `wing` quand le joueur effectue un saut ;
- `point` quand un tuyau est passe ;
- `hit` au moment de la collision ;
- `die` pendant la chute apres la mort.

Il n'y a pas de musique de fond dans le perimetre actuel. Le choix est volontaire : les sons doivent rester lisibles et ne pas masquer les feedbacks importants du gameplay.

Le son `swoosh` peut etre utilise plus tard pour renforcer les transitions d'interface, par exemple au lancement ou au retour au menu.
