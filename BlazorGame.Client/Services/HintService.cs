using System;
using System.Collections.Generic;

namespace BlazorGame.Client.Services
{
    public class HintService
    {
        private readonly Random _rng = new();
        private readonly List<string> _hints = new()
        {
            "Combattre rapporte beaucoup de points, mais attention à vos points de vie !",
            "Fouiller une salle peut révéler des trésors... ou déclencher un piège.",
            "La fuite est parfois la meilleure stratégie si vous êtes blessé.",
            "Surveillez votre barre de santé : si elle atteint 0, l'aventure est finie.",
            "L'or trouvé en fouillant augmente votre score final.",
            "Certains monstres sont plus redoutables que d'autres. Soyez prudents.",
            "Chaque salle est une nouvelle épreuve : analysez la description avant d'agir.",
            "Le classement récompense les aventuriers les plus téméraires, pas seulement les survivants."
        };

        public string Current { get; private set; } = "";

        public string Next()
        {
            if (_hints.Count == 0) return Current = "";
            Current = _hints[_rng.Next(_hints.Count)];
            return Current;
        }

        public void Add(string hint)
        {
            if (!string.IsNullOrWhiteSpace(hint)) _hints.Add(hint.Trim());
        }
    }
}
