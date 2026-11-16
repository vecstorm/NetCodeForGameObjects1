using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    Canvas canvas;

    public void SetCanvasActive(GameObject canvas)
    {
        canvas.SetActive(true);
    }

    public void SetCanvasInactive(GameObject canvas)
    {
        canvas.SetActive(false);
    }


}
