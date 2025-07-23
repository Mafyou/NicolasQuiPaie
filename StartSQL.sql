-- Insertion dans la table Categories
SET IDENTITY_INSERT dbo.Categories ON;

INSERT INTO dbo.Categories (Id, Name, Description, IconClass, Color, IsActive, SortOrder)
VALUES
(1, 'Régalien', 'Fonctions de souveraineté : police, justice, défense', 'fa-solid fa-gavel', '#002E5D', 1, 1),
(2, 'Économie', 'Fiscalité, compétitivité, marché libre', 'fa-solid fa-chart-line', '#007ACC', 1, 2),
(3, 'Gouvernance', 'Transparence, lutte contre la corruption', 'fa-solid fa-scale-balanced', '#5A189A', 1, 3),
(4, 'Social', 'Aides, redistribution, travail', 'fa-solid fa-hand-holding-heart', '#A4133C', 1, 4),
(5, 'Identité', 'Culture, langue, cohésion nationale', 'fa-solid fa-flag', '#D7263D', 1, 5),
(6, 'Retraites', 'Financement et équité inter-générationnelle', 'fa-solid fa-person-cane', '#5CC4E0', 1, 6),
(7, 'Santé', 'Organisation, efficacité, assurances', 'fa-solid fa-stethoscope', '#198754', 1, 7),
(8, 'Éducation', 'Programmes, carte scolaire, valeurs', 'fa-solid fa-school', '#FF9F1C', 1, 8),
(9, 'Institutions', 'Processus législatif et contre-pouvoirs', 'fa-solid fa-landmark', '#3C096C', 1, 9),
(10, 'Europe', 'Relations et souveraineté vis-à-vis de l’UE', 'fa-brands fa-eu', '#003399', 1, 10),
(11, 'Immigration', 'Contrôle des flux et intégration', 'fa-solid fa-passport', '#8E2A2A', 1, 11);

SET IDENTITY_INSERT dbo.Categories OFF;

-- Insertion dans la table Proposals
SET IDENTITY_INSERT dbo.Proposals ON;

INSERT INTO dbo.Proposals (Id, Title, Description, Status, CreatedAt, VotesFor, VotesAgainst, ViewsCount, IsFeatured, CreatedById, CategoryId)
VALUES
(1, 'ÉTAT RECENTRÉ SUR L''ESSENTIEL', 'Police, justice et défense sont abandonnées aujourd’hui, laissant insécurité et chaos prospérer. Nous voulons supprimer les structures et dépenses inutiles pour renforcer le régalien et protéger les Français, sans gaspiller un centime ailleurs.', 0, '2025-07-20 16:11:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 1),
(2, 'LIBERTÉ ÉCONOMIQUE PURE', 'Moins de taxes, moins de règles. Rendez aux actifs leur salaire et aux entreprises leur compétitivité.', 0, '2025-07-20 16:12:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 2),
(3, 'ZÉRO SUBVENTION, ZÉRO PRIVILÈGE', 'Supprimez toutes les subventions, aides aux entreprises, niches, exonérations et réductions fiscales. Que chacun joue à armes égales !', 0, '2025-07-20 16:13:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 2),
(4, 'NON AU CAPITALISME DE CONNIVENCE', 'Stop aux magouilles entre élites et entreprises. Fini les privilèges pour les copains du pouvoir !', 0, '2025-07-20 16:14:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 3),
(5, 'FIN DE L''ASSISTANAT', 'Basta les aides sans contreparties qui tuent l’envie de bosser. Priorité à ceux qui contribuent.', 0, '2025-07-20 16:15:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 4),
(6, 'IDENTITÉ, SOCLE DE PROSPÉRITÉ', 'Une France unie par sa culture, sa langue et ses traditions produit mieux. Protégeons ce qui nous rend forts et évacuons ce qui nous fait régresser.', 0, '2025-07-20 16:16:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 5),
(7, 'DES RETRAITES QUI PENSENT À LA JEUNESSE', 'Stop aux privilèges. Les actifs ne doivent plus financer des régimes spéciaux ni des retraites géantes pour une minorité. Instaurons un système équitable tourné vers l''avenir.', 0, '2025-07-20 16:17:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 6),
(8, 'SANTÉ PRAGMATIQUE', 'Efficacité et liberté. Moins d’État dans la santé. Favorisons la concurrence et le libre choix des soins avec des assurances accessibles. Fini les gaspillages ; nos impôts doivent mieux servir.', 0, '2025-07-20 16:18:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 7),
(9, 'ÉDUCATION CENTRÉE SUR L''ESSENTIEL', 'L’école doit transmettre notre histoire, notre langue et nos valeurs, pas des idéologies hors sol. Suppression de la carte scolaire et mise en concurrence pour davantage de liberté parentale et moins de bureaucratie.', 0, '2025-07-20 16:19:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 8),
(10, 'FIN DU BLOCAGE LÉGISLATIF', 'Certaines structures étatiques bloquent les lois dont les Français ont besoin et masquent les réalités économiques et sociales. Réformons-les pour qu’elles servent le peuple, pas une caste.', 0, '2025-07-20 16:20:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 9),
(11, 'RAPPORT DE FORCE AVEC L''UE', 'Basta les diktats de Bruxelles qui étouffent nos entreprises et nos travailleurs. Exigeons un véritable rapport de force pour défendre nos intérêts.', 0, '2025-07-20 16:21:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 10),
(12, 'FIN DE L’IMMIGRATION DE MASSE', 'Au-delà de la fin des aides sociales et de la préservation de notre identité, demandons l’arrêt des mécanismes de régulation trop permissifs ainsi que l’expulsion des étrangers clandestins, délinquants ou inactifs de longue durée.', 0, '2025-07-20 16:22:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 11),
(13, 'REFUS DES RÉCUPÉRATIONS CONTRE-NATURE', 'Nous rejetons les récupérations politiques de la gauche ou des partis étatistes opposés à ce manifeste ainsi que les idéologies contraires à la liberté et à l’identité françaises.', 0, '2025-07-20 16:23:00', 0, 0, 0, 0, 'a3d76aa2-5141-4781-98f5-9b8f654b0d4c', 5);

SET IDENTITY_INSERT dbo.Proposals OFF;
