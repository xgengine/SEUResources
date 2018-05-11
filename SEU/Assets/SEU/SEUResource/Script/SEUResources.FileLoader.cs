using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 文件加载器
/// </summary>
public class SEUFileLoader
{
    static public byte[] ReadAllBytes(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        else
        {
            return null;
        }

    }
}
