using UnityEngine;

// ���̃X�N���v�g�́AItemSlotParent�iContent�j��Transform�𑼂̃X�N���v�g�ɒ񋟂��邽�߂̂��̂ł��B
public class ItemSlotParentProvider : MonoBehaviour
{
    // ���g��Transform�����J���܂��BInspector�ɂ͕\������܂���B
    // �����InventoryUIController���擾���܂��B
    public Transform GetContentTransform()
    {
        return transform;
    }

    // �V���O���g���p�^�[���i�I�v�V�����j: ���̏ꏊ����ȒP�ɃA�N�Z�X�ł���悤�ɂ���
    // �������AContent�͈�����Ȃ��̂ŁAGetComponentInChildren�ȂǂŎ擾��������K�؂�������܂���B
    // public static ItemSlotParentProvider Instance { get; private set; }

    // void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         Instance = this;
    //     }
    // }
}