using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;
using System.Text;


public class QuicBehaviourScript : MonoBehaviour
{
    private static class NativeMethods
    {
        [DllImport("libquiche")]
        internal static extern IntPtr quiche_version();
    }

    string GetVersion()
    {
        // quiche_versionが返すのはスタック領域のメモリなので、直接stringに変換できない
        // そのため、一度IntPtrを介してからstringに変換する
        var version = NativeMethods.quiche_version();
        return Marshal.PtrToStringAnsi(version);
    }

    // Start is called before the first frame update
    void Start()
    {
        var version = GetVersion();
        Debug.Log(version);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
