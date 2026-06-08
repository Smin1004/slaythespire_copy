using UnityEngine;
using TMPro;

public class EntityBlock : MonoBehaviour
{
    [SerializeField] private Entity _entity;

    [SerializeField] private GameObject _blockRoot;
    [SerializeField] private TMP_Text _blockText;

    private void OnEnable()
    {
        _entity.OnBlockChanged += UpdateBlock;
    }

    private void OnDisable()
    {
        _entity.OnBlockChanged -= UpdateBlock;
    }

    private void Start()
    {
        UpdateBlock(_entity.CurrentBlock);
    }

    private void UpdateBlock(int block)
    {
        _blockRoot.SetActive(block > 0);

        if (block > 0)
            _blockText.text = block.ToString();
    }
}