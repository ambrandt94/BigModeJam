using ChainLink.Core;
using ChainLink.Serializer;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    public enum TileDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public List<WorldTile> UpNeighbors { get { return upNeighbors != null ? upNeighbors : new List<WorldTile>(); } set { upNeighbors = value; } }
    public List<WorldTile> DownNeighbors { get { return downNeighbors != null ? downNeighbors : new List<WorldTile>(); } set { downNeighbors = value; } }
    public List<WorldTile> LeftNeighbors { get { return leftNeighbors != null ? leftNeighbors : new List<WorldTile>(); } set { leftNeighbors = value; } }
    public List<WorldTile> RightNeighbors { get { return rightNeighbors != null ? rightNeighbors : new List<WorldTile>(); } set { rightNeighbors = value; } }

    public bool IsEmpty;

    [SerializeField]
    private List<WorldTile> upNeighbors;
    [SerializeField]
    private List<WorldTile> downNeighbors;
    [SerializeField]
    private List<WorldTile> rightNeighbors;
    [SerializeField]
    private List<WorldTile> leftNeighbors;

    public List<string> UpNeighborTags => new List<string>(upNeighborTags.Split(','));
    public List<string> DownNeighborTags => new List<string>(downNeighborTags.Split(','));
    public List<string> LeftNeighborTags => new List<string>(leftNeighborTags.Split(','));
    public List<string> RightNeighborTags => new List<string>(rightNeighborTags.Split(','));

    [SerializeField, BoxGroup("Automation/Self")]
    private string upTag;
    [SerializeField, BoxGroup("Automation/Self")]
    private string downTag;
    [SerializeField, BoxGroup("Automation/Self")]
    private string leftTag;
    [SerializeField, BoxGroup("Automation/Self")]
    private string rightTag;

    [SerializeField, FoldoutGroup("Automation")]
    private string upNeighborTags;
    [SerializeField, FoldoutGroup("Automation")]
    private string downNeighborTags;
    [SerializeField, FoldoutGroup("Automation")]
    private string leftNeighborTags;
    [SerializeField, FoldoutGroup("Automation")]
    private string rightNeighborTags;

    public bool CheckIsValidNeighbor(WorldTile tile, TileDirection directionToTile)
    {
        switch (directionToTile) {
            case TileDirection.Up:
                if (UpNeighborTags.Contains(tile.downTag.ToLower()))
                    return true;
                break;
            case TileDirection.Down:
                if (DownNeighborTags.Contains(tile.upTag.ToLower()))
                    return true;
                break;
            case TileDirection.Left:
                if (LeftNeighborTags.Contains(tile.rightTag.ToLower()))
                    return true;
                break;
            case TileDirection.Right:
                if (RightNeighborTags.Contains(tile.leftTag.ToLower()))
                    return true;
                break;
        }
        return false;
    }

    private bool ListsContainAnyCommonElements(List<string> a, List<string> b)
    {
        for (int ia = 0; ia < a.Count; ia++) {
            for (int ib = 0; ib < b.Count; ib++) {
                if (a[ia].ToLower() == b[ib].ToLower())
                    return true;
            }
        }
        return false;
    }

    //1 CW | 2 CCW
    private void RotateNeighbors(int direction)
    {
        string oldUp = upNeighborTags;
        string oldDown = downNeighborTags;
        string oldLeft = leftNeighborTags;
        string oldRight = rightNeighborTags;
        if (direction == 1) {
            upNeighborTags = oldLeft;
            rightNeighborTags = oldUp;
            downNeighborTags = oldRight;
            leftNeighborTags = oldDown;
        }
        if (direction == -1) {
            upNeighborTags = oldRight;
            rightNeighborTags = oldDown;
            downNeighborTags = oldLeft;
            leftNeighborTags = oldUp;
        }
    }
    [ButtonGroup("Automation/Control"), Button("Copy Tags")]
    private void CopyTagLists()
    {
        Dictionary<string, string> tagTable = new Dictionary<string, string>();
        tagTable.Add("Up", upNeighborTags);
        tagTable.Add("Down", downNeighborTags);
        tagTable.Add("Left", leftNeighborTags);
        tagTable.Add("Right", rightNeighborTags);
        GUIUtility.systemCopyBuffer = ChainLinkSerializer.ToJson(tagTable);
    }
    [ButtonGroup("Automation/Control"), Button("Paste Tags")]
    private void PasteTagLists()
    {
        Dictionary<string, string> tagTable = ChainLinkSerializer.FromJson<Dictionary<string, string>>(GUIUtility.systemCopyBuffer);
        List<string> tags = new List<string>();
        if (tagTable.TryGetValue("Up", out upNeighborTags)) { }
        if (tagTable.TryGetValue("Down", out downNeighborTags)) { }
        if (tagTable.TryGetValue("Left", out leftNeighborTags)) { }
        if (tagTable.TryGetValue("Right", out rightNeighborTags)) { }
    }
    [ButtonGroup("Automation/Rotate"), Button("<<< Rotate Tags")]
    private void RotateTagsCounterClockwise()
    {
        RotateNeighbors(-1);
    }
    [ButtonGroup("Automation/Rotate"), Button("Rotate Tags >>>")]
    private void RotateTagsClockwise()
    {
        RotateNeighbors(1);
    }


    private void OnValidate()
    {

    }
}

[System.Serializable]
public class WorldTileData
{
    public int X;
    public int Y;
    public WorldTile Tile;
}
