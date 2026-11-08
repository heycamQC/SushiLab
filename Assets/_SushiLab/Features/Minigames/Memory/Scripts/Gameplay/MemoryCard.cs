
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MemoryCard : MonoBehaviour
{
    [Header("Referencias visuales")]
    [SerializeField] private Image frontImage;

    private Button cardButton;
    private int pairId;
    private bool isRevealed;
    private bool isMatched;

    private Action<MemoryCard> onCardSelected;

    public int PairId => pairId;

    private void Awake()
    {
        cardButton = GetComponent<Button>();
        cardButton.onClick.AddListener(HandleClick);

        if (frontImage != null)
        {
            frontImage.gameObject.SetActive(false);
        }
    }

    public void Initialize(
        int id,
        Sprite frontSprite,
        Action<MemoryCard> selectionCallback)
    {
        pairId = id;
        onCardSelected = selectionCallback;

        isRevealed = false;
        isMatched = false;

        frontImage.sprite = frontSprite;
        frontImage.gameObject.SetActive(false);

        cardButton.interactable = true;
    }

    private void HandleClick()
    {
        if (isRevealed || isMatched)
        {
            return;
        }

        onCardSelected?.Invoke(this);
    }

    public void Reveal()
    {
        if (isRevealed || isMatched)
        {
            return;
        }

        isRevealed = true;
        frontImage.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (isMatched)
        {
            return;
        }

        isRevealed = false;
        frontImage.gameObject.SetActive(false);
    }

    public void MarkMatched()
    {
        isMatched = true;
        isRevealed = true;

        frontImage.gameObject.SetActive(true);
        cardButton.interactable = false;
    }
}
