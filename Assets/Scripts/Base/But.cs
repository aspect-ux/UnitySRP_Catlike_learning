
using UnityEngine;
using UnityEngine.UI;

public class But : BasePanel
{
    private Button b;//创建Button
    private void Start()
    {
        GetControl<Button>(this.name).onClick.AddListener(Click);
        
    }
    void Click()
    {
        Debug.Log("ButtonClick");
    }
}