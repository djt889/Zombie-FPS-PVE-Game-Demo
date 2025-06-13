using UnityEngine;

// ��������������
public class SceneWeaponManager : MonoBehaviour
{
    public static SceneWeaponManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // ��������ӵ���������
    public void AddWeaponToScene(Transform weapon)
    {
        weapon.SetParent(transform);

        // �����ת�Ƕȣ�ʹ������������������Ȼ��
        float randomYRotation = Random.Range(0f, 360f);
        weapon.rotation = Quaternion.Euler(0, randomYRotation, 0);
    }

    // �ӳ����������Ƴ�����
    public void RemoveWeaponFromScene(Transform weapon)
    {
        if (weapon.parent == transform)
        {
            weapon.SetParent(null);
        }
    }
}