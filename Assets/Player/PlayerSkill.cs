using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SkillEffect;

public class PlayerSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] private PlayerSkillData playerSkillData;
    [SerializeField] private PlayerSkillManager playerSkillManager;

    public bool isUnlock = false;// 스킬 잠금 상태
    public bool isOn = false;// 스킬 활성화 상태

    [Header("연결된 하위 스킬")]
    public PlayerSkill[] prevSkills;
    [Header("연결된 상위 스킬")]
    public PlayerSkill[] nextSkills;

    RectTransform rectTransform;
    Image image;
    [SerializeField] private TextMeshProUGUI text_skillName;
    [SerializeField] private TextMeshProUGUI skillDetailText;
    private GameObject skillDetailText_UI;

    private void OnEnable()
    {
        playerSkillManager.PlayerStats.ONLevelChanged += Update_NextSkill;
        Update_NextSkill(playerSkillManager.PlayerStats.Level);
    }
    private void OnDisable()
    {
        playerSkillManager.PlayerStats.ONLevelChanged -= Update_NextSkill;
    }

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        text_skillName.text = playerSkillData.skillName;
        skillDetailText_UI = skillDetailText.transform.parent.parent.gameObject;
        skillDetailText_UI.SetActive(false);

        if (playerSkillData.tier == PlayerSkillData.Tier.tier1) Unlock();
        else Lock();
    }

    public void OnPointerEnter(PointerEventData eventData)// 올렸을 때
    {
        rectTransform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        Update_SkillDetails();
        skillDetailText_UI.gameObject.SetActive(true);
    }
    void Update_SkillDetails()
    {
        skillDetailText_UI.GetComponent<RectTransform>().localPosition = rectTransform.localPosition;
        string _skillDetails = string.Empty;

        // 스킬 스탯 텍스트
        P_Stats stats = playerSkillData.stats;
        List<string> _statTexts = new List<string>();
        if (stats.HP > 0) _statTexts.Add("HP +" + stats.HP); else if (stats.HP < 0) _statTexts.Add("HP " + stats.HP);
        if (stats.DEF > 0) _statTexts.Add("DEF +" + stats.DEF); else if (stats.DEF < 0) _statTexts.Add("DEF " + stats.DEF);
        if (stats.STR > 0) _statTexts.Add("STR +" + stats.STR); else if (stats.STR < 0) _statTexts.Add("STR " + stats.STR);
        if (stats.DEX > 0) _statTexts.Add("DEX +" + stats.DEX); else if (stats.DEX < 0) _statTexts.Add("DEX " + stats.DEX);
        if (stats.INT > 0) _statTexts.Add("INT +" + stats.INT); else if (stats.INT < 0) _statTexts.Add("INT " + stats.INT);
        if (stats.Speed > 0) _statTexts.Add("Speed +" + stats.Speed); else if (stats.Speed < 0) _statTexts.Add("Speed " + stats.Speed);
        if (stats.Weight > 0) _statTexts.Add("Weight +" + stats.Weight); else if (stats.Weight < 0) _statTexts.Add("Weight " + stats.Weight);
        if (stats._EXP > 0) _statTexts.Add("EXP +" + stats._EXP); else if (stats._EXP < 0) _statTexts.Add("EXP " + stats._EXP);
        if (_statTexts.Count > 0)
        {
            for (int i = 0; i < _statTexts.Count; i++)
            {
                if (i > 0)
                    _skillDetails += ", ";

                _skillDetails += _statTexts[i];
            }
            _skillDetails += "\n";
        }

        // 스킬 효과 텍스트
        SkillEffect[] effects = playerSkillData.effects;
        for (int i = 0; i < effects.Length; i++)
        {
            _skillDetails += ChangeEffectName(effects[i].effect);

            string effectName = effects[i].effect.ToString();
            if (effectName.EndsWith("_percent"))
            {
                if (effects[i].value > 0)
                    _skillDetails += " +" + effects[i].value.ToString() + "%";
                else if (effects[i].value < 0)
                    _skillDetails += " " + effects[i].value.ToString() + "%";
            }
            _skillDetails += "\n";
        }

        if (isOn == false)
        {
            _skillDetails += "\n";

            // 필요 스킬포인트
            _skillDetails += "필요 SP : " + playerSkillData.SP_cost;
        }

        skillDetailText.text = _skillDetails;
    }
    string ChangeEffectName(EffectType _effect)
    {
        string s = string.Empty;
        s = _effect.GetInspectorName();
        
        return s.Replace("(%)", "");
    }

    public void OnPointerExit(PointerEventData eventData)// 나올 때
    {
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
        skillDetailText_UI.SetActive(false);

        if (isOn == false && isUnlock)
        {
            image.color = Color.white;
        }
    }

    public void OnPointerDown(PointerEventData eventData)// 눌렀을 때
    {
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)// 땔 때
    {
        rectTransform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        if (isOn == false && isUnlock)
        {
            // SP 소모
            if (playerSkillManager.PlayerStats.SP - playerSkillData.SP_cost < 0) return;
            playerSkillManager.PlayerStats.SP -= playerSkillData.SP_cost;

            isOn = true;
            Update_SkillDetails();
            switch (playerSkillData.tier)
            {
                case PlayerSkillData.Tier.tier1:
                    image.color = new Color(77 / 255f, 207 / 255f, 77 / 255f);
                    break;
                case PlayerSkillData.Tier.tier2:
                    image.color = new Color(77 / 255f, 145 / 255f, 207 / 255f);
                    break;
                case PlayerSkillData.Tier.tier3:
                    image.color = new Color(207 / 255f, 160 / 255f, 77 / 255f);
                    break;

                default:
                    image.color = new Color(207 / 255f, 93 / 255f, 77 / 255f);
                    break;
            }

            // ActivateSkill()
            playerSkillManager.Set_Total_Skill_Effects(playerSkillData.stats, playerSkillData.effects);

            Update_NextSkill(playerSkillManager.PlayerStats.Level);
        }
    }
    public void Update_NextSkill(int _level)// 다음 스킬들의 잠금상태 업데이트
    {
        for (int i = 0; i < nextSkills.Length; i++)
        {
            // 다음 스킬의 티어별 레벨 조건 확인
            int _requiredLevel = 1;
            switch (nextSkills[i]?.playerSkillData.tier)
            {
                case PlayerSkillData.Tier.tier2:
                    _requiredLevel = 10;
                    break;
                case PlayerSkillData.Tier.tier3:
                    _requiredLevel = 20;
                    break;
                case PlayerSkillData.Tier.tier4:
                    _requiredLevel = 25;
                    break;
            }
            if (_level < _requiredLevel) return;

            // 선행 노드가 모두 찍혀있는지 확인
            bool _isOn = true;

            if (nextSkills[i]?.prevSkills == null) continue;
            for (int j = 0; j < nextSkills[i].prevSkills.Length; j++)
            {
                if (nextSkills[i].prevSkills[j].isOn == false)
                {
                    _isOn = false;
                    break;
                }
            }
            if (_isOn && nextSkills[i].isUnlock == false) nextSkills[i].Unlock();
        }
    }

    void Lock()
    {
        isUnlock = false;
        image.color = Color.gray;
    }
    void Unlock()
    {
        isUnlock = true;
        image.color = Color.white;
    }

}