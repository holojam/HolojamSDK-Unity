//Synchronizable.cs
//Created by Aaron C Gaudette on 11.07.16

using UnityEngine;

namespace Holojam.Tools {
  public abstract class Synchronizable : Controller {

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private string label = "Synchronizable";

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private string scope = "";

    /// <summary>
    /// 
    /// </summary>
    [SerializeField]
    private bool sending = true;

    /// <summary>
    /// 
    /// </summary>
    public bool useMasterClient = false;

    protected override ProcessDelegate Process { get { return Sync; } }

    public override string labelField { get { return label; } }
    public override string scopeField { get { return scope; } }

    /// <summary>
    /// 
    /// </summary>
    public override bool isSending {
      get {
        return sending && (BuildManager.IsMasterClient() || !useMasterClient);
      }
    }

    //Override this in derived classes
    protected abstract void Sync();
  }
}
