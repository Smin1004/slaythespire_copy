using UnityEngine;
using TMPro;

public class EntityBlock : MonoBehaviour
{
    [SerializeField] private Entity _entity;

    [SerializeField] private GameObject _blockRoot;
    [SerializeField] private TMP_Text _blockText;

    private void OnEnable()
    {
        if (_entity != null)
            _entity.OnBlockChanged += UpdateBlock;
    }

    private void OnDisable()
    {
        if (_entity != null)
            _entity.OnBlockChanged -= UpdateBlock;
    }

    private void Start()
    {
        if (_entity != null)
            UpdateBlock(_entity.CurrentBlock);
        else
            UpdateBlock(0);
    }

    private void UpdateBlock(int block)
    {
        if (_blockRoot != null)
            _blockRoot.SetActive(block > 0);

        if (block > 0 && _blockText != null)
            _blockText.text = block.ToString();
    }

    /// <summary>
    /// Entity를 바인딩하는 함수입니다.
    /// </summary>
    public void Bind(Entity entity)
    {
        if (_entity != null)
            _entity.OnBlockChanged -= UpdateBlock;

        _entity = entity;

        if (_entity != null)
        {
            _entity.OnBlockChanged += UpdateBlock;
            UpdateBlock(_entity.CurrentBlock);
        }
    }
}
