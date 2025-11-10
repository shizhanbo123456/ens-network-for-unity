using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NOMServer : ENCServer
{
    public static NOMServer nomInstance;

    // >0为游戏过程中制造的物体的Id
    private int createdid = 1;
    public int CreatedId
    {
        get
        {
            return createdid;
        }
        set
        {
            createdid = value;
        }
    }
    internal NOMServer(int port) :base(port)
    {
        nomInstance= this;
    }
    public override void ShutDown()
    {
        nomInstance = null;
        base.ShutDown();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        nomInstance = null;
        base.ReleaseUnmanagedMenory();
    }
}
