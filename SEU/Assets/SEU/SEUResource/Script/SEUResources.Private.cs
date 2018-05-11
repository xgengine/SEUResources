//#define SEU_DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class SEUResources
{
    protected Object m_Asset = null;
    protected Object asset
    {
        get
        {
            return m_Asset;
        }
    }
    protected System.Type m_Type = typeof( UnityEngine.Object);
    protected string m_LoadPath;

    public string loadPath
    {
        get
        {
            return m_LoadPath;
        }
    }

    protected Request m_LoadAysncRequest;

    private int m_RefCount = 0;

    private SEUResourcesPool m_Pool = null;

    protected List<SEUResources> m_DependenceResources = new List<SEUResources>();

    protected SEUResources(string path,System.Type type)
    {
        m_LoadPath = path;
        m_Type = type;
    }

    private string GUID()
    {
        return  ToResGUID(m_LoadPath, m_Type);
    }

    internal void Use()
    {
        m_RefCount++;
#if SEU_DEBUG
        Debug_StackInfo.Add("[Use]" + StackTraceUtility.ExtractStackTrace());
#endif
    }

    private void UnUsed()
    {

        if (m_RefCount == 0)
        {
            Debug.LogError("多次调用的 UnLoadUsedResource "+ StackTraceUtility.ExtractStackTrace());
        }
        else
        {
            m_RefCount--;
#if SEU_DEBUG
            Debug_StackInfo.Add("[UnUse]" + StackTraceUtility.ExtractStackTrace());
#endif
            if (m_RefCount == 0)
            {
                if(m_Pool != null)
                {
                    m_Pool.PopResource(this);
                    for (int i = 0; i < m_DependenceResources.Count; i++)
                    {
                        SEUResources.UnLoadResource(m_DependenceResources[i]);
                    }
                }
                else
                {
                    Debug.LogError(GetType()+ " pool is NULL");
                }            
            }
        }    
    }

    private void AttachPool(SEUResourcesPool pool)
    {
        m_Pool = pool;
    }

    protected virtual void ReleaseResource()
    {
    
    }

    protected virtual void LoadAsset() 
    {
    }

    protected virtual IEnumerator LoadAssetAsync()
    {
        yield break;
    }

    protected Request SendLoadAsyncRequest(System.Action<SEUResources> callback =null)
    {
        if(m_LoadAysncRequest == null)
        {
            m_LoadAysncRequest = new Request(this,callback);
        }
        return m_LoadAysncRequest;
    }

    protected void AddDependenceResources(SEUResources resource)
    {
        if(m_DependenceResources.Contains(resource) == false)
        {
            m_DependenceResources.Add(resource);
            resource.Use();
#if SEU_DEBUG
            resource.Debug_MarkStackInfo();
#endif
        }
    }

    protected void LogResult()
    {
        if (m_Asset == null)
        {
            Debug.LogError(GetType()+" Load failed :" + m_LoadPath);
        }
        else
        {
            Debug.Log(GetType()+ " Load Succeed :"+m_LoadPath );
        }
        
    }

#if SEU_DEBUG||UNITY_EDITOR
    public int refCount
    {
        get
        {
            return m_RefCount;
        }
    }
    public List<SEUResources> dependenceResources
    {
        get
        {
            return m_DependenceResources;
        }
    }
    public GameObject DebugObject;
    public void DebugCreateObject()
    {
        DebugObject = new GameObject(m_LoadPath);
        SEUDebugObject debugObject = DebugObject.AddComponent<SEUDebugObject>();
        debugObject.resource = this;
    }
    public List<string> Debug_StackInfo=new List<string>();
    public void Debug_MarkStackInfo()
    {
        Debug_StackInfo.Add("[Load]" + StackTraceUtility.ExtractStackTrace());
    }
#endif

    static private void UnLoadResource(SEUResources resource)
    {
        if (resource != null)
        {
            resource.UnUsed();
        }
    }

    static private SEUResources _Load(string path, System.Type type)
    {
        path = path.ToLower();
        SEUResources result = m_ResourcePool.Load(path, type);
        return result;
    }

    static private Request _LoadAsync(string path, System.Type type, System.Action<SEUResources> callback = null)
    {
        path = path.ToLower();
        return m_ResourcePool.LoadAsyn(path, type, callback);
    }

    static private Request LoadAsync(string path, System.Type type)
    {
        Request request = _LoadAsync(path, type,
            (resource) => {
                m_ObjectPool.PushResource(resource);
            }
        );
        return request;

    }

    private static string ToResGUID(string path, System.Type type)
    {
        return path + type.ToString();
    }

    static private Request LoadAsync(string path)
    {
        return LoadAsync(path, typeof(UnityEngine.Object));
    }
    static SEUResources()
    {
        m_ObjectPool = new SEUObjectPool();
        m_ResourcePool = new SEUResourcesPool();
        m_ResourcePool.InitPool("defualt", SEULoaderType.RESOURCE);
        InitSEUResources();
    }

    static SEUResourcesPool m_ResourcePool;

    static SEUObjectPool m_ObjectPool;
}


