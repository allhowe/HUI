using UIModule;
using UnityEngine;
using UnityEngine.UI;



public class TestUI : BaseUI
{
    protected override void OnOpen()
    {
        View.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            this.Close();
        });
    }
}

public class TestParameter
{
    public string test;
    public int value;
}

public class TestUI1 : BaseUI<TestParameter>
{
    protected override void OnOpen()
    {
        Debug.Log("TestUI1 " + Parameter.value);
    }
}
public class TestUI2 : TestUI1
{
    protected override void OnOpen()
    {

        Debug.Log("TestUI2 " + Parameter.value);
    }
}