### Noms de commit

Tous les noms de commit doivent débuter pas un indicatif entre crochets pour indiquer la nature du commit. Ensuite l'indicatif doit être suivi d'un court message décrivant les modifications apportées par le commit. L'indicatif et le message doivent être sparés d'un espace.  
> Exemple:  
> `git commit -m "[UPDATE] Upgrade SFML from 2.5 to 2.6"`

Si besoin d'expliciter un peu plus, il est possible de mettre une description au commit en plus du message :
> Exemple, le 2e `-m` indique une description:  
> `git commit -m "[UPDATE] Upgrade SFML from 2.5 to 2.6" -m "SFML 2.6 provides getPosition()/getSize() for sf::Rect<T>"`

#### Indicatifs de commit

- `[ADD]` : Indique que ce commit ajoute des fichiers ou une feature sans (ou avec peu) de modification au code déjà présent.
- `[BUILD]` : Indique que ce commit est lié à une modification du build du projet.
- `[DOC]` : Indique que ce commit vient ajouter/rectifier de la documentation ou des commentaires de code.
- `[FIX]` : Indique que ce commit vient résoudre un bug, un malfonctionnement ou un quelconque problème sans pour autant ajouter une feature.
- `[GIT]` : Indique que ce commit existe pour assurer le bon fonctionnement de `git` pour l'auteur du commit ou le groupe.
- `[MERGE]` : Indique que ce commit est un merge ou est lié à un merge récent.
- `[MSG]` : Indique que ce commit n'apporte pas de modification mais est un message que l'on veut faire apparaître dans la timeline des commits (*).
- `[REPO]` : Indique que ce commit ne modifie pas de lignes de codes mais vient modifier/réorganiser le repository du projet.
- `[REFACTO]` : Indique que ce commit apporte une refactorisation de l'architecture du code.
- `[ROLLBACK]` : Indique que ce commit est un rollback du projet à un ancien commit.
- `[RM]` : Indique que ce commit supprime des fichiers ou une feature.
- `[STYLE]` : Indique que ce commit ne change pas le fonctionnement du projet mais viens améliorer la lisibilité du code, ou corriger des typos.
- `[TMP]` : Indique que ce commit est temporaire est et voué à être rollback.
- `[TEST]` : Indique que ce commit n'est pas en rapport avec le projet directement mais avec ses tests unitaires et/ou fonctionnels.
- `[UPDATE]` : Indique que ce commit viens mettre à jour quelque chose dans le repo.

(*): `git --allow-empty -m ...` permet de créer un commit sans aucune modification au code