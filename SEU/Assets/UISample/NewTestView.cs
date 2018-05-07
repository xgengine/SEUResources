using UnityEngine;
using System.Collections;
using MoleMole;

public class NewTestViewContext : BaseContext
{
    public NewTestViewContext(UIType uiType) : base(uiType)
    {

    }
}

public class NewTestView : BaseView
{
    public override void OnEnter(BaseContext context)
    {
        base.OnEnter(context);
        this.gameObject.SetActive(true);
    }
    public override void OnPause(BaseContext context)
    {
        base.OnPause(context);
        
        //Singleton<UIManager>.Instance.DestroySingleUI(UIType.NewTestView);
    }
    public override void OnExit(BaseContext context)
    {
        base.OnExit(context);
        this.gameObject.SetActive(false);
    }
    public void OnClickButton()
    {
       
        Singleton<ContextManager>.Instance.Pop();
    }
}