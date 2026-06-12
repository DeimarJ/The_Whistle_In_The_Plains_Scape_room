using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : DynamicScreen
{

    [SerializeField]
    private ConsumableSelectorUI m_consumableSelector;
    [SerializeField] private Image m_damageFlash;
    [SerializeField] private TMP_Text m_interactionPrompt;
    public ConsumableSelectorUI ConsumableSelector => m_consumableSelector;
    public TMP_Text InteractionPrompt => m_interactionPrompt;
    public Image DamageFlash => m_damageFlash;
    protected override void CustomInit()
    {
    }
    protected override void CustomOpen()
    {
        m_consumableSelector.Refresh();
    }
    protected override void CustomClose()
    {
    }

}
