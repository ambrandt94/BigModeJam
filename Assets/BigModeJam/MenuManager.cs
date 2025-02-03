using ChainLink.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [SerializeField]
    private CanvasGroup coverGroup;
    [SerializeField]
    private CanvasGroup loadingGroup;
    [SerializeField]
    private Slider loadingSlider;

    public void ToggleLoading(bool loading)
    {
        if (coverGroup != null && loadingGroup != null) {
            loadingGroup.DOFade(loading ? 1 : 0, .1f);
            coverGroup.blocksRaycasts = loading;
        }
    }

    public void SetLoadAmount(float fill)
    {
        Debug.Log("Attempting to set load amount");
        if (loadingSlider == null)
            return;
        loadingSlider.value = fill;
    }

    public void ToggleCover(bool active)
    {
        if (coverGroup == null)
            return;
        coverGroup.DOFade(active ? 1 : 0, .1f);
        coverGroup.blocksRaycasts = active;
    }

    public void TravelToGame()
    {
        SceneManager.LoadScene("Game");
        ToggleCover(true);
        ToggleLoading(true);
        SetLoadAmount(0);
    }

    public void TravelToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TravelToMenu();
    }
}
