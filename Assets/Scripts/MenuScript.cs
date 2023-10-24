using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Utility;

public class MenuScript : MonoBehaviour
{
    public GameObject menuPanel;

    public void ShowHideMenu()
    {
        if (menuPanel != null)
        {
            Animator anim = menuPanel.GetComponent<Animator>();
            if (anim != null)
            {
                bool isOpen = anim.GetBool("showMenu");
                anim.SetBool("showMenu", !isOpen);
            }
        }
    }
}