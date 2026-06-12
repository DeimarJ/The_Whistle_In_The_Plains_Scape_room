using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableSelectorUI : MonoBehaviour
{

    [Header("Previous")]
    [SerializeField] private GameObject leftEffectContainer;
    [SerializeField] private Image leftIcon;
    [SerializeField] private TMP_Text leftAmount;
    [SerializeField] private TMP_Text leftValue;
    [Header("Current")]
    [SerializeField] private Image centerIcon;
    [SerializeField] private TMP_Text centerAmount;
    [SerializeField] private TMP_Text centerValue;
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;

    [Header("Next")]
    [SerializeField] private GameObject rightEffectContainer;
    [SerializeField] private Image rightIcon;
    [SerializeField] private TMP_Text rightAmount;
    [SerializeField] private TMP_Text rightValue;


    private Dictionary<ConsumableEffectType,
        List<ConsumableData>> consumablesByEffect;

    private Dictionary<ConsumableEffectType, int>
    selectedIndexPerEffect;
    private readonly ConsumableEffectType[] effects =
{
    ConsumableEffectType.Oil,
    ConsumableEffectType.Heal,
    ConsumableEffectType.Chili
};
    private int currentEffectIndex;

    private Inventory Inventory => MainScene.Player.Inventory;
    private ConsumableDatabase Database => MainGame.ConsumableDatabase;
    public ConsumableData SelectedConsumable
    {
        get
        {
            ConsumableEffectType effect =
                effects[currentEffectIndex];

            int variantIndex =
                selectedIndexPerEffect[effect];

            return consumablesByEffect[effect][variantIndex];
        }
    }
    private void Start()
    {
        consumablesByEffect =
            new Dictionary<ConsumableEffectType,
            List<ConsumableData>>
        {
        {
            ConsumableEffectType.Oil,
            new List<ConsumableData>
            {
                Database.Get(ResourceType.SmallOil),
                Database.Get(ResourceType.BigOil)
            }
        },
        {
            ConsumableEffectType.Heal,
            new List<ConsumableData>
            {
                Database.Get(ResourceType.SmallMedice),
                Database.Get(ResourceType.MediumMedicine),
                Database.Get(ResourceType.BigMedicine)
            }
        },
        {
            ConsumableEffectType.Chili,
            new List<ConsumableData>
            {
                Database.Get(ResourceType.Chili)
            }
        }
        };

        selectedIndexPerEffect =
            new Dictionary<ConsumableEffectType, int>
        {
        { ConsumableEffectType.Oil, 0 },
        { ConsumableEffectType.Heal, 0 },
        { ConsumableEffectType.Chili, 0 }
        };

        Refresh();
    }

    public void Next()
    {
        currentEffectIndex++;

        if (currentEffectIndex >= effects.Length)
        {
            currentEffectIndex = 0;
        }

        Refresh();
    }
    public void Previous()
    {
        currentEffectIndex--;

        if (currentEffectIndex < 0)
        {
            currentEffectIndex = effects.Length - 1;
        }

        Refresh();
    }

    public void NextVariant()
    {
        ConsumableEffectType effect =
            effects[currentEffectIndex];

        List<ConsumableData> variants =
            consumablesByEffect[effect];

        if (variants.Count <= 1)
        {
            return;
        }

        int index =
            selectedIndexPerEffect[effect];

        index++;

        if (index >= variants.Count)
        {
            index = 0;
        }

        selectedIndexPerEffect[effect] = index;

        Refresh();
    }
    public void PreviousVariant()
    {

        ConsumableEffectType effect =
            effects[currentEffectIndex];

        List<ConsumableData> variants =
            consumablesByEffect[effect];

        if (variants.Count <= 1)
        {
            return;
        }

        int index =
            selectedIndexPerEffect[effect];

        index--;

        if (index < 0)
        {
            index = variants.Count - 1;
        }

        selectedIndexPerEffect[effect] = index;

        Refresh();
    }

    public void Refresh()
    {
        int effectCount = effects.Length;

        ConsumableEffectType centerEffect =
            effects[currentEffectIndex];

        SetSlot(
            centerIcon,
            centerAmount,
            centerValue,
            GetSelectedConsumable(centerEffect));

        if (effectCount == 1)
        {
            leftEffectContainer.SetActive(false);
            rightEffectContainer.SetActive(false);
        }
        else if (effectCount == 2)
        {
            int otherIndex =
                (currentEffectIndex + 1) % 2;

            ConsumableEffectType otherEffect =
                effects[otherIndex];

            SetSlot(
                leftIcon,
                leftAmount,
                leftValue,
                GetSelectedConsumable(otherEffect));

            SetSlot(
                rightIcon,
                rightAmount,
                rightValue,
                GetSelectedConsumable(otherEffect));

            leftEffectContainer.SetActive(true);
            rightEffectContainer.SetActive(true);
        }
        else
        {
            ConsumableEffectType leftEffect =
                effects[(currentEffectIndex - 1 + effectCount) % effectCount];

            ConsumableEffectType rightEffect =
                effects[(currentEffectIndex + 1) % effectCount];

            SetSlot(
                leftIcon,
                leftAmount,
                leftValue,
                GetSelectedConsumable(leftEffect));

            SetSlot(
                rightIcon,
                rightAmount,
                rightValue,
                GetSelectedConsumable(rightEffect));

            leftEffectContainer.SetActive(true);
            rightEffectContainer.SetActive(true);
        }

        RefreshVariantArrows();
    }
    private ConsumableData GetSelectedConsumable(
    ConsumableEffectType effect)
    {
        int variantIndex =
            selectedIndexPerEffect[effect];

        return consumablesByEffect[effect][variantIndex];
    }
    private void SetSlot(
    Image icon,
    TMP_Text amountText,
    TMP_Text valueText,
    ConsumableData consumable)
    {
        icon.sprite = consumable.icon;

        // Cantidad que posee el jugador
        amountText.text =
            Inventory.GetResource(
                consumable.resourceType)
            .ToString();

        // Valor del consumible
        valueText.text = (consumable.amount==1)?"":
            "+" + consumable.amount.ToString();
    }

    private void RefreshVariantArrows()
    {
        ConsumableEffectType currentEffect =
            effects[currentEffectIndex];

        bool hasVariants =
            consumablesByEffect[currentEffect].Count > 1;

        upArrow.gameObject.SetActive(hasVariants);
        downArrow.gameObject.SetActive(hasVariants);
    }

}