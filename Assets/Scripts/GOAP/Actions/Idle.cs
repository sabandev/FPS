using UnityEngine;

public class Idle : GOAP_Action
{
    public override bool PreAction()
    {
        return true;
    }
    
    public override bool DuringAction()
    {
        return true;
    }
    
    public override bool PostAction()
    {
        return true;
    }
}
