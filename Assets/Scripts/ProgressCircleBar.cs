using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ProgressCircleBar : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Image mask;
    [SerializeField]
    private Image fill;
    float ammount = 0.01f;
    bool start = true;
    bool isRunning = false;

    public void StartProgressBar()
    {
        isRunning = true;
        StartCoroutine(AddFill());
    }
    public void StopProgressBar()
    {
        isRunning = false;
        Destroy(this);
    }


    IEnumerator AddFill()
    {
        float actualAmmount = 0;
        while (isRunning)
        {
            yield return new WaitForSeconds(0.01f);
            actualAmmount = fill.fillAmount += ammount;
            if (actualAmmount >= 1.0f)
            {
                fill.fillClockwise = false;
                ammount *= -1;
            }
            else if (actualAmmount <= 0.0f)
            {
                fill.fillClockwise = true;
                ammount *= -1;
            }
        }
    }

}
