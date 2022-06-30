using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InfoText : Singleton<InfoText>
{
    public string[] MysticText =
    {
        "MYSTIC",

        "The Mystic is a spell flinging sorcress, adept at dealing huge amounts of damage\n" +
        "at a long distance. She is able to cast spells from both hands, providing her with\n" +
        "unparalelled flexibility when in the heat of battle.",

        "FIREBALL:\n" +
        "Launch a fireball, dealing damage to\n" +
        "whomever it hits",

        "LIGHTNING BOLT:\n" +
        "Fire a bolt of lightning, damaging and pushing\n" +
        "back the opponent",

        "ICE HEAL:\n" +
        "Temporarily sacrifice mobility to replenish\n" +
        "both health and mana",

        "FLAME CONE:\n" +
        "Your hands erupt in flame, dealing heavy damage\n" +
        "to nearby enemies",

        "ICE CLOUD:\n" +
        "Create a cloud of ice which damages and slows\n" +
        "enemies should they attempt to pass",

        "SHOCKWAVE:\n" +
        "Create a forcefull wave of air which launches\n" +
        "enemies in front into the distance"
    };

    public string[] KnightText =
    {
        "KNIGHT",

        "The Knight is a heavily armoured warrior, bearing both shield and sword as weapons.\n" +
        "When close, the Knight is capable of stunning enemies and using invigorating shouts to\n" +
        "improve both his attack and defence, before finishing them off with swift, lethal blows.",

        "SLASH:\n" +
        "Swing down your sword into an enemies flesh\n" +
        "with a mighty force, dealing good damage",

        "SHIELD BLOCK:\n" +
        "Hold your shield high, blocking all damage from\n" +
        "enemy attacks",

        "SHIELD SLAM:\n" +
        "Swiftly smash the foe with your heavy shield,\n" +
        "stunning them and dealing damage",

        "LIFE TRANSFER:\n" +
        "Fire a bolt of pure green energy, dealing damage\n" +
        "to those hit  and restoring health to you",

        "VALIANT SHOUT:\n" +
        "Let loose an inspiring roar, improving your\n" +
        "offensive and defensive capabilities",

        "FORCEFUL STOMP:\n" +
        "Stomp the ground, knocking back all enemies\n" +
        "who are standing close"
    };

    public string[] HuntressText =
    {
        "HUNTRESS",

        "The Huntress is a fierce Archer, wielding a powerful shortbow. She excels at a distance, firing\n" +
        "quick shots into her opponents or switching out for more powerful elemental arrows and traps,\n" +
        "luring enemies into a false sense of security before she ends them with a swift volley of arrows.",

        "QUICKSHOT:\n" +
        "Lightly damage your enemies by letting loose\n" +
        "a quick arrow from your quiver",

        "FLAME ARROW:\n" +
        "Switch your normal arrows for ones soaked in\n" +
        "pitch and flames, dealing heavy damage",

        "PARALYSING ARROW:\n" +
        "Fire an arrow which causes instant\n" +
        "incapacitation when it hits an enemy",

        "ENSNARE:\n" +
        "Lay down a near-invisible trap which holds\n" +
        "enemies in place while you fire unhindered",

        "HASTE:\n" +
        "Hasten yourself and your abilities, gaining\n" +
        "movement speed and reducing skill cooldowns",

        "CLEAVING SHOT:\n" +
        "Fire a huge arrow which cleaves waves of enemies\n" +
        "in front of you, shoving them to the side"
    };

    public string[] BarbarianText =
    {
        "BARBARIAN",

        "The Barbarian is a giant hulk of a man, impossible to beat in close-quarters combat. Bearing a\n" +
        "massive axe, he uses his anger and rage to deal more damage when his health is low. Not only\n" +
        "does the Barbarian deal high damage, he is also the fastest and most manoeuvrable of our Heroes.",

        "CHOP:\n" +
        "Swing down your heavy axe, dealing damage\n" +
        "to any enemy hit",

        "REVENGE:\n" +
        "By pumping your anger into your axes swing,\n" +
        "deal heavy damage the lower health you have",

        "RAGEFIRE:\n" +
        "Shoot a fireball filled with anger and hatred,\n" +
        "dealing higher damage the more you are hurt",

        "ENRAGE:\n" +
        "Sacrifice a third of your health, gaining\n" +
        "damage equal to the health lost",

        "SOOTHING BALM:\n" +
        "Take a moment of calm, healing yourself in\n" +
        "correlation to the health you have remaining",

        "CHARGE:\n" +
        "Raise your axe and hurtle towards your enemies,\n" +
        "pushing them along with you"
    };

    public string[] OverlordText =
    {
        "OVERLORD",

        "Take to skies as a God-like being. Rain spells of molten fire and ice down on the puny mortals\n" +
        "below, while staying safe from danger up above. The Overlord uses the Kinect to shoot down upon\n" +
        "the Arena; his spells effect the battlefield in different ways, so manipulate them wisely!",

        "METEOR:\n" +
        "Fire a ball of lava down to earth, exploding on\n" +
        "impact with the ground",

        "ICE RAIN:\n" +
        "A an icy rain falls from the heavens, damaging\n" +
        "and slowing enemies hit",

        "FIREBALL:\n" +
        "Shoot a small fireball which while very hard to hit,\n" +
        "deals good damage and can be fired frequently",

        "POLARITY:\n" +
        "Fire a powerful spell which explodes on impact,\n" +
        "pushing all characters aside",

        "",

        ""
    };
}