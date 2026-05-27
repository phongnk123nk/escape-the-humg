using UnityEngine;
using UnityEngine.SceneManagement;

public class DiDenBangXepHinh : MonoBehaviour
{
    public string sceneNameToLoad = "BangXepHinh";

    private void OnMouseDown()
    {
        if (!string.IsNullOrWhiteSpace(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
            return;
        }

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex >= 0 && nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            return;
        }

        Debug.LogWarning("Chua dien sceneNameToLoad cho nut chuyen scene.", this);
    }
}
