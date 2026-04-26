# Ce projet est un jeu de plateau au tour par tour en 3D. Chaque tour, le joueur lance un dé et avance d'autant de cases sur une boucle. En atterrissant sur une case, son effet se déclenche immédiatement : découverte d'un élément narratif, gain de ressources, ou lancement d'un mini-jeu dans une scène séparée. À la fin du mini-jeu, le joueur revient au plateau.

# 

# \---

# 

# &#x20;Les cases

# 

# Bureau du Capitaine — couleur bleu foncé. Toujours placée en première position. La visite est mémorisée par le jeu.

# 

# Note écrite — couleur jaune pâle. Le jeu mémorise que le joueur a trouvé une note et collecté cet indice. Présente en 1 exemplaire.

# 

# Objet personnel — couleur verte. Le jeu mémorise que le joueur a trouvé un objet et collecté cet indice. Présente en 1 exemplaire.

# 

# Preuve matérielle — couleur orange. Le jeu mémorise que le joueur a trouvé une preuve et collecté cet indice. Présente en 1 exemplaire.

# 

# Scène du crime — couleur rouge sombre. Le jeu mémorise que le joueur a découvert le cadavre et collecté cet indice. Présente en 1 exemplaire.

# 

# Station de recharge — couleur cyan. Le joueur reçoit des ressources. Présente en 2 exemplaires.

# 

# Puzzle — couleur violette. Lance le mini-jeu Puzzle. Présent en 2 exemplaires.

# 

# Cache-cache — couleur noire. Lance le mini-jeu Cache-cache. Présent en 3 exemplaires. 

# 

# Dé à coudre — couleur bleu clair. Lance le mini-jeu Dé à coudre. Occupe toutes les cases non attribuées — c'est donc le type le plus fréquent sur le plateau. Décoré de 2 petits cubes rouges qui évoquent le mini jeu.

# 

# \---

# 

# &#x20;Mini-jeu — Cache-cache

# 

# Le joueur doit atteindre un point d'arrivée sans se faire attraper par un zombie. Le zombie se déplace de patrol point en patrol point aléatoirement, tout les deux patrol points atteint il Spin sur lui même et un mur s'envole, réduisant le nombre de cachettes disponible. Déplacement ZQSD, caméra en vue de dessus. À la première visite un seul zombie est actif ; à partir de la deuxième visite un second zombie s'ajoute, augmentant la difficulté. Atteindre la zone cible = victoire (+10 ressources, écran vert "RÉUSSI !"). Se faire attraper = défaite (-10 ressources, écran rouge "ATTRAPÉ !"). Après 3 secondes, retour automatique au plateau.

# 

# Mini-jeu — Dé à coudre

# 

# Le joueur part d'une plateforme surélevée et doit sauter dans une piscine en contrebas. La piscine contient 25 cubes invisibles en grille 5×5. Chaque cube touché pour la première fois devient rouge et visible, et le joueur repart sur la plateforme. L'objectif est de tous les révéler sans retoucher un cube déjà rouge — le faire = défaite immédiate. Si le joueur rate la piscine et tombe dans le vide, une zone de sécurité invisible très large en bas de la scène le renvoie automatiquement sur la plateforme. Révéler les 25 cubes = victoire. ZQSD pour se déplacer, Espace pour sauter.

# 

# Mini-jeu — Puzzle

# 

# Le joueur assemble des pièces dans les bons emplacements. Toutes bien placées = victoire. Retour au plateau à la fin.

# 

# \---

# 

# Ressources et progression

# 

# Un GameManager reste actif en permanence, même quand le jeu change de scène. Il conserve les éléments suivant: les découvertes narratives faites sur le plateau (quelles cases d'enquête ont été visitées), le résultat du dernier mini-jeu, la position et le total de ressources du joueur. Chaque mini-jeu transmet son résultat à ce gestionnaire avant de revenir au plateau, qui applique alors la récompense ou la pénalité.

