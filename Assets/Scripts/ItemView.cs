using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    [SerializeField] private Text _id;
    [SerializeField] private Text _title;
    [SerializeField] private Text _body;

    public void Setup(string id, string title, string body)
    {
        _id.text = id;
        _title.text = title;
        _body.text = body;
    }
}
