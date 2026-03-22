using UIModule;
using UnityEngine;
using UnityEngine.UI;



public class TestUI : BaseUI
{
    #region ObjectBinder Auto Generated

    private ObjectBinder binder;
    public Button Button_Button { get; private set; }

    public void InitBind()
    {
        binder = View.GetComponent<ObjectBinder>();
        Button_Button = binder.Get<Button>(nameof(Button_Button));

        ObjectBinderUtility.SetButton(Button_Button,Button_ButtonOnClick);
    }

    private void Button_ButtonOnClick()
    {
        this.Close();
    }


    #endregion ObjectBinder Auto Generated

    protected override void OnOpen()
    {
        InitBind();
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