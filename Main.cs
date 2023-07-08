using BepInEx;
using System.Security;
using System.Security.Permissions;
using System.Text;
using TMPro;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace AtOHealthPercentage
{
    [BepInPlugin("com.DestroyedClone.AtOHealthPercentage", "AtO Health Percentage", "0.1.0")]
    public class Main : BaseUnityPlugin
    {
        //public static ConfigEntry<bool> cfgCShowEnemyPercentage;
        //public static ConfigEntry<bool> cfgCEnemyPercentageOnlyIfAura;
        //public static ConfigEntry<Color> cfgCColorForFullHealth;

        public static readonly string npcBattleFormat = "<size=-.5>COMBAT</size>\n{0}%";
        public static readonly string heroBattleFormat = "<size=-.5>COMBAT</size>\n{0}%";
        public static readonly string heroTravelFormat = "<size=-.5>REAL</size>\n{0}%";

        public void Awake()
        {
            //cfgCShowEnemyPercentage = Config.Bind("Enemy", "Show Health Percentage", true, "If true, also shows the enemy's health percentage.");
            //cfgCEnemyPercentageOnlyIfAura = Config.Bind("Enemy", "", true, "If true, then it will only show if ");
            On.CharacterItem.Start += CharacterItem_Start;
        }

        private void CharacterItem_Start(On.CharacterItem.orig_Start orig, CharacterItem self)
        {
            orig(self);
            bool isHero = self.Hero != null;
            var hpText = self.healthBar.transform.Find("HPText/HP_text");
            var hpTextPercent = Instantiate(hpText);
            hpTextPercent.transform.SetParent(hpText.parent);
            hpTextPercent.transform.SetLocalPositionAndRotation(new Vector3(-0.55f * (isHero ? 1f : -1f),
                0, 0), Quaternion.identity);
            var tmp = hpTextPercent.GetComponent<TextMeshPro>();
            var comp = hpTextPercent.gameObject.AddComponent<HealthPercentageDisplay>();
            comp.battleHP = tmp;
            comp.character = self;
            comp.isHero = isHero;
        }

        //from Character.GetMaxHP()
        //nstrip didnt publicize the private fields or im an idiot, anyways
        public static int GetMaxHP(Character character, bool includeItemStatModifier, bool includeAura, bool rescaleHpIfMaxHpIsLower = false)
        {
            int num = character.Hp;
            if (includeAura) num += character.GetAuraStatModifiers(0, Enums.CharacterStat.Hp);
            if (includeItemStatModifier) character.GetItemStatModifiers(Enums.CharacterStat.Hp);
            if (num <= 0)
            {
                num = 1;
            }
            if (rescaleHpIfMaxHpIsLower && character.HpCurrent > num)
            {
                character.HpCurrent = num;
            }
            return num;
        }

        public class HealthPercentageDisplay : MonoBehaviour
        {
            public CharacterItem character;
            public TextMeshPro battleHP;
            //public TextMeshPro realHP;

            public Color startColor = Color.green;
            public Color endColor = Color.red;

            public float stopwatch = 0;
            public float duration = 0.5f;

            public bool isHero;

            public string battleTextFormat = "";

            public void Awake()
            {
            }

            public void Start()
            {
                battleTextFormat = isHero ? heroBattleFormat : npcBattleFormat;
            }

            public void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch > 0)
                    return;
                stopwatch = duration;

                StringBuilder stringBuilder = new StringBuilder();
                //GetHpPercent
                //GetHP() = HPCurrent
                //var battleHPPercentage = (float)character.Hero.GetHp() / (float)character.Hero.GetMaxHP() * 100;
                int battleHPPercent;
                int travelHPPercent;
                if (isHero)
                {
                    battleHPPercent = Mathf.CeilToInt(character.Hero.GetHpPercent());
                    travelHPPercent = Mathf.CeilToInt((float)character.Hero.GetHp() / GetMaxHP(character.Hero, true, false) * 100);
                }
                else
                {
                    battleHPPercent = Mathf.CeilToInt(character.NPC.GetHpPercent());
                    travelHPPercent = Mathf.CeilToInt((float)character.NPC.GetHp() / GetMaxHP(character.NPC, true, false) * 100);
                }

                string battleText = string.Format(battleTextFormat, battleHPPercent);
                if (travelHPPercent >= 100)
                    battleText = "<color=green>" + battleText + "</color>";

                if (isHero)
                {
                    string realText = string.Format(heroTravelFormat, travelHPPercent);
                    if (travelHPPercent >= 100)
                        realText = "<color=green>" + realText + "</color>";
                    stringBuilder.AppendLine(realText);
                }

                stringBuilder.AppendLine(battleText);

                battleHP.SetText(stringBuilder.ToString());
                //battleHP.color = Color.Lerp(startColor, endColor, battleHPPercentage);
            }
        }
    }
}