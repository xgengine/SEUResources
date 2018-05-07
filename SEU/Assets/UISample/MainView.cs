using UnityEngine;
using System.Collections;
using MoleMole;
public class MainViewContext : BaseContext
{
    public MainViewContext(UIType uiType) : base(uiType)
    {

    }
}

public class MainView : BaseView {
    
    public override void OnEnter(BaseContext context)
    {
        base.OnEnter(context);
        this.gameObject.SetActive(true);
    }
    public override void OnPause(BaseContext context)
    {      
        base.OnPause(context);
        this.gameObject.SetActive(false);
        
    }
    public override void OnExit(BaseContext context)
    {
        base.OnExit(context);
    }
    public override void OnResume(BaseContext context)
    {
        base.OnResume(context);
        this.gameObject.SetActive(true);
    }

    public void OnClickButton()
    {
        
        Singleton<ContextManager>.Instance.Push(new NewTestViewContext(UIType.NewTestView));
    }
    public void OnClickButtonCloseAll()
    {
        Singleton<ContextManager>.Instance.Pop();
        Singleton<UIManager>.Instance.DestroySingleUI(UIType.MainView);
        Singleton<UIManager>.Instance.DestroySingleUI(UIType.NewTestView);
    }
}
