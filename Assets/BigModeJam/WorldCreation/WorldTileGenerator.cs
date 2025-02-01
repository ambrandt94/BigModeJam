using ChainLink.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class WorldTileGenerator : MonoBehaviour
{
    public WorldTile[] TileObjects;
    public List<WorldCell> Cells;
    public WorldCell CellPrefab;

    public int Dimensions;
    public float Spacing = 1;

    public List<WorldTileData> worldTileDatas = new List<WorldTileData>();

    private int iterations = 0;

    private void InitializeGrid()
    {
        for (int y = 0; y < Dimensions; y++) {
            for (int x = 0; x < Dimensions; x++) {
                WorldCell newCell = Instantiate(CellPrefab, new Vector3(x * Spacing, 0, y * Spacing), Quaternion.identity);
                newCell.CreateCell(false, TileObjects);
                Cells.Add(newCell);
            }
        }
        StartCoroutine(CheckEntropyRoutine());
    }

    private IEnumerator CheckEntropyRoutine()
    {
        List<WorldCell> tempGrid = new List<WorldCell>(Cells);
        tempGrid.RemoveAll(c => c.Collapsed);
        tempGrid.Sort((a, b) => { return a.TileOptions.Length - b.TileOptions.Length; });
        tempGrid.RemoveAll(x => x.TileOptions.Length != tempGrid[0].TileOptions.Length);

        yield return new WaitForSeconds(.001f);
        CollapseCell(tempGrid);
    }

    void CollapseCell(List<WorldCell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        WorldCell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.Collapsed = true;
        Debug.Log($"Cell has {cellToCollapse.TileOptions.Length} options", cellToCollapse.gameObject);
        WorldTile selectedTile = TileObjects[UnityEngine.Random.Range(0, TileObjects.Length)];
        if (cellToCollapse.TileOptions.Length > 0) {
            selectedTile = cellToCollapse.TileOptions[UnityEngine.Random.Range(0, cellToCollapse.TileOptions.Length)];
        }
        cellToCollapse.TileOptions = new WorldTile[] { selectedTile };

        WorldTile foundTile = cellToCollapse.TileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);

        UpdateGeneration();
    }

    private void CheckValidity(List<WorldTile> options, List<WorldTile> validOption)
    {
        for (int x = options.Count - 1; x >= 0; x--) {
            var element = options[x];
            if (!validOption.Contains(element)) {
                options.RemoveAt(x);
            }
        }
    }

    private void UpdateGeneration()
    {
        List<WorldCell> newGenerationCell = new List<WorldCell>(Cells);

        for (int y = 0; y < Dimensions; y++) {
            for (int x = 0; x < Dimensions; x++) {
                var index = x + y * Dimensions;
                if (Cells[index].Collapsed) {
                    newGenerationCell[index] = Cells[index];
                } else {
                    List<WorldTile> options = new List<WorldTile>();
                    foreach (WorldTile t in TileObjects) {
                        options.Add(t);
                    }

                    //update above
                    if (y > 0) {
                        int before = options.Count;
                        WorldCell up = Cells[x + (y - 1) * Dimensions];
                        List<WorldTile> validOptions = new List<WorldTile>();
                        try {
                            foreach (WorldTile possibleOptions in up.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].UpNeighbors;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        } catch (Exception e) {
                            Debug.LogError("NNork");
                        }

                        CheckValidity(options, validOptions);
                        int after = options.Count;
                        Debug.Log($"Checked UP. {before} -> {after}");
                    }

                    //update right
                    if (x < Dimensions - 1) {
                        int before = options.Count;
                        WorldCell right = Cells[x + 1 + y * Dimensions];
                        List<WorldTile> validOptions = new List<WorldTile>();

                        try {
                            foreach (WorldTile possibleOptions in right.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].LeftNeighbors;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                            CheckValidity(options, validOptions);
                            int after = options.Count;
                            Debug.Log($"Checked RIGHT. {before} -> {after}", right.gameObject);
                        } catch (Exception e) {
                            Debug.LogError($"Gen Error: {e}", right);
                        }
                    }

                    //look down
                    if (y < Dimensions - 1) {
                        int before = options.Count;
                        WorldCell down = Cells[x + (y + 1) * Dimensions];
                        List<WorldTile> validOptions = new List<WorldTile>();

                        foreach (WorldTile possibleOptions in down.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].DownNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                        int after = options.Count;
                        Debug.Log($"Checked DOWN. {before} -> {after}", down.gameObject);

                    }

                    //look left
                    if (x > 0) {
                        int before = options.Count;

                        WorldCell left = Cells[x - 1 + y * Dimensions];
                        List<WorldTile> validOptions = new List<WorldTile>();

                        foreach (WorldTile possibleOptions in left.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].RightNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                        int after = options.Count;
                        Debug.Log($"Checked LEFT. {before} -> {after}", left.gameObject);

                    }
                    WorldTile[] newTileList = new WorldTile[options.Count];

                    for (int i = 0; i < options.Count; i++) {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
            //List<WorldCell> newGenCell = new List<WorldCell>(Cells);
            //for (int y = 0; y < Dimensions; y++) {
            //    for (int x = 0; x < Dimensions; x++) {
            //        var index = x + y * Dimensions;
            //        bool isEmptyTile = false;
            //        if (Cells[index].Collapsed) {
            //            isEmptyTile = Cells[index].ContainsEmptyTile;
            //            if (!isEmptyTile) {
            //                newGenCell[index] = Cells[index];
            //            }
            //        } else {
            //            List<WorldTile> options = new List<WorldTile>();
            //            foreach (WorldTile t in TileObjects) {
            //                options.Add(t);
            //            }

            //            if (y > 0) {
            //                WorldCell up = Cells[x + (y - 1) * Dimensions];
            //                List<WorldTile> validOptions = new List<WorldTile>();
            //                foreach (WorldTile option in up.TileOptions) {
            //                    var valOption = Array.FindIndex(TileObjects, obj => obj == option);
            //                    var valid = TileObjects[valOption].UpNeighbors;

            //                    validOptions = validOptions.Concat(valid).ToList();
            //                }
            //                CheckValidity(options, validOptions);
            //            }

            //            if (x < Dimensions - 1) {
            //                WorldCell right = Cells[x + 1 + y * Dimensions];
            //                List<WorldTile> validOptions = new List<WorldTile>();

            //                foreach (WorldTile option in right.TileOptions) {
            //                    var valOption = Array.FindIndex(TileObjects, obj => obj == option);
            //                    var valid = TileObjects[valOption].LeftNeighbors;

            //                    validOptions = validOptions.Concat(valid).ToList();
            //                }
            //                CheckValidity(options, validOptions);
            //            }

            //            if (y < Dimensions - 1) {
            //                WorldCell down = Cells[x + (y + 1) * Dimensions];
            //                List<WorldTile> validOptions = new List<WorldTile>();

            //                foreach (WorldTile option in down.TileOptions) {
            //                    var valOption = Array.FindIndex(TileObjects, obj => obj == option);
            //                    var valid = TileObjects[valOption].DownNeighbors;

            //                    validOptions = validOptions.Concat(valid).ToList();
            //                }
            //                CheckValidity(options, validOptions);
            //            }

            //            if (x > 0) {
            //                WorldCell left = Cells[x - 1 + y * Dimensions];
            //                List<WorldTile> validOptions = new List<WorldTile>();

            //                foreach (WorldTile option in left.TileOptions) {
            //                    var valOption = Array.FindIndex(TileObjects, obj => obj == option);
            //                    var valid = TileObjects[valOption].RightNeighbors;

            //                    validOptions = validOptions.Concat(valid).ToList();
            //                }
            //                CheckValidity(options, validOptions);
            //            }

            //            WorldTile[] newTileList = new WorldTile[options.Count];
            //            for (int i = 0; i < options.Count; i++) {
            //                newTileList[i] = options[i];
            //            }
            //            newGenCell[index].RecreateCell(newTileList);
            //        }
            //        if (Cells[index].Collapsed && Cells[index].ContainsEmptyTile) {
            //            newGenCell[index].RecreateCell(TileObjects);
            //        }
            //    }
        }

        Cells = newGenerationCell;
        iterations++;

        if (iterations < Dimensions * Dimensions) {
            StartCoroutine(CheckEntropyRoutine());
        }
    }

    [Button("Setup tile neighbors")]
    private void SetupNeighbors()
    {
        for (int i = 0; i < TileObjects.Length; i++) {
            WorldTile editTile = TileObjects[i];
            editTile.UpNeighbors = new List<WorldTile>();
            editTile.DownNeighbors = new List<WorldTile>();
            editTile.LeftNeighbors = new List<WorldTile>();
            editTile.RightNeighbors = new List<WorldTile>();
            for (int j = 0; j < TileObjects.Length; j++) {
                WorldTile checkTile = TileObjects[j];
                if (editTile.CheckIsValidNeighbor(checkTile, WorldTile.TileDirection.Up))
                    editTile.UpNeighbors.Add(checkTile);
                if (editTile.CheckIsValidNeighbor(checkTile, WorldTile.TileDirection.Down))
                    editTile.DownNeighbors.Add(checkTile);
                if (editTile.CheckIsValidNeighbor(checkTile, WorldTile.TileDirection.Left))
                    editTile.LeftNeighbors.Add(checkTile);
                if (editTile.CheckIsValidNeighbor(checkTile, WorldTile.TileDirection.Right))
                    editTile.RightNeighbors.Add(checkTile);
            }
            ChainUtils.MarkDirty(editTile.gameObject);
        }
    }

    [Button("Check neighbors")]
    private void CheckNeighbors()
    {
        for (int i = 0; i < TileObjects.Length; i++) {
            WorldTile editTile = TileObjects[i];
            bool error = false;
            foreach (WorldTile tile in editTile.UpNeighbors) {
                if (!tile.DownNeighbors.Contains(editTile)) {
                    error = true;
                    Debug.LogError($"Conflict with : {editTile.gameObject.name}");
                }
            }
            foreach (WorldTile tile in editTile.DownNeighbors) {
                if (!tile.UpNeighbors.Contains(editTile)) {
                    error = true;
                    Debug.LogError($"Conflict with : {editTile.gameObject.name}");
                }
            }
            foreach (WorldTile tile in editTile.LeftNeighbors) {
                if (!tile.RightNeighbors.Contains(editTile)) {
                    error = true;
                    Debug.LogError($"Conflict with : {editTile.gameObject.name}");
                }
            }
            foreach (WorldTile tile in editTile.RightNeighbors) {
                if (!tile.LeftNeighbors.Contains(editTile)) {
                    error = true;
                    Debug.LogError($"Conflict with : {editTile.gameObject.name}");
                }
            }
            if (!error)
                Debug.Log($"Cleared Tile: {editTile.gameObject.name}");
        }
    }

    private void Awake()
    {
        Cells = new List<WorldCell>();
        InitializeGrid();
    }
}
