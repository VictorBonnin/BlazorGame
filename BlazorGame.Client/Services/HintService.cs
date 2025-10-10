using System;
using System.Collections.Generic;

namespace BlazorGame.Client.Services
{
    public class HintService
    {
        private readonly Random _rng = new();
        private readonly List<string> _hints = new()
        {
            "Astuce 1",
            "Astuce 2",
            "Astuce 3",
            "Astuce 4",
            "Astuce 5"
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
