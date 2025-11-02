using System;
using UnityEngine;

namespace Debrief
{
    [Serializable]
    public class KillRecord
    {
        public Sprite WeaponSprite { get; }
        public Sprite? VictimSprite { get; }
        public string Victim { get; }
        public int KillCount { get; set; }
        public TimeSpan FirstKillTime { get; set; }
        public KillRecord(Sprite weaponSprite, Sprite? victimSprite, string victim, int killCount, TimeSpan firstKillTime)
        {
            WeaponSprite = weaponSprite;
            VictimSprite = victimSprite;
            Victim = victim;
            KillCount = killCount;
            FirstKillTime = firstKillTime;
        }
        
    }
}