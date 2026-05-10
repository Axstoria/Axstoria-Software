using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App
{
    public class RemoveStatUseCase
    {
        public void Execute(Sheet sheet, string statId)
        {
            if (!sheet.HasStat(statId)) return;
            sheet.RemoveStat(statId);
        }
    }
}
