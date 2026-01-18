using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Loxodon.Framework.Binding;
using TMPro;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Button rollButton;
    public TMP_Text resultText;
    public TMP_Text historyText;

    private DiceViewModel model;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        model = new DiceViewModel(new Dice());
        
        rollButton.onClick.AddListener(model.Roll);
        
        model.Result.ValueChanged += (_, __) => resultText.text = model.Result.Value.ToString();
        model.History.CollectionChanged += (_, __) =>
        {
            historyText.text = string.Join("\n", model.History);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
