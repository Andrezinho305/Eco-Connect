using System.Collections.Generic;
using UnityEngine;

// No início da fase, apresenta os elementos listados em
// LevelData.newElements, um card por vez, destacando no tabuleiro
// onde cada elemento aparece.
public class LevelIntroManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileIntroCatalog catalog;
    [SerializeField] private PauseManager pauseManager;

    [Header("Interface")]
    [SerializeField] private TileIntroCardUI cardUI;
    [SerializeField] private TutorialHighlighter highlighter;

    private readonly List<TileIntroData> queue =
        new List<TileIntroData>();

    private int currentIndex;
    private bool isShowing;
    private bool isHiddenByPause;

    public bool IsShowing => isShowing;


    private void OnEnable()
    {
        cardUI.OnNextClicked += Next;
        cardUI.OnSkipClicked += Close;
    }

    private void OnDisable()
    {
        cardUI.OnNextClicked -= Next;
        cardUI.OnSkipClicked -= Close;

        OverlayEscape.Unregister(HandleEscape);
    }

    private void Start()
    {
        LevelData level = gridManager.CurrentLevel;

        if (level != null && level.newElements != null)
        {
            foreach (TileIntroData element in level.newElements)
            {
                if (element != null)
                    queue.Add(element);
            }
        }

        if (queue.Count == 0)
        {
            cardUI.SetVisible(false);
            return;
        }

        isShowing = true;
        currentIndex = 0;

        OverlayEscape.Register(HandleEscape);

        ShowCurrent();
    }

    private void Update()
    {
        if (!isShowing)
            return;

        // Esconde o card enquanto o jogo estiver pausado
        bool paused = pauseManager != null && pauseManager.IsPaused;

        if (paused != isHiddenByPause)
        {
            isHiddenByPause = paused;
            cardUI.SetVisible(!paused);
            highlighter.SetVisible(!paused);
        }

        if (!paused &&
            (Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Next();
        }
    }


    private void ShowCurrent()
    {
        TileIntroData element = queue[currentIndex];

        // Primeiro card abre o painel; os seguintes "viram a página"
        SFXManager.Play(currentIndex == 0 ? Sfx.PanelOpen : Sfx.TutorialStep);

        cardUI.Show(
            element.title,
            element.description,
            catalog != null ? catalog.GetIcon(element) : null,
            currentIndex + 1,
            queue.Count
        );

        highlighter.Clear();

        foreach (Vector2Int cell in
                 TileIntroCatalog.GetMatchingCells(gridManager, element))
        {
            highlighter.HighlightCell(
                gridManager.GetCellWorldPosition(cell),
                gridManager.CellSize
            );
        }
    }

    private void Next()
    {
        if (!isShowing)
            return;

        currentIndex++;

        if (currentIndex >= queue.Count)
        {
            Close();
            return;
        }

        ShowCurrent();
    }

    private void Close()
    {
        if (!isShowing)
            return;

        isShowing = false;

        SFXManager.Play(Sfx.PanelClose);

        highlighter.Clear();
        cardUI.SetVisible(false);

        OverlayEscape.Unregister(HandleEscape);
    }

    // Esc fecha os cards (equivale a "Pular")
    private bool HandleEscape()
    {
        if (!isShowing)
            return false;

        Close();
        return true;
    }
}
