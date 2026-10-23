# Factory Frenzy - VR

**Projet Unity | CESI Rouen | 2026**

Factory Frenzy est un projet développé sous Unity dans le cadre du module Réalité Augmentée / Réalité Virtuelle au CESI Rouen.

La partie VR consiste à développer un éditeur de niveaux immersif permettant de créer un parcours d'obstacles en plaçant et en configurant différents éléments dans un environnement 3D.

Les niveaux créés peuvent être exportés au format JSON afin d'être utilisés dans l'application PC.

---

## Sommaire

- [Présentation](#présentation)
- [Technologies utilisées](#technologies-utilisées)
- [Fonctionnalités](#fonctionnalités)
- [État du développement](#état-du-développement)
- [Architecture du projet](#architecture-du-projet)
- [Installation](#installation)
- [Organisation du développement](#organisation-du-développement)
- [Améliorations futures](#améliorations-futures)
- [Projet PC](#projet-pc)
- [Équipe](#équipe)

---

## Présentation

L'éditeur VR constitue la partie création de niveaux de Factory Frenzy.

L'utilisateur peut sélectionner des plateformes et des obstacles depuis un menu interactif, les placer dans l'environnement, ajuster leur position et exporter le parcours créé.

Le fonctionnement général est le suivant :

1. Se déplacer dans l'environnement VR.
2. Sélectionner un élément depuis le menu.
3. Placer et organiser les plateformes et obstacles.
4. Modifier ou supprimer les éléments si nécessaire.
5. Exporter le niveau au format JSON.
6. Utiliser le fichier exporté dans l'application PC.

## Technologies utilisées

- **Unity 2022.3.19f1** - Moteur de développement
- **C#** - Programmation des fonctionnalités
- **XR Interaction Toolkit** - Interactions en réalité virtuelle
- **JSON** - Sauvegarde des niveaux
- **Git / GitHub** - Gestion des versions et collaboration
- **Git LFS** - Gestion des fichiers volumineux

## Fonctionnalités

### Navigation et interactions

- Téléportation dans l'environnement VR
- Téléportation sur les plateformes
- Sélection des éléments depuis un menu interactif
- Création et placement de plateformes et obstacles
- Retour visuel et vibrations des contrôleurs

### Édition des niveaux

- Déplacement et positionnement des éléments
- Suppression des plateformes et obstacles
- Verrouillage et déverrouillage des éléments
- Placement intelligent sur une grille
- Alignement automatique entre plateformes (Snap)

### Export JSON

- Identification des éléments placés
- Sauvegarde des types d'objets
- Enregistrement des positions, rotations et échelles
- Exportation du niveau au format JSON
- Interface permettant de nommer et sauvegarder un niveau

## État du développement

### Fonctionnalités réalisées

- [x] VR-FP1-1 - Téléportation de l'avatar VR
- [x] VR-FP1-2 - Sélection et création des éléments depuis l'interface
- [x] VR-FP1-3 - Saisie et placement des objets
- [x] VR-FP1-4 - Suppression des éléments du niveau
- [x] VR-FP1-5 - Export JSON du niveau
- [x] VR-FP1-6 - Sons et vibrations
- [x] VR-FP2-1 - Placement intelligent et Snap
- [x] VR-FP2-2 - Verrouillage des éléments

### Fonctionnalité non terminée

- [ ] VR-FP2-3 - Paramétrage des éléments de la plateforme mobile

Cette dernière fonctionnalité reste à développer pour permettre de modifier les paramètres des plateformes mobiles directement depuis l'éditeur VR.

## Architecture du projet

Le dépôt VR suit la structure standard d'un projet Unity.

```text
Factory-Frenzy-VR/
├── Assets/             # Scènes, scripts, prefabs et ressources
├── Packages/           # Dépendances Unity
├── ProjectSettings/    # Configuration du projet
└── README.md           # Documentation du projet
```

## Installation

### Prérequis

- Unity Hub
- Unity 2022.3.19f1
- Git
- Git LFS
- Casque VR compatible avec la configuration du projet

### Récupération du projet

Cloner le dépôt :

```bash
git clone https://github.com/YLIssakha235/Factory-Frenzy-VR.git
```

Ouvrir ensuite Unity Hub :

1. Cliquer sur **Add project from disk**.
2. Sélectionner le dossier `Factory-Frenzy-VR`.
3. Ouvrir le projet avec Unity 2022.3.19f1.
4. Attendre l'importation des ressources.
5. Connecter le casque VR.
6. Ouvrir la scène VR et lancer l'application.

## Organisation du développement

Le projet Factory Frenzy est réalisé par une équipe de trois étudiants.

Le développement est réparti entre deux dépôts GitHub : un pour le jeu PC et un pour l'éditeur VR.

Notre organisation repose sur :

- Des branches de développement dédiées
- Des commits réguliers
- L'intégration des modifications sur la branche principale
- Le suivi des tâches avec GitHub Projects

## Améliorations futures

Plusieurs améliorations pourraient être envisagées :

- Finaliser le paramétrage des plateformes mobiles dans l'éditeur VR
- Ajouter davantage d'options de personnalisation des obstacles
- Améliorer l'ergonomie du menu VR
- Ajouter une prévisualisation complète du niveau avant son export
- Renforcer la validation des données exportées

## Projet PC

Le jeu PC est développé dans un dépôt GitHub séparé.

**Dépôt PC :** [Factory-Frenzy-PC](https://github.com/YLIssakha235/Factory-Frenzy-PC)

Les deux dépôts constituent les deux applications complémentaires du projet Factory Frenzy.

## Équipe

Projet réalisé au **CESI Rouen - 2026**, dans le cadre du module Réalité Augmentée / Réalité Virtuelle.

| Membre | Partie | Responsabilités |
|---|---|---|
| **KAMMOUN Achraf** | PC - dev1 | Joueur, caméra, course, menus, niveaux et import JSON |
| **YAYA LIBIS Issakha** | PC - dev2 | Plateformes, pièges, interactions et contrôleur du joueur |
| **CHAKRA Mohamad** | VR - dev3 | Éditeur VR, placement, verrouillage, interactions et export JSON |

---

**Factory Frenzy - CESI Rouen | 2026**

`#keywords:Unity,VirtualReality,XRInteractionToolkit,LevelEditor,JSON,CSharp,FactoryFrenzy`
