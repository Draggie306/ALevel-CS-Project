using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipChanger : MonoBehaviour
{
    private float timer;
    void Start()
    {
        System.Func<string> getTooltipTextFunc = () => {
            return "This is a tooltip" + timer;
        };
        TooltipScreenSpaceUI.ShowTooltip_Static(getTooltipTextFunc);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
    }
}
