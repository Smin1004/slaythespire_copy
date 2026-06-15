using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ViewCard : MonoBehaviour
{
    private Player player;

    public TextMeshProUGUI descText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nameText;
    public Image image;
    public Skill skill;

    public void Init(Skill _skill)
    {
        player = Player.Instance;
        skill = _skill;
        descText.text = skill.desc;
        costText.text = skill.cost.ToString();
        nameText.text = skill.name;
        image.sprite = skill.img;
        skill.effect.skillData = skill;
    }
}
