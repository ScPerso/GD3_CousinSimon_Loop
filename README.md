# Dans le loop hero initial, on doit jeter un dé pour avancer et faire des tours sur le plateau. Le but est de résoudre une enquête à travers différents dialogues qui s’ouvrent sur des cases spécifiques.

# 

# Dans ce rendu, il y a deux nouvelles cases, chacune correspondant à un mini-jeu : un cache-cache avec une IA et un puzzle.

# 

# Le puzzle se lance sur la case violette, mais on peut également y accéder en lançant directement la scène appelée “puzzle”. Nous avons 30 secondes pour le compléter, c’est-à-dire placer les bonnes pièces aux bons endroits. Si nous réussissons dans le temps imparti, le mini-jeu est validé et nous gagnons 10 ressources, permettant d’effectuer des lancers de dés supplémentaires dans le loop hero. À l’inverse, nous en perdons 10 en cas d’échec.

# 

# Le puzzle fonctionne avec un tableau de 9 cases vides, chacune numérotée, ainsi que 9 pièces correspondant à 9 illustrations, elles aussi numérotées. Un système de drag \& drop permet de déplacer les pièces, et un système d’ancrage et de padding sur les cases du tableau permet aux pièces de s’y emboîter correctement. Une vérification est effectuée une fois le puzzle complété : “chaque pièce est-elle positionnée sur la case correspondante ?”. Si oui, le puzzle est réussi ; sinon, c’est un échec.

# 

# Concernant le cache-cache, il se déroule sur la case orange (la première des deux). L’IA utilise un système de points de patrouille (target points), c’est-à-dire des positions dans la scène entre lesquelles elle se déplace. Elle patrouille de point en point, et dispose d’un cône de vision défini par une distance et un angle. Si le joueur entre dans ce cône, l’IA vérifie s’il y a un obstacle entre elle et le joueur pouvant bloquer sa vue. Si ce n’est pas le cas, elle passe en mode poursuite : elle accélère, change d’animation (elle court au lieu de marcher) et traque le joueur. Si elle s’éloigne trop, elle perd le joueur de vue et reprend sa patrouille. Si la distance entre elle et le joueur est inférieure ou égale à la “catch distance”, cela déclenche une attaque et donc la défaite du joueur, qui est considéré comme attrapé.

# 

# Le puzzle se joue à la souris, tandis que le cache-cache se joue avec ZQSD ou les flèches.

